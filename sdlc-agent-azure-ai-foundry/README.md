# SDLC Agent Azure AI Foundry Pattern

This project implements a multi-agent SDLC system using an **orchestrator + separate agent services** pattern.

## Services

- `SdlcAgent.Orchestrator.Api`: workflow state machine and approval gates.
- `SdlcAgent.Architect.Api`: architecture proposal and clarification questions.
- `SdlcAgent.DevOps.Api`: infrastructure and CI/CD planning.
- `SdlcAgent.Developer.Api`: application and test planning.
- `SdlcAgent.Qa.Api`: QA strategy and regression planning.
- `SdlcAgent.Shared.Contracts`: shared contracts used by all services.

## Lifecycle

1. Start run -> Architect generates design and optional clarifying questions.
2. Submit clarifications until Architect has no open questions.
3. Approve Architect design.
4. Developer + DevOps plans are generated and approved independently.
5. Both submit implementation artifacts.
6. Approve Developer code and DevOps infrastructure changes.
7. QA plan is generated and approved.
8. Run is marked completed.

## Local run (Docker)

```bash
docker compose up --build
```

Orchestrator will be available at `http://localhost:8080`.

## Local run (dotnet)

Run each service in a separate terminal:

```bash
dotnet run --project src/SdlcAgent.Architect.Api
dotnet run --project src/SdlcAgent.Developer.Api
dotnet run --project src/SdlcAgent.DevOps.Api
dotnet run --project src/SdlcAgent.Qa.Api
dotnet run --project src/SdlcAgent.Orchestrator.Api
```

`SdlcAgent.Orchestrator.Api/appsettings.Development.json` contains local service URLs.

## Example orchestrator calls

Start run:

```bash
curl -X POST http://localhost:8080/api/runs \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Order management modernization",
    "functionalDescription": "Build order API and asynchronous fulfillment workflow.",
    "technicalDetails": "Use .NET 10 services and event-driven integration."
  }'
```

Submit clarifications:

```bash
curl -X POST http://localhost:8080/api/runs/{runId}/clarifications \
  -H "Content-Type: application/json" \
  -d '{"answers":{"sla":"99.9","compliance":"GDPR"}}'
```

Approve a stage:

```bash
curl -X POST http://localhost:8080/api/runs/{runId}/approvals \
  -H "Content-Type: application/json" \
  -d '{"target":"ArchitectDesign","decision":"Approve","notes":"Approved by architecture board"}'
```

Submit implementation artifacts:

```bash
curl -X POST http://localhost:8080/api/runs/{runId}/implementation-submissions/developer
curl -X POST http://localhost:8080/api/runs/{runId}/implementation-submissions/devops
```

Get current state:

```bash
curl http://localhost:8080/api/runs/{runId}
```

## Azure AI Foundry mapping

- Replace each agent service response logic with Azure AI Foundry model calls.
- Keep orchestrator workflow/approval gates as deterministic business logic.
- Deploy each API independently to ACA/AKS/App Service.
- Route service discovery with environment variables (`AgentServices:*`).
- Configure Foundry endpoint and model deployment in `AzureAiFoundry` settings.
