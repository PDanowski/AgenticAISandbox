# Standalone SDLC Runner

This runner executes all agents in sequence:

1. Architect
2. DevOps + Developer (parallel)
3. QA

It supports two prompt profiles:

- `codex` (uses `Azure/agents/*`)
- `copilot` (uses `.github/prompts/*` + `.github/copilot-instructions.md`)

## Prerequisites

- Python 3.10+
- `OPENAI_API_KEY` set in environment

## Quick run (PowerShell)

```powershell
$env:OPENAI_API_KEY = "your_api_key"
.\Azure\runner\run_sdlc_agents.ps1 -Profile codex -FeatureText "Build notes API with .NET 10, Azure SQL, outbox and Service Bus."
```

Run Copilot profile:

```powershell
.\Azure\runner\run_sdlc_agents.ps1 -Profile copilot -FeatureText "Build notes API with .NET 10, Azure SQL, outbox and Service Bus."
```

## Quick run (Python)

```powershell
python .\Azure\runner\run_sdlc_agents.py --profile codex --feature-text "Build notes API with .NET 10, Azure SQL, outbox and Service Bus."
```

Using a file input:

```powershell
python .\Azure\runner\run_sdlc_agents.py --profile codex --feature-file .\Azure\automations\codex\inbox\REQ-001.md
```

## Output

Outputs are written by default to profile-specific folders:

- `codex`: `Azure/automations/codex/outbox/`
- `copilot`: `Azure/automations/copilot/outbox/`

Generated files:

- `<timestamp>-<profile>-architect.md`
- `<timestamp>-<profile>-devops.md`
- `<timestamp>-<profile>-developer.md`
- `<timestamp>-<profile>-qa.md`
- `<timestamp>-<profile>-consolidated.md`

## Notes

- This is an orchestrator script, not a replacement for manual architecture/PR verification.
- Final outputs must still be reviewed manually.
