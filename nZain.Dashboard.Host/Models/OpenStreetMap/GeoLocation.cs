namespace nZain.Dashboard.Models.OpenStreetMap
{
    public class GeoLocation
    {
        public GeoLocation(double lat, double lon)
        {
            this.Latitude = lat;
            this.Longitude = lon;
        }
        
        public double Latitude { get; }

        public double Longitude { get; }
    }
}