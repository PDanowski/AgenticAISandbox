# AgenticAISandbox

Azure-focused SDLC multi-agent setup for Architect, DevOps, Developer, and QA collaboration.

## What is included

- Core agent definitions: `Azure-and-dotnet-sdlc-agents/agents/`
- Codex-specific config pack: `Azure-and-dotnet-sdlc-agents/agents/codex/`
- Reusable templates: `Azure-and-dotnet-sdlc-agents/agents/templates/`
- Automation assets: `Azure-and-dotnet-sdlc-agents/automations/`
- Standalone runner script: `Azure-and-dotnet-sdlc-agents/runner/`
- Copilot repo instructions: `.github/copilot-instructions.md`
- Copilot role prompts: `.github/prompts/`
- GitHub PR template: `.github/pull_request_template.md`
- GitHub SDLC workflow automation: `.github/workflows/copilot-sdlc-orchestrator.yml`

## Quick start (Codex)

1. Open `Azure-and-dotnet-sdlc-agents/agents/codex/README.md`.
2. Create 5 agents (Orchestrator + Architect + DevOps + Developer + QA).
3. Use each `*-system-prompt.md` as the agent system prompt.
4. Keep `Azure-and-dotnet-sdlc-agents/agents/workflow.md` and template files in shared context.

## Quick start (GitHub Copilot)

1. Keep `.github/copilot-instructions.md` in repo root.
2. Use files in `.github/prompts/` as task starters per role.
3. Use `.github/pull_request_template.md` for consistent PR quality gates.

## Main docs

- `Azure-and-dotnet-sdlc-agents/agents/README.md`
- `Azure-and-dotnet-sdlc-agents/agents/workflow.md`
- `Azure-and-dotnet-sdlc-agents/agents/templates/work-item-template.md`
- `Azure-and-dotnet-sdlc-agents/agents/templates/pull-request-template.md`
- `Azure-and-dotnet-sdlc-agents/automations/README.md`
- `Azure-and-dotnet-sdlc-agents/runner/README.md`

