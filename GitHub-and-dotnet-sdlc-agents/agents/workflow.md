# SDLC Agent Workflow

## Roles and ownership

- Architect: target architecture, design constraints, diagrams, and architectural PR review.
- DevOps: GitHub Actions workflows, repository governance, environment setup, and Codespaces/devcontainer setup.
- Developer: .NET 10 application code, tests, and implementation quality.
- QA: test strategy, automation, execution, and release quality signal.

## Environments

- `dev`: fast feedback, early integration
- `test`: integration/regression/UAT stabilization
- `prod`: controlled release with strict safeguards

Each environment must be separately configured.

## Work item lifecycle

1. Requirement intake
2. Architect analysis + clarifications
3. Architecture package created (diagrams + work items)
4. DevOps/Developer implementation in parallel
5. QA test design + automation + execution
6. PR raised with evidence
7. Architect + User manual review
8. Merge and release through pipelines

## Mandatory artifacts

- Architecture diagrams (component, sequence, flow)
- DevOps workflow/repository configuration + pipeline definitions
- Developer code + unit/integration tests
- QA test assets + execution report
- PR notes with assumptions, risks, and evidence

## Definition of Done

- Architecture decisions documented and traceable.
- Delivery platform setup reproducible across `dev/test/prod`.
- CI/CD configured in GitHub Actions for each new service.
- Code on .NET 10 with required automated test coverage.
- QA coverage completed for applicable scope.
- Monitoring and alerting prepared.
- PR reviewed manually by User and Architect Agent.
