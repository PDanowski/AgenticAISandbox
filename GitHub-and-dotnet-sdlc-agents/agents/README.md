# SDLC Multi-Agent Pack (GitHub + .NET)

This folder defines four collaborating SDLC agents:

1. Architect Agent
2. DevOps Agent
3. Developer Agent
4. QA Agent

Use each file as a system prompt or role definition in your agent platform.

## Files

- `architect-agent.md`: architecture ownership, diagrams, and PR architecture review
- `devops-agent.md`: GitHub Actions, repository governance, and Codespaces setup
- `developer-agent.md`: .NET 10 service implementation with unit/integration tests
- `qa-agent.md`: smoke/API/regression/e2e quality coverage
- `core/`: optional core prompt files for each role
- `skills/`: optional shared skill docs for agent behavior
- `workflow.md`: handoffs, work item lifecycle, and Definition of Done
- `codex/`: Codex-specific configuration pack
- `templates/`: reusable work item and PR templates
- `.github/copilot-instructions.md` (repo root): Copilot global instructions
- `.github/prompts/` (repo root): Copilot task prompts per role

## Operating model

- Architect defines architecture, constraints, and approval gates.
- DevOps and Developer execute implementation plans in parallel.
- QA validates scope, test strategy, and release readiness.
- Architect and user-reviewers perform final PR signoff.

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
