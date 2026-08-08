from __future__ import annotations

import concurrent.futures
import datetime as dt
import textwrap
from pathlib import Path

from input import InputReader
from output import OutputWriter
from prompts import RolePrompt
from providers import RoleAgentFactory
from role_factory import ArchitectStep, DevOpsStep, DeveloperStep, QaStep, WorkflowContext


class WorkflowRunner:
    def __init__(
        self,
        agent_factory: RoleAgentFactory,
        prompts: dict[str, RolePrompt],
        out_dir: Path,
        profile: str,
        feature: str,
        model_name: str,
        ui: InputReader,
        run_stamp: str | None = None,
    ) -> None:
        self.agent_factory = agent_factory
        self.prompts = prompts
        self.out_dir = out_dir
        self.profile = profile
        self.feature = feature
        self.model_name = model_name
        self.ui = ui
        self._output_writer = OutputWriter(out_dir)
        self.ts = run_stamp or dt.datetime.now().strftime("%Y%m%d-%H%M%S")
        self.out_dir.mkdir(parents=True, exist_ok=True)

    def run(self, pack_key: str, provider: str) -> list[Path]:
        files: list[Path] = []
        context = WorkflowContext(pack_key, provider, self.profile, self.feature, self.model_name)

        architect_out, arch_path = self._run_architect_phase(context)
        files.append(arch_path)

        devops_out, devops_path, developer_out, developer_path = self._run_planning_phase(context)
        files.extend([devops_path, developer_path])

        self._wait_for_implementation_gate()

        qa_out, qa_path = self._run_qa_phase(context)
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
        summary_path = self._output_writer.write(f"{self.ts}-{self.profile}-app-summary.md", summary)
        files.append(summary_path)
        return files

    def _create_agent(self, role: str, system_prompt: str):
        return self.agent_factory.create(role, system_prompt)

    def _run_architect_phase(self, context: WorkflowContext) -> tuple[str, Path]:
        self.ui.write_line("\nPhase 1: Architect")
        round_no = 0
        while True:
            round_no += 1
            arch_prompt = ArchitectStep().prompt(context)
            out = self._call("architect", self.prompts["architect"].compose_system(), arch_prompt)
            path = self._output_writer.write(f"{self.ts}-{self.profile}-architect-r{round_no}.md", out)
            self.ui.write_line(f"Architect output: {path}")
            if self.ui.ask_yes_no("Approve architecture (Gate A)?", default_yes=False):
                context.architect_out = out
                return out, path
            context.feedback = self.ui.ask_required("Provide architecture rework feedback")

    def _run_planning_phase(self, context: WorkflowContext) -> tuple[str, Path, str, Path]:
        self.ui.write_line("\nPhase 2: DevOps + Developer planning")
        context.architect_out = context.architect_out or ""

        devops_prompt = DevOpsStep().prompt(context)
        developer_prompt = DeveloperStep().prompt(context)

        with concurrent.futures.ThreadPoolExecutor(max_workers=2) as executor:
            devops_future = executor.submit(self._call, "devops", self.prompts["devops"].compose_system(), devops_prompt)
            developer_future = executor.submit(self._call, "developer", self.prompts["developer"].compose_system(), developer_prompt)
            devops_out = devops_future.result()
            developer_out = developer_future.result()

        devops_round = 1
        developer_round = 1
        devops_path = self._output_writer.write(f"{self.ts}-{self.profile}-devops-r{devops_round}.md", devops_out)
        developer_path = self._output_writer.write(f"{self.ts}-{self.profile}-developer-r{developer_round}.md", developer_out)
        self.ui.write_line(f"DevOps plan: {devops_path}")
        self.ui.write_line(f"Developer plan: {developer_path}")

        while not self.ui.ask_yes_no("Approve DevOps plan (Gate B)?", default_yes=False):
            context.feedback = self.ui.ask_required("Provide DevOps rework feedback")
            devops_round += 1
            devops_out = self._call("devops", self.prompts["devops"].compose_system(), DevOpsStep().prompt(context))
            devops_path = self._output_writer.write(f"{self.ts}-{self.profile}-devops-r{devops_round}.md", devops_out)
            self.ui.write_line(f"Revised DevOps plan: {devops_path}")

        while not self.ui.ask_yes_no("Approve Developer plan (Gate C)?", default_yes=False):
            context.feedback = self.ui.ask_required("Provide Developer rework feedback")
            developer_round += 1
            developer_out = self._call("developer", self.prompts["developer"].compose_system(), DeveloperStep().prompt(context))
            developer_path = self._output_writer.write(f"{self.ts}-{self.profile}-developer-r{developer_round}.md", developer_out)
            self.ui.write_line(f"Revised Developer plan: {developer_path}")

        context.devops_out = devops_out
        context.developer_out = developer_out
        return devops_out, devops_path, developer_out, developer_path

    def _wait_for_implementation_gate(self) -> None:
        self.ui.write_line("\nPhase 3: Implementation review gate")
        while not self.ui.ask_yes_no("Have implementation PRs been reviewed and approved/merged (Gate D)?", default_yes=False):
            self.ui.write_line("Waiting for implementation review/merge. Complete reviews, then approve Gate D to continue.")

    def _run_qa_phase(self, context: WorkflowContext) -> tuple[str, Path]:
        self.ui.write_line("\nPhase 4: QA rework and test plan")
        context.devops_out = context.devops_out or ""
        context.developer_out = context.developer_out or ""

        qa_out = self._call("qa", self.prompts["qa"].compose_system(), QaStep().prompt(context))
        qa_path = self._output_writer.write(f"{self.ts}-{self.profile}-qa.md", qa_out)
        self.ui.write_line(f"QA output: {qa_path}")
        return qa_out, qa_path

    def _call(self, role: str, system_prompt: str, user_prompt: str) -> str:
        agent = self._create_agent(role, system_prompt)
        return agent.run(user_prompt)
