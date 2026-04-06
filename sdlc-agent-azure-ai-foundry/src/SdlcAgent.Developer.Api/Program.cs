using SdlcAgent.Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "developer", status = "ok" }));

app.MapPost("/api/execute", (AgentExecutionRequest request) =>
{
    var workItems = new List<WorkItem>
    {
        new("DEV-1", "Developer", "Design solution structure", "Define clean architecture layers, domain boundaries, and dependency flow."),
        new("DEV-2", "Developer", "Implement core services", "Build .NET 10 APIs and services using resilient patterns and consistent contracts."),
        new("DEV-3", "Developer", "Test strategy", "Implement unit and integration tests with pipeline-ready execution.")
    };

    var artifacts = new List<GeneratedArtifact>
    {
        new("Solution design", "/artifacts/developer/solution-architecture.md", "markdown"),
        new("API contract", "/artifacts/developer/openapi.json", "json"),
        new("Test plan", "/artifacts/developer/tests.md", "markdown")
    };

    var result = new AgentExecutionResult(
        AgentType.Developer,
        "Developer implementation plan prepared and aligned with architecture and CI/CD readiness.",
        [],
        workItems,
        artifacts);

    return Results.Ok(result);
});

app.Run();
