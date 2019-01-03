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
        private static readonly TimeSpan OneDay = TimeSpan.FromDays(1);

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
            //   day weather: (6am - 18pm]
            // night weather: (18pm - 6am *next day*]
            var s = item.DateTime;
            if (s.Hour <= WeatherForecastDay.DayTimeBeginHour)
            {
                // counts as "night weather" of the previous day
                s = s.Subtract(OneDay);
            }
            // group by yyyy-MM-dd only
            return new DateTimeOffset(s.Year, s.Month, s.Day, 0, 0, 0, s.Offset);
        }

        public string CityName { get; }

        public WeatherForecastDay[] Days { get; }

        public override string ToString()
        {
            if (this.Days == null)
            {
                return "Invalid (empty) forecast";
            }
            return $"{this.Days.Length}-day Forecast";
        }
    }
}