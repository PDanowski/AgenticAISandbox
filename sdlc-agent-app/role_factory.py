from __future__ import annotations

from abc import ABC, abstractmethod

from providers import RoleAgent, RoleAgentFactory


class WorkflowContext:
    def __init__(
        self,
        pack_key: str,
        provider: str,
        profile: str,
        feature: str,
        model_name: str,
    ) -> None:
        self.pack_key = pack_key
        self.provider = provider
        self.profile = profile
        self.feature = feature
        self.model_name = model_name
        self.architect_out: str = ""
        self.devops_out: str = ""
        self.developer_out: str = ""
        self.qa_out: str = ""
        self.feedback: str = ""


class WorkflowStep(ABC):
    @abstractmethod
    def prompt(self, context: WorkflowContext) -> str:
        raise NotImplementedError

    @abstractmethod
    def filename(self, timestamp: str, profile: str, round_number: int) -> str:
        raise NotImplementedError

    @abstractmethod
    def role(self) -> str:
        raise NotImplementedError


class ArchitectStep(WorkflowStep):
    def role(self) -> str:
        return "architect"

    def prompt(self, context: WorkflowContext) -> str:
        feedback_section = "none" if not context.feedback else context.feedback
        return f"""
        Feature request:
        {context.feature}

        Prior feedback to address:
        {feedback_section}

        Produce:
        1) Clarification questions and assumptions log
        2) Architecture summary
        3) Mermaid diagrams (component, sequence, flow)
        4) Work items split by DevOps/Developer/QA
        5) Risks/assumptions
        6) PR architecture checklist
        7) Gate A approval summary
        """.strip()

    def filename(self, timestamp: str, profile: str, round_number: int) -> str:
        return f"{timestamp}-{profile}-architect-r{round_number}.md"


class DevOpsStep(WorkflowStep):
    def role(self) -> str:
        return "devops"

    def prompt(self, context: WorkflowContext) -> str:
        feedback_section = f"\n\nRework feedback:\n{context.feedback}" if context.feedback else ""
        return f"""
        Feature request:
        {context.feature}

        Approved architecture:
        {context.architect_out}

        Produce implementation-ready DevOps plan and Gate B approval summary.{feedback_section}
        """.strip()

    def filename(self, timestamp: str, profile: str, round_number: int) -> str:
        return f"{timestamp}-{profile}-devops-r{round_number}.md"


class DeveloperStep(WorkflowStep):
    def role(self) -> str:
        return "developer"

    def prompt(self, context: WorkflowContext) -> str:
        feedback_section = f"\n\nRework feedback:\n{context.feedback}" if context.feedback else ""
        return f"""
        Feature request:
        {context.feature}

        Approved architecture:
        {context.architect_out}

        Produce implementation-ready Developer plan and Gate C approval summary.{feedback_section}
        """.strip()

    def filename(self, timestamp: str, profile: str, round_number: int) -> str:
        return f"{timestamp}-{profile}-developer-r{round_number}.md"


class QaStep(WorkflowStep):
    def role(self) -> str:
        return "qa"

    def prompt(self, context: WorkflowContext) -> str:
        return f"""
        Feature request:
        {context.feature}

        Approved architecture:
        {context.architect_out}

        Approved DevOps plan:
        {context.devops_out}

        Approved Developer plan:
        {context.developer_out}

        Produce:
        1) QA rework traceability based on implemented scope
        2) Test strategy per work item
        3) Smoke/API/regression/e2e set
        4) Pipeline integration approach
        5) Defect/risk reporting model
        6) Release recommendation criteria and residual risks
        """.strip()

    def filename(self, timestamp: str, profile: str, round_number: int) -> str:
        return f"{timestamp}-{profile}-qa.md"
