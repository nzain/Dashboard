using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.WebUtilities;
using nZain.Dashboard.Host;
using nZain.Dashboard.Models;
using nZain.Dashboard.Models.OpenWeatherMap;

namespace nZain.Dashboard.Services
{
    public class WeatherService
    {
        private const string BaseAddress = "http://api.openweathermap.org";
        private readonly string _appId;
        private readonly double _latitude;
        private readonly double _longitude;
        private readonly string _units; // e.g. "metric"
        private readonly string _language; // e.g. "en" - affects the description only

        public WeatherService(DashboardConfig cfg)
        {
            this._appId = cfg?.OpenWeatherMapAppId ?? throw new InvalidDataException("DashboardConfig.OpenWeatherMapAppId is not set");
            this._units = cfg?.OpenWeatherMapUnits ?? throw new InvalidDataException("DashboardConfig.OpenWeatherMapUnits is not set");
            this._latitude = cfg?.WeatherLocationLatitude ?? throw new InvalidDataException("DashboardConfig.WeatherLocationLatitude is not set");
            this._longitude = cfg?.WeatherLocationLongitude ?? throw new InvalidDataException("DashboardConfig.WeatherLocationLongitude is not set");
            this._language = cfg?.OpenWeatherMapLang ?? throw new InvalidDataException("DashboardConfig.OpenWeatherMapLang is not set");
        }

        public async Task<WeatherForecast> GetForecastAsync()
        {
            OWMForeCast fc = await this.GetOpenWeatherMapForecastAsync();
            return new WeatherForecast(fc);
        }

        internal async Task<OWMForeCast> GetOpenWeatherMapForecastAsync()
        {
            using (var client = this.CreateClient())
            {
                string requestUri = this.BuildRequestUri("/data/2.5/forecast/");
                HttpResponseMessage response = await client.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<OWMForeCast>();
            }
        }

        private HttpClient CreateClient()
        {
            var client = new HttpClient{ BaseAddress = new Uri(BaseAddress) };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private string BuildRequestUri(string relativePath)
        {
            // combine /data/2.5/forecast/ with query params ?appid=...&lang=de
            return QueryHelpers.AddQueryString(relativePath, this.CreateDefaultQueryParameters());
        }

        private Dictionary<string, string> CreateDefaultQueryParameters()
        {
            return new Dictionary<string, string>
            {
                ["appid"] = this._appId,
                ["units"] = this._units,
                ["lang"] = this._language,
                ["lat"] = this._latitude.ToString(NumberFormatInfo.InvariantInfo),
                ["lon"] = this._longitude.ToString(NumberFormatInfo.InvariantInfo),
            };
        }
    }
}