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
        internal const int DayTimeBeginHour = 6; // exclusive: 6am is night, 7am is day.
        internal const int DayTimeEndHour = 18; // inclusive: 6pm is day

        public WeatherForecastDay(DateTimeOffset date, ForeCastItem[] items)
        {
            this.Date = date;
            this.TempMin = +100;
            this.TempMax = -100;
            if (items == null || items.Length == 0)
            {
                return;
            }
            List<ForeCastItem> weatherNight = new List<ForeCastItem>();
            List<ForeCastItem> weatherDay = new List<ForeCastItem>();
            foreach (var item in items)
            {
                ForeCastDetails details = item.Details;
                if (details != null)
                {
                    this.TempMin = (int)Math.Round(Math.Min(this.TempMin, details.Temp));
                    this.TempMax = (int)Math.Round(Math.Max(this.TempMax, details.Temp));
                    this.Humidity = Math.Max(this.Humidity, details.Humidity);
                }
                if (item.Weather != null && item.Weather.Length > 0)
                {
                    int hour = item.DateTime.Hour;
                    if (DayTimeBeginHour < hour && hour <= DayTimeEndHour)
                    {
                        weatherDay.Add(item);
                    }
                    else
                    {
                        weatherNight.Add(item);
                    }
                }
            }

            Weather day = GetWorstWeather(weatherDay);
            this.DayDescription = day?.Description;
            this.DayIconUri = day?.IconUri;
            this.DayRain = TotalRain(weatherDay);
            this.DaySnow = TotalSnow(weatherDay);

            Weather night = GetWorstWeather(weatherNight);
            this.NightDescription = night?.Description;
            this.NightIconUri = night?.IconUri;
            this.NightRain = TotalRain(weatherNight);
            this.NightSnow = TotalSnow(weatherNight);
        }        

        public DateTimeOffset Date { get; }

        public int TempMax { get; }

        public int TempMin { get; }

        public int Humidity { get; }

        public string DayDescription { get; }

        public string DayIconUri { get; }

        public int DayRain { get; }

        public int DaySnow { get; }

        public string NightDescription { get; }

        public string NightIconUri { get; }

        public int NightRain { get; }

        public int NightSnow { get; }

        private static Weather GetWorstWeather(IEnumerable<ForeCastItem> items)
        {
            return items
                .SelectMany(s => s.Weather)
                .OrderByDescending(o => o.GetSortKey())
                .FirstOrDefault();
        }

        private static int TotalRain(IEnumerable<ForeCastItem> items)
        {
            double sum = items
                .Where(w => w.Rain?.Volume3H != null)
                .Sum(s => s.Rain.Volume3H.Value);
            return (int)Math.Round(sum);
        }

        private static int TotalSnow(IEnumerable<ForeCastItem> items)
        {
            double sum = items
                .Where(w => w.Snow?.Volume3H != null)
                .Sum(s => s.Snow.Volume3H.Value);
            return (int)Math.Round(sum);
        }
    }
}