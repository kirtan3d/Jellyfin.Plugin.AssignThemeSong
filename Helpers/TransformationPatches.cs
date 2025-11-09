using System.Text.RegularExpressions;
using Jellyfin.Plugin.AssignThemeSong.Models;

namespace Jellyfin.Plugin.AssignThemeSong.Helpers
{
    /// <summary>
    /// Transformation patches for File Transformation Plugin.
    /// </summary>
    public static class TransformationPatches
    {
        /// <summary>
        /// Patches index.html to inject our script.
        /// This is called by File Transformation plugin via reflection.
        /// </summary>
        /// <param name="content">The PatchRequestPayload containing the HTML content.</param>
        /// <returns>The modified HTML content as string.</returns>
        public static string IndexHtml(PatchRequestPayload content)
        {
            if (string.IsNullOrEmpty(content.Contents))
            {
                return content.Contents ?? string.Empty;
            }

            var pluginName = "Assign Theme Song";
            var pluginVersion = Plugin.Instance?.Version.ToString() ?? "unknown";
            var scriptUrl = "../AssignThemeSong/script";
            var scriptTag = $"<script plugin=\"{pluginName}\" version=\"{pluginVersion}\" src=\"{scriptUrl}\" defer></script>";

            // Remove any existing script tag
            var regex = new Regex($"<script[^>]*plugin=[\"']{Regex.Escape(pluginName)}[\"'][^>]*>\\s*</script>\\n?");
            var updatedContent = regex.Replace(content.Contents, string.Empty);

            // Inject the new script tag before </body>
            if (updatedContent.Contains("</body>"))
            {
                return updatedContent.Replace("</body>", $"{scriptTag}\n</body>");
            }

            return updatedContent;
        }
    }
}
