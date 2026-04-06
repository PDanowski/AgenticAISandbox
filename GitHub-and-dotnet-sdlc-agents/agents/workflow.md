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
2. Architect clarification loop (questions + assumptions)
3. Architecture package created (diagrams + work items)
4. Architecture approval gate (User approval required)
5. DevOps and Developer rework/refine implementation plans based on approved architecture
6. Plan approval gate (User approval required for DevOps plan and Developer plan)
7. DevOps and Developer implementation
8. Code/repository/workflow review and merge/deploy readiness
9. QA reworks test plan for actual implementation and executes tests
10. Final release recommendation and go-live decision

## Approval gates (mandatory)

- Gate A: Architecture approval
  - Input: Architect clarifications, architecture design, diagrams, risks.
  - Output: explicit approval/reject decision by User.
- Gate B: DevOps plan approval
  - Input: updated DevOps plan aligned to approved architecture.
  - Output: explicit approval/reject decision by User.
- Gate C: Developer plan approval
  - Input: updated Developer plan aligned to approved architecture.
  - Output: explicit approval/reject decision by User.
- Gate D: Implementation review
  - Input: PR(s), evidence, tests.
  - Output: merge/use decision and handoff to QA.

## Mandatory artifacts

- Architecture diagrams (component, sequence, flow)
- DevOps workflow/repository configuration + pipeline definitions
- Developer code + unit/integration tests
- QA test assets + execution report
- PR notes with assumptions, risks, and evidence
- Approval records for gates A/B/C/D

## Definition of Done

- Architecture decisions documented and traceable.
- Delivery platform setup reproducible across `dev/test/prod`.
- CI/CD configured in GitHub Actions for each new service.
- Code on .NET 10 with required automated test coverage.
- QA coverage completed for applicable scope.
- Monitoring and alerting prepared.
- PR reviewed manually by User and Architect Agent.
- All mandatory approval gates passed and recorded.
