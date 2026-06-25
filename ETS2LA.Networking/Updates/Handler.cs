using ETS2LA.Logging;
using ETS2LA.Settings;
using Velopack;
using Velopack.Sources;

namespace ETS2LA.Networking.Updates;

// NOTE FOR FUTURE DEV:
// ETS2LA itself is only "officially" hosted on GitHub, however if you want to support a 3rd party host
// (like those in China), then take a look at Velopack's documentation here for custom sources. You can
// add your sources in a file in this folder.
// https://docs.velopack.io/integrating/overview#configuring-updates
public class Updater
{
    private static readonly Lazy<Updater> _instance = new(() => new Updater());
    public static Updater Current => _instance.Value;

    public UpdateManager UpdateManager;
    private UpdaterSettings settings = new();
    private SettingsHandler settingsHandler;
    private UpdateInfo? latestUpdateInfo;
    
    public List<UpdaterSource> AvailableSources => new()
    {
        new UpdaterSource(new GithubSource("https://github.com/ETS2LA/Euro-Truck-Simulator-2-Lane-Assist", null, true), "GitHub")
    };

    public Updater()
    {
        settingsHandler = new SettingsHandler();
        settings = settingsHandler.Load<UpdaterSettings>("Updater.json");
        UpdateManager = new UpdateManager(GetSelectedSource().source, new UpdateOptions
        {
            // ExplicitChannel = "stable" // or beta or etc...
        });
    }

    public UpdateInfo? CheckForUpdates()
    {
        if (latestUpdateInfo != null)
        {
            Logger.Info("Update check skipped, using already cached result.");
            return latestUpdateInfo;
        }

        try
        {
            var updateInfo = UpdateManager.CheckForUpdates();
            if (updateInfo != null) { Logger.Info($"Update available: {updateInfo.TargetFullRelease.Version}"); }
            else { Logger.Info("No updates available."); }
            latestUpdateInfo = updateInfo;
            return updateInfo;
        }
        catch (Exception ex)
        {
            Logger.Error($"Error while checking for updates: {ex.Message}");
            return null;
        }
    }

    public void DownloadUpdates(UpdateInfo updateInfo, Action<int>? progressCallback = null)
    {
        try
        {
            UpdateManager.DownloadUpdates(updateInfo, progressCallback);
        }
        catch (Exception ex)
        {
            Logger.Error($"Error while downloading update: {ex.Message}");
        }
    }

    public bool ApplyUpdatesAndRestart(UpdateInfo updateInfo)
    {
        try
        {
            UpdateManager.ApplyUpdatesAndRestart(updateInfo);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Error while applying update: {ex.Message}");
        }
        return false;
    }

    public void ChangeSource(string sourceName)
    {
        var source = AvailableSources.FirstOrDefault(s => s.sourceName == sourceName);
        if (source == null)
        {
            Logger.Error($"Tried to change update source to '{sourceName}', but it was not found among available sources.");
            return;
        }
        settings.SelectedSource = sourceName;
        settingsHandler.Save("Updater.json", settings);
        UpdateManager = new UpdateManager(source.source);
        Logger.Info($"Changed update source to '{sourceName}'.");
    }

    public UpdaterSource GetSelectedSource()
    {
        var source = AvailableSources.FirstOrDefault(s => s.sourceName == settings.SelectedSource);
        if (source == null)
        {
            Logger.Warn($"Selected update source '{settings.SelectedSource}' not found, defaulting to first available source.");
            source = AvailableSources[0];
            Logger.Warn($"> '{source.sourceName}'.");
        }
        return source;
    }
}