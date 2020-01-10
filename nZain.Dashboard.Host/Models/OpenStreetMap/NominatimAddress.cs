using System.Text.Json.Serialization;

namespace nZain.Dashboard.Models.OpenStreetMap
{
    public class NominatimAddress
    {
        [JsonPropertyName("house_number")]
        public string HouseNumber { get; set; }

        [JsonPropertyName("road")]
        public string Road { get; set; }

        [JsonPropertyName("suburb")]
        public string Suburb { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("county")]
        public string County { get; set; }

        [JsonPropertyName("state_district")]
        public string StateDistrict { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("postcode")]
        public int Postcode { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(this.City))
            {
                return $"{this.City}, {this.Country}";
            }
            if (!string.IsNullOrWhiteSpace(this.StateDistrict))
            {
                return $"{this.StateDistrict}, {this.Country}";
            }
            return $"{this.State}, {this.Country}";
        }
    }
}
