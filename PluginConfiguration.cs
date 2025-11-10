using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.xThemeSong
{
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
    }
}
