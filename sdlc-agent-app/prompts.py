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


def _read_optional(path: Path) -> str | None:
    return path.read_text(encoding="utf-8") if path.exists() else None


def _read_role_prompt(repo_root: Path, pack_root: Path, role: str) -> str:
    core_path = pack_root / "agents" / "core" / f"{role}-core.md"
    shared_core_path = repo_root / "agent-core" / f"{role}-core.md"
    agent_path = pack_root / "agents" / f"{role}-agent.md"
    return _read_optional(core_path) or _read_optional(shared_core_path) or _read(agent_path)


def _load_skill_docs(repo_root: Path, pack_root: Path) -> str:
    skill_dirs = [repo_root / ".github" / "skills", pack_root / "agents" / "skills"]
    paths = []
    for skills_dir in skill_dirs:
        if skills_dir.exists():
            paths.extend(sorted(skills_dir.glob("*.md")))

    if not paths:
        return ""

    parts = ["Skills:"]
    for path in paths:
        parts.append(f"### {path.name}\n{_read(path)}")
    return "\n\n".join(parts)


def build_prompts(repo_root: Path, pack_root: Path, profile: str) -> Dict[str, RolePrompt]:
    workflow = _read(pack_root / "agents" / "workflow.md")
    wi_template = _read(pack_root / "agents" / "templates" / "work-item-template.md")
    pr_template = _read(pack_root / "agents" / "templates" / "pull-request-template.md")
    skills = _load_skill_docs(repo_root, pack_root)

    if profile == "codex":
        roles = {
            "architect": _read_role_prompt(repo_root, pack_root, "architect"),
            "devops": _read_role_prompt(repo_root, pack_root, "devops"),
            "developer": _read_role_prompt(repo_root, pack_root, "developer"),
            "qa": _read_role_prompt(repo_root, pack_root, "qa"),
        }
        shared_parts = [
            "Shared Context:",
            workflow,
            "Work Item Template:",
            wi_template,
            "PR Template:",
            pr_template,
        ]
        if skills:
            shared_parts.extend([skills])
        shared = "\n\n".join(shared_parts)
        return {k: RolePrompt(v, shared) for k, v in roles.items()}

    if profile == "copilot":
        global_inst = _read(repo_root / ".github" / "copilot-instructions.md")
        roles = {
            "architect": _read(repo_root / ".github" / "prompts" / "architect-agent.prompt.md"),
            "devops": _read(repo_root / ".github" / "prompts" / "devops-agent.prompt.md"),
            "developer": _read(repo_root / ".github" / "prompts" / "developer-agent.prompt.md"),
            "qa": _read(repo_root / ".github" / "prompts" / "qa-agent.prompt.md"),
        }
        shared_parts = [
            "Global Copilot Instructions:",
            global_inst,
            "Workflow:",
            workflow,
            "Work Item Template:",
            wi_template,
            "PR Template:",
            pr_template,
        ]
        if skills:
            shared_parts.extend([skills])
        shared = "\n\n".join(shared_parts)
        return {k: RolePrompt(v, shared) for k, v in roles.items()}

    raise ValueError(f"Unknown profile: {profile}")


def _read(path: Path) -> str:
    return path.read_text(encoding="utf-8")

