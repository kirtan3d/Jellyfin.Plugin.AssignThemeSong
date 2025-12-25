#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.xThemeSong.Models;

namespace Jellyfin.Plugin.xThemeSong.Api
{
    /// <summary>
    /// Theme Export/Import Controller
    /// </summary>
    [ApiController]
    [Authorize(Policy = "RequiresElevation")]
    [Route("xThemeSong/export")]
    public class ThemeExportController : ControllerBase
    {
        private readonly ILogger<ThemeExportController> _logger;
        private readonly ILibraryManager _libraryManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeExportController"/> class.
        /// </summary>
        public ThemeExportController(
            ILogger<ThemeExportController> logger,
            ILibraryManager libraryManager)
        {
            _logger = logger;
            _libraryManager = libraryManager;
        }

        /// <summary>
        /// Export all theme songs to JSON
        /// </summary>
        [HttpGet("json")]
        [Produces("application/json")]
        public async Task<ActionResult<ThemeExportData>> ExportJson()
        {
            try
            {
                _logger.LogInformation("Starting JSON export of theme songs");
                var themes = await CollectAllThemes();
                
                var exportData = new ThemeExportData
                {
                    PluginVersion = Plugin.Instance?.Version?.ToString() ?? "Unknown",
                    ExportedAt = DateTime.UtcNow,
                    TotalThemes = themes.Count,
                    Themes = themes
                };

                _logger.LogInformation("Exported {Count} themes to JSON", themes.Count);
                return Ok(exportData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting themes to JSON");
                return StatusCode(500, "Failed to export themes");
            }
        }

        /// <summary>
        /// Export all theme songs to CSV
        /// </summary>
        [HttpGet("csv")]
        [Produces("text/csv")]
        public async Task<FileContentResult> ExportCsv()
        {
            _logger.LogInformation("Starting CSV export of theme songs");
            var themes = await CollectAllThemes();
            
            var csv = GenerateCsv(themes);
            var bytes = Encoding.UTF8.GetBytes(csv);
            
            _logger.LogInformation("Exported {Count} themes to CSV", themes.Count);
            return new FileContentResult(bytes, "text/csv")
            {
                FileDownloadName = $"xThemeSong-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv"
            };
        }

        /// <summary>
        /// Import theme songs from JSON
        /// </summary>
        [HttpPost("import")]
        [Consumes("application/json")]
        public async Task<ActionResult<ImportResult>> Import([FromBody] ThemeExportData data)
        {
            try
            {
                _logger.LogInformation("Starting import of {Count} themes", data.TotalThemes);
                var result = await ImportThemes(data);
                
                _logger.LogInformation("Import completed: {Imported} imported, {Skipped} skipped, {Failed} failed",
                    result.Imported, result.Skipped, result.Failed);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing themes");
                return StatusCode(500, new ImportResult
                {
                    TotalThemes = data.TotalThemes,
                    Failed = data.TotalThemes,
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Collect all themes from the library
        /// </summary>
        private async Task<List<ThemeExportEntry>> CollectAllThemes()
        {
            var themes = new List<ThemeExportEntry>();

            // Get all movies and series
            var mediaItems = _libraryManager.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Series },
                Recursive = true
            });

            foreach (var item in mediaItems)
            {
                var itemDirectory = GetThemeDirectory(item);
                if (string.IsNullOrEmpty(itemDirectory))
                    continue;

                var themeJsonPath = Path.Combine(itemDirectory, "theme.json");
                var themeMp3Path = Path.Combine(itemDirectory, "theme.mp3");

                // Only export if theme files exist
                if (!System.IO.File.Exists(themeJsonPath) && !System.IO.File.Exists(themeMp3Path))
                    continue;

                ThemeMetadata? metadata = null;
                
                if (System.IO.File.Exists(themeJsonPath))
                {
                    try
                    {
                        var json = await System.IO.File.ReadAllTextAsync(themeJsonPath);
                        metadata = JsonSerializer.Deserialize<ThemeMetadata>(json);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to read theme.json for {ItemName}", item.Name);
                    }
                }

                themes.Add(new ThemeExportEntry
                {
                    ItemId = item.Id,
                    ItemName = item.Name,
                    ItemPath = item.Path ?? string.Empty,
                    ItemType = item.GetType().Name,
                    Metadata = metadata
                });
            }

            return themes;
        }

        /// <summary>
        /// Generate CSV from themes
        /// </summary>
        private string GenerateCsv(List<ThemeExportEntry> themes)
        {
            var sb = new StringBuilder();
            
            // CSV Header
            sb.AppendLine("Item Name,Item Type,Item Path,YouTube ID,YouTube URL,Is User Uploaded,Date Added");

            foreach (var theme in themes)
            {
                var youtubeId = theme.Metadata?.YouTubeId ?? string.Empty;
                var youtubeUrl = theme.Metadata?.YouTubeUrl ?? string.Empty;
                var isUserUploaded = theme.Metadata?.IsUserUploaded ?? false;
                var dateAdded = theme.Metadata != null 
                    ? theme.Metadata.DateAdded.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) 
                    : string.Empty;

                sb.AppendLine($"\"{EscapeCsv(theme.ItemName)}\",\"{theme.ItemType}\",\"{EscapeCsv(theme.ItemPath)}\",\"{youtubeId}\",\"{youtubeUrl}\",{isUserUploaded},\"{dateAdded}\"");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Escape CSV value
        /// </summary>
        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            
            return value.Replace("\"", "\"\"");
        }

        /// <summary>
        /// Import themes from export data
        /// </summary>
        private async Task<ImportResult> ImportThemes(ThemeExportData data)
        {
            var result = new ImportResult
            {
                TotalThemes = data.TotalThemes
            };

            foreach (var entry in data.Themes)
            {
                try
                {
                    // Try to find item by ID first
                    var item = _libraryManager.GetItemById(entry.ItemId);
                    
                    // If not found by ID, try to find by path
                    if (item == null && !string.IsNullOrEmpty(entry.ItemPath))
                    {
                        item = _libraryManager.FindByPath(entry.ItemPath, false);
                        
                        if (item != null)
                        {
                            result.Conflicts.Add(new ConflictInfo
                            {
                                ItemName = entry.ItemName,
                                Reason = "Item ID changed, matched by path",
                                Action = "Imported with new ID"
                            });
                        }
                    }

                    // If still not found, skip
                    if (item == null)
                    {
                        result.Skipped++;
                        result.Conflicts.Add(new ConflictInfo
                        {
                            ItemName = entry.ItemName,
                            Reason = "Item not found (ID: " + entry.ItemId + ")",
                            Action = "Skipped"
                        });
                        continue;
                    }

                    var itemDirectory = GetThemeDirectory(item);
                    if (string.IsNullOrEmpty(itemDirectory))
                    {
                        result.Failed++;
                        result.Errors.Add($"No valid directory for item: {entry.ItemName}");
                        continue;
                    }

                    var themeJsonPath = Path.Combine(itemDirectory, "theme.json");
                    
                    // Check if theme already exists
                    if (System.IO.File.Exists(themeJsonPath))
                    {
                        result.Conflicts.Add(new ConflictInfo
                        {
                            ItemName = entry.ItemName,
                            Reason = "Theme already exists",
                            Action = "Overwritten"
                        });
                    }

                    // Write theme.json
                    if (entry.Metadata != null)
                    {
                        var json = JsonSerializer.Serialize(entry.Metadata, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });
                        
                        await System.IO.File.WriteAllTextAsync(themeJsonPath, json);
                        result.Imported++;
                        
                        _logger.LogDebug("Imported theme for {ItemName}", entry.ItemName);
                    }
                    else
                    {
                        result.Skipped++;
                        result.Errors.Add($"No metadata for item: {entry.ItemName}");
                    }
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Errors.Add($"Failed to import {entry.ItemName}: {ex.Message}");
                    _logger.LogError(ex, "Failed to import theme for {ItemName}", entry.ItemName);
                }
            }

            return result;
        }

        /// <summary>
        /// Get theme directory for item (same logic as ThemeSongTask)
        /// </summary>
        private string? GetThemeDirectory(BaseItem item)
        {
            var itemPath = item.Path;
            if (string.IsNullOrEmpty(itemPath))
                return null;

            // For Series, the Path IS the series folder
            if (item is Series)
                return itemPath;

            // For Movie and other file-based items, use the containing directory
            return Path.GetDirectoryName(itemPath);
        }
    }
}
