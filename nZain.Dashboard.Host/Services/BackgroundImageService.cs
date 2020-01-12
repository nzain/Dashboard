using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using nZain.Dashboard.Host;
using nZain.Dashboard.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Jpeg;

namespace nZain.Dashboard.Services
{
    public class BackgroundImageService
    {
        private const string BasePath = "images";

        private readonly ILogger<BackgroundImageService> _logger;
        private readonly ReverseGeoCodingService _geoCodingService;

        // might be on a NAS, don't access it on every request
        private readonly string _imagesSourcePath;
        // local copy goes here on our own filesystem
        private readonly string _localCopyFullPath; // wwwroot/images/background.jpg
        // relative path to our wwwroot for browser access
        private readonly string _relativePath; // images/background.jpg
        // FIFO queue of next background images to reduce the EXIF parsing load
        private Queue<BackgroundImage> _nextBackgrounds;
        private BackgroundImage _lastImage;
        // timestamp of last change
        private DateTimeOffset _lastChange;

        public BackgroundImageService(ILogger<BackgroundImageService> logger, DashboardConfig cfg, IWebHostEnvironment env, ReverseGeoCodingService geoCodingService)
        {
            this._logger = logger;
            this._geoCodingService = geoCodingService ?? throw new ArgumentNullException();
            this._imagesSourcePath = cfg?.BackgroundImagesPath ?? throw new InvalidDataException("DashboardConfig.BackgroundImagesPath is not set");
            if (!System.IO.Directory.Exists(this._imagesSourcePath))
            {
                throw new DirectoryNotFoundException(this._imagesSourcePath);
            }
            this._localCopyFullPath = Path.Combine(env.WebRootPath, BasePath, "background.jpg");
            this._relativePath = Path.Combine(BasePath, "background.jpg");
            this._nextBackgrounds = new Queue<BackgroundImage>(31); // up to one month ahead
            this._logger.LogInformation(nameof(BackgroundImageService) + " created");
        }

        public async Task<BackgroundImage> GetBackgroundImageAsync()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
#if DEBUG
            if ((now - this._lastChange).TotalSeconds > 30) // DEBUG: every 30s
#else
            if (now.Day != this._lastChange.Day) // RELEASE: change on first request after midnight
#endif
            {
                this._logger.LogInformation("Fetching new background...");
                this._lastChange = now;
                if (this._nextBackgrounds.Count == 0) // generate random queue of next images
                {
                    this.FillBackgroundsQueue();
                }
                // async copy to local disk of the next background image
                this._lastImage = await this.CopyNextBackgroundToLocalCacheAsync();
            }

            Models.OpenStreetMap.GeoLocation location = this._lastImage?.Location;
            if (location != null && this._lastImage.LocationDisplayString == null)
            {
                this._lastImage.LocationDisplayString = 
                    await this._geoCodingService.ReverseGeoCodingAsync(location);
            }

            return this._lastImage;
        }

        private async Task<BackgroundImage> CopyNextBackgroundToLocalCacheAsync()
        {
            if (!this._nextBackgrounds.TryDequeue(out BackgroundImage srcImg))
            {
                return null; // failed, queue empty for some reason
            }
            string src = srcImg.OriginalSourceFile;
            string dst = this._localCopyFullPath;
            this._logger.LogInformation($"copy '{src}' ({srcImg.Width}x{srcImg.Height}) to '{dst}'");
            const int bufSize = 4096;
            const FileOptions opt = FileOptions.Asynchronous | FileOptions.SequentialScan;
            using (var sourceStream = new FileStream(src, FileMode.Open, FileAccess.Read, FileShare.Read, bufSize, opt))
            using (var destinationStream = new FileStream(dst, FileMode.Create, FileAccess.Write, FileShare.None, bufSize, opt))
            {
                await sourceStream.CopyToAsync(destinationStream);
                await destinationStream.FlushAsync();
            }
            srcImg.RelativeWebRootLocation = this._relativePath;
            return srcImg;
        }

        private void FillBackgroundsQueue()
        {
            this._logger.LogInformation("Regenerate upcoming queue of background images (long task)...");
            DirectoryInfo dir = new DirectoryInfo(this._imagesSourcePath);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(this._imagesSourcePath);
            }

            DateTimeOffset now = DateTimeOffset.Now;
            int month = now.Month;
            List<BackgroundImage> images = new List<BackgroundImage>(30);
            int count = 0;
            foreach (var item in dir.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                if (!TryLoad(item, out BackgroundImage bgImg))
                {
                    continue; // not a jpg or failed to read exif
                }
                if (bgImg.Timestamp.Month == month)
                {
                    // show summer pics during summer only
                    images.Add(bgImg);
                }
                if (++count % 20 == 0)
                {
                    this._logger.LogInformation(" {0,2}/{1,3} images selected...", images.Count, count);
                }
            }
            this._logger.LogInformation(" {0,2}/{1,3} images selected! Create randomized queue...", images.Count, count);

            // randomize the list and fill queue
            foreach (BackgroundImage item in FisherYatesShuffled(images))
            {
                this._nextBackgrounds.Enqueue(item);
                now = now.AddDays(1);
                if (now.Month != month)
                {
                    break;
                }
            }
            if (this._nextBackgrounds.Count == 0)
            {
                this._logger.LogError($"No images for month '{month}' in '{this._imagesSourcePath}'");
            }
            else
            {
                this._logger.LogInformation("Queue of {0} background images ready.", this._nextBackgrounds.Count);
            }
        }

        public static bool TryLoad(FileInfo file, out BackgroundImage bgImg)
        {
            bgImg = null;
            if (file == null || !file.Exists)
            {
                return false;
            }
            if (!string.Equals(file.Extension, ".jpg", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(file.Extension, ".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            IReadOnlyList<MetadataExtractor.Directory> directories = JpegMetadataReader.ReadMetadata(file.FullName);
            int width = 0;
            int height = 0;
            DateTimeOffset timestamp = default;
            string make = string.Empty;
            string model = string.Empty;
            foreach (var dir in directories)
            {
                if (timestamp == default(DateTimeOffset))
                {
                    if (dir.TryGetDateTime(ExifDirectoryBase.TagDateTime, out DateTime dateTime) ||
                        dir.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out dateTime) ||
                        dir.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out dateTime))
                    timestamp = new DateTimeOffset(dateTime);
                }
                if (width <= 0)
                {
                    if (dir.ContainsTag(ExifDirectoryBase.TagExifImageWidth))
                    {
                        dir.TryGetInt32(ExifDirectoryBase.TagExifImageWidth, out width);
                    }
                    else if (dir.ContainsTag(ExifDirectoryBase.TagImageWidth))
                    {
                        dir.TryGetInt32(ExifDirectoryBase.TagImageWidth, out width);
                    }
                }
                if (height <= 0)
                {
                    if (dir.ContainsTag(ExifDirectoryBase.TagExifImageHeight))
                    {
                        dir.TryGetInt32(ExifDirectoryBase.TagExifImageHeight, out height);
                    }
                    else if (dir.ContainsTag(ExifDirectoryBase.TagImageHeight))
                    {
                        dir.TryGetInt32(ExifDirectoryBase.TagImageHeight, out height);
                    }
                }
                if (make.Length == 0 && dir.ContainsTag(ExifDirectoryBase.TagMake))
                {
                    make = dir.GetString(ExifDirectoryBase.TagMake);
                }
                if (string.IsNullOrEmpty(model) && dir.ContainsTag(ExifDirectoryBase.TagModel))
                {
                    model = dir.GetString(ExifDirectoryBase.TagModel);
                }
            }
            string camName = $"{make ?? string.Empty} {model ?? string.Empty}";

            // gps location may be null
            var gpsLocation = directories
                .OfType<GpsDirectory>()
                .Select(s => s.GetGeoLocation())
                .Where(w => w != null)
                .Select(s => new Models.OpenStreetMap.GeoLocation(s.Latitude, s.Longitude))
                .FirstOrDefault();
            bgImg = new BackgroundImage(file.FullName, width, height, timestamp, camName, gpsLocation);
            return true;
        }

        /// <Summary>Implementation of the Fisher-Yates shuffle.</Summary>
        public static IEnumerable<T> FisherYatesShuffled<T>(List<T> elements)
        {
            if (elements.Count == 0)
            {
                yield break;
            }
            Random rng = new Random();
            for (int i = elements.Count - 1; i > 0; i--)
            {
                int swapIndex = rng.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
            yield return elements[0];
        }
    }
}