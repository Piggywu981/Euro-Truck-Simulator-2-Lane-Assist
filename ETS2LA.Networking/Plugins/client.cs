using ETS2LA.Networking.Users;
using ETS2LA.Networking.Settings;
using ETS2LA.Logging;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace ETS2LA.Networking.Plugins;

public class PluginApiClient
{
    public List<NetworkPlugin> AvailablePlugins { get; private set; } = new List<NetworkPlugin>();

    JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

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
    }
}