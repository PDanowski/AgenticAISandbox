Run the Azure SDLC multi-agent lifecycle for new feature requests found in `Azure/automations/codex/inbox/`.

For each new request file:

1. Architect phase
- Use `Azure/agents/codex/architect-system-prompt.md` and `Azure/agents/workflow.md`.
- Produce architecture overview and Mermaid diagrams (component, sequence, flow).
- Produce work items using `Azure/agents/templates/work-item-template.md`.

2. Implementation planning phase
- Split work into DevOps and Developer streams.
- Identify dependencies and required collaboration points.
- Capture expected infrastructure, code, and test deliverables.

3. QA phase
- Build test plan with smoke, API, regression, and e2e coverage where feasible.

4. PR readiness phase
- Prepare PR body using `Azure/agents/templates/pull-request-template.md`.
- Include architecture compliance checklist and residual risks.

5. Output
- Write one consolidated output file to `Azure/automations/codex/outbox/<RequestID>-sdlc-plan.md`.
- Move processed input request file to `Azure/automations/codex/processed/`.

Rules:
- Keep `dev`, `test`, and `prod` separated in recommendations.
- Assume .NET 10 and Terraform on Azure.
- If requirements are ambiguous, list focused questions and proceed with best assumptions.
- Final line must be: `Output must be manually verified.`

