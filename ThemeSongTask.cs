#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using MediaBrowser.Common;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.xThemeSong.Models;
using Jellyfin.Plugin.xThemeSong.Services;

namespace Jellyfin.Plugin.xThemeSong
{
    public class ThemeSongTask : IScheduledTask
    {
        private readonly ILogger<ThemeSongTask> _logger;
        private readonly ILibraryManager _libraryManager;
        private readonly ThemeDownloadService _downloadService;

        public ThemeSongTask(
            ILogger<ThemeSongTask> logger,
            ILibraryManager libraryManager,
            ThemeDownloadService downloadService)
        {
            _logger = logger;
            _libraryManager = libraryManager;
            _downloadService = downloadService;
        }

        /// <summary>
        /// Gets the plugin configuration safely.
        /// </summary>
        private PluginConfiguration GetConfiguration()
        {
            return Plugin.Instance?.Configuration ?? new PluginConfiguration();
        }

        public string Name => "xTheme Songs";

        public string Description => "Scans the library and assigns theme songs to media items.";

        public string Category => "xThemeSong";

        public string Key => "xThemeSongTask";

        public bool IsHidden => false;

        public bool IsEnabled => true;

        public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            _logger.LogInformation("xThemeSong task started.");

            var config = GetConfiguration();
            _logger.LogInformation($"Overwrite Existing Files: {config.OverwriteExistingFiles}");
            _logger.LogInformation($"Audio Bitrate: {config.AudioBitrate}");

            // Get all items from the library (simplified approach)
            var allItems = _libraryManager.GetItemList(new InternalItemsQuery
            {
                Recursive = true
            });

            var mediaItems = allItems.Where(item => item is MediaBrowser.Controller.Entities.Movies.Movie || 
                                                   item is MediaBrowser.Controller.Entities.TV.Series).ToList();

            var totalItems = mediaItems.Count;
            var processedItems = 0;

            foreach (var item in mediaItems)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                _logger.LogInformation($"Processing item: {item.Name}");
                await ProcessMediaItem(item, config, cancellationToken);

                processedItems++;
                var percentComplete = (double)processedItems / totalItems * 100;
                progress.Report(percentComplete);
            }

            _logger.LogInformation("xThemeSong task finished.");
        }

        /// <summary>
        /// Gets the correct directory for storing theme files based on item type.
        /// For Series: Use the series folder directly (item.Path is the folder)
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

            // For Movie and other file-based items, use the containing directory
            var directory = Path.GetDirectoryName(itemPath);
            _logger.LogDebug("Item is {Type}, using directory: {Path}", item.GetType().Name, directory);
            return directory;
        }

        private async Task ProcessMediaItem(BaseItem item, PluginConfiguration config, CancellationToken cancellationToken)
        {
            // Determine the path where the theme song should be saved
            var itemDirectory = GetThemeDirectory(item);
            if (string.IsNullOrEmpty(itemDirectory))
            {
                _logger.LogWarning("Item {ItemName} has no valid path, skipping.", item.Name);
                return;
            }

            _logger.LogDebug("Theme directory for {ItemName} ({ItemType}): {Directory}", 
                item.Name, item.GetType().Name, itemDirectory);

            var themeSongFilePath = Path.Combine(itemDirectory, "theme.mp3");
            var themeJsonPath = Path.Combine(itemDirectory, "theme.json");

            // Check if theme song already exists
            var themeSongExists = File.Exists(themeSongFilePath);

            // Feature 2: Handle theme.mp3 without theme.json - skip if mp3 exists but no json
            if (themeSongExists && !File.Exists(themeJsonPath))
            {
                _logger.LogInformation("Theme.mp3 exists without theme.json for {ItemName}, skipping (existing manual theme).", item.Name);
                return;
            }

            if (themeSongExists && !config.OverwriteExistingFiles)
            {
                _logger.LogInformation($"Theme song already exists for {item.Name} and overwrite is disabled. Skipping.");
                return;
            }

            _logger.LogInformation($"Attempting to assign theme song for {item.Name}.");

            if (!File.Exists(themeJsonPath))
            {
                _logger.LogInformation($"No theme.json found for {item.Name}, skipping.");
                return;
            }

            try
            {
                var themeMetadata = JsonSerializer.Deserialize<ThemeMetadata>(
                    await File.ReadAllTextAsync(themeJsonPath, cancellationToken));

                if (themeMetadata == null)
                {
                    _logger.LogWarning("Invalid theme.json for {ItemName} at {Path}, skipping.", item.Name, themeJsonPath);
                    return;
                }

                if (themeMetadata.IsUserUploaded)
                {
                    _logger.LogDebug("Theme song for {ItemName} is user uploaded, skipping scheduled download.", item.Name);
                    return;
                }

                if (string.IsNullOrEmpty(themeMetadata.YouTubeId))
                {
                    _logger.LogWarning("No YouTube ID found for {ItemName} in theme.json, skipping.", item.Name);
                    return;
                }

                _logger.LogInformation("Downloading theme song for {ItemName} from YouTube ID: {YouTubeId}", item.Name, themeMetadata.YouTubeId);

                try
                {
                    await _downloadService.DownloadFromYouTube(
                        themeMetadata.YouTubeId,
                        itemDirectory,
                        config.AudioBitrate,
                        cancellationToken);

                    _logger.LogInformation("Successfully downloaded theme song for {ItemName}", item.Name);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to download theme song for {ItemName} from YouTube ID {YouTubeId}", item.Name, themeMetadata.YouTubeId);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing theme.json for {ItemName} at {Path}", item.Name, themeJsonPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing theme song for {ItemName}", item.Name);
            }
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            // Daily trigger at 2 AM
            return new[]
            {
                new TaskTriggerInfo
                {
                    Type = TaskTriggerInfoType.DailyTrigger,
                    TimeOfDayTicks = TimeSpan.FromHours(2).Ticks
                }
            };
        }
    }
}
