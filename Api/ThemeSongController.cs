#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.xThemeSong.Models;
using Jellyfin.Plugin.xThemeSong.Services;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.xThemeSong.Api
{
    [ApiController]
    [Route("xThemeSong")]
    public class ThemeSongController : ControllerBase
    {
        private readonly ILogger<ThemeSongController> _logger;
        private readonly ILibraryManager _libraryManager;
        private readonly ThemeDownloadService _themeDownloadService;

        public ThemeSongController(
            ILogger<ThemeSongController> logger,
            ILibraryManager libraryManager,
            ThemeDownloadService themeDownloadService)
        {
            _logger = logger;
            _libraryManager = libraryManager;
            _themeDownloadService = themeDownloadService;
        }

        /// <summary>
        /// Gets the plugin configuration safely.
        /// </summary>
        private PluginConfiguration GetConfiguration()
        {
            return Plugin.Instance?.Configuration ?? new PluginConfiguration();
        }

        /// <summary>
        /// Gets the correct directory for storing theme files based on item type.
        /// For Series: Use the series folder directly (item.Path is the folder)
        /// For Season: Navigate to parent series folder
        /// For Movie: Use the parent directory of the movie file
        /// </summary>
        private string? GetThemeDirectory(BaseItem item)
        {
            var itemPath = item.Path;
            if (string.IsNullOrEmpty(itemPath))
            {
                return null;
            }

            // For Series, the Path IS the series folder
            if (item is Series)
            {
                _logger.LogDebug("Item is Series, using path directly: {Path}", itemPath);
                return itemPath;
            }

            // For Season, navigate up to the Series folder
            if (item is Season season)
            {
                // Get parent series
                var series = season.Series;
                if (series != null && !string.IsNullOrEmpty(series.Path))
                {
                    _logger.LogDebug("Item is Season, using parent Series path: {Path}", series.Path);
                    return series.Path;
                }
                // Fallback: go up one directory from season folder
                var parentDir = Path.GetDirectoryName(itemPath);
                _logger.LogDebug("Item is Season (no series ref), using parent: {Path}", parentDir);
                return parentDir;
            }

            // For Movie and other file-based items, use the containing directory
            var directory = Path.GetDirectoryName(itemPath);
            _logger.LogDebug("Item is {Type}, using directory: {Path}", item.GetType().Name, directory);
            return directory;
        }

        [HttpPost("{itemId}")]
        public async Task<ActionResult> AssignThemeSong([FromRoute] string itemId, [FromForm] ThemeSongRequest request)
        {
            var item = _libraryManager.GetItemById(itemId);
            if (item == null)
            {
                return NotFound($"Item {itemId} not found");
            }

            var itemDirectory = GetThemeDirectory(item);
            if (string.IsNullOrEmpty(itemDirectory))
            {
                return BadRequest("Could not determine item directory");
            }

            _logger.LogInformation("Theme directory for {ItemName} ({ItemType}): {Directory}", 
                item.Name, item.GetType().Name, itemDirectory);

            var config = GetConfiguration();

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

        [HttpGet("{itemId}/metadata")]
        public ActionResult GetThemeMetadata([FromRoute] string itemId)
        {
            var item = _libraryManager.GetItemById(itemId);
            if (item == null)
            {
                return NotFound($"Item {itemId} not found");
            }

            var itemDirectory = GetThemeDirectory(item);
            if (string.IsNullOrEmpty(itemDirectory))
            {
                return NotFound("Could not determine item directory");
            }

            var metadataPath = Path.Combine(itemDirectory, "theme.json");
            var audioPath = Path.Combine(itemDirectory, "theme.mp3");
            _logger.LogDebug("Looking for metadata at: {Path}", metadataPath);
            
            if (!System.IO.File.Exists(metadataPath))
            {
                // Feature 2: Handle theme.mp3 without theme.json
                if (System.IO.File.Exists(audioPath))
                {
                    _logger.LogDebug("Found theme.mp3 without theme.json, returning minimal metadata");
                    var fileInfo = new FileInfo(audioPath);
                    return Ok(new ThemeMetadata
                    {
                        IsUserUploaded = true,
                        Title = "(Unknown - No metadata)",
                        DateAdded = fileInfo.CreationTimeUtc,
                        DateModified = fileInfo.LastWriteTimeUtc
                    });
                }
                return NotFound("No theme metadata found");
            }

            try
            {
                var json = System.IO.File.ReadAllText(metadataPath);
                var metadata = JsonSerializer.Deserialize<ThemeMetadata>(json);
                return Ok(metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading theme metadata");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes the theme song and metadata for a media item.
        /// </summary>
        [HttpDelete("{itemId}")]
        public ActionResult DeleteThemeSong([FromRoute] string itemId)
        {
            var item = _libraryManager.GetItemById(itemId);
            if (item == null)
            {
                return NotFound($"Item {itemId} not found");
            }

            var itemDirectory = GetThemeDirectory(item);
            if (string.IsNullOrEmpty(itemDirectory))
            {
                return NotFound("Could not determine item directory");
            }

            var audioPath = Path.Combine(itemDirectory, "theme.mp3");
            var metadataPath = Path.Combine(itemDirectory, "theme.json");
            
            _logger.LogInformation("Deleting theme song for {ItemName} from {Directory}", item.Name, itemDirectory);

            var deletedFiles = new System.Collections.Generic.List<string>();
            var errors = new System.Collections.Generic.List<string>();

            // Delete theme.mp3
            if (System.IO.File.Exists(audioPath))
            {
                try
                {
                    System.IO.File.Delete(audioPath);
                    deletedFiles.Add("theme.mp3");
                    _logger.LogInformation("Deleted theme.mp3 for {ItemName}", item.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete theme.mp3 for {ItemName}", item.Name);
                    errors.Add($"Failed to delete theme.mp3: {ex.Message}");
                }
            }

            // Delete theme.json
            if (System.IO.File.Exists(metadataPath))
            {
                try
                {
                    System.IO.File.Delete(metadataPath);
                    deletedFiles.Add("theme.json");
                    _logger.LogInformation("Deleted theme.json for {ItemName}", item.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete theme.json for {ItemName}", item.Name);
                    errors.Add($"Failed to delete theme.json: {ex.Message}");
                }
            }

            if (deletedFiles.Count == 0 && errors.Count == 0)
            {
                return NotFound("No theme song files found to delete");
            }

            if (errors.Count > 0)
            {
                return StatusCode(500, new { deletedFiles, errors });
            }

            return Ok(new { deletedFiles, message = "Theme song deleted successfully" });
        }

        [HttpGet("{itemId}/audio")]
        public ActionResult GetThemeAudio([FromRoute] string itemId)
        {
            var item = _libraryManager.GetItemById(itemId);
            if (item == null)
            {
                return NotFound($"Item {itemId} not found");
            }

            var itemDirectory = GetThemeDirectory(item);
            if (string.IsNullOrEmpty(itemDirectory))
            {
                return NotFound("Could not determine item directory");
            }

            var audioPath = Path.Combine(itemDirectory, "theme.mp3");
            _logger.LogDebug("Looking for audio at: {Path}", audioPath);
            
            if (!System.IO.File.Exists(audioPath))
            {
                return NotFound("No theme audio found");
            }

            try
            {
                var stream = new FileStream(audioPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return File(stream, "audio/mpeg", "theme.mp3");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error streaming theme audio");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets an overview of all media items grouped by library with theme song status.
        /// </summary>
        [HttpGet("library/overview")]
        public ActionResult<LibraryOverviewResponse> GetLibraryOverview()
        {
            try
            {
                _logger.LogInformation("Getting library overview for theme songs");

                var response = new LibraryOverviewResponse();
                var libraryGroups = new Dictionary<string, LibraryGroup>();

                // Get movies
                var movies = _libraryManager.GetItemList(new InternalItemsQuery
                {
                    Recursive = true,
                    IncludeItemTypes = new[] { BaseItemKind.Movie }
                });

                // Get series
                var series = _libraryManager.GetItemList(new InternalItemsQuery
                {
                    Recursive = true,
                    IncludeItemTypes = new[] { BaseItemKind.Series }
                });

                var mediaItems = movies.Concat(series).ToList();

                foreach (var item in mediaItems)
                {
                    var itemDirectory = GetThemeDirectory(item);
                    if (string.IsNullOrEmpty(itemDirectory))
                    {
                        continue;
                    }

                    // Get library name
                    var libraryName = GetLibraryName(item);
                    var libraryId = item.GetTopParent()?.Id.ToString("N") ?? "unknown";

                    // Check for theme song
                    var audioPath = Path.Combine(itemDirectory, "theme.mp3");
                    var metadataPath = Path.Combine(itemDirectory, "theme.json");
                    var hasThemeSong = System.IO.File.Exists(audioPath);

                    // Read YouTube URL from metadata if exists
                    string? youtubeUrl = null;
                    if (System.IO.File.Exists(metadataPath))
                    {
                        try
                        {
                            var json = System.IO.File.ReadAllText(metadataPath);
                            var metadata = JsonSerializer.Deserialize<ThemeMetadata>(json);
                            youtubeUrl = metadata?.YouTubeUrl;
                        }
                        catch
                        {
                            // Ignore metadata read errors
                        }
                    }

                    var entry = new MediaLibraryEntry
                    {
                        ItemId = item.Id.ToString("N"),
                        Title = item.Name,
                        Type = item is Movie ? "Movie" : "Series",
                        LibraryName = libraryName,
                        HasThemeSong = hasThemeSong,
                        YouTubeUrl = youtubeUrl,
                        Path = itemDirectory
                    };

                    // Group by library
                    if (!libraryGroups.ContainsKey(libraryName))
                    {
                        libraryGroups[libraryName] = new LibraryGroup
                        {
                            LibraryName = libraryName,
                            LibraryId = libraryId,
                            Items = new List<MediaLibraryEntry>()
                        };
                    }

                    libraryGroups[libraryName].Items.Add(entry);
                }

                // Sort items within each library
                foreach (var group in libraryGroups.Values)
                {
                    group.Items = group.Items.OrderBy(i => i.Title).ToList();
                }

                response.Libraries = libraryGroups.Values.OrderBy(g => g.LibraryName).ToList();

                _logger.LogInformation("Found {LibraryCount} libraries with {ItemCount} total items", 
                    response.Libraries.Count, response.Libraries.Sum(l => l.Items.Count));

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting library overview");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves a YouTube URL for a media item (creates/updates theme.json without downloading).
        /// </summary>
        [HttpPost("{itemId}/url")]
        public ActionResult SaveYouTubeUrl([FromRoute] string itemId, [FromBody] SaveYouTubeUrlRequest request)
        {
            var item = _libraryManager.GetItemById(itemId);
            if (item == null)
            {
                return NotFound($"Item {itemId} not found");
            }

            var itemDirectory = GetThemeDirectory(item);
            if (string.IsNullOrEmpty(itemDirectory))
            {
                return BadRequest("Could not determine item directory");
            }

            var metadataPath = Path.Combine(itemDirectory, "theme.json");

            try
            {
                ThemeMetadata metadata;

                // Read existing metadata or create new
                if (System.IO.File.Exists(metadataPath))
                {
                    var json = System.IO.File.ReadAllText(metadataPath);
                    metadata = JsonSerializer.Deserialize<ThemeMetadata>(json) ?? new ThemeMetadata();
                }
                else
                {
                    metadata = new ThemeMetadata
                    {
                        DateAdded = DateTime.UtcNow
                    };
                }

                // Update YouTube URL
                metadata.YouTubeUrl = request.YouTubeUrl;
                metadata.DateModified = DateTime.UtcNow;

                // Extract video ID from URL
                if (!string.IsNullOrEmpty(request.YouTubeUrl))
                {
                    metadata.YouTubeId = ExtractVideoId(request.YouTubeUrl);
                }

                // Save metadata
                var updatedJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                System.IO.File.WriteAllText(metadataPath, updatedJson);

                _logger.LogInformation("Saved YouTube URL for {ItemName}: {Url}", item.Name, request.YouTubeUrl);

                return Ok(new { message = "YouTube URL saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving YouTube URL for {ItemName}", item.Name);
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the library name for an item.
        /// </summary>
        private string GetLibraryName(BaseItem item)
        {
            var topParent = item.GetTopParent();
            return topParent?.Name ?? "Unknown Library";
        }

        /// <summary>
        /// Extracts the video ID from a YouTube URL.
        /// </summary>
        private string? ExtractVideoId(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            // If it's already just a video ID
            if (!input.Contains("youtube.com") && !input.Contains("youtu.be") && input.Length == 11)
            {
                return input;
            }

            // Extract from full URL
            if (input.Contains("youtube.com/watch?v="))
            {
                return input.Split(new[] { "v=" }, StringSplitOptions.None)[1].Split('&')[0];
            }
            else if (input.Contains("youtu.be/"))
            {
                return input.Split(new[] { "youtu.be/" }, StringSplitOptions.None)[1].Split('?')[0];
            }

            return input;
        }
    }

    public class ThemeSongRequest
    {
        public string? YouTubeUrl { get; set; }
        
        public IFormFile? UploadedFile { get; set; }
    }
}
