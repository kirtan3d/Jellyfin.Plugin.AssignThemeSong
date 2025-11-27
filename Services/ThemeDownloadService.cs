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

        /// <summary>
        /// Gets the FFmpeg path, checking environment variables and common locations.
        /// </summary>
        private string GetFfmpegPath()
        {
            // Check Jellyfin's environment variable first (used in Docker)
            var jellyfinFfmpeg = Environment.GetEnvironmentVariable("JELLYFIN_FFMPEG");
            if (!string.IsNullOrEmpty(jellyfinFfmpeg) && File.Exists(jellyfinFfmpeg))
            {
                _logger.LogInformation("Using FFmpeg from JELLYFIN_FFMPEG environment variable: {Path}", jellyfinFfmpeg);
                return jellyfinFfmpeg;
            }

            // Common FFmpeg locations to check
            var possiblePaths = new[]
            {
                // Linux/Docker Jellyfin locations
                "/usr/lib/jellyfin-ffmpeg/ffmpeg",
                "/usr/bin/ffmpeg",
                "/usr/local/bin/ffmpeg",
                
                // macOS locations
                "/opt/homebrew/bin/ffmpeg",
                "/usr/local/bin/ffmpeg",
                
                // Windows locations
                @"C:\Program Files\Jellyfin\Server\ffmpeg.exe",
                @"C:\ProgramData\chocolatey\bin\ffmpeg.exe",
                @"C:\ffmpeg\bin\ffmpeg.exe"
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    _logger.LogInformation("Found FFmpeg at: {Path}", path);
                    return path;
                }
            }

            // Fall back to PATH - just use "ffmpeg" and let the OS find it
            var ffmpegName = OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";
            _logger.LogInformation("Using FFmpeg from system PATH: {Name}", ffmpegName);
            return ffmpegName;
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

                _logger.LogInformation("Downloading audio from YouTube video: {VideoId}", videoId);

                // Get video details
                var video = await _youtube.Videos.GetAsync(videoId, cancellationToken);
                var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId, cancellationToken);
                
                // Get the best audio stream
                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                
                // Create temporary file for downloaded audio
                var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.{audioStreamInfo.Container.Name}");
                
                try
                {
                    // Download the audio
                    _logger.LogInformation("Downloading audio stream to temp file: {TempFile}", tempFile);
                    await _youtube.Videos.Streams.DownloadAsync(audioStreamInfo, tempFile, null, cancellationToken);

                    // Get FFmpeg path
                    var ffmpegPath = GetFfmpegPath();

                    // Output path for the theme song
                    var outputPath = Path.Combine(outputDirectory, "theme.mp3");

                    _logger.LogInformation("Converting to MP3 using FFmpeg: {FfmpegPath}", ffmpegPath);

                    // Convert to MP3 using FFmpeg with specified bitrate
                    var process = new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = ffmpegPath,
                            Arguments = $"-i \"{tempFile}\" -b:a {bitrate}k -vn \"{outputPath}\" -y",
                            UseShellExecute = false,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };

                    process.Start();
                    
                    // Read error output for debugging
                    var errorOutput = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync(cancellationToken);

                    if (process.ExitCode != 0)
                    {
                        _logger.LogError("FFmpeg conversion failed with exit code {ExitCode}. Error: {Error}", process.ExitCode, errorOutput);
                        throw new Exception($"FFmpeg conversion failed: {errorOutput}");
                    }

                    _logger.LogInformation("Successfully converted to MP3: {OutputPath}", outputPath);

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

                    _logger.LogInformation("Theme song downloaded successfully for video: {Title}", video.Title);
                    return metadata;
                }
                finally
                {
                    // Clean up temp file
                    if (File.Exists(tempFile))
                    {
                        try 
                        {
                            File.Delete(tempFile);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete temp file {TempFile}", tempFile);
                        }
                    }
                }
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
                // Get FFmpeg path
                var ffmpegPath = GetFfmpegPath();

                var outputPath = Path.Combine(outputDirectory, "theme.mp3");

                _logger.LogInformation("Converting uploaded file to MP3 using FFmpeg: {FfmpegPath}", ffmpegPath);

                // Convert/normalize uploaded MP3 using FFmpeg
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = $"-i \"{sourcePath}\" -b:a {bitrate}k -vn \"{outputPath}\" -y",
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                
                // Read error output for debugging
                var errorOutput = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync(cancellationToken);

                if (process.ExitCode != 0)
                {
                    _logger.LogError("FFmpeg conversion failed with exit code {ExitCode}. Error: {Error}", process.ExitCode, errorOutput);
                    throw new Exception($"FFmpeg conversion failed: {errorOutput}");
                }

                _logger.LogInformation("Successfully converted uploaded file to MP3: {OutputPath}", outputPath);

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
