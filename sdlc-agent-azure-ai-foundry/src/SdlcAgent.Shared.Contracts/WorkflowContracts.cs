namespace SdlcAgent.Shared.Contracts;

public enum AgentType
{
    Architect,
    Developer,
    DevOps,
    Qa
}

public enum RunStage
{
    ArchitectDesignInProgress,
    AwaitingArchitectClarifications,
    AwaitingArchitectApproval,
    AwaitingImplementationPlanApprovals,
    InImplementation,
    AwaitingCodeReviewApprovals,
    QaPlanningInProgress,
    AwaitingQaApproval,
    Completed,
    Rejected
}

public enum Decision
{
    Approve,
    Reject
}

public enum ReviewTarget
{
    ArchitectDesign,
    DeveloperPlan,
    DevOpsPlan,
    DeveloperCode,
    DevOpsInfrastructure,
    QaPlan
}

public sealed record StartRunRequest(
    string Title,
    string FunctionalDescription,
    string? TechnicalDetails = null);

public sealed record ClarificationResponseRequest(
    Dictionary<string, string> Answers);

public sealed record ApprovalRequest(
    ReviewTarget Target,
    Decision Decision,
    string? Notes = null);

public sealed record AgentExecutionRequest(
    Guid RunId,
    string Title,
    string FunctionalDescription,
    string? TechnicalDetails,
    Dictionary<string, string>? Clarifications = null,
    Dictionary<string, string>? Context = null);

public sealed record WorkItem(
    string Id,
    string Owner,
    string Title,
    string Description);

public sealed record GeneratedArtifact(
    string Name,
    string Path,
    string Type);

public sealed record AgentExecutionResult(
    AgentType Agent,
    string Summary,
    IReadOnlyList<string> Questions,
    IReadOnlyList<WorkItem> WorkItems,
    IReadOnlyList<GeneratedArtifact> Artifacts);

public sealed record AgentPlanState(
    AgentType Agent,
    AgentExecutionResult? Result,
    bool Approved,
    string? ApprovalNotes);

public sealed record WorkflowRunResponse(
    Guid RunId,
    string Title,
    RunStage Stage,
    Dictionary<string, string> Clarifications,
    AgentExecutionResult? ArchitectDesign,
    AgentPlanState DeveloperPlan,
    AgentPlanState DevOpsPlan,
    AgentExecutionResult? QaPlan,
    bool DeveloperCodeSubmitted,
    bool DevOpsInfraSubmitted,
    bool DeveloperCodeApproved,
    bool DevOpsInfraApproved,
    IReadOnlyList<string> Timeline);
