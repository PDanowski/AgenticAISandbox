using SdlcAgentApp.Core;
using SdlcAgentApp.Services;

namespace SdlcAgentApp;

public static class SdlcInteractiveApp
{
    public static async Task<int> RunAsync()
    {
        try
        {
            var ui = new ConsoleUi();
            Console.WriteLine();
            Console.WriteLine("SDLC Agent App (.NET interactive)");
            Console.WriteLine();

            var resolver = new PathResolver();
            var repoRoot = resolver.FindRepoRoot();
            var appConfig = AppConfig.LoadFromFile(Path.Combine(repoRoot, "sdlc-agent-app-dotnet", "appsettings.json"));

            var packKey = ui.AskChoice("Pack", appConfig.Packs.Keys.Order().ToList(), "github");
            var profile = ui.AskChoice("Profile", ["codex", "copilot"], "codex");
            var provider = ui.AskChoice("Provider", appConfig.Providers.Keys.Order().ToList(), "openai");
            var preset = ui.AskChoice("Model preset", ["quality", "balanced", "fast"], "balanced");

            Console.Write("Explicit model (optional, press Enter to use preset): ");
            var explicitModel = (Console.ReadLine() ?? string.Empty).Trim();
            var model = string.IsNullOrWhiteSpace(explicitModel)
                ? appConfig.ModelPresets[provider][preset]
                : explicitModel;

            var packRoot = Path.Combine(repoRoot, appConfig.Packs[packKey]);
            var outDir = Path.Combine(packRoot, "automations", profile, "outbox");
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");

            var modelClient = BuildClient(ui, provider, appConfig);
            var promptLoader = new PromptLoader();
            var prompts = promptLoader.BuildPrompts(repoRoot, packRoot, profile);
            var feature = ui.AskMultiLine("Feature request");

            var writer = new OutputWriter(outDir);
            var workflow = new WorkflowRunner(ui, modelClient, prompts, writer, model, feature, profile, timestamp);
            var files = await workflow.RunAsync(packKey, provider);

            Console.WriteLine();
            Console.WriteLine("Done. Generated output paths:");
            foreach (var file in files)
            {
                Console.WriteLine($"- {file}");
            }

            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Cancelled.");
            return 130;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR: {ex.Message}");
            return 1;
        }
    }

    private static IModelClient BuildClient(ConsoleUi ui, string provider, AppConfig appConfig)
    {
        var providerCfg = appConfig.Providers[provider];
        if (provider == "openai")
        {
            var token = Environment.GetEnvironmentVariable(providerCfg.TokenEnv) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException($"{providerCfg.TokenEnv} is not set.");
            }
            Console.Write("OpenAI base URL (optional): ");
            var baseUrl = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = providerCfg.BaseUrl;
            }
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(providerCfg.TimeoutSec) };
            return new OpenAiClient(client, token, baseUrl);
        }

        var githubToken = Environment.GetEnvironmentVariable(providerCfg.TokenEnv) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(githubToken))
        {
            throw new InvalidOperationException($"{providerCfg.TokenEnv} is not set.");
        }
        Console.Write("GitHub Models base URL (optional): ");
        var ghBaseUrl = (Console.ReadLine() ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(ghBaseUrl))
        {
            ghBaseUrl = providerCfg.BaseUrl;
        }
        Console.Write("GitHub org (optional): ");
        var githubOrg = (Console.ReadLine() ?? string.Empty).Trim();
        var ghClient = new HttpClient { Timeout = TimeSpan.FromSeconds(providerCfg.TimeoutSec) };
        return new GitHubModelsClient(ghClient, githubToken, ghBaseUrl, githubOrg, providerCfg.GitHubApiVersion);
    }
}
