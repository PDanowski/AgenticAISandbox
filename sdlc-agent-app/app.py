#!/usr/bin/env python3
from __future__ import annotations

import os
import sys

from config import ROOT, load_app_config
from input import InputReader
from output import OutputWriter
from prompts import build_prompts
from providers import GitHubModelsClient, ModelClientRoleAgentFactory, OpenAiClient
from workflow import WorkflowRunner


def main() -> int:
    ui = InputReader()
    ui.write_line("\nSDLC Agent App (interactive)\n")

    cfg = load_app_config()
    pack_key = ui.ask_choice("Pack", ("github", "azure"), "github")
    profile = ui.ask_choice("Profile", ("codex", "copilot"), "codex")
    provider = ui.ask_choice("Provider", ("openai", "github-models"), "openai")
    preset = ui.ask_choice("Model preset", ("quality", "balanced", "fast"), "balanced")

    model = ui.ask_optional("Explicit model (optional, press Enter to use preset):", cfg.model_presets[provider][preset])

    pack_root = cfg.packs[pack_key]
    out_dir = pack_root / "automations" / profile / "outbox"
    provider_cfg = cfg.providers[provider]

    token = (os.environ.get(provider_cfg.token_env) or "").strip()
    if not token:
        ui.write_line(f"ERROR: {provider_cfg.token_env} is not set.")
        return 2

    if provider == "openai":
        base_url = ui.ask_optional("OpenAI base URL (optional):", provider_cfg.base_url)
        client = OpenAiClient(api_key=token, base_url=base_url, timeout_sec=provider_cfg.timeout_sec)
    else:
        base_url = ui.ask_optional("GitHub Models base URL (optional):", provider_cfg.base_url)
        github_org = ui.ask_optional("GitHub org (optional, for org-scoped endpoint):", "")
        client = GitHubModelsClient(
            token=token,
            base_url=base_url,
            github_api_version=provider_cfg.github_api_version,
            github_org=github_org,
            timeout_sec=provider_cfg.timeout_sec,
        )

    agent_factory = ModelClientRoleAgentFactory(model_client=client, model_name=model)
    prompts = build_prompts(ROOT, pack_root, profile)
    feature = ui.ask_multiline("Feature request")

    writer = OutputWriter(out_dir)
    runner = WorkflowRunner(
        agent_factory=agent_factory,
        prompts=prompts,
        out_dir=out_dir,
        profile=profile,
        feature=feature,
        model_name=model,
        ui=ui,
        output_writer=writer,
    )
    files = runner.run(pack_key=pack_key, provider=provider)

    ui.write_line("\nDone. Generated output paths:")
    for file in files:
        ui.write_line(f"- {file}")
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
