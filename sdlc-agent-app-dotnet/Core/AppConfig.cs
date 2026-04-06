using System.Text.Json;

namespace SdlcAgentApp.Core;

public sealed class AppConfig
{
    public Dictionary<string, string> Packs { get; init; } = new();
    public Dictionary<string, ProviderConfig> Providers { get; init; } = new();
    public Dictionary<string, Dictionary<string, string>> ModelPresets { get; init; } = new();

    public static AppConfig LoadFromFile(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var cfg = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        if (cfg is null)
        {
            throw new InvalidOperationException("Failed to load appsettings.json");
        }
        return cfg;
    }
}

public sealed class ProviderConfig
{
    public string TokenEnv { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
    public string GitHubApiVersion { get; init; } = "2026-03-10";
    public int TimeoutSec { get; init; } = 240;
}

