using System;
using System.Text.RegularExpressions;
using Jellyfin.Plugin.xThemeSong.Models;

namespace Jellyfin.Plugin.xThemeSong.Helpers
{
    /// <summary>
    /// Transformation patches for File Transformation Plugin.
    /// </summary>
    public static class TransformationPatches
    {
        private static string _basePath = string.Empty;
        private static string _version = "Unknown";

        /// <summary>
        /// Sets the base path and version for script injection.
        /// </summary>
        /// <param name="basePath">The base path for script URLs.</param>
        /// <param name="version">The plugin version.</param>
        public static void SetBasePathAndVersion(string basePath, string version)
        {
            _basePath = basePath;
            _version = version;
        }

        /// <summary>
        /// Patches index.html to inject our script.
        /// This is called by File Transformation plugin via reflection.
        /// </summary>
        /// <param name="payload">The PatchRequestPayload containing the HTML content.</param>
        /// <returns>The modified HTML content as string.</returns>
        public static string IndexHtml(PatchRequestPayload payload)
        {
            try
            {
                var content = payload?.Contents ?? string.Empty;

                if (string.IsNullOrEmpty(content))
                {
                    return content;
                }

                // Transform the content
                string scriptReplace = "<script plugin=\"xThemeSong\".*?></script>";
                string scriptElement = string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "<script plugin=\"xThemeSong\" version=\"{1}\" src=\"{0}/xThemeSong/plugin\" defer></script>",
                    _basePath,
                    _version);

                // Check if script is already injected
                if (content.Contains(scriptElement, StringComparison.Ordinal))
                {
                    return content;
                }

                // Remove old xThemeSong scripts
                content = Regex.Replace(content, scriptReplace, string.Empty);

                // Find closing body tag
                int bodyClosing = content.LastIndexOf("</body>", StringComparison.Ordinal);
                if (bodyClosing == -1)
                {
                    return content;
                }

                // Insert script before closing body tag
                content = content.Insert(bodyClosing, scriptElement);

                return content;
            }
            catch
            {
                // If transformation fails, return original content
                return payload?.Contents ?? string.Empty;
            }
        }
    }
}
