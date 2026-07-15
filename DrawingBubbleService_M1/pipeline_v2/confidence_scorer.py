from __future__ import annotations

from .contracts import Assignment


def requires_review(assignment: Assignment, threshold: float = 0.65) -> bool:
    return assignment.review_required or assignment.confidence < threshold

