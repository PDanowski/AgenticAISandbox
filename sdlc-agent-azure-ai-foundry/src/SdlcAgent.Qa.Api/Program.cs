using SdlcAgent.Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "qa", status = "ok" }));

app.MapPost("/api/execute", (AgentExecutionRequest request) =>
{
    var workItems = new List<WorkItem>
    {
        new("QA-1", "QA", "Prepare smoke tests", "Validate happy path and critical service health checks."),
        new("QA-2", "QA", "API and integration tests", "Validate contract compatibility and cross-service behaviors."),
        new("QA-3", "QA", "Regression suite", "Create reusable automated suite for release gating in CI/CD.")
    };

    var artifacts = new List<GeneratedArtifact>
    {
        new("QA strategy", "/artifacts/qa/strategy.md", "markdown"),
        new("API tests", "/artifacts/qa/api-tests.http", "text"),
        new("Regression matrix", "/artifacts/qa/regression-matrix.md", "markdown")
    };

    var result = new AgentExecutionResult(
        AgentType.Qa,
        "QA plan prepared after Developer and DevOps artifacts were approved.",
        [],
        workItems,
        artifacts);

    return Results.Ok(result);
});

app.Run();
