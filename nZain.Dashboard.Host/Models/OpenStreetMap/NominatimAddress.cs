using Newtonsoft.Json;

namespace nZain.Dashboard.Models.OpenStreetMap
{
    public class NominatimAddress
    {
        [JsonProperty("house_number")]
        public string HouseNumber { get; set; }

        [JsonProperty("road")]
        public string Road { get; set; }

        [JsonProperty("suburb")]
        public string Suburb { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("county")]
        public string County { get; set; }

        [JsonProperty("state_district")]
        public string StateDistrict { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("postcode")]
        public int Postcode { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("country_code")]
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
