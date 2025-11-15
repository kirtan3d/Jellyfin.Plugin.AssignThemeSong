using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Jellyfin.Plugin.xThemeSong.Models;
using Jellyfin.Plugin.xThemeSong.Services;
using MediaBrowser.Common;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.xThemeSong.Api
{
    [ApiController]
    [Route("Items/{itemId}/ThemeSong")]
    public class ThemeSongController : ControllerBase
    {
        private readonly ILogger<ThemeSongController> _logger;
        private readonly ILibraryManager _libraryManager;
        private readonly Plugin _plugin;

        public ThemeSongController(
            ILogger<ThemeSongController> logger,
            ILibraryManager libraryManager)
        {
            _logger = logger;
            _libraryManager = libraryManager;
            _plugin = Plugin.Instance;
        }

        [HttpPost]
        public async Task<ActionResult> xThemeSong([FromRoute] string itemId, [FromForm] xThemeSongRequest request)
        {
            var item = _libraryManager.GetItemById(itemId);
            if (item == null)
            {
                return NotFound($"Item {itemId} not found");
            }

            var itemPath = item.Path;
            if (string.IsNullOrEmpty(itemPath))
            {
                return BadRequest("Item has no valid path");
            }

            var itemDirectory = Path.GetDirectoryName(itemPath);
            if (string.IsNullOrEmpty(itemDirectory))
            {
                return BadRequest("Could not determine item directory");
            }

            var config = _plugin.Configuration;

            try
            {
                if (!string.IsNullOrEmpty(request.YouTubeUrl))
                {
                    // Create a logger for ThemeDownloadService
                    var loggerFactory = (ILoggerFactory)HttpContext.RequestServices.GetService(typeof(ILoggerFactory));
                    var downloadServiceLogger = loggerFactory.CreateLogger<ThemeDownloadService>();
                    var downloadService = new ThemeDownloadService(downloadServiceLogger);
                    
                    await downloadService.DownloadFromYouTube(
                        request.YouTubeUrl,
                        itemDirectory,
                        config.AudioBitrate,
                        default);
                }
                else if (request.UploadedFile != null && request.UploadedFile.Length > 0)
                {
                    // Create a logger for ThemeDownloadService
                    var loggerFactory = (ILoggerFactory)HttpContext.RequestServices.GetService(typeof(ILoggerFactory));
                    var downloadServiceLogger = loggerFactory.CreateLogger<ThemeDownloadService>();
                    var downloadService = new ThemeDownloadService(downloadServiceLogger);
                    
                    // Save uploaded file temporarily
                    var tempPath = Path.GetTempFileName();
                    using (var stream = new FileStream(tempPath, FileMode.Create))
                    {
                        await request.UploadedFile.CopyToAsync(stream);
                    }

                    await downloadService.SaveUploadedTheme(
                        tempPath,
                        itemDirectory,
                        config.AudioBitrate,
                        default);

                    // Clean up temp file
                    if (System.IO.File.Exists(tempPath))
                    {
                        System.IO.File.Delete(tempPath);
                    }
                }
                else
                {
                    return BadRequest("Either YouTubeUrl or UploadedFile must be provided");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning theme song");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }

    public class xThemeSongRequest
    {
        public string? YouTubeUrl { get; set; }
        
        public IFormFile? UploadedFile { get; set; }
    }
}
