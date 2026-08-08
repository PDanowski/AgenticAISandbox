# AgenticAISandbox

AgenticAISandbox is a multi-agent SDLC starter repository for building and running Architect, DevOps, Developer, and QA workflows.

## What is included

- `Azure-and-dotnet-sdlc-agents/`: Azure-focused SDLC pack with .NET agent prompts, workflows, and runner scripts.
- `GitHub-and-dotnet-sdlc-agents/`: GitHub Actions/Codespaces-focused SDLC pack with .NET agent prompts, workflows, and runner scripts.
- `sdlc-agent-app/`: Python interactive SDLC app with approval gates, role-agent workflow, and SOLID abstractions for input, output, prompt loading, and workflow orchestration.
- `sdlc-agent-app-dotnet/`: .NET interactive SDLC app with approval gates, role-agent workflow, and SOLID abstractions for UI, prompt loading, model agent factory, and output writing.
- `sdlc-agent-azure-ai-foundry/`: Azure AI Foundry deployment pattern with orchestrator and service-based agent APIs.
- `agent-core/`: shared repo-level core prompt definitions for standard SDLC roles.
- `.github/skills/`: reusable skill docs for agent behavior and orchestration.

## Key concepts

- Role agents: Architect, DevOps, Developer, and QA each get a dedicated system prompt and execution model.
- Packed workflows: `agents/workflow.md` and profiles define handoffs, approvals, and work item lifecycles.
- Approval gates: architecture approval, plan approval, implementation review, and QA validation.
- Provider support: OpenAI / GitHub Models with configurable model presets and endpoints.

## Quick start

1. Choose a pack:
   - `Azure-and-dotnet-sdlc-agents`
   - `GitHub-and-dotnet-sdlc-agents`
2. Review the role prompts and workflow in the pack's `agents/` folder.
3. Use `agents/codex/README.md` to wire up Codex prompts or `.github/` files for Copilot-style prompts.
4. Run the standalone runner from the pack's `runner/` folder, or use the interactive Python/.NET apps.

## Running the interactive apps

- Python app: `sdlc-agent-app/app.py`
- .NET app: `sdlc-agent-app-dotnet/SdlcAgentApp.csproj`

Both apps support OpenAI and GitHub Models providers, ask for feature requests and approvals, and write outputs to the selected pack outbox.

## Provider and model options

- OpenAI provider: requires `OPENAI_API_KEY`
- GitHub Models provider: requires `GITHUB_TOKEN`
- Model presets: `quality`, `balanced`, `fast`
- Or pass an explicit model name via app prompts or CLI arguments.

## Useful documentation

- `Azure-and-dotnet-sdlc-agents/runner/README.md`
- `GitHub-and-dotnet-sdlc-agents/runner/README.md`
- `Azure-and-dotnet-sdlc-agents/automations/README.md`
- `GitHub-and-dotnet-sdlc-agents/automations/README.md`
- `sdlc-agent-app/README.md`
- `sdlc-agent-app-dotnet/README.md`
- `sdlc-agent-azure-ai-foundry/README.md`

## Azure deployment option

- GitHub Actions + Bicep: `.github/workflows/sdlc-agent-azure-foundry-deploy.yml`
- Bicep source: `sdlc-agent-azure-ai-foundry/infra/bicep/main.bicep`

## Notes

- This repository is a starter kit for SDLC workflows, not a finished production system.
- Generated outputs and agent plans should always be reviewed manually.
