#nullable enable

using System;
using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.xThemeSong
{
    /// <summary>
    /// Permission mode for theme song management.
    /// </summary>
    public enum ThemePermissionMode
    {
        /// <summary>
        /// Only server administrators can manage theme songs.
        /// </summary>
        AdminsOnly = 0,

        /// <summary>
        /// Administrators and library managers can manage theme songs.
        /// </summary>
        LibraryManagers = 1,

        /// <summary>
        /// All authenticated users can manage theme songs.
        /// </summary>
        Everyone = 2
    }

    /// <summary>
    /// Plugin configuration.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether to overwrite existing theme files.
        /// </summary>
        public bool OverwriteExistingFiles { get; set; }

        /// <summary>
        /// Gets or sets the audio bitrate for downloaded theme songs.
        /// </summary>
        public int AudioBitrate { get; set; } = 192;

        /// <summary>
        /// Gets or sets the custom FFmpeg path. If empty, auto-detection will be used.
        /// </summary>
        public string? FFmpegPath { get; set; }

        /// <summary>
        /// Gets or sets the permission mode for managing theme songs.
        /// </summary>
        public ThemePermissionMode PermissionMode { get; set; } = ThemePermissionMode.LibraryManagers;

        /// <summary>
        /// Gets or sets the list of user IDs with explicit permission to manage themes.
        /// Only used when PermissionMode is set to a custom mode (future feature).
        /// </summary>
        public List<Guid> AllowedUserIds { get; set; } = new List<Guid>();
    }
}
