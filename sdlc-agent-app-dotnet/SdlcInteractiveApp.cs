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

            var packKey = ui.AskChoice("Pack", ["github", "azure"], "github");
            var profile = ui.AskChoice("Profile", ["codex", "copilot"], "codex");
            var provider = ui.AskChoice("Provider", ["openai", "github-models"], "openai");
            var preset = ui.AskChoice("Model preset", ["quality", "balanced", "fast"], "balanced");

            Console.Write("Explicit model (optional, press Enter to use preset): ");
            var explicitModel = (Console.ReadLine() ?? string.Empty).Trim();
            var model = string.IsNullOrWhiteSpace(explicitModel)
                ? AppConstants.ModelPresets[provider][preset]
                : explicitModel;

            var packRoot = Path.Combine(repoRoot, AppConstants.Packs[packKey]);
            var outDir = Path.Combine(packRoot, "automations", profile, "outbox");
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");

            var modelClient = BuildClient(ui, provider);
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

    private static IModelClient BuildClient(ConsoleUi ui, string provider)
    {
        if (provider == "openai")
        {
            var token = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("OPENAI_API_KEY is not set.");
            }
            Console.Write("OpenAI base URL (optional): ");
            var baseUrl = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = "https://api.openai.com/v1";
            }
            return new OpenAiClient(new HttpClient(), token, baseUrl);
        }

        var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(githubToken))
        {
            throw new InvalidOperationException("GITHUB_TOKEN is not set.");
        }
        Console.Write("GitHub Models base URL (optional): ");
        var ghBaseUrl = (Console.ReadLine() ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(ghBaseUrl))
        {
            ghBaseUrl = "https://models.github.ai";
        }
        Console.Write("GitHub org (optional): ");
        var githubOrg = (Console.ReadLine() ?? string.Empty).Trim();
        return new GitHubModelsClient(new HttpClient(), githubToken, ghBaseUrl, githubOrg);
    }
}

