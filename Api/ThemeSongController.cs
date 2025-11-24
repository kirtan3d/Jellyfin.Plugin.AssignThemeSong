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
    [Route("xThemeSong/{itemId}")]
    public class ThemeSongController : ControllerBase
    {
        private readonly ILogger<ThemeSongController> _logger;
        private readonly ILibraryManager _libraryManager;
        private readonly ThemeDownloadService _themeDownloadService;
        private readonly Plugin _plugin;

        public ThemeSongController(
            ILogger<ThemeSongController> logger,
            ILibraryManager libraryManager,
            ThemeDownloadService themeDownloadService)
        {
            _logger = logger;
            _libraryManager = libraryManager;
            _themeDownloadService = themeDownloadService;
            _plugin = Plugin.Instance;
        }

        [HttpPost]
        public async Task<ActionResult> AssignThemeSong([FromRoute] string itemId, [FromForm] ThemeSongRequest request)
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
                    _logger.LogInformation("Downloading theme from YouTube URL: {Url} for item {ItemName}", 
                        request.YouTubeUrl, item.Name);
                        
                    await _themeDownloadService.DownloadFromYouTube(
                        request.YouTubeUrl,
                        itemDirectory,
                        config.AudioBitrate,
                        default);
                        
                    _logger.LogInformation("Successfully downloaded theme from YouTube for {ItemName}", item.Name);
                }
                else if (request.UploadedFile != null && request.UploadedFile.Length > 0)
                {
                    _logger.LogInformation("Processing uploaded file: {FileName} ({FileSize} bytes) for {ItemName}", 
                        request.UploadedFile.FileName, request.UploadedFile.Length, item.Name);
                        
                    var tempPath = Path.GetTempFileName();
                    try
                    {
                        await using (var stream = new FileStream(tempPath, FileMode.Create))
                        {
                            await request.UploadedFile.CopyToAsync(stream);
                        }

                        await _themeDownloadService.SaveUploadedTheme(
                            tempPath,
                            itemDirectory,
                            config.AudioBitrate,
                            request.UploadedFile.FileName,
                            default);
                            
                        _logger.LogInformation("Successfully processed uploaded theme for {ItemName}", item.Name);
                    }
                    finally
                    {
                        if (System.IO.File.Exists(tempPath))
                        {
                            try
                            {
                                System.IO.File.Delete(tempPath);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to delete temporary file {TempFile}", tempPath);
                            }
                        }
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

    public class ThemeSongRequest
    {
        public string? YouTubeUrl { get; set; }
        
        public IFormFile? UploadedFile { get; set; }
    }
}
