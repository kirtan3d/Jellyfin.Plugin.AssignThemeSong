#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.xThemeSong;

/// <summary>
/// The main plugin class.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    private readonly IApplicationPaths _appPaths;
    private readonly ILogger<Plugin> _logger;
    private static string _transformBasePath = string.Empty;
    private static string _transformVersion = "Unknown";

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
        _appPaths = applicationPaths;
        _logger = logger!; // Null-forgiving operator - logger is required by DI
        
        try
        {
            _logger.LogInformation("xThemeSong: Plugin constructor started - WITH File Transformation registration");
            _logger.LogInformation($"xThemeSong v{Version} initialized");

            // Try File Transformation plugin first, fall back to direct injection
            if (!TryRegisterFileTransformation(configurationManager))
            {
                _logger.LogWarning("xThemeSong: File Transformation plugin not available, attempting direct script injection...");
                InjectScriptDirectly(configurationManager, applicationPaths);
            }
            else
            {
                _logger.LogInformation("xThemeSong: Successfully registered with File Transformation plugin");
            }
        }
        catch (Exception ex)
        {
            // Log error but DON'T re-throw - allow plugin to load even if initialization fails
            _logger?.LogError(ex, "xThemeSong: Error in constructor: {Message}", ex.Message);
        }
    }

    private void InjectScriptDirectly(IServerConfigurationManager configurationManager, IApplicationPaths applicationPaths)
    {
        try
        {
            var indexPath = Path.Combine(applicationPaths.WebPath, "index.html");
            if (!File.Exists(indexPath))
            {
                _logger.LogError("xThemeSong: Could not find index.html at {Path} for direct injection", indexPath);
                return;
            }

            var content = File.ReadAllText(indexPath);

            var basePath = GetBasePathFromConfiguration(configurationManager);
            var version = Version.ToString();

            string scriptElement = string.Format(
                CultureInfo.InvariantCulture,
                "<script plugin=\"xThemeSong\" version=\"{1}\" src=\"{0}/xThemeSong/plugin\" defer></script>",
                basePath,
                version);

            if (content.Contains(scriptElement, StringComparison.Ordinal))
            {
                _logger.LogInformation("xThemeSong: Script already injected directly, skipping.");
                return;
            }

            string scriptReplace = "<script plugin=\"xThemeSong\".*?></script>";
            content = Regex.Replace(content, scriptReplace, string.Empty);

            int bodyClosing = content.LastIndexOf("</body>", StringComparison.Ordinal);
            if (bodyClosing == -1)
            {
                _logger.LogWarning("xThemeSong: No </body> tag found in index.html for direct injection");
                return;
            }
            
            content = content.Insert(bodyClosing, scriptElement);
            File.WriteAllText(indexPath, content);
            _logger.LogInformation("xThemeSong: Successfully performed direct script injection into index.html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "xThemeSong: Failed to perform direct script injection");
        }
    }
    
    /// <summary>
    /// Attempts to register transformation with File Transformation plugin using reflection.
    /// </summary>
    /// <param name="configurationManager">Configuration manager for network settings.</param>
    /// <returns>True if registration succeeded, false otherwise.</returns>
    private bool TryRegisterFileTransformation(IServerConfigurationManager configurationManager)
    {
        try
        {
            _logger.LogInformation("xThemeSong: Attempting to register transformation with File Transformation plugin...");

            // Find the FileTransformation assembly
            Assembly? fileTransformationAssembly =
                AssemblyLoadContext.All
                    .SelectMany(x => x.Assemblies)
                    .FirstOrDefault(x => x.FullName?.Contains("FileTransformation", StringComparison.OrdinalIgnoreCase) ?? false);

            if (fileTransformationAssembly == null)
            {
                _logger.LogWarning("xThemeSong: File Transformation plugin assembly not found. Web UI features will not work.");
                return false;
            }

            _logger.LogDebug("xThemeSong: Found File Transformation assembly: {AssemblyName}", fileTransformationAssembly.FullName);

            // Get the PluginInterface type
            Type? pluginInterfaceType = fileTransformationAssembly.GetType("Jellyfin.Plugin.FileTransformation.PluginInterface");
            if (pluginInterfaceType == null)
            {
                _logger.LogWarning("xThemeSong: Could not find PluginInterface type in File Transformation assembly");
                return false;
            }

            // Get the RegisterTransformation method
            MethodInfo? registerMethod = pluginInterfaceType.GetMethod("RegisterTransformation");
            if (registerMethod == null)
            {
                _logger.LogWarning("xThemeSong: Could not find RegisterTransformation method");
                return false;
            }

            // Get base path for script URL
            var basePath = GetBasePathFromConfiguration(configurationManager);
            var version = Version.ToString();

            // Store for callback use
            _transformBasePath = basePath;
            _transformVersion = version;

            // Create transformation payload - IMPORTANT: Use plain filename "index.html", not regex
            var transformationPayload = new JObject
            {
                ["id"] = "97db7543-d64d-45e1-b2e7-62729b56371f",
                ["fileNamePattern"] = "index.html",
                ["callbackAssembly"] = GetType().Assembly.FullName,
                ["callbackClass"] = GetType().FullName,
                ["callbackMethod"] = nameof(TransformIndexHtmlCallback)
            };

            // Register the transformation
            registerMethod.Invoke(null, new object[] { transformationPayload });

            _logger.LogInformation("xThemeSong: Successfully registered transformation with File Transformation plugin");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "xThemeSong: Failed to register with File Transformation plugin: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Callback method invoked by File Transformation plugin.
    /// </summary>
    /// <param name="payload">Payload containing the file contents to transform.</param>
    /// <returns>Transformed HTML content as string.</returns>
    public static string TransformIndexHtmlCallback(Jellyfin.Plugin.xThemeSong.Models.PatchRequestPayload payload)
    {
        try
        {
            Instance?._logger?.LogInformation("xThemeSong: TransformIndexHtmlCallback invoked!");
            
            var content = payload?.Contents ?? string.Empty;

            if (string.IsNullOrEmpty(content))
            {
                Instance?._logger?.LogWarning("xThemeSong: Callback received empty content");
                return content;
            }

            Instance?._logger?.LogDebug("xThemeSong: Content length: {Length}, BasePath: '{BasePath}', Version: '{Version}'", 
                content.Length, _transformBasePath, _transformVersion);

            // Transform the content
            string scriptReplace = "<script plugin=\"xThemeSong\".*?></script>";
            string scriptElement = string.Format(
                CultureInfo.InvariantCulture,
                "<script plugin=\"xThemeSong\" version=\"{1}\" src=\"{0}/xThemeSong/plugin\" defer></script>",
                _transformBasePath,
                _transformVersion);

            Instance?._logger?.LogDebug("xThemeSong: Script element to inject: {Script}", scriptElement);

            // Check if script is already injected
            if (content.Contains(scriptElement, StringComparison.Ordinal))
            {
                Instance?._logger?.LogInformation("xThemeSong: Script already injected, skipping");
                return content;
            }

            // Remove old xThemeSong scripts
            content = Regex.Replace(content, scriptReplace, string.Empty);

            // Find closing body tag
            int bodyClosing = content.LastIndexOf("</body>", StringComparison.Ordinal);
            if (bodyClosing == -1)
            {
                Instance?._logger?.LogWarning("xThemeSong: No </body> tag found in content");
                return content;
            }

            // Insert script before closing body tag
            content = content.Insert(bodyClosing, scriptElement);

            Instance?._logger?.LogInformation("xThemeSong: Successfully injected script tag into index.html");

            return content;
        }
        catch (Exception ex)
        {
            Instance?._logger?.LogError(ex, "xThemeSong: Error in TransformIndexHtmlCallback: {Message}", ex.Message);
            // If transformation fails, return original content
            return payload?.Contents ?? string.Empty;
        }
    }

    /// <summary>
    /// Retrieves the base path from Jellyfin's network configuration.
    /// </summary>
    /// <param name="configurationManager">Configuration manager instance.</param>
    /// <returns>The configured base path or empty string if unavailable.</returns>
    private string GetBasePathFromConfiguration(IServerConfigurationManager configurationManager)
    {
        try
        {
            var networkConfig = configurationManager.GetConfiguration("network");
            var configType = networkConfig.GetType();
            var basePathField = configType.GetProperty("BaseUrl");
            var confBasePath = basePathField?.GetValue(networkConfig)?.ToString()?.Trim('/');

            var basePath = string.IsNullOrEmpty(confBasePath) ? "" : "/" + confBasePath;

            _logger.LogDebug("xThemeSong: Retrieved base path: '{BasePath}'", basePath);
            return basePath;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "xThemeSong: Unable to get base path from network configuration, using default");
            return "";
        }
    }

    /// <inheritdoc />
    public override string Name => "xThemeSong";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("97db7543-d64d-45e1-b2e7-62729b56371f");

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
            },
            new PluginPageInfo
            {
                Name = "xThemeSong Preferences",
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.userPreferences.html", GetType().Namespace),
                DisplayName = "xThemeSong Preferences",
                EnableInMainMenu = true  // Shows in sidebar under pluginMenuOptions
            }
        ];
    }
}
