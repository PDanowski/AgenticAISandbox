# SDLC Agent App

Interactive Python app that runs the SDLC agent workflow with approval gates and role-based agent execution.

Flow:

1. Architect clarification/design
2. Architecture approval (Gate A)
3. DevOps + Developer plan generation and rework
4. Plan approvals (Gate B + Gate C)
5. Implementation review approval (Gate D)
6. QA rework and test planning

The app asks for required user input/approvals, executes role agents via a shared factory, and prints generated output paths.

## Configuration

Runtime configuration is in:

- `sdlc-agent-app/settings.json`

This includes pack paths, provider URLs, token environment variables, and model preset mappings.

## Requirements

- Python 3.10+
- `OPENAI_API_KEY` for `openai` provider
- `GITHUB_TOKEN` for `github-models` provider

## Run locally

```powershell
python .\sdlc-agent-app\app.py
```

## Run in Docker

Build the container:

```powershell
docker build -t sdlc-agent-app -f .\sdlc-agent-app\Dockerfile .
```

Run with OpenAI:

```powershell
docker run -it --rm -e OPENAI_API_KEY=%OPENAI_API_KEY% sdlc-agent-app
```

Run with GitHub Models:

```powershell
docker run -it --rm -e GITHUB_TOKEN=%GITHUB_TOKEN% sdlc-agent-app
```

Optional mount for local persistence:

```powershell
docker run -it --rm -v "${PWD}:/workspace" -e OPENAI_API_KEY=%OPENAI_API_KEY% sdlc-agent-app
```

## Output

Generated outputs are written to the active pack outbox:

- `GitHub-and-dotnet-sdlc-agents/automations/<profile>/outbox/`
- `Azure-and-dotnet-sdlc-agents/automations/<profile>/outbox/`

## Notes

- The app uses role agents to separate Architect, DevOps, Developer, and QA behavior.
- Generated content should be reviewed before being used in production.
