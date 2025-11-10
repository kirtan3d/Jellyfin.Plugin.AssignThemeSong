using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller;
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
    private readonly ILogger<Plugin> _logger;
    private readonly IServerConfigurationManager _configurationManager;
    private readonly IServerApplicationHost _serverApplicationHost;
    private static bool _registrationAttempted = false;
    private static bool _registrationSuccessful = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{Plugin}"/> interface.</param>
    /// <param name="configurationManager">Instance of the <see cref="IServerConfigurationManager"/> interface.</param>
    /// <param name="serverApplicationHost">Instance of the <see cref="IServerApplicationHost"/> interface.</param>
    public Plugin(
        IApplicationPaths applicationPaths,
        IXmlSerializer xmlSerializer,
        ILogger<Plugin> logger,
        IServerConfigurationManager configurationManager,
        IServerApplicationHost serverApplicationHost)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
        _logger = logger;
        _configurationManager = configurationManager;
        _serverApplicationHost = serverApplicationHost;

        // Try initial registration in constructor
        _ = Task.Run(async () =>
        {
            try
            {
                _logger.LogInformation("[xThemeSong] Plugin constructor initialization started...");
                
                // Give File Transformation plugin time to load (alphabetical order issue)
                await Task.Delay(TimeSpan.FromSeconds(5));
                
                if (await TryRegisterFileTransformation())
                {
                    _logger.LogInformation("[xThemeSong] Successfully registered with File Transformation plugin in constructor");
                    _registrationSuccessful = true;
                }
                else
                {
                    _logger.LogWarning("[xThemeSong] Failed to register with File Transformation plugin in constructor, will retry in scheduled task");
                }
                
                _registrationAttempted = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[xThemeSong] Error during plugin constructor initialization");
            }
        });
    }

    /// <summary>
    /// Waits for File Transformation plugin readiness with exponential backoff.
    /// </summary>
    private async Task<(bool success, int elapsedSeconds)> WaitForFileTransformationReady(HttpClient client, int maxAttempts = 60, int intervalSeconds = 5)
    {
        int totalDelaySeconds = 0;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var response = await client.GetAsync("/FileTransformation/Ready");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"[xThemeSong] File Transformation plugin ready (attempt {attempt})");
                    return (true, totalDelaySeconds);
                }

                _logger.LogDebug($"[xThemeSong] File Transformation not ready (attempt {attempt}/{maxAttempts}): {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"[xThemeSong] Readiness check failed (attempt {attempt}/{maxAttempts}): {ex.Message}");
            }

            totalDelaySeconds += intervalSeconds;
            await Task.Delay(intervalSeconds * 1000);
        }

        return (false, totalDelaySeconds);
    }

    /// <summary>
    /// Attempts to register transformation with File Transformation plugin using HTTP client.
    /// </summary>
    /// <returns>True if registration succeeded, false otherwise.</returns>
    private async Task<bool> TryRegisterFileTransformation()
    {
        try
        {
            _logger.LogInformation("[xThemeSong] Attempting to register transformation with File Transformation plugin using HTTP client...");

            string? publishedServerUrl = _serverApplicationHost.GetType()
                .GetProperty("PublishedServerUrl", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_serverApplicationHost) as string;

            using HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(publishedServerUrl ?? $"http://localhost:{_serverApplicationHost.HttpPort}");

            // Wait for File Transformation plugin to be ready
            var (success, elapsedSeconds) = await WaitForFileTransformationReady(client);
            if (!success)
            {
                _logger.LogError($"[xThemeSong] File Transformation plugin not ready after {elapsedSeconds}s. Cannot register transformation.");
                return false;
            }

            // Create transformation payload
            var transformationPayload = new JObject
            {
                ["id"] = "97db7543-d64d-45e1-b2e7-62729b56371f", // Use our plugin GUID
                ["fileNamePattern"] = "index.html", // Plain filename, not regex
                ["callbackAssembly"] = typeof(Helpers.TransformationPatches).Assembly.FullName,
                ["callbackClass"] = typeof(Helpers.TransformationPatches).FullName,
                ["callbackMethod"] = "IndexHtml"
            };

            // Register transformation via HTTP POST
            var response = await client.PostAsync("/FileTransformation/RegisterTransformation",
                new StringContent(transformationPayload.ToString(Newtonsoft.Json.Formatting.None),
                    MediaTypeHeaderValue.Parse("application/json")));

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("[xThemeSong] Successfully registered transformation with File Transformation plugin via HTTP");
                return true;
            }
            else
            {
                _logger.LogWarning($"[xThemeSong] Failed to register transformation: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[xThemeSong] Failed to register with File Transformation plugin via HTTP");
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
            }
        ];
    }
}
