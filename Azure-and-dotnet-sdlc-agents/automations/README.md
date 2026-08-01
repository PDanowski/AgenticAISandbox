# SDLC Automations

This folder contains automation assets for Codex and Copilot/GitHub driven SDLC workflows.

## Codex automation

- `codex/sdlc-orchestrator.prompt.md`: task prompt for recurring Codex automation.
- `codex/sdlc-orchestrator.automation.toml`: suggested automation definition.
- `codex/feature-request-template.md`: feature request format for the automation inbox.

## Copilot/GitHub automation

- `.github/ISSUE_TEMPLATE/sdlc-feature-request.yml`: feature request intake form.
- `.github/workflows/copilot-sdlc-orchestrator.yml`: GitHub workflow for `sdlc-request` issues.
- `copilot/outbox/`: default runner output folder for `copilot` profile.

## How it works

1. A feature request is created via Codex inbox file or GitHub issue.
2. Automation starts the Architect-first workflow.
3. Architect, DevOps, Developer, and QA outputs are generated through gated handoffs.
4. Final PRs should use the repository templates and receive manual Architect + User review.
