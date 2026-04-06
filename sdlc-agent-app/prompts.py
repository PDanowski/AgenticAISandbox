from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path
from typing import Dict


@dataclass(frozen=True)
class RolePrompt:
    role_prompt: str
    shared_context: str

    def compose_system(self) -> str:
        return f"{self.role_prompt}\n\n{self.shared_context}"


def build_prompts(repo_root: Path, pack_root: Path, profile: str) -> Dict[str, RolePrompt]:
    workflow = _read(pack_root / "agents" / "workflow.md")
    wi_template = _read(pack_root / "agents" / "templates" / "work-item-template.md")
    pr_template = _read(pack_root / "agents" / "templates" / "pull-request-template.md")

    if profile == "codex":
        roles = {
            "architect": _read(pack_root / "agents" / "architect-agent.md"),
            "devops": _read(pack_root / "agents" / "devops-agent.md"),
            "developer": _read(pack_root / "agents" / "developer-agent.md"),
            "qa": _read(pack_root / "agents" / "qa-agent.md"),
        }
        shared = "\n\n".join(
            [
                "Shared Context:",
                workflow,
                "Work Item Template:",
                wi_template,
                "PR Template:",
                pr_template,
            ]
        )
        return {k: RolePrompt(v, shared) for k, v in roles.items()}

    if profile == "copilot":
        global_inst = _read(repo_root / ".github" / "copilot-instructions.md")
        roles = {
            "architect": _read(repo_root / ".github" / "prompts" / "architect-agent.prompt.md"),
            "devops": _read(repo_root / ".github" / "prompts" / "devops-agent.prompt.md"),
            "developer": _read(repo_root / ".github" / "prompts" / "developer-agent.prompt.md"),
            "qa": _read(repo_root / ".github" / "prompts" / "qa-agent.prompt.md"),
        }
        shared = "\n\n".join(
            [
                "Global Copilot Instructions:",
                global_inst,
                "Workflow:",
                workflow,
                "Work Item Template:",
                wi_template,
                "PR Template:",
                pr_template,
            ]
        )
        return {k: RolePrompt(v, shared) for k, v in roles.items()}

    raise ValueError(f"Unknown profile: {profile}")


def _read(path: Path) -> str:
    return path.read_text(encoding="utf-8")

