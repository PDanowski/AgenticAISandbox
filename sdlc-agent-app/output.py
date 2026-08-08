from __future__ import annotations

from pathlib import Path


class OutputWriter:
    def __init__(self, out_dir: Path) -> None:
        self._out_dir = out_dir
        self._out_dir.mkdir(parents=True, exist_ok=True)

    def write(self, file_name: str, content: str) -> Path:
        path = self._out_dir / file_name
        path.write_text(content, encoding="utf-8")
        return path
