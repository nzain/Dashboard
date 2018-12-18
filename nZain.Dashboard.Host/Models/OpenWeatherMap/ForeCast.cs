using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace nZain.Dashboard.Models.OpenWeatherMap
{
    public class ForeCast
    {
        [JsonProperty("cod")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public double Message { get; set; }

        /// <Summary>Number of lines returned by this API call </Summary>
        [JsonProperty("cnt")]
        public int Count { get; set; }

        /// <Summary></Summary>
        [JsonProperty("list")]
        public ForeCastItem[] List { get; set; }

        /// <Summary></Summary>
        [JsonProperty("city")]
        public City City { get; set; }
    }

    public class City
    {
        /// <Summary></Summary>
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <Summary></Summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <Summary></Summary>
        [JsonProperty("coord")]
        public Coord Coord { get; set; }

        /// <Summary></Summary>
        [JsonProperty("country")]
        public string Country { get; set; }

        /// <Summary></Summary>
        [JsonProperty("population")]
        public long Population { get; set; }
    }

    public class Coord
    {
        /// <Summary></Summary>
        [JsonProperty("lat")]
        public double Lat { get; set; }

        /// <Summary></Summary>
        [JsonProperty("lon")]
        public double Lon { get; set; }
    }

    public class ForeCastItem
    {
        /// <Summary></Summary>
        [JsonProperty("dt")]
        public long Dt { get; set; }

        /// <Summary></Summary>
        [JsonProperty("main")]
        public MainClass Main { get; set; }

        /// <Summary></Summary>
        [JsonProperty("weather")]
        public Weather[] Weather { get; set; }

        /// <Summary></Summary>
        [JsonProperty("clouds")]
        public Clouds Clouds { get; set; }

        /// <Summary></Summary>
        [JsonProperty("wind")]
        public Wind Wind { get; set; }

        /// <Summary></Summary>
        [JsonProperty("rain")]
        public Rain Rain { get; set; }

        /// <Summary></Summary>
        [JsonProperty("sys")]
        public Sys Sys { get; set; }

        /// <Summary></Summary>
        [JsonProperty("dt_txt")]
        public DateTimeOffset DtTxt { get; set; }
    }

    public class Clouds
    {
        /// <Summary></Summary>
        [JsonProperty("all")]
        public long All { get; set; }
    }

    public class MainClass
    {
        /// <Summary></Summary>
        [JsonProperty("temp")]
        public double Temp { get; set; }

        /// <Summary></Summary>
        [JsonProperty("temp_min")]
        public double TempMin { get; set; }

        /// <Summary></Summary>
        [JsonProperty("temp_max")]
        public double TempMax { get; set; }

        /// <Summary></Summary>
        [JsonProperty("pressure")]
        public double Pressure { get; set; }

        /// <Summary></Summary>
        [JsonProperty("sea_level")]
        public double SeaLevel { get; set; }

        /// <Summary></Summary>
        [JsonProperty("grnd_level")]
        public double GrndLevel { get; set; }

        /// <Summary></Summary>
        [JsonProperty("humidity")]
        public long Humidity { get; set; }

        /// <Summary></Summary>
        [JsonProperty("temp_kf")]
        public double TempKf { get; set; }
    }

    public class Rain
    {
        /// <Summary></Summary>
        [JsonProperty("3h", NullValueHandling = NullValueHandling.Ignore)]
        public double? The3H { get; set; }
    }

    public class Weather
    {
        /// <Summary></Summary>
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <Summary></Summary>
        [JsonProperty("main")]
        public WeatherType Main { get; set; }

        /// <Summary></Summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <Summary></Summary>
        [JsonProperty("icon")]
        public string Icon { get; set; }
    }

    public enum WeatherType { Clear, Clouds, Rain };

    public class Wind
    {
        /// <Summary></Summary>
        [JsonProperty("speed")]
        public double Speed { get; set; }

        /// <Summary></Summary>
        [JsonProperty("deg")]
        public double Deg { get; set; }
    }

    public class Sys
    {
        /// <Summary></Summary>
        [JsonProperty("pod")]
        public Pod Pod { get; set; }
    }

    public enum Pod { D, N };
}
