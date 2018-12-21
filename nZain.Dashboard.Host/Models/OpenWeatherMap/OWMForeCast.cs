using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace nZain.Dashboard.Models.OpenWeatherMap
{
    // generated by https://app.quicktype.io/ on 2018-12-18
    // documentation from https://openweathermap.org/forecast5
    public class OWMForeCast
    {
        [JsonProperty("cod")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        /// <Summary></Summary>
        [JsonProperty("list")]
        public ForeCastItem[] List { get; set; }

        /// <Summary></Summary>
        [JsonProperty("city")]
        public City City { get; set; }

        
        public override string ToString()
        {
            if (this.List == null || this.List.Length == 0)
            {
                return $"Code={this.Code} Message={this.Message}";
            }
            return $"Forcast({this.City?.Name}): {this.List[0]}";
        }
    }

    public class City
    {
        /// <Summary>City ID</Summary>
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <Summary>City name</Summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <Summary>City geo location</Summary>
        [JsonProperty("coord")]
        public Coord Coord { get; set; }

        /// <Summary>Country code (GB, JP, etc)</Summary>
        [JsonProperty("country")]
        public string Country { get; set; }

        /// <Summary></Summary>
        [JsonProperty("population")]
        public long Population { get; set; }

        public override string ToString()
        {
            return $"{this.Name} [{this.Country}] Population: {this.Population}";
        }
    }

    public class Coord
    {
        /// <Summary>City geo location, latitude</Summary>
        [JsonProperty("lat")]
        public double Latitude { get; set; }

        /// <Summary>City geo location, longitude</Summary>
        [JsonProperty("lon")]
        public double Longitude { get; set; }
    }

    public class ForeCastItem
    {
        /// <Summary>Time of data forecasted, unix, UTC</Summary>
        [JsonProperty("dt")]
        public long TimestampUnixUtc { get; set; }

        /// <Summary></Summary>
        [JsonProperty("main")]
        public ForeCastDetails Details { get; set; }

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
        [JsonProperty("snow")]
        public Snow Snow { get; set; }

        /// <Summary>Data/time of calculation, UTC</Summary>
        [JsonProperty("dt_txt")]
        public DateTimeOffset DateTime { get; set; }
        

        public override string ToString()
        {
            return $"[{this.DateTime.ToString("yyyy-MM-dd_HH:mm")}] {this.Details} {this.Weather.FirstOrDefault()}";
        }
    }

    public class ForeCastDetails
    {
        /// <Summary>Temperature. Unit Default: Kelvin, Metric: Celsius, Imperial: Fahrenheit.</Summary>
        [JsonProperty("temp")]
        public double Temp { get; set; }

        /// <Summary>Minimum temperature at the moment of calculation. This is deviation from 'temp' that is possible for large cities and megalopolises geographically expanded (use these parameter optionally). Unit Default: Kelvin, Metric: Celsius, Imperial: Fahrenheit.</Summary>
        [JsonProperty("temp_min")]
        public double TempMin { get; set; }

        /// <Summary>Maximum temperature at the moment of calculation. This is deviation from 'temp' that is possible for large cities and megalopolises geographically expanded (use these parameter optionally). Unit Default: Kelvin, Metric: Celsius, Imperial: Fahrenheit.</Summary>
        [JsonProperty("temp_max")]
        public double TempMax { get; set; }

        /// <Summary>Atmospheric pressure on the sea level by default, hPa</Summary>
        [JsonProperty("pressure")]
        public double Pressure { get; set; }

        /// <Summary>Atmospheric pressure on the sea level, hPa</Summary>
        [JsonProperty("sea_level")]
        public double SeaLevel { get; set; }

        /// <Summary>Atmospheric pressure on the ground level, hPa</Summary>
        [JsonProperty("grnd_level")]
        public double GrndLevel { get; set; }

        /// <Summary>Humidity, %</Summary>
        [JsonProperty("humidity")]
        public int Humidity { get; set; }

        public override string ToString()
        {
            return $"{this.Temp:F0}°C (humidity {this.Humidity}%)";
        }
    }

    public class Weather
    {
        /// <Summary>Weather condition id</Summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <Summary>Group of weather parameters (Rain, Snow, Extreme etc.)</Summary>
        [JsonProperty("main")]
        public string Condition { get; set; }

        /// <Summary>Weather condition within the group</Summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <Summary>Weather icon id</Summary>
        [JsonProperty("icon")]
        public string Icon { get; set; }

        public string IconUri => $"http://openweathermap.org/img/w/{this.Icon}.png";

        public bool IsSnow() => 600 <= this.Id && this.Id < 700;
        
        public int GetSortKey()
        {
            if (this.Id >= 900) // UNDEFINED
                return this.Id;
            if (this.Id == 800) // clear sky, best :)
                return 0;
            if (this.Id >= 800) // clear sky or clouds only
                return 1;
            if (this.Id >= 700) // Atmosphere (mist/fog)
                return 2;
            if (this.Id >= 600) // snow
                return 5;
            if (this.Id >= 500) // rain
                return 4;
            if (this.Id >= 400) // UNDEFINED
                return this.Id;
            if (this.Id >= 300) // drizzle
                return 3;
            if (this.Id >= 200) // thunderstorm
                return 6;
            if (this.Id >= 100) // UNDEFINED
                return this.Id;
            return this.Id; // UNDEFINED, not set, whatever
        }

        public override string ToString()
        {
            return $"[{this.Id}|{this.Icon}.png] {this.Description} (Condition={this.Condition})";
        }
    }

    public class Clouds
    {
        /// <Summary>Cloudiness, %</Summary>
        [JsonProperty("all")]
        public int CloudinessPercent { get; set; }

        public override string ToString()
        {
            return $"Cloudiness {this.CloudinessPercent}%";
        }
    }

    public class Wind
    {
        /// <Summary>Wind speed. Unit Default: meter/sec, Metric: meter/sec, Imperial: miles/hour.</Summary>
        [JsonProperty("speed")]
        public double Speed { get; set; }

        /// <Summary>Wind direction, degrees (meteorological)</Summary>
        [JsonProperty("deg")]
        public double Deg { get; set; }
        
        public override string ToString()
        {
            return $"Wind {this.Speed:F0}m/s from {this.Deg:F0}°";
        }
    }

    public class Rain
    {
        /// <Summary>Rain volume for last 3 hours, mm</Summary>
        [JsonProperty("3h", NullValueHandling = NullValueHandling.Ignore)]
        public double? Volume3H { get; set; }

        public override string ToString()
        {
            return $"Rain {this.Volume3H:F0}mm";
        }
    }

    public class Snow
    {
        /// <Summary>Snow volume for last 3 hours</Summary>
        [JsonProperty("3h", NullValueHandling = NullValueHandling.Ignore)]
        public double? Volume3H { get; set; }
        
        public override string ToString()
        {
            return $"Snow +{this.Volume3H}mm";
        }
    }
}
