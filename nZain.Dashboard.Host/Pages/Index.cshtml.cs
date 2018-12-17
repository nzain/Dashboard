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
        private readonly GoogleCalendarService _calendarService;

        public IndexModel(GoogleCalendarService calendarService)
        {
            this._calendarService = calendarService;
            this.NextDays = new CalendarDay[0];
        }

        // public void OnGet()
        // {
        //     this.NextDays = this._calendarService.EnumerateDays(5).ToArray();
        // }

        public async Task OnGetAsync()
        {
            this.NextDays = await this._calendarService.GetDataAsync(5);
        }

        public CalendarDay[] NextDays { get; private set; }
    }
}
