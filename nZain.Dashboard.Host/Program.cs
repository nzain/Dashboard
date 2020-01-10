using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using nZain.Dashboard.Services;
using static System.Environment;

namespace nZain.Dashboard.Host
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // NLog: setup the logger first to catch all errors
            LogFactory nlog = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config");
#if DEBUG
            DebugLoggingConfig.ActivateDebugLogging();
#endif
            var logger = nlog.GetCurrentClassLogger();
            logger.Info("Startup...");

            try
            {

                // 1) read private config file (not on github)
                string secretsFilename = "Secrets/DashboardConfig.json";
                if (File.Exists(secretsFilename))
                {
                    using (var r = File.OpenRead(secretsFilename))
                    {
                        Config = await JsonSerializer.DeserializeAsync<DashboardConfig>(r);
                    }
                }
                else
                {
                    Config = new DashboardConfig();
                    Directory.CreateDirectory("Secrets/");
                    using (var w = File.Create(secretsFilename))
                    {
                        var options = new JsonSerializerOptions{WriteIndented = true};
                        await JsonSerializer.SerializeAsync(w, Config, options);
                    }
                }
#if DEBUG
                // this UNC path doesn't work on raspbian!
                Config.BackgroundImagesPath = @"\\SynologyDS218j\photo\DashboardBackgrounds";
#endif

                // 2) this one is complex, async, and long. We can't properly call this during ConfigureServices...
                GoogleService = await GoogleCalendarService.GoogleCalendarAuthAsync();

                // Want to know, which calendar IDs you have?
                // foreach (var kvp in await CalendarListService.QueryCalendarIdsAsync(GoogleService))
                // {
                //     Console.WriteLine($"CalendarID;Summary: {kvp.Key};{kvp.Value}");
                // }

#if RELEASE
                // 3) PIR sensor to control the monitor
                PirSensorService = new PirSensorService(Config, new MonitorService());
                PirSensorService.Start();
#endif

                logger.Info("Build WebHost...");
                var host = CreateWebHostBuilder(args).Build();
                logger.Info("Run WebHost!");
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        internal static DashboardConfig Config { get; private set; }

        internal static CalendarService GoogleService { get; private set; }

        internal static PirSensorService PirSensorService { get; private set; }

        public static string WebRoot => RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            ? "/home/pi/webapp/wwwroot/" // not sure why we need this on linux.. ? Otherwise images/css don't show
            : "./wwwroot/";

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                // .UseKestrel(options =>
                //     {
                //         options.Listen(IPAddress.Loopback, 5000);  // http:localhost:5000
                //         //options.Listen(IPAddress.Any, 80);         // http:*:80
                //         // options.Listen(IPAddress.Loopback, 443, listenOptions =>
                //         // {
                //         //     listenOptions.UseHttps("certificate.pfx", "password");
                //         // });
                //     })
                .UseKestrel(o => o.Listen(IPAddress.Loopback, 5000)) // http:localhost:5000
                .UseWebRoot(WebRoot)
                .UseStartup<Startup>()
                .ConfigureLogging(logging => logging.ClearProviders())
                .UseNLog();  // NLog: setup NLog for Dependency injection
    }
}
