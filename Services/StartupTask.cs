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
        }

        /// <inheritdoc />
        public string Name => "Assign Theme Song Startup";

        /// <inheritdoc />
        public string Description => "Registers script injection with File Transformation plugin (fallback)";

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
            
            // Check if registration was already successful in Plugin constructor
            if (Plugin.RegistrationSuccessful)
            {
                _logger.LogInformation("[AssignThemeSong] Registration already successful in Plugin constructor, skipping StartupTask registration");
                progress?.Report(100);
                return;
            }

            _logger.LogInformation("[AssignThemeSong] Registration not successful in constructor, attempting fallback registration in StartupTask");

            try
            {
                await Task.Run(async () => await RegisterWithFileTransformationAsync(cancellationToken), cancellationToken);
                progress?.Report(100);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[AssignThemeSong] Could not register with File Transformation plugin in StartupTask. Web UI features may not work. Please install File Transformation plugin: https://github.com/IAmParadox27/jellyfin-plugin-file-transformation");
                progress?.Report(100);
            }
        }

        private async Task RegisterWithFileTransformationAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[AssignThemeSong] Starting fallback script injection registration in StartupTask");

            // More aggressive retry mechanism for fallback
            int maxRetries = 15;
            int baseRetryDelayMs = 2000; // 2 seconds base delay

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    _logger.LogInformation($"[AssignThemeSong] Fallback attempt {attempt}/{maxRetries} to register with File Transformation plugin");

                    // Get the File Transformation plugin assembly
                    Assembly? fileTransformationAssembly = AssemblyLoadContext.All
                        .SelectMany(x => x.Assemblies)
                        .FirstOrDefault(x => x.FullName?.Contains(".FileTransformation") ?? false);

                    if (fileTransformationAssembly == null)
                    {
                        _logger.LogWarning($"[AssignThemeSong] File Transformation plugin not found on fallback attempt {attempt}. Will retry in {baseRetryDelayMs * attempt}ms");
                        if (attempt < maxRetries)
                        {
                            await Task.Delay(baseRetryDelayMs * attempt, cancellationToken);
                            continue;
                        }
                        else
                        {
                            _logger.LogWarning("[AssignThemeSong] File Transformation plugin not found after all fallback retries. Please install it from: https://github.com/IAmParadox27/jellyfin-plugin-file-transformation");
                            return;
                        }
                    }

                    // Get the PluginInterface type
                    Type? pluginInterfaceType = fileTransformationAssembly.GetType("Jellyfin.Plugin.FileTransformation.PluginInterface");
                    if (pluginInterfaceType == null)
                    {
                        _logger.LogWarning($"[AssignThemeSong] Could not find PluginInterface in FileTransformation assembly on fallback attempt {attempt}");
                        if (attempt < maxRetries)
                        {
                            await Task.Delay(baseRetryDelayMs * attempt, cancellationToken);
                            continue;
                        }
                        else
                        {
                            _logger.LogWarning("[AssignThemeSong] Could not find PluginInterface after all fallback retries");
                            return;
                        }
                    }

                    var thisAssembly = GetType().Assembly;
                    
                    // Create payload using JObject with correct property names
                    var payload = new JObject
                    {
                        ["id"] = "6a7b8c9d-0e1f-2345-6789-abcdef012345", // Use our plugin GUID as ID
                        ["fileNamePattern"] = "index.html",
                        ["callbackAssembly"] = thisAssembly.FullName,
                        ["callbackClass"] = typeof(TransformationPatches).FullName,
                        ["callbackMethod"] = "IndexHtml"
                    };

                    _logger.LogInformation($"[AssignThemeSong] Registering transformation with payload: {payload}");

                    // Register the transformation
                    var registerMethod = pluginInterfaceType.GetMethod("RegisterTransformation");
                    if (registerMethod != null)
                    {
                        registerMethod.Invoke(null, new object?[] { payload });
                        _logger.LogInformation("[AssignThemeSong] SUCCESS: Successfully registered script injection with File Transformation Plugin in StartupTask fallback");
                        return; // Success, exit the retry loop
                    }
                    else
                    {
                        _logger.LogWarning("[AssignThemeSong] Could not find RegisterTransformation method in fallback");
                        if (attempt < maxRetries)
                        {
                            await Task.Delay(baseRetryDelayMs * attempt, cancellationToken);
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"[AssignThemeSong] Error registering transformation on fallback attempt {attempt}");
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(baseRetryDelayMs * attempt, cancellationToken);
                        continue;
                    }
                    else
                    {
                        _logger.LogError(ex, "[AssignThemeSong] Error registering transformation with File Transformation plugin after all fallback retries");
                        throw;
                    }
                }
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
