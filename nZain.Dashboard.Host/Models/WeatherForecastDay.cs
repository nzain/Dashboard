using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Google.Apis.Calendar.v3.Data;
using nZain.Dashboard.Models.OpenWeatherMap;

namespace nZain.Dashboard.Models
{
    public class WeatherForecastDay
    {
        public WeatherForecastDay(DateTimeOffset day, ForeCastItem[] items)
        {
            this.Day = day;
            if (items == null || items.Length == 0)
            {
                return;
            }
            this.TempMax = items.Where(w => w.Details != null).Max(m => m.Details.Temp);
            this.TempMin = items.Where(w => w.Details != null).Min(m => m.Details.Temp);
            this.Conditions = items
                .Where(w => w.Weather != null)
                .SelectMany(s => s.Weather)
                .Select(s => s.Condition)
                .Distinct()
                .ToArray();
            this.IconUri = items.SelectMany(s => s.Weather).FirstOrDefault(f => f != null)?.IconUri;
        }

        public DateTimeOffset Day { get; }

        public double TempMax { get; }

        public double TempMin { get; }

        public string[] Conditions { get; }

        public string ConditionsString => string.Join(" ", this.Conditions);

        public string IconUri { get; }
    }
}