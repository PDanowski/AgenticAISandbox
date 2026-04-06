# SDLC Multi-Agent Pack (GitHub + .NET)

This folder defines 4 collaborating agents for your SDLC process:

1. Architect Agent
2. DevOps Agent
3. Developer Agent
4. QA Agent

Use each file as a system prompt (or role definition) in your agent platform.

## Files

- `architect-agent.md`: architecture ownership, diagrams, and PR architecture review
- `devops-agent.md`: GitHub Actions, repository governance, and Codespaces setup
- `developer-agent.md`: .NET 10 service implementation with unit/integration tests
- `qa-agent.md`: smoke/API/regression/e2e quality coverage
- `workflow.md`: handoffs, work item lifecycle, and Definition of Done
- `codex/`: Codex-specific configuration pack
- `templates/`: reusable work item and PR templates
- `.github/copilot-instructions.md` (repo root): Copilot global instructions
- `.github/prompts/` (repo root): Copilot task prompts per role

## Operating model

- Architect defines architecture and constraints.
- DevOps and Developer implement in parallel with tight collaboration.
- QA validates every work item where applicable.
- Architect and User perform manual final review on pull requests.

## Standard work item flow

1. Architect refines requirements and creates design artifacts.
2. Architect publishes implementation-ready work items.
3. DevOps and Developer execute work items and sync continuously.
4. QA validates implemented scope.
5. PR is raised and reviewed by Architect + User.
6. Merge after all quality gates pass.

## Gated delivery model

Use mandatory approvals:

- Gate A: Architecture approval (after clarifications and design)
- Gate B: DevOps plan approval (after plan rework)
- Gate C: Developer plan approval (after plan rework)
- Gate D: Implementation review/merge approval (before QA rework execution)
