using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace nZain.Dashboard.Models.OpenStreetMap
{
    public class NominatimResponse
    {
        [JsonProperty("place_id")]
        public long PlaceId { get; set; }

        [JsonProperty("licence")]
        public string Licence { get; set; }

        [JsonProperty("osm_type")]
        public string OsmType { get; set; }

        [JsonProperty("osm_id")]
        public long OsmId { get; set; }

        [JsonProperty("lat")]
        public double Latitude { get; set; }

        [JsonProperty("lon")]
        public double Longitude { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("address")]
        public NominatimAddress Address { get; set; }

        [JsonProperty("boundingbox")]
        public double[] Boundingbox { get; set; }

        public override string ToString()
        {
            if (this.Address != null)
            {
                return this.Address.ToString(); // shorter
            }
            return this.DisplayName; // really too verbose
        }
    }
}
