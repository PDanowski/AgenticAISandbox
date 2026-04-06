# AgenticAISandbox

Multi-agent SDLC starter kits for Architect, DevOps, Developer, and QA workflows with .NET.

## Packs

- Azure-oriented pack: `Azure-and-dotnet-sdlc-agents/`
- GitHub Actions/Codespaces-oriented pack: `GitHub-and-dotnet-sdlc-agents/`

Each pack includes:

- `agents/` role prompts and workflow
- `agents/codex/` Codex-ready system prompts
- `agents/templates/` work item + PR templates
- `automations/` recurring automation assets
- `runner/` standalone script to run Architect -> (DevOps + Developer) -> QA

## Copilot integration (repo-wide)

- Global instructions: `.github/copilot-instructions.md`
- Role prompts: `.github/prompts/`
- PR template: `.github/pull_request_template.md`
- SDLC issue automation: `.github/workflows/copilot-sdlc-orchestrator.yml`

## Quick start

1. Choose a pack (`Azure-and-dotnet-sdlc-agents` or `GitHub-and-dotnet-sdlc-agents`).
2. Open `<pack>/agents/codex/README.md` and create 5 agents (Orchestrator + Architect + DevOps + Developer + QA).
3. Use `<pack>/agents/workflow.md` and templates as shared context.
4. Run the standalone runner from `<pack>/runner/`.

## Runner provider/model options

Runners support:

- `openai` provider (requires `OPENAI_API_KEY`)
- `github-models` provider (requires `GITHUB_TOKEN`)

Model selection:

- Preset: `quality`, `balanced`, `fast`
- Or explicit model name via `-Model` / `--model`

## Useful docs

- `Azure-and-dotnet-sdlc-agents/runner/README.md`
- `GitHub-and-dotnet-sdlc-agents/runner/README.md`
- `Azure-and-dotnet-sdlc-agents/automations/README.md`
- `GitHub-and-dotnet-sdlc-agents/automations/README.md`
- `sdlc-agent-app/README.md`

## Interactive app alternative

If you prefer guided interaction with explicit approval gates, use:

- `sdlc-agent-app/app.py`
- `sdlc-agent-app-dotnet/Program.cs`

It runs locally or in Docker, asks for missing input/approvals, and prints generated output paths.
