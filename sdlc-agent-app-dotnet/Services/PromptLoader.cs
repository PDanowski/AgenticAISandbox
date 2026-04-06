using SdlcAgentApp.Core;

namespace SdlcAgentApp.Services;

public sealed class PromptLoader
{
    public Dictionary<string, RolePrompt> BuildPrompts(string repoRoot, string packRoot, string profile)
    {
        var workflow = File.ReadAllText(Path.Combine(packRoot, "agents", "workflow.md"));
        var wiTemplate = File.ReadAllText(Path.Combine(packRoot, "agents", "templates", "work-item-template.md"));
        var prTemplate = File.ReadAllText(Path.Combine(packRoot, "agents", "templates", "pull-request-template.md"));

        if (profile == "codex")
        {
            var roles = new Dictionary<string, string>
            {
                ["architect"] = File.ReadAllText(Path.Combine(packRoot, "agents", "architect-agent.md")),
                ["devops"] = File.ReadAllText(Path.Combine(packRoot, "agents", "devops-agent.md")),
                ["developer"] = File.ReadAllText(Path.Combine(packRoot, "agents", "developer-agent.md")),
                ["qa"] = File.ReadAllText(Path.Combine(packRoot, "agents", "qa-agent.md"))
            };
            var shared = string.Join(
                "\n\n",
                "Shared Context:",
                workflow,
                "Work Item Template:",
                wiTemplate,
                "PR Template:",
                prTemplate
            );
            return roles.ToDictionary(x => x.Key, x => new RolePrompt(x.Value, shared));
        }

        if (profile == "copilot")
        {
            var globalInst = File.ReadAllText(Path.Combine(repoRoot, ".github", "copilot-instructions.md"));
            var roles = new Dictionary<string, string>
            {
                ["architect"] = File.ReadAllText(Path.Combine(repoRoot, ".github", "prompts", "architect-agent.prompt.md")),
                ["devops"] = File.ReadAllText(Path.Combine(repoRoot, ".github", "prompts", "devops-agent.prompt.md")),
                ["developer"] = File.ReadAllText(Path.Combine(repoRoot, ".github", "prompts", "developer-agent.prompt.md")),
                ["qa"] = File.ReadAllText(Path.Combine(repoRoot, ".github", "prompts", "qa-agent.prompt.md"))
            };
            var shared = string.Join(
                "\n\n",
                "Global Copilot Instructions:",
                globalInst,
                "Workflow:",
                workflow,
                "Work Item Template:",
                wiTemplate,
                "PR Template:",
                prTemplate
            );
            return roles.ToDictionary(x => x.Key, x => new RolePrompt(x.Value, shared));
        }

        throw new InvalidOperationException($"Unknown profile: {profile}");
    }
}

