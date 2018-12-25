using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using nZain.Dashboard.Models;
using nZain.Dashboard.Services;

namespace nZain.Dashboard.Host.Pages
{
    public class IndexModel : PageModel
    {
        private readonly BackgroundImageService _backgroundImageService;
        private readonly GoogleCalendarService _calendarService;
        private readonly WeatherService _weatherService;

        public IndexModel(BackgroundImageService backgroundImageService, GoogleCalendarService calendarService, WeatherService weatherService)
        {
            this._backgroundImageService = backgroundImageService;
            this._calendarService = calendarService;
            this._weatherService = weatherService;
            this.NextDays = new CalendarDay[0];
        }

        public async Task OnGetAsync()
        {
            try
            {
                // background will change every day
                BackgroundImage bgImg = await this._backgroundImageService.GetBackgroundImageAsync();
                this.ViewData["Background"] = bgImg.RelativeWebRootLocation;
                this.Background = bgImg;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            try
            {
                // calendar
                this.NextDays = await this._calendarService.GetDataAsync(5);

                // weather forecast
                WeatherForecast fc = await this._weatherService.GetForecastAsync();
                foreach (var day in this.NextDays)
                {
                    day.SetWeather(fc);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public CalendarDay[] NextDays { get; private set; }

        public BackgroundImage Background { get; private set; }
    }
}
