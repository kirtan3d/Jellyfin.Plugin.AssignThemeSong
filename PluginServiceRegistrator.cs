using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Jellyfin.Plugin.xThemeSong.Services;

namespace Jellyfin.Plugin.xThemeSong
{
    /// <summary>
    /// Register xThemeSong services.
    /// </summary>
    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        /// <inheritdoc />
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            serviceCollection.AddSingleton<ThemeDownloadService>();
            // Note: StartupService removed - registration now happens in Plugin constructor
        }
    }
}
