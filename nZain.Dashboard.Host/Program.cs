using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using nZain.Dashboard.Services;

namespace nZain.Dashboard.Host
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var service = new WeatherService();
            var forecast = await service.GetForecastAsync();
            System.Console.WriteLine(forecast);



            // this one is complex, async, and long. We can't properly call this during ConfigureServices...
            GoogleService = await GoogleCalendarService.GoogleCalendarAuthAsync();

            // Want to know, which calendar IDs you have?
            // foreach (var kvp in await CalendarListService.QueryCalendarIdsAsync(GoogleService))
            // {
            //     Console.WriteLine($"CalendarID;Summary: {kvp.Key};{kvp.Value}");
            // }

            // load CalendarIds from a file, we don't want to commit this to public repo ;)
            CalendarListService = await CalendarListService.LoadAsync();
            
            var host = CreateWebHostBuilder(args).Build();
            await host.RunAsync();
        }

        internal static CalendarService GoogleService { get; private set; }

        internal static CalendarListService CalendarListService { get; private set; }

        public static string WebRoot => RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            ? "/home/pi/webapp/wwwroot/"
            : "./wwwroot";

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
                .UseStartup<Startup>();
    }
}
