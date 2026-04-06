# SDLC Agent App

Simple interactive app that runs the SDLC agent flow with approval gates:

1. Architect clarification/design
2. Architecture approval (Gate A)
3. DevOps + Developer plan rework
4. Plan approvals (Gate B + Gate C)
5. Implementation review approval (Gate D)
6. QA rework and test planning

The app asks for required user input/approvals and prints generated output paths.

## Run locally

Prerequisites:

- Python 3.10+
- API token in environment:
  - `OPENAI_API_KEY` for provider `openai`
  - `GITHUB_TOKEN` for provider `github-models`

Run:

```powershell
python .\sdlc-agent-app\app.py
```

## Run in Docker

Build:

```powershell
docker build -t sdlc-agent-app -f .\sdlc-agent-app\Dockerfile .
```

Run with OpenAI provider token:

```powershell
docker run -it --rm -e OPENAI_API_KEY=%OPENAI_API_KEY% sdlc-agent-app
```

Run with GitHub Models token:

```powershell
docker run -it --rm -e GITHUB_TOKEN=%GITHUB_TOKEN% sdlc-agent-app
```

## Output

Outputs are written under selected pack outbox:

- `GitHub-and-dotnet-sdlc-agents/automations/<profile>/outbox/`
- `Azure-and-dotnet-sdlc-agents/automations/<profile>/outbox/`

