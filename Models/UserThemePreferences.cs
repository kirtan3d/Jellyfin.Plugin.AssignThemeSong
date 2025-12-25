using System;

namespace Jellyfin.Plugin.xThemeSong.Models
{
    /// <summary>
    /// Per-user theme song preferences
    /// </summary>
    public class UserThemePreferences
    {
        /// <summary>
        /// Whether theme songs are enabled for this user
        /// </summary>
        public bool EnableThemeSongs { get; set; } = true;

        /// <summary>
        /// Maximum duration in seconds (0 = full duration)
        /// </summary>
        public int MaxDurationSeconds { get; set; } = 0;

        /// <summary>
        /// Volume level (0.0 to 1.0)
        /// </summary>
        public float Volume { get; set; } = 1.0f;

        /// <summary>
        /// When these preferences were last modified
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }
}
