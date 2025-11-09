using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
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
    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    /// <param name="logger">Instance of the <see cref="ILogger"/> interface.</param>
    /// <param name="configurationManager">Instance of the <see cref="IServerConfigurationManager"/> interface.</param>
    public Plugin(
        IApplicationPaths applicationPaths,
        IXmlSerializer xmlSerializer,
        ILogger<Plugin> logger,
        IServerConfigurationManager configurationManager)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;

        try
        {
            // Try to register with File Transformation plugin
            if (!TryRegisterFileTransformation(configurationManager, logger))
            {
                logger.LogWarning("File Transformation plugin not available, UI features may not work");
            }
            else
            {
                logger.LogInformation("Successfully registered with File Transformation plugin");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during plugin initialization: {Message}", ex.Message);
        }
    }

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

    /// <summary>
    /// Attempts to register transformation with File Transformation plugin using reflection.
    /// </summary>
    /// <param name="configurationManager">Configuration manager for network settings.</param>
    /// <param name="logger">Logger instance for debug output.</param>
    /// <returns>True if registration succeeded, false otherwise.</returns>
    private bool TryRegisterFileTransformation(IServerConfigurationManager configurationManager, ILogger<Plugin> logger)
    {
        try
        {
            logger.LogDebug("Attempting to register transformation with File Transformation plugin...");

            // Find the FileTransformation assembly using reflection
            Assembly? fileTransformationAssembly =
                AssemblyLoadContext.All
                    .SelectMany(x => x.Assemblies)
                    .FirstOrDefault(x => x.FullName?.Contains("FileTransformation", StringComparison.OrdinalIgnoreCase) ?? false);

            if (fileTransformationAssembly == null)
            {
                logger.LogWarning("File Transformation plugin assembly not found in loaded assemblies");
                return false;
            }

            logger.LogDebug("Found File Transformation assembly: {AssemblyName}", fileTransformationAssembly.FullName);

            // Get the PluginInterface type
            Type? pluginInterfaceType = fileTransformationAssembly.GetType("Jellyfin.Plugin.FileTransformation.PluginInterface");
            if (pluginInterfaceType == null)
            {
                logger.LogWarning("Could not find PluginInterface type in File Transformation assembly");
                return false;
            }

            // Get the RegisterTransformation method
            MethodInfo? registerMethod = pluginInterfaceType.GetMethod("RegisterTransformation");
            if (registerMethod == null)
            {
                logger.LogWarning("Could not find RegisterTransformation method in File Transformation plugin");
                return false;
            }

            // Get base path for script URL
            var basePath = GetBasePathFromConfiguration(configurationManager, logger);

            // Create transformation payload as JObject (File Transformation expects Newtonsoft.Json.Linq.JObject)
            // IMPORTANT: Use plain filename "index.html", not regex pattern
            var transformationPayload = new JObject
            {
                ["id"] = Id.ToString(), // Use plugin GUID as string
                ["fileNamePattern"] = "index.html", // Plain filename, not regex
                ["callbackAssembly"] = GetType().Assembly.FullName,
                ["callbackClass"] = GetType().FullName,
                ["callbackMethod"] = nameof(TransformIndexHtmlCallback)
            };

            // Store basePath for callback use
            _transformBasePath = basePath;

            // Invoke the registration method
            registerMethod.Invoke(null, new object[] { transformationPayload });

            logger.LogInformation("Successfully registered transformation with File Transformation plugin");

            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to register with File Transformation plugin: {Message}", ex.Message);
            return false;
        }
    }

    // Store these for the callback method
    private static string _transformBasePath = string.Empty;

    /// <summary>
    /// Callback method invoked by File Transformation plugin.
    /// This method signature must match what the File Transformation plugin expects.
    /// </summary>
    /// <param name="payload">Payload containing the file contents to transform.</param>
    /// <returns>Transformed HTML content as string.</returns>
    public static string TransformIndexHtmlCallback(Jellyfin.Plugin.AssignThemeSong.Models.PatchRequestPayload payload)
    {
        try
        {
            var content = payload?.Contents ?? string.Empty;

            if (string.IsNullOrEmpty(content))
            {
                return content;
            }

            // Use the transformation helper
            return Jellyfin.Plugin.AssignThemeSong.Helpers.TransformationPatches.IndexHtml(payload);
        }
        catch
        {
            // If transformation fails, return original content
            return payload?.Contents ?? string.Empty;
        }
    }

    /// <summary>
    /// Retrieves the base path from Jellyfin's network configuration.
    /// </summary>
    /// <param name="configurationManager">Configuration manager instance.</param>
    /// <param name="logger">Logger instance for debug output.</param>
    /// <returns>The configured base path or empty string if unavailable.</returns>
    private string GetBasePathFromConfiguration(IServerConfigurationManager configurationManager, ILogger<Plugin> logger)
    {
        try
        {
            logger.LogDebug("Retrieving base path from network configuration...");

            var networkConfig = configurationManager.GetConfiguration("network");
            var configType = networkConfig.GetType();
            var basePathField = configType.GetProperty("BaseUrl");
            var confBasePath = basePathField?.GetValue(networkConfig)?.ToString()?.Trim('/');

            var basePath = string.IsNullOrEmpty(confBasePath) ? "" : "/" + confBasePath;

            logger.LogDebug("Retrieved base path: '{BasePath}'", basePath);
            return basePath;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to get base path from network configuration, using default '/': {Message}", ex.Message);
            return "";
        }
    }

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
