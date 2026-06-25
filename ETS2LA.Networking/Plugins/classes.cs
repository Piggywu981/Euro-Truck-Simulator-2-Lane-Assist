// These classes HAVE to be matching with the classes in the backend.
// Do not change them in any way, they're handled by the ETS2LA team!

namespace ETS2LA.Networking.Plugins;

[Serializable]
public enum Region
{
    Global,
    China
}

[Serializable]
public enum OperatingSystem
{
    Windows,
    Linux,
    MacOS
}

[Serializable]
public enum NetworkPluginTags
{
    Plugin,
    Library,
    AIAssisted,
    OpenSource,
    ClosedSource
}

[Serializable]
public class NetworkPlugin
{
    // Plugin ID, in the format of "author.pluginname.somethingelseifneeded"
    public string Id { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty; // has to match the db username for the author
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;

    public List<NetworkPluginVersion> Versions { get; set; } = new();
    public List<NetworkPluginTags> Tags { get; set; } = new();
}

[Serializable]
public class NetworkPluginVersion
{
    public string Version { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;

    public string Changelog { get; set; } = string.Empty;
    
    public List<string> Dependencies { get; set; } = new(); // Targets other Plugin.Id values
    public List<OperatingSystem> SupportedOperatingSystems { get; set; } = new();
    public Dictionary<Region, string> DownloadUrl { get; set; } = new();
}