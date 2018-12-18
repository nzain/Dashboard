
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
            TimeSpan delta = date - DateTimeOffset.Now;
            int deltaDays = (int)Math.Round(delta.TotalDays);
            switch(deltaDays)
            {
                case 0: this.DisplayDate = "Heute"; break;
                case 1: this.DisplayDate = "Morgen"; break;
                default: this.DisplayDate = date.ToString("d. MMMM"); break;
            }
        }

        public DateTimeOffset Date { get; }

        public string DisplayDate { get; }

        public CalendarEvent[] Events { get; }

        public WeatherForecastDay Weather { get; private set; }

        public void SetWeather(WeatherForecast fc)
        {
            this.Weather = fc.Days.FirstOrDefault(f => f.Day.Year == this.Date.Year 
                                                    && f.Day.Month == this.Date.Month 
                                                    && f.Day.Day == this.Date.Day);
        }

        public override string ToString()
        {
            return this.Date.ToString();
        }
    }
}