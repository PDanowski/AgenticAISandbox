namespace SdlcAgentApp.Core;

public static class AppConstants
{
    public static readonly IReadOnlyDictionary<string, string> Packs = new Dictionary<string, string>
    {
        ["github"] = "GitHub-and-dotnet-sdlc-agents",
        ["azure"] = "Azure-and-dotnet-sdlc-agents"
    };

    public static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ModelPresets =
        new Dictionary<string, IReadOnlyDictionary<string, string>>
        {
            ["openai"] = new Dictionary<string, string>
            {
                ["quality"] = "gpt-5.4",
                ["balanced"] = "gpt-5.4-mini",
                ["fast"] = "gpt-4.1-mini"
            },
            ["github-models"] = new Dictionary<string, string>
            {
                ["quality"] = "openai/gpt-4.1",
                ["balanced"] = "openai/gpt-4.1",
                ["fast"] = "openai/gpt-4.1"
            }
        };
}

