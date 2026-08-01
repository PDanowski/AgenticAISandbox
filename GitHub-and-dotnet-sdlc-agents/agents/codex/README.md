# Codex Config Pack

This folder contains Codex-oriented configuration files for the SDLC multi-agent model.

## Files

- `orchestrator-agent.md`: controls sequencing, handoffs, and quality gates.
- `architect-system-prompt.md`: Architect role prompt.
- `devops-system-prompt.md`: DevOps role prompt.
- `developer-system-prompt.md`: Developer role prompt.
- `qa-system-prompt.md`: QA role prompt.

## Usage

1. Create 5 agents in Codex (orchestrator + 4 specialist roles).
2. Use each file as the corresponding system prompt.
3. Provide `workflow.md` and templates as shared context.
4. Require PR review by Architect + User before merge.

