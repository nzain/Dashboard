using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Google.Apis.Calendar.v3.Data;
using nZain.Dashboard.Host;
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
            var today = DateTimeOffset.Now;
            this.IsToday = date.Year  == today.Year
                        && date.Month == today.Month
                        && date.Day   == today.Day;

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

            this.DayRain = TotalRain(weatherDay);
            this.DaySnow = TotalSnow(weatherDay);
            Weather day = GetWorstWeather(weatherDay);
            if (day != null)
            {
                this.DayIconUri = day.IconUri;
                this.DayDescription = "Tag: " + day.Description;
                int volume = this.DayRain + this.DaySnow;
                if (volume > 0)
                {
                    this.DayDescription += day.IsSnow()
                        ? $" {volume}mm" // Schnee 4mm
                        : $" {volume}L"; // Leichter Regen 3L
                }
            }
            else
            {
                this.DayDescription = "\u00A0"; // &nbsp;
            }
            
            this.NightRain = TotalRain(weatherNight);
            this.NightSnow = TotalSnow(weatherNight);
            Weather night = GetWorstWeather(weatherNight);
            if (night != null)
            {
                this.NightIconUri = night.IconUri;
                this.NightDescription = "Nacht: " + night.Description;
                int volume = this.NightRain + this.NightSnow;
                if (volume > 0)
                {
                    this.NightDescription += night.IsSnow()
                        ? $" {volume}mm" // Schnee 4mm
                        : $" {volume}L"; // Leichter Regen 3L
                }
            }
            else
            {
                this.NightDescription = "\u00A0"; // &nbsp;
            }

            if (TryGetIconUri(day, true, this.DayRain, this.DaySnow, out string uri))
            {
                this.DayIconUri = uri;
            }
            if (TryGetIconUri(night, false, this.NightRain, this.NightSnow, out uri)) 
            {
                this.NightIconUri = uri;
            }
        }        

        public DateTimeOffset Date { get; }
        
        public bool IsToday { get; }

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
                .ThenByDescending(o => o.Id)
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

        private static bool TryGetIconUri(Weather weather, bool day, int rainVol, int snowVol, out string uri)
        {
            if (weather == null || string.IsNullOrWhiteSpace(weather.Icon))
            {
                uri = null;
                return false;
            }
            string iconId = day
                ? weather.Icon.Replace('n', 'd')
                : weather.Icon.Replace('d', 'n');
            
            // 1. try specialized icon for given volume (rain/snow)
            uri = BuildRainSnowIconUri(iconId, rainVol + snowVol);
            if (File.Exists(Path.Combine(Program.WebRoot, uri)))
            {
                return true;
            }
            // 2. fallback to default icon
            uri = BuildSimpleIconUri(iconId);
            return File.Exists(Path.Combine(Program.WebRoot, uri));
        }

        private static string BuildRainSnowIconUri(string iconId, int volume)
        {
            if (volume < 1) volume = 1; // not below 1, zero is a typical argument.
            return $"images/weather/{iconId}-r{volume}.svg";
        }

        private static string BuildSimpleIconUri(string iconId)
        {
            return $"images/weather/{iconId}.svg";
        }
    }
}