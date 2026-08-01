# SDLC Agent Azure AI Foundry Pattern

This project implements a multi-agent SDLC system with an orchestrator service and separate agent APIs.

## Services

- `SdlcAgent.Orchestrator.Api`: orchestrator workflow, approval gates, and state transitions.
- `SdlcAgent.Architect.Api`: architecture proposals and clarification questions.
- `SdlcAgent.DevOps.Api`: infrastructure and CI/CD planning.
- `SdlcAgent.Developer.Api`: application and test planning.
- `SdlcAgent.Qa.Api`: QA strategy, regression planning, and release readiness.
- `SdlcAgent.Shared.Contracts`: shared request/response contracts for all services.

## Workflow

1. Start a run and Architect generates design plus clarifying questions.
2. Submit clarifications until Architect is satisfied.
3. Approve Architect design.
4. DevOps and Developer plans are generated and approved.
5. Implementation artifacts are submitted.
6. Developer and DevOps changes are approved.
7. QA plan is generated and approved.
8. The run completes.

## Local development

### Docker

```bash
docker compose up --build
```

The orchestrator is available at `http://localhost:8080` by default.

### .NET services

Run each service in a separate terminal:

```bash
dotnet run --project src/SdlcAgent.Architect.Api

dotnet run --project src/SdlcAgent.Developer.Api

dotnet run --project src/SdlcAgent.DevOps.Api

dotnet run --project src/SdlcAgent.Qa.Api

dotnet run --project src/SdlcAgent.Orchestrator.Api
```

Local service URLs are configured in `SdlcAgent.Orchestrator.Api/appsettings.Development.json`.

## Example API calls

Start a run:

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

Get run state:

```bash
curl http://localhost:8080/api/runs/{runId}
```

## Azure AI Foundry mapping

- Replace service response stubs with Azure AI Foundry inference calls.
- Keep orchestrator workflow and approval gates as deterministic logic.
- Deploy each agent API independently to ACA/AKS/App Service.
- Configure service discovery via environment variables.
- Set Foundry connection values in `AzureAiFoundry` settings.

## GitHub Actions + Bicep deployment

This repo includes:

- Bicep template: `infra/bicep/main.bicep`
- Parameter files:
  - `infra/bicep/params/dev.parameters.json`
  - `infra/bicep/params/test.parameters.json`
  - `infra/bicep/params/prod.parameters.json`
- Workflow: `.github/workflows/sdlc-agent-azure-foundry-deploy.yml`

### Deployment prerequisites

Add GitHub secrets:

- `AZURE_CLIENT_ID`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`

These should map to an Entra workload identity or service principal configured for GitHub OIDC.

### Deployment inputs

Required workflow inputs:

- `environment` (`dev` / `test` / `prod`)
- `azure_location`
- `resource_group`
- `name_prefix`
- `acr_name`

Optional workflow inputs:

- `foundry_project_endpoint`
- `foundry_model_deployment`
- `foundry_managed_identity_client_id`
- `bootstrap_foundry`
- `foundry_resource_name`
- `foundry_project_name`
- `foundry_model_name`
- `foundry_model_version`
- `foundry_model_provider`
- `foundry_deployment_name`
- `foundry_sku_name`
- `foundry_sku_capacity`

### Validation checklist

1. Open the orchestrator URL from the workflow output.
2. Verify the health endpoint.
3. Start a workflow run.
4. Check service logs.

## Notes

- This pattern is a deployment-ready architecture for service-based agent workflows.
- Generated outputs and model-driven responses should be validated before production use.
2. Authenticate with managed identity.
3. Use Foundry endpoint + model deployment name from configuration.
4. Keep orchestrator approval/state logic deterministic as-is.

Without step 7, infrastructure is deployed and callable, but agent reasoning remains stubbed.
