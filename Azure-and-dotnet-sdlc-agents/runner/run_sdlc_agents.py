#!/usr/bin/env python3
"""
Standalone SDLC multi-agent runner.

Flow:
1) Architect
2) DevOps + Developer (parallel)
3) QA

Supports prompt profiles:
- codex
- copilot
"""

from __future__ import annotations

import argparse
import concurrent.futures
import datetime as dt
import json
import os
import sys
import textwrap
import urllib.error
import urllib.request
from pathlib import Path
from typing import Dict, Tuple


ROOT = Path(__file__).resolve().parents[2]
PACK_ROOT = Path(__file__).resolve().parents[1]

MODEL_PRESETS: Dict[str, Dict[str, str]] = {
    "openai": {
        "quality": "gpt-5.4",
        "balanced": "gpt-5.4-mini",
        "fast": "gpt-4.1-mini",
    },
    "github-models": {
        "quality": "openai/gpt-4.1",
        "balanced": "openai/gpt-4.1",
        "fast": "openai/gpt-4.1",
    },
}


def read_text(path: Path) -> str:
    return path.read_text(encoding="utf-8")


def call_openai_api(
    openai_api_key: str,
    model: str,
    system_prompt: str,
    user_prompt: str,
    base_url: str = "https://api.openai.com/v1",
    timeout_sec: int = 180,
) -> str:
    url = f"{base_url.rstrip('/')}/responses"
    payload = {
        "model": model,
        "input": [
            {
                "role": "system",
                "content": [{"type": "input_text", "text": system_prompt}],
            },
            {
                "role": "user",
                "content": [{"type": "input_text", "text": user_prompt}],
            },
        ],
    }
    data = json.dumps(payload).encode("utf-8")
    req = urllib.request.Request(
        url,
        data=data,
        headers={
            "Authorization": f"Bearer {openai_api_key}",
            "Content-Type": "application/json",
        },
        method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=timeout_sec) as resp:
            raw = resp.read()
    except urllib.error.HTTPError as exc:
        err = exc.read().decode("utf-8", errors="replace")
        raise RuntimeError(f"API HTTP {exc.code}: {err}") from exc
    except urllib.error.URLError as exc:
        raise RuntimeError(f"API connection error: {exc}") from exc

    parsed = json.loads(raw.decode("utf-8"))
    text_parts = []
    for item in parsed.get("output", []):
        if item.get("type") != "message":
            continue
        for content in item.get("content", []):
            if content.get("type") == "output_text":
                text_parts.append(content.get("text", ""))
    if not text_parts:
        raise RuntimeError(f"No text output found in API response: {parsed}")
    return "\n".join(part for part in text_parts if part).strip()


def call_github_models_api(
    github_token: str,
    model: str,
    system_prompt: str,
    user_prompt: str,
    base_url: str = "https://models.github.ai",
    timeout_sec: int = 180,
    github_api_version: str = "2026-03-10",
    github_org: str = "",
) -> str:
    if github_org:
        url = f"{base_url.rstrip('/')}/orgs/{github_org}/inference/chat/completions"
    else:
        url = f"{base_url.rstrip('/')}/inference/chat/completions"
    payload = {
        "model": model,
        "messages": [
            {"role": "system", "content": system_prompt},
            {"role": "user", "content": user_prompt},
        ],
    }
    data = json.dumps(payload).encode("utf-8")
    req = urllib.request.Request(
        url,
        data=data,
        headers={
            "Accept": "application/vnd.github+json",
            "Authorization": f"Bearer {github_token}",
            "X-GitHub-Api-Version": github_api_version,
            "Content-Type": "application/json",
        },
        method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=timeout_sec) as resp:
            raw = resp.read()
    except urllib.error.HTTPError as exc:
        err = exc.read().decode("utf-8", errors="replace")
        raise RuntimeError(f"API HTTP {exc.code}: {err}") from exc
    except urllib.error.URLError as exc:
        raise RuntimeError(f"API connection error: {exc}") from exc

    parsed = json.loads(raw.decode("utf-8"))
    try:
        msg = parsed["choices"][0]["message"]["content"]
        if isinstance(msg, str):
            return msg.strip()
    except Exception as exc:
        raise RuntimeError(f"No text output found in GitHub Models response: {parsed}") from exc
    raise RuntimeError(f"No text output found in GitHub Models response: {parsed}")


def build_prompts(profile: str) -> Dict[str, Tuple[str, str]]:
    workflow = read_text(PACK_ROOT / "agents" / "workflow.md")
    wi_template = read_text(PACK_ROOT / "agents" / "templates" / "work-item-template.md")
    pr_template = read_text(PACK_ROOT / "agents" / "templates" / "pull-request-template.md")

    if profile == "codex":
        roles = {
            "architect": read_text(PACK_ROOT / "agents" / "architect-agent.md"),
            "devops": read_text(PACK_ROOT / "agents" / "devops-agent.md"),
            "developer": read_text(PACK_ROOT / "agents" / "developer-agent.md"),
            "qa": read_text(PACK_ROOT / "agents" / "qa-agent.md"),
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
        return {k: (v, shared) for k, v in roles.items()}

    if profile == "copilot":
        global_inst = read_text(ROOT / ".github" / "copilot-instructions.md")
        roles = {
            "architect": read_text(ROOT / ".github" / "prompts" / "architect-agent.prompt.md"),
            "devops": read_text(ROOT / ".github" / "prompts" / "devops-agent.prompt.md"),
            "developer": read_text(ROOT / ".github" / "prompts" / "developer-agent.prompt.md"),
            "qa": read_text(ROOT / ".github" / "prompts" / "qa-agent.prompt.md"),
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
        return {k: (v, shared) for k, v in roles.items()}

    raise ValueError(f"Unknown profile: {profile}")


def write_output(out_dir: Path, name: str, content: str) -> Path:
    out_dir.mkdir(parents=True, exist_ok=True)
    path = out_dir / name
    path.write_text(content, encoding="utf-8")
    return path


def run(
    profile: str,
    provider: str,
    feature_text: str,
    model: str,
    out_dir: Path,
    token: str,
    base_url: str,
    github_api_version: str,
    github_org: str,
) -> None:
    prompts = build_prompts(profile)
    ts = dt.datetime.now().strftime("%Y%m%d-%H%M%S")

    if provider == "openai":
        def call_model(system_prompt: str, user_prompt: str) -> str:
            return call_openai_api(token, model, system_prompt, user_prompt, base_url=base_url)
    elif provider == "github-models":
        def call_model(system_prompt: str, user_prompt: str) -> str:
            return call_github_models_api(
                token,
                model,
                system_prompt,
                user_prompt,
                base_url=base_url,
                github_api_version=github_api_version,
                github_org=github_org,
            )
    else:
        raise ValueError(f"Unknown provider: {provider}")

    def system(role: str) -> str:
        role_prompt, shared = prompts[role]
        return f"{role_prompt}\n\n{shared}"

    # 1) Architect
    arch_user = textwrap.dedent(
        f"""
        Feature request:
        {feature_text}

        Produce:
        1) Architecture summary
        2) Mermaid diagrams (component, sequence, flow)
        3) Work items split by DevOps/Developer/QA
        4) Risks/assumptions
        5) PR architecture checklist
        """
    ).strip()
    architect_out = call_model(system("architect"), arch_user)
    arch_path = write_output(out_dir, f"{ts}-{profile}-architect.md", architect_out)
    print(f"[ok] architect -> {arch_path}")

    # 2) DevOps + Developer (parallel)
    devops_user = textwrap.dedent(
        f"""
        Feature request:
        {feature_text}

        Architect output:
        {architect_out}

        Produce implementation-ready DevOps plan and PR evidence checklist.
        """
    ).strip()
    developer_user = textwrap.dedent(
        f"""
        Feature request:
        {feature_text}

        Architect output:
        {architect_out}

        Produce implementation-ready Developer plan and PR evidence checklist.
        """
    ).strip()

    with concurrent.futures.ThreadPoolExecutor(max_workers=2) as ex:
        fut_devops = ex.submit(call_model, system("devops"), devops_user)
        fut_developer = ex.submit(call_model, system("developer"), developer_user)
        devops_out = fut_devops.result()
        developer_out = fut_developer.result()

    devops_path = write_output(out_dir, f"{ts}-{profile}-devops.md", devops_out)
    developer_path = write_output(out_dir, f"{ts}-{profile}-developer.md", developer_out)
    print(f"[ok] devops -> {devops_path}")
    print(f"[ok] developer -> {developer_path}")

    # 3) QA
    qa_user = textwrap.dedent(
        f"""
        Feature request:
        {feature_text}

        Architect output:
        {architect_out}

        DevOps output:
        {devops_out}

        Developer output:
        {developer_out}

        Produce:
        1) Test strategy per work item
        2) Smoke/API/regression/e2e set
        3) Pipeline integration approach
        4) Defect/risk reporting model
        5) Release recommendation criteria and residual risks
        """
    ).strip()
    qa_out = call_model(system("qa"), qa_user)
    qa_path = write_output(out_dir, f"{ts}-{profile}-qa.md", qa_out)
    print(f"[ok] qa -> {qa_path}")

    consolidated = textwrap.dedent(
        f"""
        # SDLC Multi-Agent Run

        - Timestamp: {ts}
        - Profile: {profile}
        - Provider: {provider}
        - Model: {model}

        ## Feature Request
        {feature_text}

        ## Architect
        {architect_out}

        ## DevOps
        {devops_out}

        ## Developer
        {developer_out}

        ## QA
        {qa_out}
        """
    ).strip()
    consolidated_path = write_output(out_dir, f"{ts}-{profile}-consolidated.md", consolidated)
    print(f"[ok] consolidated -> {consolidated_path}")


def parse_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(description="Run SDLC multi-agent workflow (Architect -> DevOps/Developer -> QA).")
    p.add_argument("--profile", choices=["codex", "copilot"], default="codex")
    p.add_argument("--provider", choices=["openai", "github-models"], default="openai")
    p.add_argument("--model", default="", help="Explicit model name. Overrides --model-preset.")
    p.add_argument("--model-preset", choices=["quality", "balanced", "fast"], default="balanced")
    p.add_argument("--feature-file", help="Path to markdown/txt file with feature request.")
    p.add_argument("--feature-text", help="Feature request text (alternative to --feature-file).")
    p.add_argument(
        "--out-dir",
        default="",
        help="Output directory for generated artifacts.",
    )
    p.add_argument("--base-url", default="")
    p.add_argument("--github-api-version", default="2026-03-10")
    p.add_argument("--github-org", default="")
    return p.parse_args()


def main() -> int:
    args = parse_args()
    provider_models = MODEL_PRESETS.get(args.provider, {})
    model = args.model.strip() if args.model else provider_models[args.model_preset]

    if args.provider == "openai":
        token = os.environ.get("OPENAI_API_KEY", "")
        if not token:
            print("ERROR: OPENAI_API_KEY is not set.", file=sys.stderr)
            return 2
        base_url = args.base_url or "https://api.openai.com/v1"
    else:
        token = os.environ.get("GITHUB_TOKEN", "")
        if not token:
            print("ERROR: GITHUB_TOKEN is not set.", file=sys.stderr)
            return 2
        base_url = args.base_url or "https://models.github.ai"

    if bool(args.feature_file) == bool(args.feature_text):
        print("ERROR: provide exactly one of --feature-file or --feature-text.", file=sys.stderr)
        return 2

    if args.feature_file:
        feature_text = read_text(Path(args.feature_file).resolve()).strip()
    else:
        feature_text = args.feature_text.strip()

    if args.out_dir:
        out_dir = Path(args.out_dir).resolve()
    else:
        profile_out = PACK_ROOT / "automations" / args.profile / "outbox"
        out_dir = profile_out.resolve()

    try:
        run(
            profile=args.profile,
            provider=args.provider,
            feature_text=feature_text,
            model=model,
            out_dir=out_dir,
            token=token,
            base_url=base_url,
            github_api_version=args.github_api_version,
            github_org=args.github_org,
        )
    except Exception as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        return 1

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
