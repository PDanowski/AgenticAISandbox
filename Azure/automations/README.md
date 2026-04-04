# SDLC Automations

This folder contains automation assets for both Codex and Copilot/GitHub.

## Codex automation

- `codex/sdlc-orchestrator.prompt.md`: task prompt for recurring Codex automation.
- `codex/sdlc-orchestrator.automation.toml`: suggested automation definition.
- `codex/feature-request-template.md`: request format used by the automation inbox.

## Copilot/GitHub automation

- `.github/ISSUE_TEMPLATE/sdlc-feature-request.yml`: feature request intake form.
- `.github/workflows/copilot-sdlc-orchestrator.yml`: automation workflow for `sdlc-request` issues.
- `copilot/outbox/`: default standalone runner output folder for `copilot` profile.

## How this works

1. New feature request enters intake (Codex inbox file or GitHub issue).
2. Automation triggers architect-first workflow.
3. Work is split for DevOps + Developer, then QA.
4. PR must use template and requires manual Architect + User review.
