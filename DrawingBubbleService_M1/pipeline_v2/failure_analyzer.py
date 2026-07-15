from __future__ import annotations

from collections import Counter
from dataclasses import asdict
from typing import Any, Dict, List, Tuple

import cv2
import numpy as np

from .contracts import Assignment, BalloonCandidate, BalloonOcrResult, LeaderTrace, PipelineV2Result


def _avg(values: List[float]) -> float:
    return round(float(sum(values) / len(values)), 4) if values else 0.0


def _leader_reasons(leader: LeaderTrace, segmentation_pixel_count: int) -> List[str]:
    reasons: List[str] = []
    metrics = leader.debug_metrics or {}

    if segmentation_pixel_count < 100:
        reasons.append("magenta_mask_too_sparse")
        reasons.append("no_color_evidence")

    if leader.method == "weak_color_component":
        reasons.append("weak_color_fallback_used")
    elif leader.method == "grayscale_hough":
        reasons.append("grayscale_hough_fallback_used")
        reasons.append("possible_black_leader_detected")
        if metrics.get("color_trace_failed_but_hough_succeeded"):
            reasons.append("color_trace_failed_but_hough_succeeded")
        if metrics.get("grayscale_hough_ambiguous") or leader.status == "ambiguous":
            reasons.append("grayscale_hough_ambiguous")
        if metrics.get("grayscale_hough_endpoint_unstable"):
            reasons.append("grayscale_hough_endpoint_unstable")
            reasons.append("grayscale_hough_multiple_competing_directions")
        if metrics.get("drawing_line_penalty", 0.0) > 0:
            reasons.append("grayscale_hough_long_drawing_line_penalty")
        if leader.status == "not_found":
            reasons.append("grayscale_hough_no_candidate")

    if leader.status == "not_found":
        reasons.append("no_component_touching_boundary")
        reasons.append("missing_colored_leader_stroke")
        reason = str(metrics.get("reason", ""))
        if "no line-like component" in reason:
            reasons.append("component_not_line_like")
        return reasons

    if leader.status == "low_confidence":
        reasons.append("leader_endpoint_uncertain")
        if leader.method == "grayscale_hough":
            reasons.append("fallback_endpoint_low_confidence")

    line_like = float(metrics.get("line_likeness_score", metrics.get("radial_alignment", 0.0)) or 0.0)
    outward = float(metrics.get("outward_extension_score", 0.0) or 0.0)
    arc = float(metrics.get("arc_penalty", 0.0) or 0.0)

    if line_like < 0.35:
        reasons.append("component_not_line_like")
    if outward < 0.20:
        reasons.append("component_does_not_extend_outward")
    if arc > 0.50:
        reasons.append("component_mostly_arc")
    if outward < 0.20 and arc > 0.50:
        reasons.append("missing_colored_leader_stroke")
    if leader.target_endpoint is None:
        reasons.append("leader_endpoint_uncertain")

    return list(dict.fromkeys(reasons))


def _assignment_reasons(
    assignment: Assignment,
    leader: LeaderTrace | None,
    nearby_dimension_count: int,
) -> List[str]:
    reasons: List[str] = []
    if leader is None:
        reasons.append("missing_colored_leader_stroke")
    elif leader.status != "found":
        reasons.append("leader_endpoint_uncertain")

    if assignment.dimension_text == "NO_DIMENSION":
        reasons.append("no_dimension_ocr_near_endpoint")
    if assignment.review_reason:
        if assignment.review_reason == "dimension_inside_suppressed_region":
            reasons.append("dimension_candidate_suppressed")
        elif assignment.review_reason == "low_confidence":
            reasons.append("association_low_confidence")
        else:
            reasons.append(assignment.review_reason)
    if nearby_dimension_count > 1:
        reasons.append("multiple_dimension_candidates_close")
    if assignment.confidence and assignment.confidence < 0.65:
        reasons.append("association_low_confidence")
    return list(dict.fromkeys(reasons))


def _nearby_dimension_count(result: PipelineV2Result, leader: LeaderTrace | None, max_dist: float = 120.0) -> int:
    if leader is None or leader.target_endpoint is None:
        return 0
    ex, ey = leader.target_endpoint
    count = 0
    for cand in result.text_candidates:
        if cand.suppressed:
            continue
        x1, y1, x2, y2 = cand.bbox
        dx = max(x1 - ex, 0.0, ex - x2)
        dy = max(y1 - ey, 0.0, ey - y2)
        if float(np.hypot(dx, dy)) <= max_dist:
            count += 1
    return count


def build_metrics(result: PipelineV2Result, processing_time_ms: float = 0.0) -> Dict[str, Any]:
    balloons = result.balloons
    leaders = result.leaders
    assignments = result.assignments
    leader_found = [l for l in leaders if l.status == "found"]
    assigned = [a for a in assignments if a.dimension_text != "NO_DIMENSION"]
    review = [a for a in assignments if a.review_required]

    # Per-method / per-status leader breakdown so the evaluator can
    # tell whether a "found" rate is coming from reliable colour
    # tracing or from the lower-confidence grayscale Hough fallback.
    def _found(method: str) -> int:
        return sum(1 for l in leaders if l.method == method and l.status == "found")

    ambiguous_leaders = [l for l in leaders if l.status == "ambiguous"]
    low_conf_leaders = [l for l in leaders if l.status == "low_confidence"]
    no_leaders = [
        l for l in leaders
        if l.status not in ("found", "ambiguous", "low_confidence")
    ]

    return {
        "image_name": result.image,
        "detected_balloon_count": len(balloons),
        "leader_found_count": len(leader_found),
        "leader_found_rate": round(len(leader_found) / max(len(balloons), 1), 4),
        "color_component_found_count": _found("color_component"),
        "weak_color_component_found_count": _found("weak_color_component"),
        "grayscale_hough_found_count": _found("grayscale_hough"),
        "ambiguous_leader_count": len(ambiguous_leaders),
        "low_confidence_leader_count": len(low_conf_leaders),
        "no_leader_count": len(no_leaders),
        "assignment_count": len(assigned),
        "assignment_rate": round(len(assigned) / max(len(balloons), 1), 4),
        "review_required_count": len(review),
        "average_balloon_confidence": _avg([b.confidence for b in balloons]),
        "average_leader_confidence": _avg([l.confidence for l in leaders]),
        "average_assignment_confidence": _avg([a.confidence for a in assignments]),
        "processing_time_ms": round(float(processing_time_ms), 2),
        "has_ground_truth": False,
        "ground_truth_metrics": None,
        "errors": [],
    }


def build_failure_report(result: PipelineV2Result) -> Dict[str, Any]:
    leaders_by_balloon = {l.balloon_candidate_id: l for l in result.leaders}
    ocr_by_balloon = {o.balloon_candidate_id: o for o in result.balloon_ocr}
    assignment_by_balloon = {a.balloon_candidate_id: a for a in result.assignments}

    reason_counter: Counter[str] = Counter()
    balloon_failures: List[Dict[str, Any]] = []
    leader_failures: List[Dict[str, Any]] = []
    assignment_failures: List[Dict[str, Any]] = []

    for leader in result.leaders:
        reasons = _leader_reasons(leader, result.segmentation.pixel_count)
        reason_counter.update(reasons)
        leader_failures.append({
            "leader_candidate_id": f"l_{leader.balloon_candidate_id}",
            "balloon_candidate_id": leader.balloon_candidate_id,
            "status": leader.status,
            "confidence": round(float(leader.confidence), 4),
            "method": leader.method,
            "fallback_stage": leader.fallback_stage,
            "debug_metrics": leader.debug_metrics,
            "reasons": reasons,
        })

    for balloon in result.balloons:
        leader = leaders_by_balloon.get(balloon.candidate_id)
        ocr = ocr_by_balloon.get(balloon.candidate_id)
        assignment = assignment_by_balloon.get(balloon.candidate_id)
        reasons: List[str] = []

        if balloon.confidence < 0.55:
            reasons.append("circle_shape_low_confidence")
        if ocr is None or ocr.status != "found" or ocr.confidence < 0.55:
            reasons.append("OCR_low_confidence")
        if leader is None:
            status = "no_leader_found"
            reasons.append("missing_colored_leader_stroke")
        elif leader.status == "found":
            status = "assigned" if assignment and assignment.dimension_text != "NO_DIMENSION" else "no_dimension_candidate"
        elif leader.status == "low_confidence":
            status = "leader_low_confidence"
        elif leader.status == "ambiguous":
            status = "leader_low_confidence"
        else:
            status = "no_leader_found"

        if assignment:
            nearby_count = _nearby_dimension_count(result, leader)
            assignment_reasons = _assignment_reasons(assignment, leader, nearby_count)
            if assignment.dimension_text == "NO_DIMENSION":
                status = "no_dimension_candidate" if leader and leader.status == "found" else status
            elif assignment.review_required:
                status = "association_low_confidence"
            reasons.extend(assignment_reasons)

        reasons.extend(_leader_reasons(leader, result.segmentation.pixel_count) if leader else [])
        reasons = list(dict.fromkeys(reasons))
        reason_counter.update(reasons)
        balloon_failures.append({
            "balloon_candidate_id": balloon.candidate_id,
            "balloon_id": ocr.text if ocr and ocr.text else None,
            "status": status,
            "confidence": round(float(assignment.confidence if assignment else balloon.confidence), 4),
            "reasons": reasons,
        })

    for assignment in result.assignments:
        leader = leaders_by_balloon.get(assignment.balloon_candidate_id)
        nearby_count = _nearby_dimension_count(result, leader)
        reasons = _assignment_reasons(assignment, leader, nearby_count)
        reason_counter.update(reasons)
        if assignment.dimension_text == "NO_DIMENSION" or assignment.review_required:
            assignment_failures.append({
                "balloon_candidate_id": assignment.balloon_candidate_id,
                "leader_status": leader.status if leader else "not_found",
                "assignment_status": "assigned_review" if assignment.dimension_text != "NO_DIMENSION" else "not_assigned",
                "reasons": reasons,
            })

    leaders_found = sum(1 for l in result.leaders if l.status == "found")
    assignments = sum(1 for a in result.assignments if a.dimension_text != "NO_DIMENSION")
    return {
        "image_path": result.image,
        "summary": {
            "balloons": len(result.balloons),
            "leaders_found": leaders_found,
            "assignments": assignments,
            "review_required": any(a.review_required for a in result.assignments),
            "top_failure_reasons": [
                {"reason": reason, "count": count}
                for reason, count in reason_counter.most_common(8)
            ],
        },
        "balloon_failures": balloon_failures,
        "leader_failures": leader_failures,
        "assignment_failures": assignment_failures,
    }


def build_leader_candidates_report(result: PipelineV2Result) -> Dict[str, Any]:
    leaders_by_balloon = {l.balloon_candidate_id: l for l in result.leaders}
    balloons = []
    for balloon in result.balloons:
        leader = leaders_by_balloon.get(balloon.candidate_id)
        selected = None
        candidates: List[Dict[str, Any]] = []
        if leader is not None:
            selected = {
                "method": leader.method,
                "status": leader.status,
                "confidence": round(float(leader.confidence), 4),
                "source_endpoint": list(leader.source_endpoint) if leader.source_endpoint else None,
                "target_endpoint": list(leader.target_endpoint) if leader.target_endpoint else None,
            }
            stages = leader.debug_metrics.get("stages", {}) if leader.debug_metrics else {}
            for stage_name in ("stage1", "stage2", "stage3"):
                stage = stages.get(stage_name)
                if not stage:
                    continue
                metrics = stage.get("metrics", {}) or {}
                candidate = {
                    "method": stage.get("method", "none"),
                    "stage": int(stage_name.replace("stage", "")),
                    "status": stage.get("status", "not_found"),
                    "score": float(metrics.get("total_score", 0.0) or 0.0),
                    "source_endpoint": None,
                    "target_endpoint": None,
                    "debug_metrics": metrics,
                    "reasons": _leader_reasons(
                        LeaderTrace(
                            balloon_candidate_id=balloon.candidate_id,
                            polyline=[],
                            target_endpoint=None,
                            source_endpoint=None,
                            component_id=None,
                            confidence=float(stage.get("confidence", 0.0) or 0.0),
                            status=str(stage.get("status", "not_found")),
                            method=str(stage.get("method", "none")),
                            fallback_stage=int(stage_name.replace("stage", "")),
                            debug_metrics=metrics,
                        ),
                        result.segmentation.pixel_count,
                    ),
                }
                candidates.append(candidate)
                if stage.get("method") == "grayscale_hough":
                    candidates.extend(metrics.get("top_candidates", [])[:10])

            if leader.method == "grayscale_hough" and not any(c.get("method") == "grayscale_hough" and c.get("accepted") for c in candidates):
                candidates.extend(leader.debug_metrics.get("top_candidates", [])[:10])

        balloons.append({
            "balloon_candidate_id": balloon.candidate_id,
            "selected": selected,
            "candidates": candidates,
        })
    return {
        "image_path": result.image,
        "balloons": balloons,
    }


def overlay_failure_analysis(image: np.ndarray, result: PipelineV2Result, failure_report: Dict[str, Any]) -> np.ndarray:
    out = image.copy()
    balloons_by_id = {b.candidate_id: b for b in result.balloons}
    leaders_by_id = {l.balloon_candidate_id: l for l in result.leaders}
    failure_by_balloon = {f["balloon_candidate_id"]: f for f in failure_report.get("balloon_failures", [])}

    for balloon_id, failure in failure_by_balloon.items():
        balloon = balloons_by_id.get(balloon_id)
        if balloon is None:
            continue
        status = failure.get("status", "")
        if status == "assigned":
            color = (0, 200, 0)
        elif status in ("leader_low_confidence", "association_low_confidence", "low_confidence"):
            color = (0, 200, 255)
        else:
            color = (0, 0, 255)

        cx, cy = int(round(balloon.center[0])), int(round(balloon.center[1]))
        cv2.circle(out, (cx, cy), int(round(balloon.radius)), color, 2)
        leader = leaders_by_id.get(balloon_id)
        if leader and leader.target_endpoint:
            cv2.circle(out, leader.target_endpoint, 5, color, -1)
            if len(leader.polyline) >= 2:
                pts = np.array(leader.polyline, dtype=np.int32).reshape((-1, 1, 2))
                cv2.polylines(out, [pts], False, color, 2, cv2.LINE_AA)

        reasons = failure.get("reasons", [])
        label_reason = reasons[0] if reasons else status
        method = leaders_by_id.get(balloon_id).method if leaders_by_id.get(balloon_id) else "none"
        label = f"{balloon_id}:{method}:{label_reason}"
        cv2.putText(out, label, (cx + 6, max(15, cy - 8)),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.42, color, 1, cv2.LINE_AA)

    return out
