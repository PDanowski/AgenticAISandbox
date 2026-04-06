from __future__ import annotations

from typing import Iterable


def ask_choice(label: str, options: Iterable[str], default: str) -> str:
    options = tuple(options)
    rendered = ", ".join([f"{o}{' (default)' if o == default else ''}" for o in options])
    while True:
        raw = input(f"{label} [{rendered}]: ").strip().lower()
        if not raw:
            return default
        if raw in options:
            return raw
        print(f"Invalid value: {raw}")


def ask_yes_no(label: str, default_yes: bool = True) -> bool:
    d = "Y/n" if default_yes else "y/N"
    while True:
        raw = input(f"{label} [{d}]: ").strip().lower()
        if not raw:
            return default_yes
        if raw in ("y", "yes"):
            return True
        if raw in ("n", "no"):
            return False
        print(f"Invalid value: {raw}")


def ask_required(label: str) -> str:
    while True:
        raw = input(f"{label}: ").strip()
        if raw:
            return raw
        print("Value is required.")


def ask_multiline(label: str) -> str:
    print(f"{label} (type END on a new line to finish)")
    lines = []
    while True:
        line = input()
        if line.strip() == "END":
            break
        lines.append(line)
    text = "\n".join(lines).strip()
    if not text:
        raise RuntimeError("Input cannot be empty.")
    return text

