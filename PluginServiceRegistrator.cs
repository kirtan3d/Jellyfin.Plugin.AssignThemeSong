using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.AssignThemeSong
{
    /// <summary>
    /// Register Assign Theme Song services.
    /// </summary>
    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        /// <inheritdoc />
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            serviceCollection.AddSingleton<Services.ThemeDownloadService>();
            serviceCollection.AddSingleton<Services.StartupTask>();
        }
    }
}
