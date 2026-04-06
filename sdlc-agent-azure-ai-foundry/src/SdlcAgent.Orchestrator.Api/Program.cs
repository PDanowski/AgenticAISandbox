using System.Collections.Concurrent;
using System.Net.Http.Json;
using SdlcAgent.Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient("architect", static (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(config["AgentServices:Architect"] ?? "http://architect-api:8080");
});
builder.Services.AddHttpClient("developer", static (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(config["AgentServices:Developer"] ?? "http://developer-api:8080");
});
builder.Services.AddHttpClient("devops", static (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(config["AgentServices:DevOps"] ?? "http://devops-api:8080");
});
builder.Services.AddHttpClient("qa", static (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(config["AgentServices:Qa"] ?? "http://qa-api:8080");
});
builder.Services.AddSingleton<RunStore>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "orchestrator", status = "ok" }));

app.MapPost("/api/runs", async (
    StartRunRequest request,
    IHttpClientFactory httpClientFactory,
    RunStore runStore,
    CancellationToken cancellationToken) =>
{
    var run = OrchestrationRun.Start(request);
    runStore.Upsert(run);

    run.Timeline.Add("Architect design started.");
    var architectResult = await InvokeAgentAsync(httpClientFactory, "architect", run, cancellationToken);
    run.ArchitectDesign = architectResult;

    if (architectResult.Questions.Count > 0)
    {
        run.Stage = RunStage.AwaitingArchitectClarifications;
        run.Timeline.Add("Architect requested clarifications.");
    }
    else
    {
        run.Stage = RunStage.AwaitingArchitectApproval;
        run.Timeline.Add("Architect design submitted for approval.");
    }

    runStore.Upsert(run);
    return Results.Accepted($"/api/runs/{run.RunId}", run.ToResponse());
});

app.MapPost("/api/runs/{runId:guid}/clarifications", async (
    Guid runId,
    ClarificationResponseRequest request,
    IHttpClientFactory httpClientFactory,
    RunStore runStore,
    CancellationToken cancellationToken) =>
{
    if (!runStore.TryGet(runId, out var run))
    {
        return Results.NotFound();
    }

    if (run.Stage != RunStage.AwaitingArchitectClarifications)
    {
        return Results.BadRequest($"Run is in stage '{run.Stage}' and cannot accept clarifications.");
    }

    foreach (var answer in request.Answers)
    {
        run.Clarifications[answer.Key] = answer.Value;
    }

    run.Timeline.Add("Clarifications sent to Architect.");
    run.Stage = RunStage.ArchitectDesignInProgress;

    var architectResult = await InvokeAgentAsync(httpClientFactory, "architect", run, cancellationToken);
    run.ArchitectDesign = architectResult;

    if (architectResult.Questions.Count > 0)
    {
        run.Stage = RunStage.AwaitingArchitectClarifications;
        run.Timeline.Add("Architect requested more clarifications.");
    }
    else
    {
        run.Stage = RunStage.AwaitingArchitectApproval;
        run.Timeline.Add("Architect design ready for approval.");
    }

    runStore.Upsert(run);
    return Results.Ok(run.ToResponse());
});

app.MapPost("/api/runs/{runId:guid}/approvals", async (
    Guid runId,
    ApprovalRequest request,
    IHttpClientFactory httpClientFactory,
    RunStore runStore,
    CancellationToken cancellationToken) =>
{
    if (!runStore.TryGet(runId, out var run))
    {
        return Results.NotFound();
    }

    if (request.Decision == Decision.Reject)
    {
        run.Stage = RunStage.Rejected;
        run.Timeline.Add($"Rejected at target '{request.Target}'. Notes: {request.Notes ?? "n/a"}");
        runStore.Upsert(run);
        return Results.Ok(run.ToResponse());
    }

    switch (request.Target)
    {
        case ReviewTarget.ArchitectDesign:
        {
            if (run.Stage != RunStage.AwaitingArchitectApproval)
            {
                return Results.BadRequest($"Run is in stage '{run.Stage}' and cannot approve architect design.");
            }

            run.Timeline.Add("Architect design approved.");
            run.Timeline.Add("Developer and DevOps planning started.");

            run.DeveloperPlan = run.DeveloperPlan with { Result = await InvokeAgentAsync(httpClientFactory, "developer", run, cancellationToken) };
            run.DevOpsPlan = run.DevOpsPlan with { Result = await InvokeAgentAsync(httpClientFactory, "devops", run, cancellationToken) };
            run.Stage = RunStage.AwaitingImplementationPlanApprovals;
            break;
        }
        case ReviewTarget.DeveloperPlan:
        {
            if (run.Stage != RunStage.AwaitingImplementationPlanApprovals)
            {
                return Results.BadRequest($"Run is in stage '{run.Stage}' and cannot approve developer plan.");
            }

            run.DeveloperPlan = run.DeveloperPlan with { Approved = true, ApprovalNotes = request.Notes };
            run.Timeline.Add("Developer implementation plan approved.");
            break;
        }
        case ReviewTarget.DevOpsPlan:
        {
            if (run.Stage != RunStage.AwaitingImplementationPlanApprovals)
            {
                return Results.BadRequest($"Run is in stage '{run.Stage}' and cannot approve DevOps plan.");
            }

            run.DevOpsPlan = run.DevOpsPlan with { Approved = true, ApprovalNotes = request.Notes };
            run.Timeline.Add("DevOps implementation plan approved.");
            break;
        }
        case ReviewTarget.DeveloperCode:
        {
            if (run.Stage != RunStage.AwaitingCodeReviewApprovals)
            {
                return Results.BadRequest($"Run is in stage '{run.Stage}' and cannot approve developer code.");
            }

            if (!run.DeveloperCodeSubmitted)
            {
                return Results.BadRequest("Developer code was not submitted yet.");
            }

            run.DeveloperCodeApproved = true;
            run.Timeline.Add("Developer code approved for merge.");
            break;
        }
        case ReviewTarget.DevOpsInfrastructure:
        {
            if (run.Stage != RunStage.AwaitingCodeReviewApprovals)
            {
                return Results.BadRequest($"Run is in stage '{run.Stage}' and cannot approve DevOps infrastructure.");
            }

            if (!run.DevOpsInfraSubmitted)
            {
                return Results.BadRequest("DevOps infrastructure changes were not submitted yet.");
            }

            run.DevOpsInfraApproved = true;
            run.Timeline.Add("DevOps infrastructure changes approved.");
            break;
        }
        case ReviewTarget.QaPlan:
        {
            if (run.Stage != RunStage.AwaitingQaApproval)
            {
                return Results.BadRequest($"Run is in stage '{run.Stage}' and cannot approve QA.");
            }

            run.Stage = RunStage.Completed;
            run.Timeline.Add("QA plan approved. Workflow completed.");
            break;
        }
        default:
            return Results.BadRequest($"Unsupported review target '{request.Target}'.");
    }

    if (run.Stage == RunStage.AwaitingImplementationPlanApprovals &&
        run.DeveloperPlan.Approved &&
        run.DevOpsPlan.Approved)
    {
        run.Stage = RunStage.InImplementation;
        run.Timeline.Add("Implementation can start for Developer and DevOps.");
    }

    if (run.Stage == RunStage.AwaitingCodeReviewApprovals &&
        run.DeveloperCodeApproved &&
        run.DevOpsInfraApproved)
    {
        run.Stage = RunStage.QaPlanningInProgress;
        run.Timeline.Add("QA planning started.");
        run.QaPlan = await InvokeAgentAsync(httpClientFactory, "qa", run, cancellationToken);
        run.Stage = RunStage.AwaitingQaApproval;
        run.Timeline.Add("QA plan ready for approval.");
    }

    runStore.Upsert(run);
    return Results.Ok(run.ToResponse());
});

app.MapPost("/api/runs/{runId:guid}/implementation-submissions/{agent}", (
    Guid runId,
    string agent,
    RunStore runStore) =>
{
    if (!runStore.TryGet(runId, out var run))
    {
        return Results.NotFound();
    }

    if (run.Stage != RunStage.InImplementation && run.Stage != RunStage.AwaitingCodeReviewApprovals)
    {
        return Results.BadRequest($"Run is in stage '{run.Stage}' and cannot accept implementation submissions.");
    }

    if (agent.Equals("developer", StringComparison.OrdinalIgnoreCase))
    {
        run.DeveloperCodeSubmitted = true;
        run.Timeline.Add("Developer submitted code changes.");
    }
    else if (agent.Equals("devops", StringComparison.OrdinalIgnoreCase))
    {
        run.DevOpsInfraSubmitted = true;
        run.Timeline.Add("DevOps submitted infrastructure changes.");
    }
    else
    {
        return Results.BadRequest("Agent must be 'developer' or 'devops'.");
    }

    if (run.DeveloperCodeSubmitted && run.DevOpsInfraSubmitted)
    {
        run.Stage = RunStage.AwaitingCodeReviewApprovals;
        run.Timeline.Add("Both submissions received. Awaiting code-review approvals.");
    }

    runStore.Upsert(run);
    return Results.Ok(run.ToResponse());
});

app.MapGet("/api/runs/{runId:guid}", (Guid runId, RunStore runStore) =>
{
    if (!runStore.TryGet(runId, out var run))
    {
        return Results.NotFound();
    }

    return Results.Ok(run.ToResponse());
});

app.Run();

static async Task<AgentExecutionResult> InvokeAgentAsync(
    IHttpClientFactory factory,
    string clientName,
    OrchestrationRun run,
    CancellationToken cancellationToken)
{
    var payload = new AgentExecutionRequest(
        run.RunId,
        run.Title,
        run.FunctionalDescription,
        run.TechnicalDetails,
        run.Clarifications,
        new Dictionary<string, string>
        {
            ["stage"] = run.Stage.ToString()
        });

    var response = await factory.CreateClient(clientName).PostAsJsonAsync("/api/execute", payload, cancellationToken);
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<AgentExecutionResult>(cancellationToken: cancellationToken);
    return result ?? throw new InvalidOperationException($"Agent '{clientName}' returned empty result.");
}

internal sealed class RunStore
{
    private readonly ConcurrentDictionary<Guid, OrchestrationRun> _runs = new();

    public void Upsert(OrchestrationRun run) => _runs[run.RunId] = run;

    public bool TryGet(Guid runId, out OrchestrationRun run) => _runs.TryGetValue(runId, out run!);
}

internal sealed class OrchestrationRun
{
    public Guid RunId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string FunctionalDescription { get; init; } = string.Empty;
    public string? TechnicalDetails { get; init; }
    public RunStage Stage { get; set; }
    public Dictionary<string, string> Clarifications { get; } = [];
    public AgentExecutionResult? ArchitectDesign { get; set; }
    public AgentPlanState DeveloperPlan { get; set; } = new(AgentType.Developer, null, false, null);
    public AgentPlanState DevOpsPlan { get; set; } = new(AgentType.DevOps, null, false, null);
    public AgentExecutionResult? QaPlan { get; set; }
    public bool DeveloperCodeSubmitted { get; set; }
    public bool DevOpsInfraSubmitted { get; set; }
    public bool DeveloperCodeApproved { get; set; }
    public bool DevOpsInfraApproved { get; set; }
    public List<string> Timeline { get; } = [];

    public static OrchestrationRun Start(StartRunRequest request) =>
        new()
        {
            RunId = Guid.NewGuid(),
            Title = request.Title,
            FunctionalDescription = request.FunctionalDescription,
            TechnicalDetails = request.TechnicalDetails,
            Stage = RunStage.ArchitectDesignInProgress
        };

    public WorkflowRunResponse ToResponse() =>
        new(
            RunId,
            Title,
            Stage,
            new Dictionary<string, string>(Clarifications),
            ArchitectDesign,
            DeveloperPlan,
            DevOpsPlan,
            QaPlan,
            DeveloperCodeSubmitted,
            DevOpsInfraSubmitted,
            DeveloperCodeApproved,
            DevOpsInfraApproved,
            Timeline.ToArray());
}
