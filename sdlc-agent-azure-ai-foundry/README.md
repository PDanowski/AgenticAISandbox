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

## Deploy with GitHub Actions + Bicep

This repo now includes:

- Bicep template: `infra/bicep/main.bicep`
- Environment parameter files:
  - `infra/bicep/params/dev.parameters.json`
  - `infra/bicep/params/test.parameters.json`
  - `infra/bicep/params/prod.parameters.json`
- Workflow: `.github/workflows/sdlc-agent-azure-foundry-deploy.yml`

### 1. Configure GitHub OIDC secrets

Add repository secrets:

- `AZURE_CLIENT_ID`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`

These should map to an Entra app/service principal configured for GitHub OIDC federation.

### 2. Set parameters

Update one of:

- `infra/bicep/params/dev.parameters.json`
- `infra/bicep/params/test.parameters.json`
- `infra/bicep/params/prod.parameters.json`

At minimum replace:

- `acrName` with a globally unique registry name.

### 3. Run deployment workflow

From GitHub Actions run:

- `Deploy SDLC Agent (Azure + Bicep)`

Required inputs:

- `environment` (`dev` / `test` / `prod`)
- `azure_location`
- `resource_group`
- `name_prefix`
- `acr_name`

Optional inputs:

- `foundry_project_endpoint`
- `foundry_model_deployment`
- `foundry_managed_identity_client_id`
- `bootstrap_foundry` (`true`/`false`)
- `foundry_resource_name`
- `foundry_project_name`
- `foundry_model_name`
- `foundry_model_version`
- `foundry_model_provider`
- `foundry_deployment_name`
- `foundry_sku_name`
- `foundry_sku_capacity`

The workflow will:

1. Create/update the resource group.
2. Deploy infrastructure from Bicep.
3. Build and push all service images to ACR.
4. Redeploy container apps with the pushed image tags.
5. Print orchestrator URL in workflow output.

If `bootstrap_foundry=true`, the workflow also:

1. Creates/updates Foundry `AIServices` resource with project management enabled.
2. Creates Foundry project if missing.
3. Creates model deployment if missing.
4. Prints Foundry inference endpoint.

## Exact deployment runbook (Azure AI Foundry)

Use this checklist to get the system running end-to-end.

### 1. Create Azure AI Foundry project and model deployment

1. Create an Azure AI Foundry project.
2. Deploy a model in that project.
3. Save:
   - Foundry project endpoint
   - Model deployment name

### 2. Configure GitHub OIDC to Azure

Create Entra workload identity/federation for this repository and grant:

- `Contributor` on target resource group
- `AcrPush` on ACR scope
- `User Access Administrator` (or `Owner`) where role assignments are created

### 3. Add GitHub secrets

Add repository (or environment) secrets:

- `AZURE_CLIENT_ID`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`

### 4. Prepare environment parameter file

Edit one of:

- `infra/bicep/params/dev.parameters.json`
- `infra/bicep/params/test.parameters.json`
- `infra/bicep/params/prod.parameters.json`

Set at minimum:

- `acrName` (globally unique)
- `namePrefix`
- `location`

Optional but recommended:

- `foundryProjectEndpoint`
- `foundryModelDeployment`
- `foundryManagedIdentityClientId`

### 5. Run GitHub workflow

Run:

- `.github/workflows/sdlc-agent-azure-foundry-deploy.yml`

Provide inputs:

- `environment` (`dev` first)
- `azure_location`
- `resource_group`
- `name_prefix`
- `acr_name`
- optional Foundry values

If you set `bootstrap_foundry=true`, also provide:

- `foundry_resource_name`
- `foundry_project_name`
- `foundry_model_name`
- `foundry_model_version`
- `foundry_model_provider`
- `foundry_deployment_name`

### 6. Validate deployment

1. Open orchestrator URL from workflow output.
2. Check health endpoint:
   - `GET /`
3. Start a workflow run:
   - `POST /api/runs`
4. Verify logs in Container Apps and Log Analytics.

### 7. Final production step (required)

Current agent APIs return mocked/static outputs. To run real AI behavior:

1. Replace static `AgentExecutionResult` creation in each agent API with Foundry inference calls.
2. Authenticate with managed identity.
3. Use Foundry endpoint + model deployment name from configuration.
4. Keep orchestrator approval/state logic deterministic as-is.

Without step 7, infrastructure is deployed and callable, but agent reasoning remains stubbed.
