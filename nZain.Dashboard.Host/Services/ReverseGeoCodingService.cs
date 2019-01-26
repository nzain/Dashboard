using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using nZain.Dashboard.Models.OpenStreetMap;

namespace nZain.Dashboard.Services
{
    public class ReverseGeoCodingService
    {
        #region Static CTOR

        static ReverseGeoCodingService()
        {
            AssemblyName asm = Assembly.GetExecutingAssembly().GetName();
            string name = asm.Name;
            Version version = asm.Version;
            UserAgent = new ProductInfoHeaderValue(name, version.ToString());
        }

        private static readonly ProductInfoHeaderValue UserAgent;

        #endregion
        
        #region Class

        private readonly ILogger<ReverseGeoCodingService> _logger;

        public ReverseGeoCodingService(ILogger<ReverseGeoCodingService> logger)
        {
            this._logger = logger;
        }

        public async Task<string> ReverseGeoCodingAsync(GeoLocation loc)
        {
            if (loc == null)
            {
                return null;
            }
            Uri baseAddress = new Uri("http://nominatim.openstreetmap.org");
            const string relativePath = "reverse";
            Dictionary<string, string> queryParameters = new Dictionary<string, string>
            {
                {"format", "json"},
                {"accept-language", "de"},
                {"lat", loc.Latitude.ToString(NumberFormatInfo.InvariantInfo)},
                {"lon", loc.Longitude.ToString(NumberFormatInfo.InvariantInfo)}
            };
            string requestUri = QueryHelpers.AddQueryString(relativePath, queryParameters);

            try
            {
                this._logger.LogInformation($"GET {baseAddress}/{requestUri}");
                using (HttpClient client = new HttpClient { BaseAddress = baseAddress })
                {
                    client.DefaultRequestHeaders.UserAgent.Add(UserAgent);
                    // execute web request and extract content from response
                    HttpResponseMessage response = await client.GetAsync(requestUri);
                    if (!response.IsSuccessStatusCode)
                    {
                        this._logger.LogWarning($"reverse geo coding failed with HTTP Status Code {response.StatusCode}");
                        return null;
                    }
                    NominatimResponse nominatim = await response.Content.ReadAsAsync<NominatimResponse>();
                    if (nominatim == null)
                    {
                        this._logger.LogError("reverse geo coding failed, NominatimResponse is null");
                        return null;
                    }
                    string displayName = nominatim.ToString();
                    this._logger.LogInformation($"Reverse geo coding success: '{displayName}'");
                    return displayName;
                }
            }
            catch (Exception e)
            {
                this._logger.LogError(e, "reverse geo coding failed");
                return null;
            }
        }

        #endregion
    }
}