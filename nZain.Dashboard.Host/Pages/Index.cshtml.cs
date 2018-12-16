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
        private readonly CalendarService _calendarService;

        public IndexModel(CalendarService calendarService)
        {
            this._calendarService = calendarService;
            this.NextDays = new CalendarDay[0];
        }

        public void OnGet()
        {
            this.NextDays = this._calendarService.EnumerateDays(5).ToArray();
        }

        public CalendarDay[] NextDays { get; private set; }
    }
}
