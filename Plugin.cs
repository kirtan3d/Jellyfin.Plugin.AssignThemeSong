using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.AssignThemeSong;

/// <summary>
/// The main plugin class.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    private readonly ILogger<Plugin> _logger;
    private readonly IServerConfigurationManager _configurationManager;
    private static bool _registrationAttempted = false;
    private static bool _registrationSuccessful = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{Plugin}"/> interface.</param>
    /// <param name="configurationManager">Instance of the <see cref="IServerConfigurationManager"/> interface.</param>
    public Plugin(
        IApplicationPaths applicationPaths,
        IXmlSerializer xmlSerializer,
        ILogger<Plugin> logger,
        IServerConfigurationManager configurationManager)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
        _logger = logger;
        _configurationManager = configurationManager;

        // Try initial registration in constructor
        _ = Task.Run(async () =>
        {
            try
            {
                _logger.LogInformation("[AssignThemeSong] Plugin constructor initialization started...");
                
                // Give File Transformation plugin time to load (alphabetical order issue)
                await Task.Delay(TimeSpan.FromSeconds(5));
                
                if (await TryRegisterFileTransformation())
                {
                    _logger.LogInformation("[AssignThemeSong] Successfully registered with File Transformation plugin in constructor");
                    _registrationSuccessful = true;
                }
                else
                {
                    _logger.LogWarning("[AssignThemeSong] Failed to register with File Transformation plugin in constructor, will retry in scheduled task");
                }
                
                _registrationAttempted = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AssignThemeSong] Error during plugin constructor initialization");
            }
        });
    }

    /// <summary>
    /// Attempts to register transformation with File Transformation plugin using reflection.
    /// </summary>
    /// <returns>True if registration succeeded, false otherwise.</returns>
    private async Task<bool> TryRegisterFileTransformation()
    {
        try
        {
            _logger.LogInformation("[AssignThemeSong] Attempting to register transformation with File Transformation plugin...");

            // Try multiple times with increasing delays
            for (int attempt = 1; attempt <= 10; attempt++)
            {
                // Find the FileTransformation assembly using reflection
                Assembly? fileTransformationAssembly = 
                    AssemblyLoadContext.All
                        .SelectMany(x => x.Assemblies)
                        .FirstOrDefault(x => x.FullName?.Contains("FileTransformation", StringComparison.OrdinalIgnoreCase) ?? false);

                if (fileTransformationAssembly == null)
                {
                    _logger.LogWarning("[AssignThemeSong] File Transformation plugin assembly not found in loaded assemblies (attempt {Attempt}/10)", attempt);
                    
                    if (attempt < 10)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(attempt * 2)); // Exponential backoff
                        continue;
                    }
                    return false;
                }

                _logger.LogInformation("[AssignThemeSong] Found File Transformation assembly: {AssemblyName}", fileTransformationAssembly.FullName);

                // Get the PluginInterface type
                Type? pluginInterfaceType = fileTransformationAssembly.GetType("Jellyfin.Plugin.FileTransformation.PluginInterface");
                if (pluginInterfaceType == null)
                {
                    _logger.LogWarning("[AssignThemeSong] Could not find PluginInterface type in File Transformation assembly (attempt {Attempt}/10)", attempt);
                    
                    if (attempt < 10)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
                        continue;
                    }
                    return false;
                }

                // Get the RegisterTransformation method
                MethodInfo? registerMethod = pluginInterfaceType.GetMethod("RegisterTransformation");
                if (registerMethod == null)
                {
                    _logger.LogWarning("[AssignThemeSong] Could not find RegisterTransformation method in File Transformation plugin (attempt {Attempt}/10)", attempt);
                    
                    if (attempt < 10)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
                        continue;
                    }
                    return false;
                }

                // Create transformation payload as JObject (File Transformation expects Newtonsoft.Json.Linq.JObject)
                var transformationPayload = new JObject
                {
                    ["id"] = "6a7b8c9d-0e1f-2345-6789-abcdef012345", // Use our plugin GUID
                    ["fileNamePattern"] = "index.html", // Plain filename, not regex
                    ["callbackAssembly"] = typeof(Helpers.TransformationPatches).Assembly.FullName,
                    ["callbackClass"] = typeof(Helpers.TransformationPatches).FullName,
                    ["callbackMethod"] = "IndexHtml"
                };

                // Invoke the registration method
                registerMethod.Invoke(null, new object[] { transformationPayload });

                _logger.LogInformation("[AssignThemeSong] Successfully registered transformation with File Transformation plugin on attempt {Attempt}", attempt);
                return true;
            }

            _logger.LogError("[AssignThemeSong] Failed to register with File Transformation plugin after 10 attempts");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[AssignThemeSong] Failed to register with File Transformation plugin");
            return false;
        }
    }

    /// <summary>
    /// Gets whether registration with File Transformation plugin has been attempted.
    /// </summary>
    public static bool RegistrationAttempted => _registrationAttempted;

    /// <summary>
    /// Gets whether registration with File Transformation plugin was successful.
    /// </summary>
    public static bool RegistrationSuccessful => _registrationSuccessful;

    /// <inheritdoc />
    public override string Name => "Assign Theme Song";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("6a7b8c9d-0e1f-2345-6789-abcdef012345");

    /// <summary>
    /// Gets the plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <inheritdoc />
    public override string Description => "Download theme songs from YouTube or upload custom MP3s for your media library";


    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace)
            }
        ];
    }
}
