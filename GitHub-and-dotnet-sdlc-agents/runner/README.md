# Standalone SDLC Runner

This runner executes all agents in sequence:

1. Architect
2. DevOps + Developer (parallel)
3. QA

It supports two prompt profiles:

- `codex` (uses `GitHub-and-dotnet-sdlc-agents/agents/*`)
- `copilot` (uses `.github/prompts/*` + `.github/copilot-instructions.md`)

## Prerequisites

- Python 3.10+
- `OPENAI_API_KEY` set in environment

## Quick run (PowerShell)

```powershell
$env:OPENAI_API_KEY = "your_api_key"
.\GitHub-and-dotnet-sdlc-agents\\runner\run_sdlc_agents.ps1 -Profile codex -FeatureText "Build notes API with .NET 10 and GitHub Actions/Codespaces ready SDLC."
```

Run Copilot profile:

```powershell
.\GitHub-and-dotnet-sdlc-agents\\runner\run_sdlc_agents.ps1 -Profile copilot -FeatureText "Build notes API with .NET 10 and GitHub Actions/Codespaces ready SDLC."
```

## Quick run (Python)

```powershell
python .\GitHub-and-dotnet-sdlc-agents\\runner\run_sdlc_agents.py --profile codex --feature-text "Build notes API with .NET 10 and GitHub Actions/Codespaces ready SDLC."
```

Using a file input:

```powershell
python .\GitHub-and-dotnet-sdlc-agents\\runner\run_sdlc_agents.py --profile codex --feature-file .\GitHub-and-dotnet-sdlc-agents\\automations\codex\inbox\REQ-001.md
```

## Output

Outputs are written by default to profile-specific folders:

- `codex`: `GitHub-and-dotnet-sdlc-agents/automations/codex/outbox/`
- `copilot`: `GitHub-and-dotnet-sdlc-agents/automations/copilot/outbox/`

Generated files:

- `<timestamp>-<profile>-architect.md`
- `<timestamp>-<profile>-devops.md`
- `<timestamp>-<profile>-developer.md`
- `<timestamp>-<profile>-qa.md`
- `<timestamp>-<profile>-consolidated.md`

## Notes

- This is an orchestrator script, not a replacement for manual architecture/PR verification.
- Final outputs must still be reviewed manually.

