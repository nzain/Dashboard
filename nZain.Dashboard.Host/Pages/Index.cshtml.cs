using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using nZain.Dashboard.Models;
using nZain.Dashboard.Services;

namespace nZain.Dashboard.Host.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly BackgroundImageService _backgroundImageService;
        private readonly GoogleCalendarService _calendarService;
        private readonly WeatherService _weatherService;

        public IndexModel(ILogger<IndexModel> logger, BackgroundImageService backgroundImageService, GoogleCalendarService calendarService, WeatherService weatherService)
        {
            this._logger = logger;
            this._backgroundImageService = backgroundImageService;
            this._calendarService = calendarService;
            this._weatherService = weatherService;
            this.NextDays = new CalendarDay[0];
        }

        public async Task OnGetAsync()
        {
            this._logger.LogInformation($"OnGetAsync -----------------------------------------------");
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                // background will change every day
                this._logger.LogInformation(" >> GetBackgroundImageAsync...");
                BackgroundImage bgImg = await this._backgroundImageService.GetBackgroundImageAsync();
                this.ViewData["Background"] = bgImg?.RelativeWebRootLocation;
                this.Background = bgImg;
                this._logger.LogInformation($" >> GetBackgroundImageAsync done after {sw.Elapsed.TotalSeconds:F3}s with {bgImg}");
            }
            catch(Exception e)
            {
                this._logger.LogError(e, "Get Background Image failed");
            }

            try
            {
                // calendar
                sw.Restart();
                this._logger.LogInformation($" >> GetCalendarEventsAsync...");
                this.NextDays = await this._calendarService.GetCalendarEventsAsync(5);
                this._logger.LogInformation($" >> GetCalendarEventsAsync done after {sw.Elapsed.TotalSeconds:F3}s for {this.NextDays?.Length} days");

                // weather forecast
                this._logger.LogInformation($" >> GetWeatherForecastAsync...");
                WeatherForecast fc = await this._weatherService.GetForecastAsync();
                foreach (var day in this.NextDays)
                {
                    day.SetWeather(fc);
                }
                this._logger.LogInformation($" >> GetWeatherForecastAsync done after {sw.Elapsed.TotalSeconds:F3}s with {fc}");
            }
            catch(Exception e)
            {
                this._logger.LogError(e, "Calendar/Weather Webrequest failed");
            }
        }

        public BackgroundImage Background { get; private set; }

        public CalendarDay[] NextDays { get; private set; }

        public string TodayWeatherDay => this.NextDays.FirstOrDefault()?.Weather?.DayDescription;
        public string TodayWeatherNight => this.NextDays.FirstOrDefault()?.Weather?.NightDescription;
    }
}
