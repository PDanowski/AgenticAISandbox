namespace SdlcAgentApp.Services;

public sealed class PathResolver
{
    public string FindRepoRoot()
    {
        var candidates = new[]
        {
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory
        };

        foreach (var c in candidates)
        {
            var d = new DirectoryInfo(c);
            while (d is not null)
            {
                var hasGitHubFolder = Directory.Exists(Path.Combine(d.FullName, ".github"));
                var hasPackFolders = Directory.Exists(Path.Combine(d.FullName, "GitHub-and-dotnet-sdlc-agents"))
                                     && Directory.Exists(Path.Combine(d.FullName, "Azure-and-dotnet-sdlc-agents"));
                if (hasGitHubFolder && hasPackFolders)
                {
                    return d.FullName;
                }
                d = d.Parent;
            }
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }
}

