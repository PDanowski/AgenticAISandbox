#!/usr/bin/env python3
from __future__ import annotations

import os
import sys

from config import ROOT, load_app_config
from prompts import build_prompts
from providers import GitHubModelsClient, OpenAiClient
from ui import ask_choice, ask_multiline
from workflow import WorkflowRunner


def main() -> int:
    print("\nSDLC Agent App (interactive)\n")
    cfg = load_app_config()
    pack_key = ask_choice("Pack", ("github", "azure"), "github")
    profile = ask_choice("Profile", ("codex", "copilot"), "codex")
    provider = ask_choice("Provider", ("openai", "github-models"), "openai")
    preset = ask_choice("Model preset", ("quality", "balanced", "fast"), "balanced")

    custom_model = input("Explicit model (optional, press Enter to use preset): ").strip()
    model = custom_model if custom_model else cfg.model_presets[provider][preset]

    pack_root = cfg.packs[pack_key]
    out_dir = pack_root / "automations" / profile / "outbox"
    provider_cfg = cfg.providers[provider]

    if provider == "openai":
        token = (os.environ.get(provider_cfg.token_env) or "").strip()
        if not token:
            print(f"ERROR: {provider_cfg.token_env} is not set.")
            return 2
        base_url = input("OpenAI base URL (optional): ").strip() or provider_cfg.base_url
        client = OpenAiClient(api_key=token, base_url=base_url, timeout_sec=provider_cfg.timeout_sec)
    else:
        token = (os.environ.get(provider_cfg.token_env) or "").strip()
        if not token:
            print(f"ERROR: {provider_cfg.token_env} is not set.")
            return 2
        base_url = input("GitHub Models base URL (optional): ").strip() or provider_cfg.base_url
        github_org = input("GitHub org (optional, for org-scoped endpoint): ").strip()
        client = GitHubModelsClient(
            token=token,
            base_url=base_url,
            github_api_version=provider_cfg.github_api_version,
            github_org=github_org,
            timeout_sec=provider_cfg.timeout_sec,
        )

    prompts = build_prompts(ROOT, pack_root, profile)
    feature = ask_multiline("Feature request")

    runner = WorkflowRunner(
        model_client=client,
        model_name=model,
        prompts=prompts,
        out_dir=out_dir,
        profile=profile,
        feature=feature,
    )
    files = runner.run(pack_key=pack_key, provider=provider)

    print("\nDone. Generated output paths:")
    for f in files:
        print(f"- {f}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except KeyboardInterrupt:
        print("\nInterrupted.")
        raise SystemExit(130)
    except Exception as exc:
        print(f"\nERROR: {exc}", file=sys.stderr)
        raise SystemExit(1)
