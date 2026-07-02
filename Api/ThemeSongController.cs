#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Data;
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
        private readonly IUserManager _userManager;

        public ThemeSongController(
            ILogger<ThemeSongController> logger,
            ILibraryManager libraryManager,
            ThemeDownloadService themeDownloadService,
            IUserManager userManager)
        {
            _logger = logger;
            _libraryManager = libraryManager;
            _themeDownloadService = themeDownloadService;
            _userManager = userManager;
        }

        /// <summary>
        /// Gets the plugin configuration safely.
        /// </summary>
        private PluginConfiguration GetConfiguration()
        {
            return Plugin.Instance?.Configuration ?? new PluginConfiguration();
        }

        /// <summary>
        /// Checks if the current user has permission to manage theme songs.
        /// </summary>
        private bool HasThemeManagementPermission(BaseItem? item = null)
        {
            var config = GetConfiguration();
            
            // Check if user is an administrator using role claim
            var isAdmin = User.IsInRole("Administrator");
            
            // Check permission mode
            switch (config.PermissionMode)
            {
                case ThemePermissionMode.AdminsOnly:
                    // Only administrators can manage themes
                    return isAdmin;
                    
                case ThemePermissionMode.LibraryManagers:
                    // Administrators only for now
                    // LibraryManager role check would require deeper integration
                    return isAdmin;
                    
                case ThemePermissionMode.Everyone:
                    // All authenticated users
                    return User.Identity?.IsAuthenticated ?? false;
                    
                default:
                    // Default to admin-only for safety
                    return isAdmin;
            }
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

            // Check permissions
            if (!HasThemeManagementPermission(item))
            {
                return Forbid();
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
                // Determine target type based on item type if not explicitly provided
                var targetType = request.TargetType;
                if (string.IsNullOrEmpty(targetType) || targetType == "Movie")
                {
                    targetType = item switch
                    {
                        Series => "Series",
                        Season => "Season",
                        Movie => "Movie",
                        _ => "Movie"
                    };
                }
                
                // Determine parent ID for inheritance
                var parentId = request.ParentId;
                if (string.IsNullOrEmpty(parentId))
                {
                    if (item is Season season)
                    {
                        parentId = season.SeriesId.ToString("N");
                    }
                }

                if (!string.IsNullOrEmpty(request.YouTubeUrl))
                {
                    _logger.LogInformation("Downloading theme from YouTube URL: {Url} for item {ItemName} ({TargetType})", 
                        request.YouTubeUrl, item.Name, targetType);
                        
                    await _themeDownloadService.DownloadFromYouTube(
                        request.YouTubeUrl,
                        itemDirectory,
                        config.AudioBitrate,
                        default,
                        targetType,
                        parentId);
                        
                    _logger.LogInformation("Successfully downloaded theme from YouTube for {ItemName}", item.Name);
                }
                else if (request.UploadedFile != null && request.UploadedFile.Length > 0)
                {
                    _logger.LogInformation("Processing uploaded file: {FileName} ({FileSize} bytes) for {ItemName} ({TargetType})", 
                        request.UploadedFile.FileName, request.UploadedFile.Length, item.Name, targetType);
                        
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
                            default,
                            targetType,
                            parentId);
                            
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

            // Check permissions
            if (!HasThemeManagementPermission(item))
            {
                return Forbid();
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

                    // Get image URL
                    string? imageUrl = null;
                    if (item.ImageInfos != null && item.ImageInfos.Any(i => i.Type == MediaBrowser.Model.Entities.ImageType.Primary))
                    {
                        imageUrl = Url.ActionLink(
                            "GetImage",
                            "Items",
                            new { itemId = item.Id.ToString("N"), type = "Primary", maxWidth = 100 },
                            Request.Scheme);
                    }

                    var entry = new MediaLibraryEntry
                    {
                        ItemId = item.Id.ToString("N"),
                        Title = item.Name,
                        Type = item is Movie ? "Movie" : "Series",
                        LibraryName = libraryName,
                        HasThemeSong = hasThemeSong,
                        YouTubeUrl = youtubeUrl,
                        Path = itemDirectory,
                        ImageUrl = imageUrl
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

            // Check permissions
            if (!HasThemeManagementPermission(item))
            {
                return Forbid();
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
        /// Gets the theme hierarchy for an item, showing direct and inherited themes.
        /// </summary>
        [HttpGet("{itemId}/hierarchy")]
        public ActionResult<ThemeHierarchyResponse> GetThemeHierarchy([FromRoute] string itemId)
        {
            var item = _libraryManager.GetItemById(itemId);
            if (item == null)
            {
                return NotFound($"Item {itemId} not found");
            }

            var response = new ThemeHierarchyResponse
            {
                ItemId = itemId,
                ItemName = item.Name,
                ItemType = item switch
                {
                    Movie => "Movie",
                    Series => "Series",
                    Season => "Season",
                    _ => item.GetType().Name
                }
            };

            // Check for direct theme
            var itemDirectory = GetThemeDirectory(item);
            if (!string.IsNullOrEmpty(itemDirectory))
            {
                var audioPath = Path.Combine(itemDirectory, "theme.mp3");
                if (System.IO.File.Exists(audioPath))
                {
                    response.HasDirectTheme = true;
                    response.DirectThemePath = audioPath;
                }
            }

            // Check for inherited theme
            if (item is Season season)
            {
                var series = season.Series;
                if (series != null)
                {
                    var seriesDirectory = GetThemeDirectory(series);
                    if (!string.IsNullOrEmpty(seriesDirectory))
                    {
                        var seriesAudioPath = Path.Combine(seriesDirectory, "theme.mp3");
                        if (System.IO.File.Exists(seriesAudioPath))
                        {
                            response.HasInheritedTheme = true;
                            response.InheritedFromItemId = series.Id.ToString("N");
                            response.InheritedFromItemName = series.Name;
                            response.InheritedThemePath = seriesAudioPath;
                        }
                    }
                }
            }
            else if (item is Movie)
            {
                // Check if movie is part of a box set by querying box sets
                var boxSets = _libraryManager.GetItemList(new InternalItemsQuery
                {
                    IncludeItemTypes = new[] { BaseItemKind.BoxSet },
                    Recursive = true
                });
                
                foreach (var boxSet in boxSets)
                {
                    // Get items in this box set
                    var boxSetItems = _libraryManager.GetItemList(new InternalItemsQuery
                    {
                        ParentId = boxSet.Id,
                        IncludeItemTypes = new[] { BaseItemKind.Movie }
                    });
                    
                    if (boxSetItems != null && boxSetItems.Any(c => c.Id == item.Id))
                    {
                        var boxSetDirectory = GetThemeDirectory(boxSet);
                        if (!string.IsNullOrEmpty(boxSetDirectory))
                        {
                            var boxSetAudioPath = Path.Combine(boxSetDirectory, "theme.mp3");
                            if (System.IO.File.Exists(boxSetAudioPath))
                            {
                                response.HasInheritedTheme = true;
                                response.InheritedFromItemId = boxSet.Id.ToString("N");
                                response.InheritedFromItemName = boxSet.Name;
                                response.InheritedThemePath = boxSetAudioPath;
                            }
                        }
                        break;
                    }
                }
            }

            return Ok(response);
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
        
        /// <summary>
        /// Target type for theme assignment: Movie, Series, Season, or BoxSet
        /// </summary>
        public string TargetType { get; set; } = "Movie";
        
        /// <summary>
        /// Parent ID for inheritance (Series ID for seasons, Collection ID for BoxSets)
        /// </summary>
        public string? ParentId { get; set; }
    }
    
    /// <summary>
    /// Request model for theme hierarchy information
    /// </summary>
    public class ThemeHierarchyResponse
    {
        public string ItemId { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public bool HasDirectTheme { get; set; }
        public string? DirectThemePath { get; set; }
        public bool HasInheritedTheme { get; set; }
        public string? InheritedFromItemId { get; set; }
        public string? InheritedFromItemName { get; set; }
        public string? InheritedThemePath { get; set; }
    }
}
