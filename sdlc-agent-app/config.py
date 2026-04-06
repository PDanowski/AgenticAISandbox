from __future__ import annotations

import json
from dataclasses import dataclass
from pathlib import Path
from typing import Dict


ROOT = Path(__file__).resolve().parents[1]
SETTINGS_PATH = Path(__file__).with_name("settings.json")


@dataclass(frozen=True)
class RuntimeConfig:
    pack_key: str
    profile: str
    provider: str
    model_preset: str
    model: str
    token: str
    base_url: str
    github_org: str
    pack_root: Path
    out_dir: Path


@dataclass(frozen=True)
class ProviderConfig:
    token_env: str
    base_url: str
    timeout_sec: int
    github_api_version: str = "2026-03-10"


@dataclass(frozen=True)
class AppConfig:
    packs: Dict[str, Path]
    providers: Dict[str, ProviderConfig]
    model_presets: Dict[str, Dict[str, str]]


def load_app_config(path: Path = SETTINGS_PATH) -> AppConfig:
    raw = json.loads(path.read_text(encoding="utf-8"))
    packs = {k: ROOT / v for k, v in raw["packs"].items()}
    providers = {
        k: ProviderConfig(
            token_env=v["token_env"],
            base_url=v["base_url"],
            timeout_sec=int(v.get("timeout_sec", 240)),
            github_api_version=v.get("github_api_version", "2026-03-10"),
        )
        for k, v in raw["providers"].items()
    }
    model_presets = raw["model_presets"]
    return AppConfig(packs=packs, providers=providers, model_presets=model_presets)
