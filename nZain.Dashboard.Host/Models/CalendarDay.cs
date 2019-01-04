
using System;
using System.Collections.Generic;
using System.Linq;

namespace nZain.Dashboard.Models
{
    public class CalendarDay
    {
        public CalendarDay()
        {
            // empty JSON ctor
        }

        public CalendarDay(DateTimeOffset date, CalendarEvent[] events)
        {
            this.Date = date;
            this.Events = events ?? new CalendarEvent[0];
            
            var now = DateTimeOffset.Now;
            if (date.Day == now.Day)
            {
                this.DisplayDate = events.Length == 0 ? "Heute keine Termine" : null;
                this.IsToday = true;
            }
            else
            {
                this.DisplayDate = events.Length == 0 
                    ? $"{date:dddd, d. MMMM} - keine Termine"
                    : $"{date:dddd, d. MMMM}";
                this.IsToday = false;
            }
        }

        public DateTimeOffset Date { get; }

        public bool IsToday { get; }

        public string DisplayDate { get; }

        public CalendarEvent[] Events { get; }

        public WeatherForecastDay Weather { get; private set; }

        public void SetWeather(WeatherForecast fc)
        {
            this.Weather = fc.Days.FirstOrDefault(f => f.Date.Year  == this.Date.Year 
                                                    && f.Date.Month == this.Date.Month 
                                                    && f.Date.Day   == this.Date.Day);
        }

        public override string ToString()
        {
            return this.Date.ToString();
        }
    }
}