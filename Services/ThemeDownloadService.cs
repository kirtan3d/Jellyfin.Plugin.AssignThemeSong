using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using Jellyfin.Plugin.xThemeSong.Models;
using System.Text.Json;

namespace Jellyfin.Plugin.xThemeSong.Services
{
    public class ThemeDownloadService
    {
        private readonly ILogger<ThemeDownloadService> _logger;
        private readonly YoutubeClient _youtube;

        public ThemeDownloadService(ILogger<ThemeDownloadService> logger)
        {
            _logger = logger;
            _youtube = new YoutubeClient();
        }

        public async Task<ThemeMetadata> DownloadFromYouTube(string input, string outputDirectory, int bitrate, CancellationToken cancellationToken)
        {
            try
            {
                // Extract video ID from URL or use directly if it's an ID
                string videoId = input;
                if (input.Contains("youtube.com") || input.Contains("youtu.be"))
                {
                    // Simple video ID extraction
                    if (input.Contains("youtube.com/watch?v="))
                    {
                        videoId = input.Split(new[] { "v=" }, StringSplitOptions.None)[1].Split('&')[0];
                    }
                    else if (input.Contains("youtu.be/"))
                    {
                        videoId = input.Split(new[] { "youtu.be/" }, StringSplitOptions.None)[1].Split('?')[0];
                    }
                }

                // Get video details
                var video = await _youtube.Videos.GetAsync(videoId, cancellationToken);
                var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId, cancellationToken);
                
                // Get the best audio stream
                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                
                // Create temporary file for downloaded audio
                var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.{audioStreamInfo.Container.Name}");
                
                // Download the audio
                await _youtube.Videos.Streams.DownloadAsync(audioStreamInfo, tempFile, null, cancellationToken);

                // Use FFmpeg from system PATH (Jellyfin should have it available)
                var ffmpegPath = "ffmpeg";
                if (OperatingSystem.IsWindows())
                {
                    ffmpegPath += ".exe";
                }

                // Output path for the theme song
                var outputPath = Path.Combine(outputDirectory, "theme.mp3");

                // Convert to MP3 using FFmpeg with specified bitrate
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = $"-i \"{tempFile}\" -b:a {bitrate}k -vn \"{outputPath}\" -y",
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync(cancellationToken);

                // Clean up temp file
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                // Create metadata
                var metadata = new ThemeMetadata
                {
                    YouTubeId = videoId,
                    YouTubeUrl = $"https://www.youtube.com/watch?v={videoId}",
                    Title = video.Title,
                    Uploader = video.Author.ChannelTitle,
                    DateAdded = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow,
                    IsUserUploaded = false
                };

                // Save metadata
                var metadataPath = Path.Combine(outputDirectory, "theme.json");
                await File.WriteAllTextAsync(metadataPath, JsonSerializer.Serialize(metadata, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                }), cancellationToken);

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading theme song from YouTube");
                throw;
            }
        }

        public async Task<ThemeMetadata> SaveUploadedTheme(string sourcePath, string outputDirectory, int bitrate, string originalFileName, CancellationToken cancellationToken)
        {
            try
            {
                // Use FFmpeg from system PATH (Jellyfin should have it available)
                var ffmpegPath = "ffmpeg";
                if (OperatingSystem.IsWindows())
                {
                    ffmpegPath += ".exe";
                }

                var outputPath = Path.Combine(outputDirectory, "theme.mp3");

                // Convert/normalize uploaded MP3 using FFmpeg
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = $"-i \"{sourcePath}\" -b:a {bitrate}k -vn \"{outputPath}\" -y",
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync(cancellationToken);

                // Create metadata
                var metadata = new ThemeMetadata
                {
                    IsUserUploaded = true,
                    OriginalFileName = originalFileName,
                    DateAdded = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow
                };

                // Save metadata
                var metadataPath = Path.Combine(outputDirectory, "theme.json");
                await File.WriteAllTextAsync(metadataPath, JsonSerializer.Serialize(metadata, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                }), cancellationToken);

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving uploaded theme song");
                throw;
            }
        }
    }
}
