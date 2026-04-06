using SdlcAgent.Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "architect", status = "ok" }));

app.MapPost("/api/execute", (AgentExecutionRequest request) =>
{
    var hasClarifications = request.Clarifications is { Count: > 0 };
    var questions = new List<string>();

    if (!hasClarifications)
    {
        questions.Add("What are the non-functional requirements (SLA, latency, throughput)?");
        questions.Add("Do we need strict data residency or compliance controls?");
    }

    var workItems = new List<WorkItem>
    {
        new("ARCH-1", "Architect", "Define target architecture", "Prepare component diagram and service boundaries for .NET services on Azure AI Foundry."),
        new("ARCH-2", "Architect", "Define delivery architecture", "Specify CI/CD, environment strategy, secrets, and release gates."),
        new("ARCH-3", "Architect", "Design code-level patterns", "Recommend DDD boundaries, CQRS usage, and resilient integration patterns.")
    };

    var artifacts = new List<GeneratedArtifact>
    {
        new("Azure component diagram", "/artifacts/architecture/azure-components.mmd", "diagram"),
        new("Sequence diagram", "/artifacts/architecture/solution-sequence.mmd", "diagram"),
        new("Implementation backlog", "/artifacts/architecture/work-items.md", "markdown")
    };

    var result = new AgentExecutionResult(
        AgentType.Architect,
        "Architecture proposal prepared for orchestrator + separate agent services. Manual review required before implementation.",
        questions,
        workItems,
        artifacts);

    return Results.Ok(result);
});

app.Run();
