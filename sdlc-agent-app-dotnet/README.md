# SDLC Agent App (.NET)

Interactive `.NET 10` alternative to the Python app.

Flow:

1. Architect clarification/design
2. Gate A: architecture approval
3. DevOps + Developer plan generation/rework
4. Gate B + Gate C: plan approvals
5. Gate D: implementation review/merge confirmation
6. QA rework + test planning

The app asks for required inputs and approvals and prints generated output paths.

## Run locally

Prerequisites:

- .NET 10 SDK
- Token:
  - `OPENAI_API_KEY` for provider `openai`
  - `GITHUB_TOKEN` for provider `github-models`

Run:

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

Optional (recommended) host bind mount to persist outputs directly in your local repo:

```powershell
docker run -it --rm -v "${PWD}:/workspace" -e OPENAI_API_KEY=%OPENAI_API_KEY% sdlc-agent-app-dotnet
```

## Output

Outputs are written under selected pack outbox:

- `GitHub-and-dotnet-sdlc-agents/automations/<profile>/outbox/`
- `Azure-and-dotnet-sdlc-agents/automations/<profile>/outbox/`
