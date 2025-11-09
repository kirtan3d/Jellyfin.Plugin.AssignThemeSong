using System;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.AssignThemeSong.Models
{
    public class ThemeMetadata
    {
        [JsonPropertyName("YouTubeId")]
        public string? YouTubeId { get; set; }

        [JsonPropertyName("YouTubeUrl")]
        public string? YouTubeUrl { get; set; }

        [JsonPropertyName("Title")]
        public string? Title { get; set; }

        [JsonPropertyName("Uploader")]
        public string? Uploader { get; set; }

        [JsonPropertyName("DateAdded")]
        public DateTime DateAdded { get; set; }

        [JsonPropertyName("DateModified")]
        public DateTime DateModified { get; set; }

        [JsonPropertyName("IsUserUploaded")]
        public bool IsUserUploaded { get; set; }

        [JsonPropertyName("OriginalFileName")]
        public string? OriginalFileName { get; set; }
    }
}