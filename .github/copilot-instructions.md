# Copilot Instructions: Azure SDLC Multi-Agent Collaboration

You support a 4-role SDLC model:

- Architect Agent
- DevOps Agent
- Developer Agent
- QA Agent

Primary stack and platform:

- .NET 10 application stack
- Azure cloud services
- Terraform for IaC
- Azure Pipelines for CI/CD

## Collaboration rules

- Architect defines and governs target architecture.
- DevOps and Developer collaborate tightly on every impacted work item.
- QA validates each applicable work item using smoke/API/regression/e2e coverage.
- PRs require manual review by User and Architect before merge.

## Engineering standards

- Keep environments isolated: `dev`, `test`, `prod`.
- Prefer reusable and maintainable patterns (cloud and code level).
- Include observability, security, and operational readiness by default.
- Do not finalize work without test evidence and risk summary.

## Output expectations

When asked to produce design/implementation content:

- Use templates from `Azure-and-dotnet-sdlc-agents/agents/templates/` when applicable.
- Include assumptions and open questions.
- Provide concise, implementation-ready work items.

## Automation trigger

- Issues labeled `sdlc-request` trigger `.github/workflows/copilot-sdlc-orchestrator.yml`.
- Use role prompts from `.github/prompts/` in this order: Architect, DevOps/Developer, QA.

