
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.MetaData;
using SixLabors.ImageSharp.MetaData.Profiles.Exif;
using SixLabors.ImageSharp.Primitives;

namespace nZain.Dashboard.Models
{
    public class BackgroundImage
    {
        public BackgroundImage(string fullPath, int width, int height, DateTimeOffset timestamp,
            string cameraModel, string location = null)
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

        public string Location { get; }

        public string RelativeWebRootLocation { get; set; }
    }
}