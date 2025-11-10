using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.xThemeSong.Helpers;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.xThemeSong.Services
{
    /// <summary>
    /// Startup service for xThemeSong plugin.
    /// </summary>
    public class StartupService : IScheduledTask
    {
        private readonly ILogger<StartupService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupService"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public StartupService(ILogger<StartupService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public string Name => "xThemeSong Startup";

        /// <inheritdoc />
        public string Key => "Jellyfin.Plugin.xThemeSong.Startup";

        /// <inheritdoc />
        public string Description => "Startup Service for xThemeSong";

        /// <inheritdoc />
        public string Category => "Startup Services";

        /// <inheritdoc />
        public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            _logger.LogInformation("xThemeSong Startup Service running");

            // Get the File Transformation plugin assembly
            Assembly? fileTransformationAssembly = 
                AssemblyLoadContext.All.SelectMany(x => x.Assemblies).FirstOrDefault(x => 
                    x.FullName?.Contains(".FileTransformation") ?? false);

            if (fileTransformationAssembly == null)
            {
                _logger.LogInformation("File Transformation plugin not found. Web UI features will not work. Please install File Transformation plugin: https://github.com/IAmParadox27/jellyfin-plugin-file-transformation");
                progress?.Report(100);
                return Task.CompletedTask;
            }

            // Get the PluginInterface type
            Type? pluginInterfaceType = fileTransformationAssembly.GetType("Jellyfin.Plugin.FileTransformation.PluginInterface");
            if (pluginInterfaceType == null)
            {
                _logger.LogInformation("Could not find PluginInterface in FileTransformation assembly. Web UI features will not work.");
                progress?.Report(100);
                return Task.CompletedTask;
            }

            // Create payload using JObject with correct property names
            var payload = new JObject
            {
                ["id"] = "97db7543-d64d-45e1-b2e7-62729b56371f", // Use our plugin GUID as ID
                ["fileNamePattern"] = "index.html",
                ["callbackAssembly"] = GetType().Assembly.FullName,
                ["callbackClass"] = typeof(TransformationPatches).FullName,
                ["callbackMethod"] = nameof(TransformationPatches.IndexHtml)
            };

            _logger.LogInformation("Registering xThemeSong transformation with File Transformation plugin");

            // Register the transformation
            var registerMethod = pluginInterfaceType.GetMethod("RegisterTransformation");
            if (registerMethod != null)
            {
                registerMethod.Invoke(null, new object?[] { payload });
                _logger.LogInformation("Successfully registered xThemeSong transformation with File Transformation Plugin");
            }
            else
            {
                _logger.LogInformation("Could not find RegisterTransformation method in File Transformation plugin");
            }

            progress?.Report(100);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return new[]
            {
                new TaskTriggerInfo
                {
                    Type = TaskTriggerInfo.TriggerStartup
                }
            };
        }
    }
}
