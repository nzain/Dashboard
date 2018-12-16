
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

        public CalendarDay(DateTimeOffset date)
        {
            this.Date = date;
            TimeSpan delta = date - DateTimeOffset.Now;
            int deltaDays = (int)Math.Round(delta.TotalDays);
            switch(deltaDays)
            {
                case 0: this.DisplayDate = "Heute"; break;
                case 1: this.DisplayDate = "Morgen"; break;
                default: this.DisplayDate = date.ToString("d. MMMM"); break;
            }
        }

        public DateTimeOffset Date { get; set; }

        public string DisplayDate { get; }

        public override string ToString()
        {
            return this.Date.ToString();
        }
    }
}