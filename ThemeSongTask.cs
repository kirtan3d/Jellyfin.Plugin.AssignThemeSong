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
        private readonly Plugin _plugin;
        private readonly ThemeDownloadService _downloadService;

        public ThemeSongTask(
            ILogger<ThemeSongTask> logger,
            ILibraryManager libraryManager,
            ThemeDownloadService downloadService)
        {
            _logger = logger;
            _libraryManager = libraryManager;
            _downloadService = downloadService;
            _plugin = Plugin.Instance;
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

            var config = _plugin.Configuration;
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

        private async Task ProcessMediaItem(BaseItem item, PluginConfiguration config, CancellationToken cancellationToken)
        {
            // Determine the path where the theme song should be saved
            var itemPath = item.Path;
            if (string.IsNullOrEmpty(itemPath))
            {
                _logger.LogWarning($"Item {item.Name} has no path, skipping.");
                return;
            }

            var itemDirectory = Path.GetDirectoryName(itemPath);
            if (string.IsNullOrEmpty(itemDirectory))
            {
                _logger.LogWarning($"Could not determine directory for item {item.Name}, skipping.");
                return;
            }

            var themeSongFilePath = Path.Combine(itemDirectory, "theme.mp3");

            // Check if theme song already exists
            var themeSongExists = File.Exists(themeSongFilePath);

            if (themeSongExists && !config.OverwriteExistingFiles)
            {
                _logger.LogInformation($"Theme song already exists for {item.Name} and overwrite is disabled. Skipping.");
                return;
            }

            _logger.LogInformation($"Attempting to assign theme song for {item.Name}.");

            var themeJsonPath = Path.Combine(itemDirectory, "theme.json");
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
                    Type = TaskTriggerInfo.TriggerDaily,
                    TimeOfDayTicks = TimeSpan.FromHours(2).Ticks
                }
            };
        }
    }
}
