using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using nZain.Dashboard.Services;

namespace nZain.Dashboard.Host
{
    public class DashboardConfig
    {
        public string BackgroundImagesPath { get; set; }
        
        // calendars to query <google calendar ID, display name>
        public Dictionary<string, string> Calendars { get; set; }

        public string OpenWeatherMapAppId { get; set; }

        public string OpenWeatherMapUnits { get; set; } // e.g. "metric"

        public string OpenWeatherMapLang { get; set; } // e.g. "en"

        public double WeatherLocationLatitude { get; set; } // degrees

        public double WeatherLocationLongitude { get; set; } // degrees
    }
}