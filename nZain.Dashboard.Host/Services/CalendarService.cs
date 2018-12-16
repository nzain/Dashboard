using System;
using System.Collections.Generic;
using System.Linq;
using nZain.Dashboard.Models;

namespace nZain.Dashboard.Services
{
    public class CalendarService
    {
        public CalendarService()
        {
            
        }

        public IEnumerable<CalendarDay> EnumerateDays(int n)
        {
            DateTimeOffset d = DateTimeOffset.Now;
            for (int i = 0; i < n; i++)
            {
                CalendarDay item = new CalendarDay(d);
                yield return item;
                d = d.AddDays(+1);
            }
        }
    }
}