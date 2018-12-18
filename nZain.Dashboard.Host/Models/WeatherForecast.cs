using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Google.Apis.Calendar.v3.Data;
using nZain.Dashboard.Models.OpenWeatherMap;

namespace nZain.Dashboard.Models
{
    public class WeatherForecast
    {
        public WeatherForecast(OWMForeCast owm)
        {
            if (owm.List == null)
            {
                throw new ArgumentException(owm.ToString());
            }
            this.CityName = owm.City?.Name;
            this.Days = owm.List
                .GroupBy(DateSelector)
                .Select(grp => new WeatherForecastDay(grp.Key, grp.ToArray()))
                .ToArray();
        }

        private static DateTimeOffset DateSelector(ForeCastItem item)
        {
            var s = item.DateTime;
            return new DateTimeOffset(s.Year, s.Month, s.Day, 0, 0, 0, s.Offset);
        }

        public string CityName { get; }

        public WeatherForecastDay[] Days { get; }
    }
}