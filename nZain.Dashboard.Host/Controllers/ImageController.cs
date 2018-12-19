using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

  
namespace nZain.Dashboard.Host
{
    [Route("api/images")]
    public class ImageController : Controller
    {
        private readonly IHostingEnvironment _env;
        public ImageController(IHostingEnvironment env)
        {
            this._env = env;
        }

        // [HttpGet("Background")]
        // public IActionResult GetBackground()
        // {
        //     var dir = Path.Combine(this._env.WebRootPath, "images");
        //     DirectoryInfo info = new DirectoryInfo(dir);
        //     FileInfo imgFile = info.EnumerateFiles("*.jpg").FirstOrDefault();
        //     if (imgFile == null)
        //     {
        //         return this.NotFound();
        //     }
        //     return base.File($"images/{imgFile.Name}", "image/jpeg");  
        // }
    }
}