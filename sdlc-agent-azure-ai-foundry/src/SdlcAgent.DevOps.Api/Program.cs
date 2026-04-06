using SdlcAgent.Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "devops", status = "ok" }));

app.MapPost("/api/execute", (AgentExecutionRequest request) =>
{
    var workItems = new List<WorkItem>
    {
        new("DEVOPS-1", "DevOps", "Provision environments", "Create IaC for dev/test/prod with separate state and secure parameterization."),
        new("DEVOPS-2", "DevOps", "Configure CI/CD", "Create reusable Azure Pipelines with gated deployment stages."),
        new("DEVOPS-3", "DevOps", "Observability", "Configure monitoring, dashboards, alerts, and custom metrics ingestion.")
    };

    var artifacts = new List<GeneratedArtifact>
    {
        new("Terraform root", "/artifacts/devops/terraform/main.tf", "terraform"),
        new("Pipeline definition", "/artifacts/devops/pipelines/service-pipeline.yml", "yaml"),
        new("Monitoring baseline", "/artifacts/devops/monitoring/alerts.md", "markdown")
    };

    var result = new AgentExecutionResult(
        AgentType.DevOps,
        "DevOps execution plan prepared from approved architecture assumptions.",
        [],
        workItems,
        artifacts);

    return Results.Ok(result);
});

app.Run();
