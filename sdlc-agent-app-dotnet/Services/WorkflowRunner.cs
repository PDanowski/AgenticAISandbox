using SdlcAgentApp.Core;

namespace SdlcAgentApp.Services;

public sealed class WorkflowRunner
{
    private readonly IUserInterface _ui;
    private readonly IRoleAgentFactory _agentFactory;
    private readonly Dictionary<string, RolePrompt> _prompts;
    private readonly IOutputWriter _outputWriter;
    private readonly string _feature;
    private readonly string _profile;
    private readonly string _timestamp;
    private readonly string _modelName;

    public WorkflowRunner(
        IUserInterface ui,
        IRoleAgentFactory agentFactory,
        Dictionary<string, RolePrompt> prompts,
        IOutputWriter outputWriter,
        string feature,
        string profile,
        string timestamp,
        string modelName)
    {
        _ui = ui;
        _agentFactory = agentFactory;
        _prompts = prompts;
        _outputWriter = outputWriter;
        _feature = feature;
        _profile = profile;
        _timestamp = timestamp;
        _modelName = modelName;
    }

    public async Task<IReadOnlyList<string>> RunAsync(string packKey, string provider)
    {
        var files = new List<string>();

        var (architectOut, archPath) = await RunArchitectAsync();
        files.Add(archPath);

        var (devopsOut, devopsPath, developerOut, developerPath) = await RunPlanningAsync(architectOut);
        files.Add(devopsPath);
        files.Add(developerPath);

        WaitForImplementationGate();

        var (_, qaPath) = await RunQaAsync(architectOut, devopsOut, developerOut);
        files.Add(qaPath);

        var summary = $"""
        # SDLC App Run Summary

        - Timestamp: {_timestamp}
        - Pack: {packKey}
        - Profile: {_profile}
        - Provider: {provider}
        - Model: {_modelName}

        ## Output Files
        - Architect: {archPath}
        - DevOps: {devopsPath}
        - Developer: {developerPath}
        - QA: {qaPath}
        """;
        files.Add(_outputWriter.Write($"{_timestamp}-{_profile}-app-summary.md", summary));
        return files;
    }

    private async Task<(string Output, string Path)> RunArchitectAsync()
    {
        _ui.WriteLine(string.Empty);
        _ui.WriteLine("Phase 1: Architect");

        var round = 0;
        var feedback = string.Empty;
        while (true)
        {
            round++;
            var prompt = $"""
            Feature request:
            {_feature}

            Prior feedback to address:
            {(string.IsNullOrWhiteSpace(feedback) ? "none" : feedback)}

            Produce:
            1) Clarification questions and assumptions log
            2) Architecture summary
            3) Mermaid diagrams (component, sequence, flow)
            4) Work items split by DevOps/Developer/QA
            5) Risks/assumptions
            6) PR architecture checklist
            7) Gate A approval summary
            """;

            var output = await CallRoleAsync("architect", prompt);
            var path = _outputWriter.Write($"{_timestamp}-{_profile}-architect-r{round}.md", output);
            _ui.WriteLine($"Architect output: {path}");

            if (_ui.AskYesNo("Approve architecture (Gate A)?", defaultYes: false))
            {
                return (output, path);
            }

            feedback = _ui.AskRequired("Provide architecture rework feedback");
        }
    }

    private async Task<(string DevOpsOut, string DevOpsPath, string DeveloperOut, string DeveloperPath)> RunPlanningAsync(string architectOut)
    {
        _ui.WriteLine(string.Empty);
        _ui.WriteLine("Phase 2: DevOps + Developer planning");

        var devopsPrompt = $"""
        Feature request:
        {_feature}

        Approved architecture:
        {architectOut}

        Produce implementation-ready DevOps plan and Gate B approval summary.
        """;
        var developerPrompt = $"""
        Feature request:
        {_feature}

        Approved architecture:
        {architectOut}

        Produce implementation-ready Developer plan and Gate C approval summary.
        """;

        var devopsTask = CallRoleAsync("devops", devopsPrompt);
        var developerTask = CallRoleAsync("developer", developerPrompt);
        await Task.WhenAll(devopsTask, developerTask);

        var devopsOut = devopsTask.Result;
        var developerOut = developerTask.Result;

        var devopsRound = 1;
        var developerRound = 1;
        var devopsPath = _outputWriter.Write($"{_timestamp}-{_profile}-devops-r{devopsRound}.md", devopsOut);
        var developerPath = _outputWriter.Write($"{_timestamp}-{_profile}-developer-r{developerRound}.md", developerOut);
        _ui.WriteLine($"DevOps plan: {devopsPath}");
        _ui.WriteLine($"Developer plan: {developerPath}");

        while (!_ui.AskYesNo("Approve DevOps plan (Gate B)?", defaultYes: false))
        {
            var feedback = _ui.AskRequired("Provide DevOps rework feedback");
            devopsRound++;
            var reworkPrompt = $"""
            Feature request:
            {_feature}

            Approved architecture:
            {architectOut}

            Rework feedback:
            {feedback}

            Produce revised DevOps plan and Gate B approval summary.
            """;
            devopsOut = await CallRoleAsync("devops", reworkPrompt);
            devopsPath = _outputWriter.Write($"{_timestamp}-{_profile}-devops-r{devopsRound}.md", devopsOut);
            _ui.WriteLine($"Revised DevOps plan: {devopsPath}");
        }

        while (!_ui.AskYesNo("Approve Developer plan (Gate C)?", defaultYes: false))
        {
            var feedback = _ui.AskRequired("Provide Developer rework feedback");
            developerRound++;
            var reworkPrompt = $"""
            Feature request:
            {_feature}

            Approved architecture:
            {architectOut}

            Rework feedback:
            {feedback}

            Produce revised Developer plan and Gate C approval summary.
            """;
            developerOut = await CallRoleAsync("developer", reworkPrompt);
            developerPath = _outputWriter.Write($"{_timestamp}-{_profile}-developer-r{developerRound}.md", developerOut);
            _ui.WriteLine($"Revised Developer plan: {developerPath}");
        }

        return (devopsOut, devopsPath, developerOut, developerPath);
    }

    private void WaitForImplementationGate()
    {
        _ui.WriteLine(string.Empty);
        _ui.WriteLine("Phase 3: Implementation review gate");
        while (!_ui.AskYesNo("Have implementation PRs been reviewed and approved/merged (Gate D)?", defaultYes: false))
        {
            _ui.WriteLine("Waiting for implementation review/merge. Complete reviews, then approve Gate D to continue.");
        }
    }

    private async Task<(string Output, string Path)> RunQaAsync(string architectOut, string devopsOut, string developerOut)
    {
        _ui.WriteLine(string.Empty);
        _ui.WriteLine("Phase 4: QA rework and test plan");

        var qaPrompt = $"""
        Feature request:
        {_feature}

        Approved architecture:
        {architectOut}

        Approved DevOps plan:
        {devopsOut}

        Approved Developer plan:
        {developerOut}

        Produce:
        1) QA rework traceability based on implemented scope
        2) Test strategy per work item
        3) Smoke/API/regression/e2e set
        4) Pipeline integration approach
        5) Defect/risk reporting model
        6) Release recommendation criteria and residual risks
        """;

        var output = await CallRoleAsync("qa", qaPrompt);
        var path = _outputWriter.Write($"{_timestamp}-{_profile}-qa.md", output);
        _ui.WriteLine($"QA output: {path}");
        return (output, path);
    }

    private Task<string> CallRoleAsync(string role, string userPrompt)
    {
        var systemPrompt = _prompts[role].ComposeSystemPrompt();
        var agent = _agentFactory.Create(role, systemPrompt);
        return agent.RunAsync(userPrompt);
    }
}

