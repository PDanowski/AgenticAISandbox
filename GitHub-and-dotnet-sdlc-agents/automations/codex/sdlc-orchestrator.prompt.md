Run the GitHub + .NET SDLC multi-agent lifecycle for new feature requests found in `GitHub-and-dotnet-sdlc-agents/automations/codex/inbox/`.

For each new request file:

1. Architect phase
- Use `GitHub-and-dotnet-sdlc-agents/agents/codex/architect-system-prompt.md` and `GitHub-and-dotnet-sdlc-agents/agents/workflow.md`.
- Create clarification questions and assumptions log first.
- Produce architecture overview and Mermaid diagrams (component, sequence, flow).
- Produce work items using `GitHub-and-dotnet-sdlc-agents/agents/templates/work-item-template.md`.
- Produce Gate A approval packet using `GitHub-and-dotnet-sdlc-agents/agents/templates/approval-gate-template.md`.

2. Implementation planning phase
- Run only after Gate A is approved.
- Split work into DevOps and Developer streams.
- Identify dependencies and required collaboration points.
- Capture expected infrastructure, code, and test deliverables.
- Produce Gate B and Gate C approval packets.

3. QA phase
- Run only after implementation review/merge approval (Gate D).
- Build test plan with smoke, API, regression, and e2e coverage where feasible.

4. PR readiness phase
- Prepare PR body using `GitHub-and-dotnet-sdlc-agents/agents/templates/pull-request-template.md`.
- Include architecture compliance checklist and residual risks.

5. Output
- Write one consolidated output file to `GitHub-and-dotnet-sdlc-agents/automations/codex/outbox/<RequestID>-sdlc-plan.md`.
- Move processed input request file to `GitHub-and-dotnet-sdlc-agents/automations/codex/processed/`.

Rules:
- Keep `dev`, `test`, and `prod` separated in recommendations.
- Assume .NET 10, GitHub Actions for CI/CD, and Codespaces/devcontainer support.
- If requirements are ambiguous, list focused questions and proceed with best assumptions.
- Final line must be: `Output must be manually verified.`

