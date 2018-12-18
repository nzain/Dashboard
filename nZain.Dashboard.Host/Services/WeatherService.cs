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
using nZain.Dashboard.Models;
using nZain.Dashboard.Models.OpenWeatherMap;

namespace nZain.Dashboard.Services
{
    public class WeatherService
    {
        private const string BaseAddress = "http://api.openweathermap.org";
        private const string FileName = "Secrets/OpenWeatherMap.csv";
        private readonly string _appId;
        private readonly double _latitude;
        private readonly double _longitude;
        private readonly string _units; // e.g. "metric"
        private readonly string _language; // e.g. "en" - affects the description only

        public WeatherService()
        {
            if (!File.Exists(FileName))
            {
                throw new FileNotFoundException(FileName);
            }
            using (var r = new StreamReader(FileName))
            {
                string line = r.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    throw new InvalidDataException("expected three columns: appid;lat;lon;units;lang");
                }
                string[] columns = line.Split(';');
                if (columns.Length != 5)
                {
                    throw new InvalidDataException("expected three columns: appid;lat;lon;units;lang");
                }
                this._appId = columns[0];
                if (!double.TryParse(columns[1], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out this._latitude) ||
                    !double.TryParse(columns[2], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out this._longitude))
                {
                    throw new InvalidDataException($"failed to parse lat/lon from '{line}'");
                }
                this._units = columns[3];
                this._language = columns[4];
            }
        }

        public async Task<ForeCast> GetForecastAsync()
        {
            using (var client = this.CreateClient())
            {
                string requestUri = this.BuildRequestUri("/data/2.5/forecast/");
                HttpResponseMessage response = await client.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<ForeCast>();
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