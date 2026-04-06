from __future__ import annotations

import concurrent.futures
import datetime as dt
import textwrap
from pathlib import Path

from prompts import RolePrompt
from providers import ModelClient
from ui import ask_required, ask_yes_no


class WorkflowRunner:
    def __init__(
        self,
        model_client: ModelClient,
        model_name: str,
        prompts: dict[str, RolePrompt],
        out_dir: Path,
        profile: str,
        feature: str,
        run_stamp: str | None = None,
    ) -> None:
        self.model_client = model_client
        self.model_name = model_name
        self.prompts = prompts
        self.out_dir = out_dir
        self.profile = profile
        self.feature = feature
        self.ts = run_stamp or dt.datetime.now().strftime("%Y%m%d-%H%M%S")
        self.out_dir.mkdir(parents=True, exist_ok=True)

    def run(self, pack_key: str, provider: str) -> list[Path]:
        files: list[Path] = []
        architect_out, arch_path = self._run_architect_phase()
        files.append(arch_path)

        devops_out, devops_path, developer_out, developer_path = self._run_planning_phase(architect_out)
        files.extend([devops_path, developer_path])

        self._wait_for_implementation_gate()

        qa_out, qa_path = self._run_qa_phase(architect_out, devops_out, developer_out)
        files.append(qa_path)

        summary = textwrap.dedent(
            f"""
            # SDLC App Run Summary

            - Timestamp: {self.ts}
            - Pack: {pack_key}
            - Profile: {self.profile}
            - Provider: {provider}
            - Model: {self.model_name}

            ## Output Files
            - Architect: {arch_path}
            - DevOps: {devops_path}
            - Developer: {developer_path}
            - QA: {qa_path}
            """
        ).strip()
        summary_path = self._write(f"{self.ts}-{self.profile}-app-summary.md", summary)
        files.append(summary_path)
        return files

    def _call(self, role: str, user_prompt: str) -> str:
        return self.model_client.call(
            self.model_name,
            self.prompts[role].compose_system(),
            user_prompt,
        )

    def _run_architect_phase(self) -> tuple[str, Path]:
        print("\nPhase 1: Architect")
        feedback = ""
        round_no = 0
        while True:
            round_no += 1
            arch_user = textwrap.dedent(
                f"""
                Feature request:
                {self.feature}

                Prior feedback to address:
                {feedback or 'none'}

                Produce:
                1) Clarification questions and assumptions log
                2) Architecture summary
                3) Mermaid diagrams (component, sequence, flow)
                4) Work items split by DevOps/Developer/QA
                5) Risks/assumptions
                6) PR architecture checklist
                7) Gate A approval summary
                """
            ).strip()
            out = self._call("architect", arch_user)
            path = self._write(f"{self.ts}-{self.profile}-architect-r{round_no}.md", out)
            print(f"Architect output: {path}")
            if ask_yes_no("Approve architecture (Gate A)?", default_yes=False):
                return out, path
            feedback = ask_required("Provide architecture rework feedback")

    def _run_planning_phase(self, architect_out: str) -> tuple[str, Path, str, Path]:
        print("\nPhase 2: DevOps + Developer planning")
        devops_user = textwrap.dedent(
            f"""
            Feature request:
            {self.feature}

            Approved architecture:
            {architect_out}

            Produce implementation-ready DevOps plan and Gate B approval summary.
            """
        ).strip()
        developer_user = textwrap.dedent(
            f"""
            Feature request:
            {self.feature}

            Approved architecture:
            {architect_out}

            Produce implementation-ready Developer plan and Gate C approval summary.
            """
        ).strip()
        with concurrent.futures.ThreadPoolExecutor(max_workers=2) as ex:
            f_devops = ex.submit(self._call, "devops", devops_user)
            f_developer = ex.submit(self._call, "developer", developer_user)
            devops_out = f_devops.result()
            developer_out = f_developer.result()

        devops_round = 1
        developer_round = 1
        devops_path = self._write(f"{self.ts}-{self.profile}-devops-r{devops_round}.md", devops_out)
        developer_path = self._write(f"{self.ts}-{self.profile}-developer-r{developer_round}.md", developer_out)
        print(f"DevOps plan: {devops_path}")
        print(f"Developer plan: {developer_path}")

        while not ask_yes_no("Approve DevOps plan (Gate B)?", default_yes=False):
            feedback = ask_required("Provide DevOps rework feedback")
            devops_round += 1
            rework = textwrap.dedent(
                f"""
                Feature request:
                {self.feature}

                Approved architecture:
                {architect_out}

                Rework feedback:
                {feedback}

                Produce revised DevOps plan and Gate B approval summary.
                """
            ).strip()
            devops_out = self._call("devops", rework)
            devops_path = self._write(f"{self.ts}-{self.profile}-devops-r{devops_round}.md", devops_out)
            print(f"Revised DevOps plan: {devops_path}")

        while not ask_yes_no("Approve Developer plan (Gate C)?", default_yes=False):
            feedback = ask_required("Provide Developer rework feedback")
            developer_round += 1
            rework = textwrap.dedent(
                f"""
                Feature request:
                {self.feature}

                Approved architecture:
                {architect_out}

                Rework feedback:
                {feedback}

                Produce revised Developer plan and Gate C approval summary.
                """
            ).strip()
            developer_out = self._call("developer", rework)
            developer_path = self._write(f"{self.ts}-{self.profile}-developer-r{developer_round}.md", developer_out)
            print(f"Revised Developer plan: {developer_path}")

        return devops_out, devops_path, developer_out, developer_path

    def _wait_for_implementation_gate(self) -> None:
        print("\nPhase 3: Implementation review gate")
        while not ask_yes_no("Have implementation PRs been reviewed and approved/merged (Gate D)?", default_yes=False):
            print("Waiting for implementation review/merge. Complete reviews, then approve Gate D to continue.")

    def _run_qa_phase(self, architect_out: str, devops_out: str, developer_out: str) -> tuple[str, Path]:
        print("\nPhase 4: QA rework and test plan")
        qa_user = textwrap.dedent(
            f"""
            Feature request:
            {self.feature}

            Approved architecture:
            {architect_out}

            Approved DevOps plan:
            {devops_out}

            Approved Developer plan:
            {developer_out}

            Produce:
            1) QA rework traceability based on implemented scope
            2) Test strategy per work item
            3) Smoke/API/regression/e2e set
            4) Pipeline integration approach
            5) Defect/risk reporting model
            6) Release recommendation criteria and residual risks
            """
        ).strip()
        out = self._call("qa", qa_user)
        path = self._write(f"{self.ts}-{self.profile}-qa.md", out)
        print(f"QA output: {path}")
        return out, path

    def _write(self, name: str, content: str) -> Path:
        p = self.out_dir / name
        p.write_text(content, encoding="utf-8")
        return p

