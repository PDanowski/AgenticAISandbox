# SDLC Agent App (.NET)

Interactive `.NET 10` SDLC app that runs the Architect, DevOps, Developer, and QA workflow with approval gates.
The application now uses a more SOLID structure with separate UI, prompt loading, agent factory, workflow orchestration, and output writer components.

Flow:

1. Architect clarification/design
2. Gate A: architecture approval
3. DevOps + Developer plan generation/rework
4. Gate B + Gate C: plan approvals
5. Gate D: implementation review confirmation
6. QA rework and test planning

The app prompts for required inputs and approvals, uses a role-agent execution model, and writes generated outputs.

## Configuration

Runtime configuration lives in:

- `sdlc-agent-app-dotnet/appsettings.json`

This includes pack paths, provider URLs, token environment variables, and model presets.

## Requirements

- .NET 10 SDK
- `OPENAI_API_KEY` for `openai` provider
- `GITHUB_TOKEN` for `github-models` provider

## Run locally

```powershell
dotnet run --project .\sdlc-agent-app-dotnet\SdlcAgentApp.csproj
```

## Run in Docker

Build:

```powershell
docker build -t sdlc-agent-app-dotnet -f .\sdlc-agent-app-dotnet\Dockerfile .
```

Run with OpenAI token:

```powershell
docker run -it --rm -e OPENAI_API_KEY=%OPENAI_API_KEY% sdlc-agent-app-dotnet
```

Run with GitHub token:

```powershell
docker run -it --rm -e GITHUB_TOKEN=%GITHUB_TOKEN% sdlc-agent-app-dotnet
```

Optional mount for output persistence:

```powershell
docker run -it --rm -v "${PWD}:/workspace" -e OPENAI_API_KEY=%OPENAI_API_KEY% sdlc-agent-app-dotnet
```

## Output

Outputs are written to the selected pack outbox:

- `GitHub-and-dotnet-sdlc-agents/automations/<profile>/outbox/`
- `Azure-and-dotnet-sdlc-agents/automations/<profile>/outbox/`

## Notes

- The app uses role agents to separate behavior across SDLC phases.
- Generated content should be reviewed before being applied in production.
