
using System;
using System.Collections.Generic;
using System.Linq;
using nZain.Dashboard.Models.OpenStreetMap;

namespace nZain.Dashboard.Models
{
    public class BackgroundImage
    {
        public BackgroundImage(string fullPath, int width, int height, DateTimeOffset timestamp,
            string cameraModel, GeoLocation location)
        {
            this.OriginalSourceFile = fullPath;
            this.Width = width;
            this.Height = height;
            this.Timestamp = timestamp;
            this.CameraModel = cameraModel;
            this.Location = location;
        }

        public string OriginalSourceFile { get; }

        public int Width { get; }

        public int Height { get; }

        public DateTimeOffset Timestamp { get; }

        public string CameraModel { get; }

        public GeoLocation Location { get; } // may be null

        public string LocationDisplayString { get; set; } // may be null

        public string RelativeWebRootLocation { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(this.LocationDisplayString))
            {
                return $"{this.Timestamp.Year} {this.LocationDisplayString}";
            }
            if (this.Location != null)
            {
                return $"{this.Timestamp.Year} ({this.CameraModel}) lat={this.Location.Latitude:F3}° lon={this.Location.Longitude:F3}°";
            }
            return $"{this.Timestamp.Year} ({this.CameraModel})";
        }
    }
}