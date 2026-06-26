using ETS2LA.Networking.Users;
using ETS2LA.Networking.Settings;
using ETS2LA.Backend;
using ETS2LA.Backend.Plugins;
using ETS2LA.Notifications;
using ETS2LA.Logging;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Reflection;

namespace ETS2LA.Networking.Plugins;

public class PluginApiClient
{
    public List<NetworkPlugin> AvailablePlugins { get; private set; } = new List<NetworkPlugin>();

    JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private void Log(string message, NotificationLevel level = NotificationLevel.Information)
    {
        switch (level)
        {
            case NotificationLevel.Information:
                Logger.Info(message);
                break;
            case NotificationLevel.Warning:
                Logger.Warn(message);
                break;
            case NotificationLevel.Danger:
                Logger.Error(message);
                break;
            case NotificationLevel.Success:
                Logger.Success(message);
                break;
            default:
                Logger.Info(message);
                break;
        }

        NotificationHandler.Current.SendNotification(new Notification
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Plugin Installer",
            Content = message,
            Level = level
        });
    }

    public async Task FetchAvailablePluginsAsync()
    {
        var apiServer = NetworkingSettings.Current.CurrentApiServer;
        if (apiServer == null)
        {
            throw new InvalidOperationException("CurrentApiServer is not set.");
        }

        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"{apiServer.Value.BaseUrl}/plugins");
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        AvailablePlugins = JsonSerializer.Deserialize<List<NetworkPlugin>>(jsonResponse, jsonOptions) ?? new List<NetworkPlugin>();

        Log($"Fetched {AvailablePlugins.Count} plugins from {apiServer.Value.BaseUrl}");
    }

    public bool PluginHasUpdateAvailable(string pluginId)
    {
        var plugin = AvailablePlugins.FirstOrDefault(p => p.Id == pluginId);
        if (plugin == null)
        {
            Log($"Plugin with ID {pluginId} not found in available plugins.", NotificationLevel.Warning);
            return false;
        }

        InstalledPlugin? installedPlugin = InstalledPluginManifest.Current.InstalledPlugins.FirstOrDefault(p => p.Id == pluginId);
        if (installedPlugin == null)
        {
            Log($"Plugin with ID {pluginId} is not installed.", NotificationLevel.Warning);
            return false;
        }

        var appVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "0.0.0";
        OperatingSystem currentOS = Environment.OSVersion.Platform != PlatformID.Unix ? OperatingSystem.Windows : OperatingSystem.Linux;
        var latestVersion = plugin.GetLatestCompatibleVersion(appVersion, currentOS);

        if (latestVersion == null)
        {
            Log($"No valid versions found for plugin with ID {pluginId}.", NotificationLevel.Warning);
            return false;
        }

        return new Version(latestVersion.Version) > new Version(installedPlugin.Value.Version);
    }

    public bool InstallPlugin(string pluginId)
    {
        var plugin = AvailablePlugins.FirstOrDefault(p => p.Id == pluginId);
        if (plugin == null)
        {
            Log($"Plugin with ID {pluginId} not found.", NotificationLevel.Warning);
            return false;   
        }

        var appVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "0.0.0";
        OperatingSystem currentOS = Environment.OSVersion.Platform != PlatformID.Unix ? OperatingSystem.Windows : OperatingSystem.Linux;
        var latestVersion = plugin.GetLatestCompatibleVersion(appVersion, currentOS);
        if (latestVersion == null)
        {
            Log($"No valid versions found for plugin with ID {pluginId}.", NotificationLevel.Warning);
            return false;
        }

        // Downloading is done from whatever region the user is in.
        Region currentRegion = NetworkingSettings.Current.CurrentApiServer?.Name == "China" ? Region.China : Region.Global;
        string downloadUrl = latestVersion.DownloadUrl.FirstOrDefault(d => d.Key == currentRegion).Value;
        if (string.IsNullOrEmpty(downloadUrl))
        {
            Log($"No download URL found for plugin with ID {pluginId} in region {currentRegion}.", NotificationLevel.Warning);
            return false;
        }

        if (latestVersion.Dependencies.Count > 0)
        {
            bool allDependenciesInstalled = true;
            foreach (var dependencyId in latestVersion.Dependencies)
            {
                if (!InstalledPluginManifest.Current.InstalledPlugins.Any(p => p.Id == dependencyId))
                {
                    Log($"Dependency {dependencyId} for plugin {pluginId} is not installed.", NotificationLevel.Warning);
                    allDependenciesInstalled = false;
                    break;
                }
            }
            if (!allDependenciesInstalled)
            {
                Log($"Not all dependencies for plugin {pluginId} are installed.", NotificationLevel.Warning);
                return false;
            }
        }

        string tempFilePath = Path.GetTempFileName();
        using (var httpClient = new HttpClient())
        {
            var downloadTask = httpClient.GetAsync(downloadUrl);
            downloadTask.Wait();
            var downloadResponse = downloadTask.Result;
            if (!downloadResponse.IsSuccessStatusCode)
            {
                Log($"Failed to download plugin with ID {pluginId} from {downloadUrl}. Status code: {downloadResponse.StatusCode}", NotificationLevel.Warning);
                return false;
            }
            using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var copyTask = downloadResponse.Content.CopyToAsync(fs);
                copyTask.Wait();
            }
        }

        // And the output path is determined by the PluginBackend's PluginRootPath.
        // On windows that's set to none so it's in /Plugins or /Libraries.
        string location = PluginBackend.Current.PluginHandler?.PluginRootPath ?? string.Empty;

        string type = plugin.Tags.Contains(NetworkPluginTags.Plugin) ? "Plugin" : "Library";
        string folder = type == "Plugin" ? "Plugins" : "Libraries";
        string outputPath = Path.Combine(location, folder, plugin.Id);
        Directory.CreateDirectory(outputPath);

        System.IO.Compression.ZipFile.ExtractToDirectory(tempFilePath, outputPath, true);
        File.Delete(tempFilePath);

        // Finally we just have to register this plugin in the InstalledPluginManifest.
        InstalledPluginManifest.Current.InstalledPlugins.Add(new InstalledPlugin
        {
            Id = plugin.Id,
            Version = latestVersion.Version,
            DllPath = Path.Combine(outputPath, latestVersion.DllPath),
            Type = type == "Plugin" ? PluginType.Plugin : PluginType.Library
        });
        InstalledPluginManifest.Current.Save();

        Log($"Successfully installed plugin {plugin.Name} ({plugin.Id}, {latestVersion.Version})", NotificationLevel.Success);
        return true;
    }

    public bool UpdatePlugin(string pluginId)
    {
        if (!PluginHasUpdateAvailable(pluginId))
        {
            Log($"No update available for plugin with ID {pluginId}.", NotificationLevel.Information);
            return false;
        }

        // Uninstall the current version first.
        if (!UninstallPlugin(pluginId))
        {
            Log($"Failed to uninstall current version of plugin with ID {pluginId}.", NotificationLevel.Warning);
            return false;
        }

        // Then install the latest version.
        if (!InstallPlugin(pluginId))
        {
            Log($"Failed to install latest version of plugin with ID {pluginId}.", NotificationLevel.Warning);
            return false;
        }

        Log($"Successfully updated plugin with ID {pluginId}.", NotificationLevel.Success);
        return true;
    }

    public bool UninstallPlugin(string pluginId)
    {
        InstalledPlugin? installedPlugin = InstalledPluginManifest.Current.InstalledPlugins.FirstOrDefault(p => p.Id == pluginId);
        if (installedPlugin == null)
        {
            Log($"Installed plugin with ID {pluginId} not found.", NotificationLevel.Warning);
            return false;
        }

        // Remove the plugin's files from the filesystem.
        string pluginPath = Path.Combine(
            PluginBackend.Current.PluginHandler?.PluginRootPath ?? string.Empty, 
            installedPlugin.Value.Type == PluginType.Plugin ? "Plugins" 
                                                            : "Libraries", 
            installedPlugin.Value.Id
        );

        if (Directory.Exists(pluginPath)) Directory.Delete(pluginPath, true);
        else
        {
            Log($"Apparent plugin directory {pluginPath} does not exist.", NotificationLevel.Warning);
            return false;
        }

        // And then we remove it from the InstalledPluginManifest.
        InstalledPluginManifest.Current.InstalledPlugins.Remove(installedPlugin.Value);
        InstalledPluginManifest.Current.Save();

        Log($"Successfully uninstalled plugin with ID {pluginId}", NotificationLevel.Success);
        return true;
    }
}