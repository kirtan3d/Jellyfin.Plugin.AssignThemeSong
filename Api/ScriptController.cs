using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.AssignThemeSong.Api
{
    /// <summary>
    /// Controller to serve plugin scripts.
    /// </summary>
    [ApiController]
    [Route("AssignThemeSong")]
    public class ScriptController : ControllerBase
    {
        /// <summary>
        /// Serves the main plugin script.
        /// </summary>
        /// <returns>The plugin JavaScript.</returns>
        [HttpGet("script")]
        [Produces("application/javascript")]
        public ActionResult GetScript()
        {
            return GetScriptFile("plugin.js", "Jellyfin.Plugin.AssignThemeSong.web.plugin.js");
        }

        /// <summary>
        /// Serves the assignThemeSong module script.
        /// </summary>
        /// <returns>The assignThemeSong JavaScript module.</returns>
        [HttpGet("assignThemeSong")]
        [Produces("application/javascript")]
        public ActionResult GetAssignThemeSongScript()
        {
            return GetScriptFile("assignThemeSong.js", "Jellyfin.Plugin.AssignThemeSong.web.assignThemeSong.js");
        }

        private ActionResult GetScriptFile(string fileName, string resourcePath)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(resourcePath);
            if (stream == null)
            {
                // Fallback to file system if not embedded
                var filePath = Path.Combine(Path.GetDirectoryName(assembly.Location) ?? "", "web", fileName);
                if (System.IO.File.Exists(filePath))
                {
                    var content = System.IO.File.ReadAllText(filePath);
                    return Content(content, "application/javascript");
                }
                
                return NotFound($"Plugin script {fileName} not found");
            }

            using var reader = new StreamReader(stream, Encoding.UTF8);
            var scriptContent = reader.ReadToEnd();
            return Content(scriptContent, "application/javascript");
        }
    }
}
