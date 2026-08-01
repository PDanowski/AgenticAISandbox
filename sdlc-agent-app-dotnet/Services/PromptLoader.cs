using SdlcAgentApp.Core;

namespace SdlcAgentApp.Services;

public sealed class PromptLoader
{
    private static string ReadOptional(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private static string ReadRolePrompt(string repoRoot, string packRoot, string role)
    {
        var corePath = Path.Combine(packRoot, "agents", "core", $"{role}-core.md");
        var sharedCorePath = Path.Combine(repoRoot, "agent-core", $"{role}-core.md");
        var agentPath = Path.Combine(packRoot, "agents", $"{role}-agent.md");
        var prompt = ReadOptional(corePath);
        if (!string.IsNullOrEmpty(prompt))
        {
            return prompt;
        }

        prompt = ReadOptional(sharedCorePath);
        if (!string.IsNullOrEmpty(prompt))
        {
            return prompt;
        }

        return File.ReadAllText(agentPath);
    }

    private static string LoadSkillDocs(string repoRoot, string packRoot)
    {
        var skillDirs = new[]
        {
            Path.Combine(repoRoot, ".github", "skills"),
            Path.Combine(packRoot, "agents", "skills")
        };

        var paths = skillDirs
            .Where(Directory.Exists)
            .SelectMany(dir => Directory.GetFiles(dir, "*.md"))
            .OrderBy(path => path)
            .ToArray();

        if (paths.Length == 0)
        {
            return string.Empty;
        }

        var parts = new List<string> { "Skills:" };
        foreach (var file in paths)
        {
            parts.Add($"### {Path.GetFileName(file)}\n{File.ReadAllText(file)}");
        }

        return string.Join("\n\n", parts);
    }

    public Dictionary<string, RolePrompt> BuildPrompts(string repoRoot, string packRoot, string profile)
    {
        var workflow = File.ReadAllText(Path.Combine(packRoot, "agents", "workflow.md"));
        var wiTemplate = File.ReadAllText(Path.Combine(packRoot, "agents", "templates", "work-item-template.md"));
        var prTemplate = File.ReadAllText(Path.Combine(packRoot, "agents", "templates", "pull-request-template.md"));
        var skills = LoadSkillDocs(repoRoot, packRoot);

        if (profile == "codex")
        {
            var roles = new Dictionary<string, string>
            {
                ["architect"] = ReadRolePrompt(repoRoot, packRoot, "architect"),
                ["devops"] = ReadRolePrompt(repoRoot, packRoot, "devops"),
                ["developer"] = ReadRolePrompt(repoRoot, packRoot, "developer"),
                ["qa"] = ReadRolePrompt(repoRoot, packRoot, "qa")
            };
            var sharedParts = new List<string>
            {
                "Shared Context:",
                workflow,
                "Work Item Template:",
                wiTemplate,
                "PR Template:",
                prTemplate
            };
            if (!string.IsNullOrEmpty(skills))
            {
                sharedParts.Add(skills);
            }
            var shared = string.Join("\n\n", sharedParts);
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
            var sharedParts = new List<string>
            {
                "Global Copilot Instructions:",
                globalInst,
                "Workflow:",
                workflow,
                "Work Item Template:",
                wiTemplate,
                "PR Template:",
                prTemplate
            };
            if (!string.IsNullOrEmpty(skills))
            {
                sharedParts.Add(skills);
            }
            var shared = string.Join("\n\n", sharedParts);
            return roles.ToDictionary(x => x.Key, x => new RolePrompt(x.Value, shared));
        }

        throw new InvalidOperationException($"Unknown profile: {profile}");
    }
}

