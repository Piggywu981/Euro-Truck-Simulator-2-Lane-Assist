using ETS2LA.Networking.Settings;
using ETS2LA.Networking.Plugins;

namespace ETS2LA.Networking;

public class NetworkingClient
{
    private static readonly Lazy<NetworkingClient> _instance = new(() => new NetworkingClient());
    public static NetworkingClient Current => _instance.Value;

    private List<ApiServer> apiServers = new()
    {
        new ApiServer { Name = "Global", BaseUrl = "https://api.ets2la.com/api/v1" },
        new ApiServer { Name = "China", BaseUrl = "https://api.ets2la.cn/api/v1" }
    };

    public PluginApiClient Plugins { get; } = new();

    public NetworkingClient()
    {
        // TODO: Check if we're in China and use the China server by default if so.
        if (NetworkingSettings.Current.CurrentApiServer == null)
        {
            NetworkingSettings.Current.CurrentApiServer = apiServers[0];
            NetworkingSettings.Current.Save();
        }

        Plugins.FetchAvailablePluginsAsync();
    }
}