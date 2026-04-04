# AgenticAISandbox

Azure-focused SDLC multi-agent setup for Architect, DevOps, Developer, and QA collaboration.

## What is included

- Core agent definitions: `Azure/agents/`
- Codex-specific config pack: `Azure/agents/codex/`
- Reusable templates: `Azure/agents/templates/`
- Automation assets: `Azure/automations/`
- Copilot repo instructions: `.github/copilot-instructions.md`
- Copilot role prompts: `.github/prompts/`
- GitHub PR template: `.github/pull_request_template.md`
- GitHub SDLC workflow automation: `.github/workflows/copilot-sdlc-orchestrator.yml`

## Quick start (Codex)

1. Open `Azure/agents/codex/README.md`.
2. Create 5 agents (Orchestrator + Architect + DevOps + Developer + QA).
3. Use each `*-system-prompt.md` as the agent system prompt.
4. Keep `Azure/agents/workflow.md` and template files in shared context.

## Quick start (GitHub Copilot)

1. Keep `.github/copilot-instructions.md` in repo root.
2. Use files in `.github/prompts/` as task starters per role.
3. Use `.github/pull_request_template.md` for consistent PR quality gates.

## Main docs

- `Azure/agents/README.md`
- `Azure/agents/workflow.md`
- `Azure/agents/templates/work-item-template.md`
- `Azure/agents/templates/pull-request-template.md`
- `Azure/automations/README.md`
