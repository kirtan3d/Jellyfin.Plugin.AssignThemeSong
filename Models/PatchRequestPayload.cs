#nullable enable

namespace Jellyfin.Plugin.xThemeSong.Models
{
    /// <summary>
    /// Payload for File Transformation Plugin patches.
    /// </summary>
    public class PatchRequestPayload
    {
        /// <summary>
        /// Gets or sets the content to be patched.
        /// </summary>
        public string? Contents { get; set; }
    }
}
