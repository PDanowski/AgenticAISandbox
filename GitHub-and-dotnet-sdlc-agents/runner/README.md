# Standalone SDLC Runner

This runner executes the SDLC agent workflow in sequence:

1. Architect
2. DevOps + Developer (parallel)
3. QA

It supports two prompt profiles:

- `codex` (uses `GitHub-and-dotnet-sdlc-agents/agents/*`)
- `copilot` (uses `.github/prompts/*` + `.github/copilot-instructions.md`)

## Prerequisites

- Python 3.10+
- `OPENAI_API_KEY` for OpenAI provider
- `GITHUB_TOKEN` for GitHub Models provider (requires `models:read`)

## Provider selection

- `openai`: OpenAI Responses API
- `github-models`: GitHub Models inference API

## Model selection

- Presets:
  - `quality` → `gpt-5.4`
  - `balanced` → `gpt-5.4-mini` (default)
  - `fast` → `gpt-4.1-mini`
- Or pass an explicit model name with `-Model` / `--model`.

## Quick run (PowerShell)

```powershell
$env:OPENAI_API_KEY = "your_api_key"
.\GitHub-and-dotnet-sdlc-agents\runner\run_sdlc_agents.ps1 -Profile codex -Provider openai -ModelPreset quality -FeatureText "Build notes API with .NET 10 and GitHub Actions/Codespaces ready SDLC."
```

Run Copilot profile:

```powershell
.\GitHub-and-dotnet-sdlc-agents\runner\run_sdlc_agents.ps1 -Profile copilot -Model gpt-5.4 -FeatureText "Build notes API with .NET 10 and GitHub Actions/Codespaces ready SDLC."
```

GitHub Models provider:

```powershell
$env:GITHUB_TOKEN = "your_github_token"
.\GitHub-and-dotnet-sdlc-agents\runner\run_sdlc_agents.ps1 -Profile codex -Provider github-models -Model openai/gpt-4.1 -FeatureText "Build notes API with .NET 10 and GitHub Actions/Codespaces ready SDLC."
```

## Quick run (Python)

```powershell
python .\GitHub-and-dotnet-sdlc-agents\runner\run_sdlc_agents.py --profile codex --provider openai --model-preset quality --feature-text "Build notes API with .NET 10 and GitHub Actions/Codespaces ready SDLC."
```

File input mode:

```powershell
python .\GitHub-and-dotnet-sdlc-agents\runner\run_sdlc_agents.py --profile codex --model gpt-5.4 --feature-file .\GitHub-and-dotnet-sdlc-agents\automations\codex\inbox\REQ-001.md
```

GitHub Models default endpoint:

```powershell
python .\GitHub-and-dotnet-sdlc-agents\runner\run_sdlc_agents.py --profile codex --provider github-models --model openai/gpt-4.1 --feature-text "Build notes API with .NET 10 and GitHub Actions/Codespaces ready SDLC."
```

## Output

Default output locations:

- `codex`: `GitHub-and-dotnet-sdlc-agents/automations/codex/outbox/`
- `copilot`: `GitHub-and-dotnet-sdlc-agents/automations/copilot/outbox/`

Generated files:

- `<timestamp>-<profile>-architect.md`
- `<timestamp>-<profile>-devops.md`
- `<timestamp>-<profile>-developer.md`
- `<timestamp>-<profile>-qa.md`
- `<timestamp>-<profile>-consolidated.md`

## Notes

- This runner orchestrates role agents and outputs drafts for manual review.
- Always validate generated plans and PR artifacts before use.

