using ETS2LA.Settings;

namespace ETS2LA.Backend.Plugins;

[Serializable]
public enum PluginType
{
    Plugin,
    Library
}

[Serializable]
public struct InstalledPlugin
{
    public string Id { get; set; }
    public string Version { get; set; }
    public string DllPath { get; set; }
    public List<string> Dependencies { get; set; }
    public PluginType Type { get; set; }
}

[Serializable]
public class InstalledPluginManifest
{
    [NonSerialized]
    private static readonly Lazy<InstalledPluginManifest> _instance = new(() => new InstalledPluginManifest(loadSettings: true));
    public static InstalledPluginManifest Current => _instance.Value;
    /// ---
    
    public List<InstalledPlugin> InstalledPlugins { get; set; } = new List<InstalledPlugin>();

    /// ---
    [NonSerialized]
    private SettingsHandler? _settingsHandler;

    public InstalledPluginManifest(bool loadSettings = false)
    {
        if (loadSettings)
        {
            _settingsHandler = new SettingsHandler();
            var loadedSettings = _settingsHandler.Load<InstalledPluginManifest>("InstalledPluginManifest.json");
            if (loadedSettings != null)
            {
                InstalledPlugins = loadedSettings.InstalledPlugins;
            }
            _settingsHandler.RegisterListener<InstalledPluginManifest>("InstalledPluginManifest.json", OnSettingsChanged);
        }
    }

    public InstalledPluginManifest() { }

    public void Save()
    {
        _settingsHandler?.Save<InstalledPluginManifest>("InstalledPluginManifest.json", this);
    }

    public void OnSettingsChanged(InstalledPluginManifest newSettings)
    {
        InstalledPlugins = newSettings.InstalledPlugins;
    }
}