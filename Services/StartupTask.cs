using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.AssignThemeSong.Helpers;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.AssignThemeSong.Services
{
    /// <summary>
    /// Startup task for Assign Theme Song plugin.
    /// </summary>
    public class StartupTask : IScheduledTask
    {
        private readonly ILogger<StartupTask> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupTask"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public StartupTask(ILogger<StartupTask> logger)
        {
            _logger = logger;
            _logger.LogInformation("[AssignThemeSong] StartupTask constructor called");
            Console.WriteLine("[AssignThemeSong] StartupTask constructor called");
        }

        /// <inheritdoc />
        public string Name => "Assign Theme Song Startup";

        /// <inheritdoc />
        public string Description => "Registers script injection with File Transformation plugin";

        /// <inheritdoc />
        public string Category => "Assign Theme Song";

        /// <inheritdoc />
        public string Key => "AssignThemeSongStartup";

        /// <inheritdoc />
        public bool IsHidden => false;

        /// <inheritdoc />
        public bool IsEnabled => true;

        /// <inheritdoc />
        public bool IsLogged => true;

        /// <inheritdoc />
        public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[AssignThemeSong] StartupTask ExecuteAsync called");
            Console.WriteLine("[AssignThemeSong] StartupTask ExecuteAsync called");
            _logger.LogInformation("Assign Theme Song: Starting script injection registration");
            
            try
            {
                await Task.Run(() => RegisterWithFileTransformation(), cancellationToken);
                _logger.LogInformation("Assign Theme Song: Successfully registered script injection");
                progress?.Report(100);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Assign Theme Song: Could not register with File Transformation plugin. Web UI features may not work. Please install File Transformation plugin: https://github.com/IAmParadox27/jellyfin-plugin-file-transformation");
                progress?.Report(100);
            }
        }

    private void RegisterWithFileTransformation()
    {
        _logger.LogInformation("Assign Theme Song: Starting script injection registration");

        // Get the File Transformation plugin assembly
        Assembly? fileTransformationAssembly = AssemblyLoadContext.All
            .SelectMany(x => x.Assemblies)
            .FirstOrDefault(x => x.FullName?.Contains(".FileTransformation") ?? false);

        if (fileTransformationAssembly == null)
        {
            _logger.LogWarning("File Transformation plugin not found. Please install it from: https://github.com/IAmParadox27/jellyfin-plugin-file-transformation");
            return;
        }

        // Get the PluginInterface type
        Type? pluginInterfaceType = fileTransformationAssembly.GetType("Jellyfin.Plugin.FileTransformation.PluginInterface");
        if (pluginInterfaceType == null)
        {
            _logger.LogWarning("Could not find PluginInterface in FileTransformation assembly");
            return;
        }

        try
        {
            var thisAssembly = GetType().Assembly;
            var pluginId = "6a7b8c9d-0e1f-2345-6789-abcdef012345";
            
            // Create payload using JObject with correct property names
            var payload = new JObject
            {
                ["id"] = Guid.NewGuid().ToString(), // Use a unique ID for this transformation
                ["fileNamePattern"] = "index.html",
                ["callbackAssembly"] = thisAssembly.FullName,
                ["callbackClass"] = typeof(TransformationPatches).FullName,
                ["callbackMethod"] = "IndexHtml"
            };

            _logger.LogInformation($"Registering transformation with payload: {payload}");

            // Register the transformation
            var registerMethod = pluginInterfaceType.GetMethod("RegisterTransformation");
            if (registerMethod != null)
            {
                registerMethod.Invoke(null, new object?[] { payload });
                _logger.LogInformation("Assign Theme Song: Successfully registered script injection with File Transformation Plugin");
            }
            else
            {
                _logger.LogWarning("Could not find RegisterTransformation method");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering transformation with File Transformation plugin");
            throw;
        }
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
