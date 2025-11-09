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
        /// <param name="payload">JObject with "contents" field containing the HTML.</param>
        /// <returns>JObject with modified "contents" field.</returns>
        public static object PatchIndexHtml(object payload)
        {
            // Extract contents from the payload JObject
            var payloadJson = payload as Newtonsoft.Json.Linq.JObject;
            if (payloadJson == null)
            {
                return payload;
            }

            var contents = payloadJson["contents"]?.ToString();
            if (string.IsNullOrEmpty(contents))
            {
                return payload;
            }

            var pluginName = "Assign Theme Song";
            var pluginVersion = Plugin.Instance?.Version.ToString() ?? "unknown";
            var scriptUrl = "/AssignThemeSong/script";
            var scriptTag = $"<script plugin=\"{pluginName}\" version=\"{pluginVersion}\" src=\"{scriptUrl}\" defer></script>";

            // Remove any existing script tag
            var regex = new Regex($"<script[^>]*plugin=[\"']{Regex.Escape(pluginName)}[\"'][^>]*>\\s*</script>\\n?");
            var updatedContent = regex.Replace(contents, string.Empty);

            // Inject the new script tag before </body>
            if (updatedContent.Contains("</body>"))
            {
                updatedContent = updatedContent.Replace("</body>", $"{scriptTag}\n</body>");
            }

            // Return the modified payload
            payloadJson["contents"] = updatedContent;
            return payloadJson;
        }
    }
}
