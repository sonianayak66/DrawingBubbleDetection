from __future__ import annotations

import json
from pathlib import Path
from typing import Iterable, List, Tuple

import cv2
import numpy as np

from .contracts import (
    Assignment,
    BalloonCandidate,
    BalloonOcrResult,
    LeaderTrace,
    PipelineV2Result,
    TextCandidate,
)


class DebugWriter:
    def __init__(self, debug_dir: str | Path):
        self.debug_dir = Path(debug_dir)
        self.debug_dir.mkdir(parents=True, exist_ok=True)

    def write_image(self, name: str, image: np.ndarray) -> None:
        cv2.imwrite(str(self.debug_dir / name), image)

    def write_json(self, name: str, payload) -> None:
        with open(self.debug_dir / name, "w", encoding="utf-8") as f:
            json.dump(payload, f, indent=2)

    def write_result(self, result: PipelineV2Result) -> None:
        self.write_json("result.json", result.to_dict())


def overlay_balloons(image: np.ndarray, balloons: Iterable[BalloonCandidate]) -> np.ndarray:
    out = image.copy()
    for b in balloons:
        cx, cy = int(round(b.center[0])), int(round(b.center[1]))
        r = int(round(b.radius))
        cv2.circle(out, (cx, cy), r, (0, 255, 0), 2)
        x1, y1, x2, y2 = b.bbox
        cv2.rectangle(out, (x1, y1), (x2, y2), (0, 180, 0), 1)
        cv2.putText(out, f"{b.candidate_id} {b.confidence:.2f}", (x1, max(15, y1 - 5)),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.45, (0, 120, 0), 1, cv2.LINE_AA)
    return out


def overlay_components(mask: np.ndarray) -> np.ndarray:
    num, labels, _stats, _centroids = cv2.connectedComponentsWithStats(mask, connectivity=8)
    rng = np.random.default_rng(12345)
    colors = rng.integers(40, 255, size=(max(num, 1), 3), dtype=np.uint8)
    colors[0] = np.array([0, 0, 0], dtype=np.uint8)
    return colors[labels]


def overlay_leaders(image: np.ndarray, leaders: List[LeaderTrace]) -> np.ndarray:
    out = image.copy()
    for leader in leaders:
        if leader.method == "color_component":
            color = (0, 255, 0)
        elif leader.method == "weak_color_component":
            color = (255, 255, 0)
        elif leader.method == "grayscale_hough":
            color = (0, 200, 255)
        else:
            color = (0, 0, 255)
        if leader.status not in ("found", "low_confidence", "ambiguous"):
            color = (0, 0, 255)
        if len(leader.polyline) >= 2:
            pts = np.array(leader.polyline, dtype=np.int32).reshape((-1, 1, 2))
            cv2.polylines(out, [pts], False, color, 2, cv2.LINE_AA)
        if leader.source_endpoint:
            cv2.circle(out, leader.source_endpoint, 4, (255, 0, 0), -1)
        if leader.target_endpoint:
            cv2.circle(out, leader.target_endpoint, 5, (0, 0, 255), -1)
            cv2.putText(out, f"{leader.balloon_candidate_id} s{leader.fallback_stage}:{leader.method} {leader.confidence:.2f}",
                        (leader.target_endpoint[0] + 6, leader.target_endpoint[1] - 6),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.45, color, 1, cv2.LINE_AA)
    return out


def overlay_text_candidates(image: np.ndarray, candidates: List[TextCandidate]) -> np.ndarray:
    out = image.copy()
    for cand in candidates:
        color = (80, 80, 80) if cand.suppressed else (255, 0, 0)
        x1, y1, x2, y2 = cand.bbox
        cv2.rectangle(out, (x1, y1), (x2, y2), color, 1)
        cv2.putText(out, f"{cand.candidate_id}:{cand.text}", (x1, max(15, y1 - 4)),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.4, color, 1, cv2.LINE_AA)
    return out


def overlay_suppressed_regions(image: np.ndarray, regions: List[Tuple[int, int, int, int]]) -> np.ndarray:
    out = image.copy()
    shade = out.copy()
    for x1, y1, x2, y2 in regions:
        cv2.rectangle(shade, (x1, y1), (x2, y2), (0, 180, 255), -1)
        cv2.rectangle(out, (x1, y1), (x2, y2), (0, 120, 255), 2)
    out = cv2.addWeighted(shade, 0.20, out, 0.80, 0)
    return out


def overlay_assignments(
    image: np.ndarray,
    balloons: List[BalloonCandidate],
    leaders: List[LeaderTrace],
    assignments: List[Assignment],
) -> np.ndarray:
    out = image.copy()
    balloons_by_id = {b.candidate_id: b for b in balloons}
    leaders_by_id = {l.balloon_candidate_id: l for l in leaders}
    for assignment in assignments:
        balloon = balloons_by_id.get(assignment.balloon_candidate_id)
        if balloon is None:
            continue
        cx, cy = int(round(balloon.center[0])), int(round(balloon.center[1]))
        color = (0, 255, 0) if not assignment.review_required else (0, 165, 255)
        cv2.circle(out, (cx, cy), int(round(balloon.radius)), color, 2)
        leader = leaders_by_id.get(balloon.candidate_id)
        if leader and leader.target_endpoint:
            cv2.line(out, (cx, cy), leader.target_endpoint, color, 1, cv2.LINE_AA)
        if assignment.dimension_bbox:
            x1, y1, x2, y2 = assignment.dimension_bbox
            cv2.rectangle(out, (x1, y1), (x2, y2), color, 2)
        cv2.putText(out, f"{assignment.balloon_id}->{assignment.dimension_text} {assignment.confidence:.2f}",
                    (cx + 6, cy - 6), cv2.FONT_HERSHEY_SIMPLEX, 0.45, color, 1, cv2.LINE_AA)
    return out


def overlay_schema_view(
    image: np.ndarray,
    balloons: List[BalloonCandidate],
    balloon_ocr: List[BalloonOcrResult],
    leaders: List[LeaderTrace],
    text_candidates: List[TextCandidate],
    assignments: List[Assignment],
) -> np.ndarray:
    """Black-canvas schema: bubbles + leaders + dim text boxes with labels.

    Strips the drawing entirely and shows only the structure the
    detector is reasoning over — useful both for diagnosing assignment
    failures and as a clean canvas for human-in-the-loop labelling
    (and later, as the natural input shape for a learned relation
    resolver).

    Layers:
      * magenta circles + OCR'd number  — bubbles
      * cyan polylines + yellow target dot  — found leaders
      * green text bboxes with the OCR'd text  — dimension candidates
      * thin grey link  — detector's current bubble -> dim assignment
        (orange when the assignment is needs_review)
    """
    out = np.zeros_like(image)
    h, w = out.shape[:2]
    ocr_by_balloon = {o.balloon_candidate_id: o for o in balloon_ocr}

    for b in balloons:
        cx, cy = int(round(b.center[0])), int(round(b.center[1]))
        r = int(round(b.radius))
        cv2.circle(out, (cx, cy), r, (255, 0, 255), 2)
        ocr = ocr_by_balloon.get(b.candidate_id)
        label = ocr.text if ocr and ocr.text else "?"
        (tw, th), _ = cv2.getTextSize(label, cv2.FONT_HERSHEY_SIMPLEX, 0.7, 2)
        cv2.putText(out, label, (cx - tw // 2, cy + th // 2),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 0, 255), 2, cv2.LINE_AA)

    for leader in leaders:
        if leader.status not in ("found", "low_confidence", "ambiguous"):
            continue
        if len(leader.polyline) >= 2:
            pts = np.array(leader.polyline, dtype=np.int32).reshape((-1, 1, 2))
            cv2.polylines(out, [pts], False, (255, 255, 0), 2, cv2.LINE_AA)
        if leader.target_endpoint:
            cv2.circle(out, leader.target_endpoint, 4, (0, 255, 255), -1)

    for cand in text_candidates:
        if cand.suppressed:
            continue
        x1, y1, x2, y2 = cand.bbox
        cv2.rectangle(out, (x1, y1), (x2, y2), (0, 255, 0), 1)
        cv2.putText(out, cand.text, (x1, min(h - 4, y2 + 14)),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.45, (0, 255, 0), 1, cv2.LINE_AA)

    balloons_by_id = {b.candidate_id: b for b in balloons}
    for a in assignments:
        if not a.dimension_bbox:
            continue
        balloon = balloons_by_id.get(a.balloon_candidate_id)
        if not balloon:
            continue
        bx, by = int(round(balloon.center[0])), int(round(balloon.center[1]))
        x1, y1, x2, y2 = a.dimension_bbox
        dx, dy = (x1 + x2) // 2, (y1 + y2) // 2
        color = (60, 165, 255) if a.review_required else (200, 200, 200)
        cv2.line(out, (bx, by), (dx, dy), color, 1, cv2.LINE_AA)

    return out


def overlay_unified_view(
    image: np.ndarray,
    balloons: List[BalloonCandidate],
    balloon_ocr: List[BalloonOcrResult],
    leaders: List[LeaderTrace],
    text_candidates: List[TextCandidate],
) -> np.ndarray:
    """Unified white-on-black ink view of the detected structure.

    Strips colour entirely — every detected feature (bubbles + numbers,
    leader segments and target dots, dimension-text bboxes and the
    OCR'd text itself) is rendered as plain WHITE on a black canvas.

    Rationale: source drawings use a mix of red/magenta/purple/maroon
    annotations across uploads, but the dimension text is always
    black. A color-coded schema view (14_) is good for human eyes;
    this binary view is colour-agnostic, gives a clean topology for
    connected-component reasoning, and is the natural single-channel
    input shape for a future learned relation resolver.

    No assignment links are drawn — this is a "structure only" view
    of what the detector found, not what it guessed.
    """
    out = np.zeros_like(image)
    h, w = out.shape[:2]
    WHITE = (255, 255, 255)
    ocr_by_balloon = {o.balloon_candidate_id: o.text for o in balloon_ocr}

    # Bubbles + their OCR'd number inside the circle
    for b in balloons:
        cx, cy = int(round(b.center[0])), int(round(b.center[1]))
        r = int(round(b.radius))
        cv2.circle(out, (cx, cy), r, WHITE, 2)
        label = ocr_by_balloon.get(b.candidate_id) or "?"
        (tw, th), _ = cv2.getTextSize(label, cv2.FONT_HERSHEY_SIMPLEX, 0.7, 2)
        cv2.putText(out, label, (cx - tw // 2, cy + th // 2),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.7, WHITE, 2, cv2.LINE_AA)

    # Leader segments + target endpoints
    for leader in leaders:
        if leader.status not in ("found", "low_confidence", "ambiguous"):
            continue
        if len(leader.polyline) >= 2:
            pts = np.array(leader.polyline, dtype=np.int32).reshape((-1, 1, 2))
            cv2.polylines(out, [pts], False, WHITE, 2, cv2.LINE_AA)
        if leader.target_endpoint:
            cv2.circle(out, leader.target_endpoint, 4, WHITE, -1)

    # Dimension text bboxes — outline + the OCR'd text rendered inside
    # (centered) so a reader can SEE what each box says without colour.
    for cand in text_candidates:
        if cand.suppressed:
            continue
        x1, y1, x2, y2 = cand.bbox
        cv2.rectangle(out, (x1, y1), (x2, y2), WHITE, 1)
        text = cand.text or ""
        if not text:
            continue
        # Fit the text inside the box if it's tall enough; otherwise
        # drop it just below to avoid overlapping the bbox border.
        font_scale = 0.5
        (tw, th), _ = cv2.getTextSize(text, cv2.FONT_HERSHEY_SIMPLEX, font_scale, 1)
        if (y2 - y1) >= th + 6 and (x2 - x1) >= tw + 4:
            tx = x1 + ((x2 - x1) - tw) // 2
            ty = y1 + ((y2 - y1) + th) // 2
        else:
            tx = x1
            ty = min(h - 4, y2 + th + 2)
        cv2.putText(out, text, (tx, ty),
                    cv2.FONT_HERSHEY_SIMPLEX, font_scale, WHITE, 1, cv2.LINE_AA)

    return out


def overlay_leader_candidates(image: np.ndarray, candidate_report: dict) -> np.ndarray:
    out = image.copy()
    method_colors = {
        "color_component": (0, 255, 0),
        "weak_color_component": (255, 255, 0),
        "grayscale_hough": (0, 200, 255),
    }
    for balloon in candidate_report.get("balloons", []):
        selected = balloon.get("selected") or {}
        selected_source = selected.get("source_endpoint")
        selected_target = selected.get("target_endpoint")
        for cand in balloon.get("candidates", []):
            line = cand.get("line")
            source = cand.get("source_endpoint")
            target = cand.get("target_endpoint")
            if line and len(line) == 2:
                source, target = line[0], line[1]
            if not source or not target:
                continue
            method = cand.get("method", "none")
            accepted = bool(cand.get("accepted"))
            is_selected = selected_source == source and selected_target == target
            color = method_colors.get(method, (120, 120, 120))
            if not accepted and not is_selected:
                color = (90, 90, 90) if not cand.get("rejection_reason") else (0, 0, 220)
            thickness = 3 if is_selected or accepted else 1
            p1 = (int(source[0]), int(source[1]))
            p2 = (int(target[0]), int(target[1]))
            cv2.line(out, p1, p2, color, thickness, cv2.LINE_AA)
            label = f"{method.split('_')[0]} {float(cand.get('score', 0.0) or 0.0):.2f}"
            cv2.putText(out, label, (p2[0] + 4, max(12, p2[1] - 4)),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.36, color, 1, cv2.LINE_AA)

        if selected_target:
            cv2.circle(out, (int(selected_target[0]), int(selected_target[1])), 5, (255, 0, 255), -1)
            cv2.putText(out, f"{balloon.get('balloon_candidate_id')}:{selected.get('method')}:{selected.get('status')}",
                        (int(selected_target[0]) + 7, int(selected_target[1]) + 14),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.42, (255, 0, 255), 1, cv2.LINE_AA)
    return out
