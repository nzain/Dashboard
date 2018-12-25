using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using nZain.Dashboard.Host;
using nZain.Dashboard.Models;

namespace nZain.Dashboard.Services
{
    public class BackgroundImageService
    {
        private const string BasePath = "images";

        // might be on a NAS, don't access it on every request
        private readonly string _imagesSourcePath;
         
        // local copy goes here on our own filesystem
        private readonly string _localCopyFullPath; // wwwroot/images/background.jpg
        
        // relative path to our wwwroot for browser access
        private readonly string _relativePath; // images/background.jpg

        // timestamp of last change
        private DateTimeOffset _lastChange;

        // FIFO queue of next background images to reduce the EXIF parsing load
        private Queue<string> _nextBackgrounds;

        public BackgroundImageService(DashboardConfig cfg, IHostingEnvironment env)
        {
            this._imagesSourcePath = cfg?.BackgroundImagesPath ?? throw new InvalidDataException("DashboardConfig.BackgroundImagesPath is not set");
            if (!Directory.Exists(this._imagesSourcePath))
            {
                throw new DirectoryNotFoundException(this._imagesSourcePath);
            }
            this._localCopyFullPath = Path.Combine(env.WebRootPath, BasePath, "background.jpg");
            this._relativePath = Path.Combine(BasePath, "background.jpg");
            this._nextBackgrounds = new Queue<string>(31); // up to one month ahead
        }

        public async Task<string> GetBackgroundImageAsync()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
#if DEBUG
            if ((now - this._lastChange).TotalSeconds > 30) // DEBUG: every 30s
#else
            if (now.Day != this._lastChange.Day) // RELEASE: change on first request after midnight
#endif
            {
                this._lastChange = now;
                if (this._nextBackgrounds.Count == 0) // generate random queue of next images
                {
                    this.FillNextBackgroundsQueue();
                }
                // async copy to local disk of the next background image
                await this.CopyNextBackgroundToLocalCacheAsync();
            }

            // return value is constant, but we might copy a new image into our local cache before we return.
            return this._relativePath; 
        }

        private void FillNextBackgroundsQueue()
        {
            DirectoryInfo dir = new DirectoryInfo(this._imagesSourcePath);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(this._imagesSourcePath);
            }
            
            DateTimeOffset now = DateTimeOffset.Now;
            int month = now.Month;
            foreach (var item in dir.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                if (!string.Equals(item.Extension, ".jpg", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(item.Extension, ".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    continue; // not a jpg file
                }
                // TODO read exif data
                this._nextBackgrounds.Enqueue(item.FullName);

                now = now.AddDays(1);
                if (now.Month != month)
                {
                    break;
                }
            }

            // validation
            if (this._nextBackgrounds.Count == 0)
            {
                throw new InvalidOperationException($"no images found at all in '{this._imagesSourcePath}'");
            }
        }

        private async Task CopyNextBackgroundToLocalCacheAsync()
        {
            if (this._nextBackgrounds.TryDequeue(out string src))
            {
                string dst = this._localCopyFullPath;
                const int bufSize = 4096;
                const FileOptions opt = FileOptions.Asynchronous | FileOptions.SequentialScan;
                using (var sourceStream = new FileStream(src, FileMode.Open, FileAccess.Read, FileShare.Read, bufSize, opt))
                using (var destinationStream = new FileStream(dst, FileMode.Create, FileAccess.Write, FileShare.None, bufSize, opt))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                    await destinationStream.FlushAsync();
                }
            }
        }
    }
}