namespace SdlcAgentApp.Core;

public sealed record RolePrompt(string RoleContent, string SharedContext)
{
    public string ComposeSystemPrompt() => $"{RoleContent}\n\n{SharedContext}";
}

