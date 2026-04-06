from __future__ import annotations

import json
import urllib.error
import urllib.request
from abc import ABC, abstractmethod


class ModelClient(ABC):
    @abstractmethod
    def call(self, model: str, system_prompt: str, user_prompt: str) -> str:
        raise NotImplementedError


class OpenAiClient(ModelClient):
    def __init__(self, api_key: str, base_url: str, timeout_sec: int = 240) -> None:
        self.api_key = api_key
        self.base_url = base_url.rstrip("/")
        self.timeout_sec = timeout_sec

    def call(self, model: str, system_prompt: str, user_prompt: str) -> str:
        url = f"{self.base_url}/responses"
        payload = {
            "model": model,
            "input": [
                {"role": "system", "content": [{"type": "input_text", "text": system_prompt}]},
                {"role": "user", "content": [{"type": "input_text", "text": user_prompt}]},
            ],
        }
        req = urllib.request.Request(
            url,
            data=json.dumps(payload).encode("utf-8"),
            headers={
                "Authorization": f"Bearer {self.api_key}",
                "Content-Type": "application/json",
            },
            method="POST",
        )
        parsed = _send(req, self.timeout_sec)
        parts = []
        for item in parsed.get("output", []):
            if item.get("type") != "message":
                continue
            for content in item.get("content", []):
                if content.get("type") == "output_text":
                    parts.append(content.get("text", ""))
        out = "\n".join([p for p in parts if p]).strip()
        if not out:
            raise RuntimeError(f"No text output found in OpenAI response: {parsed}")
        return out


class GitHubModelsClient(ModelClient):
    def __init__(
        self,
        token: str,
        base_url: str,
        github_api_version: str = "2026-03-10",
        github_org: str = "",
        timeout_sec: int = 240,
    ) -> None:
        self.token = token
        self.base_url = base_url.rstrip("/")
        self.github_api_version = github_api_version
        self.github_org = github_org
        self.timeout_sec = timeout_sec

    def call(self, model: str, system_prompt: str, user_prompt: str) -> str:
        if self.github_org:
            url = f"{self.base_url}/orgs/{self.github_org}/inference/chat/completions"
        else:
            url = f"{self.base_url}/inference/chat/completions"
        payload = {
            "model": model,
            "messages": [
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": user_prompt},
            ],
        }
        req = urllib.request.Request(
            url,
            data=json.dumps(payload).encode("utf-8"),
            headers={
                "Accept": "application/vnd.github+json",
                "Authorization": f"Bearer {self.token}",
                "X-GitHub-Api-Version": self.github_api_version,
                "Content-Type": "application/json",
            },
            method="POST",
        )
        parsed = _send(req, self.timeout_sec)
        try:
            text = parsed["choices"][0]["message"]["content"]
        except Exception as exc:
            raise RuntimeError(f"No text output found in GitHub Models response: {parsed}") from exc
        if not isinstance(text, str) or not text.strip():
            raise RuntimeError(f"No text output found in GitHub Models response: {parsed}")
        return text.strip()


def _send(req: urllib.request.Request, timeout_sec: int) -> dict:
    try:
        with urllib.request.urlopen(req, timeout=timeout_sec) as resp:
            return json.loads(resp.read().decode("utf-8"))
    except urllib.error.HTTPError as exc:
        err = exc.read().decode("utf-8", errors="replace")
        raise RuntimeError(f"API HTTP {exc.code}: {err}") from exc
    except urllib.error.URLError as exc:
        raise RuntimeError(f"API connection error: {exc}") from exc

