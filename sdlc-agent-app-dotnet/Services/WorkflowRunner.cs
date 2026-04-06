using SdlcAgentApp.Core;

namespace SdlcAgentApp.Services;

public sealed class WorkflowRunner(
    ConsoleUi ui,
    IModelClient modelClient,
    Dictionary<string, RolePrompt> prompts,
    OutputWriter outputWriter,
    string model,
    string feature,
    string profile,
    string timestamp)
{
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

        - Timestamp: {timestamp}
        - Pack: {packKey}
        - Profile: {profile}
        - Provider: {provider}
        - Model: {model}

        ## Output Files
        - Architect: {archPath}
        - DevOps: {devopsPath}
        - Developer: {developerPath}
        - QA: {qaPath}
        """;
        files.Add(outputWriter.Write($"{timestamp}-{profile}-app-summary.md", summary));
        return files;
    }

    private async Task<(string Out, string Path)> RunArchitectAsync()
    {
        Console.WriteLine();
        Console.WriteLine("Phase 1: Architect");
        var feedback = string.Empty;
        var round = 0;
        while (true)
        {
            round++;
            var prompt = $"""
            Feature request:
            {feature}

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
            var path = outputWriter.Write($"{timestamp}-{profile}-architect-r{round}.md", output);
            Console.WriteLine($"Architect output: {path}");
            if (ui.AskYesNo("Approve architecture (Gate A)?", false))
            {
                return (output, path);
            }

            feedback = ui.AskRequired("Provide architecture rework feedback");
        }
    }

    private async Task<(string DevOpsOut, string DevOpsPath, string DeveloperOut, string DeveloperPath)> RunPlanningAsync(string architectOut)
    {
        Console.WriteLine();
        Console.WriteLine("Phase 2: DevOps + Developer planning");

        var devopsPrompt = $"""
        Feature request:
        {feature}

        Approved architecture:
        {architectOut}

        Produce implementation-ready DevOps plan and Gate B approval summary.
        """;
        var developerPrompt = $"""
        Feature request:
        {feature}

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
        var devopsPath = outputWriter.Write($"{timestamp}-{profile}-devops-r{devopsRound}.md", devopsOut);
        var developerPath = outputWriter.Write($"{timestamp}-{profile}-developer-r{developerRound}.md", developerOut);
        Console.WriteLine($"DevOps plan: {devopsPath}");
        Console.WriteLine($"Developer plan: {developerPath}");

        while (!ui.AskYesNo("Approve DevOps plan (Gate B)?", false))
        {
            var feedback = ui.AskRequired("Provide DevOps rework feedback");
            devopsRound++;
            var reworkPrompt = $"""
            Feature request:
            {feature}

            Approved architecture:
            {architectOut}

            Rework feedback:
            {feedback}

            Produce revised DevOps plan and Gate B approval summary.
            """;
            devopsOut = await CallRoleAsync("devops", reworkPrompt);
            devopsPath = outputWriter.Write($"{timestamp}-{profile}-devops-r{devopsRound}.md", devopsOut);
            Console.WriteLine($"Revised DevOps plan: {devopsPath}");
        }

        while (!ui.AskYesNo("Approve Developer plan (Gate C)?", false))
        {
            var feedback = ui.AskRequired("Provide Developer rework feedback");
            developerRound++;
            var reworkPrompt = $"""
            Feature request:
            {feature}

            Approved architecture:
            {architectOut}

            Rework feedback:
            {feedback}

            Produce revised Developer plan and Gate C approval summary.
            """;
            developerOut = await CallRoleAsync("developer", reworkPrompt);
            developerPath = outputWriter.Write($"{timestamp}-{profile}-developer-r{developerRound}.md", developerOut);
            Console.WriteLine($"Revised Developer plan: {developerPath}");
        }

        return (devopsOut, devopsPath, developerOut, developerPath);
    }

    private void WaitForImplementationGate()
    {
        Console.WriteLine();
        Console.WriteLine("Phase 3: Implementation review gate");
        while (!ui.AskYesNo("Have implementation PRs been reviewed and approved/merged (Gate D)?", false))
        {
            Console.WriteLine("Waiting for implementation review/merge. Complete reviews, then approve Gate D to continue.");
        }
    }

    private async Task<(string Out, string Path)> RunQaAsync(string architectOut, string devopsOut, string developerOut)
    {
        Console.WriteLine();
        Console.WriteLine("Phase 4: QA rework and test plan");
        var qaPrompt = $"""
        Feature request:
        {feature}

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
        var path = outputWriter.Write($"{timestamp}-{profile}-qa.md", output);
        Console.WriteLine($"QA output: {path}");
        return (output, path);
    }

    private Task<string> CallRoleAsync(string role, string userPrompt)
    {
        var rp = prompts[role];
        return modelClient.CallAsync(model, rp.ComposeSystemPrompt(), userPrompt);
    }
}

