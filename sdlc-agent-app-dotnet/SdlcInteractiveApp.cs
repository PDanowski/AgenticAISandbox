using SdlcAgentApp.Core;
using SdlcAgentApp.Services;

namespace SdlcAgentApp;

public static class SdlcInteractiveApp
{
    public static async Task<int> RunAsync()
    {
        try
        {
            IUserInterface ui = new ConsoleUi();
            ui.WriteLine(string.Empty);
            ui.WriteLine("SDLC Agent App (.NET interactive)");
            ui.WriteLine(string.Empty);

            var resolver = new PathResolver();
            var repoRoot = resolver.FindRepoRoot();
            var appConfig = AppConfig.LoadFromFile(Path.Combine(repoRoot, "sdlc-agent-app-dotnet", "appsettings.json"));

            var packKey = ui.AskChoice("Pack", appConfig.Packs.Keys.Order().ToList(), "github");
            var profile = ui.AskChoice("Profile", new[] { "codex", "copilot" }, "codex");
            var provider = ui.AskChoice("Provider", appConfig.Providers.Keys.Order().ToList(), "openai");
            var preset = ui.AskChoice("Model preset", new[] { "quality", "balanced", "fast" }, "balanced");
            var model = AskModel(ui, appConfig, provider, preset);

            var packRoot = Path.Combine(repoRoot, appConfig.Packs[packKey]);
            var outDir = Path.Combine(packRoot, "automations", profile, "outbox");
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");

            var agentFactory = BuildAgentFactory(ui, provider, appConfig, model);
            var promptLoader = new PromptLoader();
            var prompts = promptLoader.BuildPrompts(repoRoot, packRoot, profile);
            var feature = ui.AskMultiLine("Feature request");

            var writer = new OutputWriter(outDir);
            var workflow = new WorkflowRunner(ui, agentFactory, prompts, writer, feature, profile, timestamp, model);
            var files = await workflow.RunAsync(packKey, provider);

            ui.WriteLine(string.Empty);
            ui.WriteLine("Done. Generated output paths:");
            foreach (var file in files)
            {
                ui.WriteLine($"- {file}");
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

    private static string AskModel(IUserInterface ui, AppConfig appConfig, string provider, string preset)
    {
        ui.WriteLine("Explicit model (optional, press Enter to use preset):");
        var explicitModel = (Console.ReadLine() ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(explicitModel)
            ? appConfig.ModelPresets[provider][preset]
            : explicitModel;
    }

    private static IRoleAgentFactory BuildAgentFactory(IUserInterface ui, string provider, AppConfig appConfig, string model)
    {
        var providerCfg = appConfig.Providers[provider];
        if (provider == "openai")
        {
            var token = Environment.GetEnvironmentVariable(providerCfg.TokenEnv) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException($"{providerCfg.TokenEnv} is not set.");
            }

            var baseUrl = ui.AskOptional("OpenAI base URL (optional):", providerCfg.BaseUrl);
            return new OpenAiRoleAgentFactory(token, baseUrl, model);
        }

        var githubToken = Environment.GetEnvironmentVariable(providerCfg.TokenEnv) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(githubToken))
        {
            throw new InvalidOperationException($"{providerCfg.TokenEnv} is not set.");
        }

        var ghBaseUrl = ui.AskOptional("GitHub Models base URL (optional):", providerCfg.BaseUrl);
        var githubOrg = ui.AskOptional("GitHub org (optional):", string.Empty);
        var ghClient = new HttpClient { Timeout = TimeSpan.FromSeconds(providerCfg.TimeoutSec) };
        return new GitHubRoleAgentFactory(ghClient, githubToken, ghBaseUrl, githubOrg, providerCfg.GitHubApiVersion, model);
    }
}
