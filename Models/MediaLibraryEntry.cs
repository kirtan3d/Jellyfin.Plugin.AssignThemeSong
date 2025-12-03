#nullable enable

using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.xThemeSong.Models
{
    /// <summary>
    /// Represents a media item entry for the library overview table.
    /// </summary>
    public class MediaLibraryEntry
    {
        /// <summary>
        /// Gets or sets the unique item ID.
        /// </summary>
        [JsonPropertyName("itemId")]
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the media title.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the media type (Movie/Series).
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the library name.
        /// </summary>
        [JsonPropertyName("libraryName")]
        public string LibraryName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the item has a theme song.
        /// </summary>
        [JsonPropertyName("hasThemeSong")]
        public bool HasThemeSong { get; set; }

        /// <summary>
        /// Gets or sets the YouTube URL from the theme.json metadata.
        /// </summary>
        [JsonPropertyName("youtubeUrl")]
        public string? YouTubeUrl { get; set; }

        /// <summary>
        /// Gets or sets the path to the item for internal use.
        /// </summary>
        [JsonPropertyName("path")]
        public string? Path { get; set; }
    }

    /// <summary>
    /// Response containing grouped library entries.
    /// </summary>
    public class LibraryOverviewResponse
    {
        /// <summary>
        /// Gets or sets the list of library groups.
        /// </summary>
        [JsonPropertyName("libraries")]
        public System.Collections.Generic.List<LibraryGroup> Libraries { get; set; } = new();
    }

    /// <summary>
    /// Group of media entries belonging to a library.
    /// </summary>
    public class LibraryGroup
    {
        /// <summary>
        /// Gets or sets the library name.
        /// </summary>
        [JsonPropertyName("libraryName")]
        public string LibraryName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the library ID.
        /// </summary>
        [JsonPropertyName("libraryId")]
        public string LibraryId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the media entries in this library.
        /// </summary>
        [JsonPropertyName("items")]
        public System.Collections.Generic.List<MediaLibraryEntry> Items { get; set; } = new();
    }

    /// <summary>
    /// Request to save YouTube URL for a media item.
    /// </summary>
    public class SaveYouTubeUrlRequest
    {
        /// <summary>
        /// Gets or sets the YouTube URL.
        /// </summary>
        [JsonPropertyName("youtubeUrl")]
        public string? YouTubeUrl { get; set; }
    }
}
