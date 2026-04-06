from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path
from typing import Dict


ROOT = Path(__file__).resolve().parents[1]

PACKS: Dict[str, Path] = {
    "github": ROOT / "GitHub-and-dotnet-sdlc-agents",
    "azure": ROOT / "Azure-and-dotnet-sdlc-agents",
}

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

