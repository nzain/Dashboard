using System.Text.Json.Serialization;

namespace nZain.Dashboard.Models.OpenStreetMap
{
    public class NominatimResponse
    {
        [JsonPropertyName("place_id")]
        public long PlaceId { get; set; }

        [JsonPropertyName("licence")]
        public string Licence { get; set; }

        [JsonPropertyName("osm_type")]
        public string OsmType { get; set; }

        [JsonPropertyName("osm_id")]
        public long OsmId { get; set; }

        [JsonPropertyName("lat")]
        public double Latitude { get; set; }

        [JsonPropertyName("lon")]
        public double Longitude { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("address")]
        public NominatimAddress Address { get; set; }

        [JsonPropertyName("boundingbox")]
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
