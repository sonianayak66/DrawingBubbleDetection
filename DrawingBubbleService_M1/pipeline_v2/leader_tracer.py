from __future__ import annotations

from collections import deque
from typing import Any, Dict, List, Optional, Tuple

import cv2
import numpy as np

from .contracts import BalloonCandidate, LeaderTrace, Point


FOUND_THRESHOLD = 0.45


def _line_point_distance(px: float, py: float, ax: float, ay: float, bx: float, by: float) -> float:
    vx, vy = bx - ax, by - ay
    wx, wy = px - ax, py - ay
    denom = vx * vx + vy * vy
    if denom <= 1e-6:
        return float(np.hypot(px - ax, py - ay))
    t = max(0.0, min(1.0, (wx * vx + wy * vy) / denom))
    qx, qy = ax + t * vx, ay + t * vy
    return float(np.hypot(px - qx, py - qy))


def _trace_mask_hough(local: np.ndarray, offset: Tuple[int, int], balloon: BalloonCandidate, *, method: str, stage: int) -> Optional[LeaderTrace]:
    cx, cy = balloon.center
    radius = max(1.0, float(balloon.radius))
    ox, oy = offset
    min_len = max(12, int(radius * 0.55))
    gap = max(5, int(radius * 0.20))
    lines = cv2.HoughLinesP(
        local,
        rho=1,
        theta=np.pi / 180.0,
        threshold=max(8, int(radius * 0.22)),
        minLineLength=min_len,
        maxLineGap=gap,
    )
    if lines is None:
        return None

    best = None
    boundary_tol = max(5.0, radius * 0.45)
    for idx, raw in enumerate(lines[:, 0, :], start=1):
        x1, y1, x2, y2 = [int(v) for v in raw]
        ax, ay = x1 + ox, y1 + oy
        bx, by = x2 + ox, y2 + oy
        length = float(np.hypot(bx - ax, by - ay))
        if length < min_len:
            continue

        d1 = float(np.hypot(ax - cx, ay - cy))
        d2 = float(np.hypot(bx - cx, by - cy))
        min_boundary_delta = min(abs(d1 - radius), abs(d2 - radius), _line_point_distance(cx, cy, ax, ay, bx, by))
        if min_boundary_delta > boundary_tol:
            continue

        far_endpoint = (int(ax), int(ay)) if d1 >= d2 else (int(bx), int(by))
        near_endpoint = (int(bx), int(by)) if d1 >= d2 else (int(ax), int(ay))
        far_dist = max(d1, d2)
        outward_extension = max(0.0, far_dist - radius)
        if outward_extension < radius * 0.35:
            continue

        # Prefer segments that radiate away from the balloon center.
        vx = far_endpoint[0] - near_endpoint[0]
        vy = far_endpoint[1] - near_endpoint[1]
        rx = far_endpoint[0] - cx
        ry = far_endpoint[1] - cy
        denom = max(float(np.hypot(vx, vy) * np.hypot(rx, ry)), 1e-6)
        radial_alignment = max(0.0, (vx * rx + vy * ry) / denom)
        length_score = min(1.0, length / max(radius * 3.0, 1.0))
        boundary_score = max(0.0, 1.0 - min_boundary_delta / boundary_tol)
        extension_score = min(1.0, outward_extension / max(radius * 3.0, 1.0))
        score = 2.0 * boundary_score + 2.0 * extension_score + 1.5 * radial_alignment + length_score

        if best is None or score > best["score"]:
            best = {
                "score": score,
                "line_id": idx,
                "source": near_endpoint,
                "target": far_endpoint,
                "length": length,
                "boundary_score": boundary_score,
                "extension_score": extension_score,
                "radial_alignment": radial_alignment,
                "min_boundary_delta": min_boundary_delta,
            }

    if best is None:
        return None

    confidence = float(max(0.0, min(1.0, best["score"] / 5.8)))
    return LeaderTrace(
        balloon_candidate_id=balloon.candidate_id,
        polyline=[best["source"], best["target"]],
        target_endpoint=best["target"],
        source_endpoint=best["source"],
        component_id=int(best["line_id"]),
        confidence=confidence,
        status="found" if confidence >= 0.45 else "low_confidence",
        debug_metrics={
            "method": method,
            "line_length": round(float(best["length"]), 2),
            "boundary_contact_score": round(float(best["boundary_score"]), 3),
            "outward_extension_score": round(float(best["extension_score"]), 3),
            "radial_alignment": round(float(best["radial_alignment"]), 3),
            "min_boundary_delta": round(float(best["min_boundary_delta"]), 2),
            "total_score": round(float(best["score"]), 3),
        },
        method=method,
        fallback_stage=stage,
    )


def _component_line_metrics(points_xy: np.ndarray, center: Tuple[float, float], radius: float) -> Dict[str, float]:
    if len(points_xy) < 2:
        return {
            "line_likeness": 0.0,
            "radial_spread": 0.0,
            "length_score": 0.0,
            "arc_penalty": 1.0,
            "blob_penalty": 1.0,
        }

    pts = points_xy.astype(np.float32)
    mean = pts.mean(axis=0)
    centered = pts - mean
    cov = np.cov(centered.T)
    eigvals = np.linalg.eigvalsh(cov)
    eigvals = np.sort(np.maximum(eigvals, 1e-6))
    elongation = float(eigvals[-1] / eigvals[0])
    line_likeness = float(min(1.0, np.log1p(elongation) / np.log(25.0)))

    dx = pts[:, 0] - center[0]
    dy = pts[:, 1] - center[1]
    dists = np.sqrt(dx * dx + dy * dy)
    radial_spread = float(dists.max() - dists.min())
    length_score = float(min(1.0, radial_spread / max(radius * 3.0, 1.0)))

    near_band = np.mean(np.abs(dists - radius) < max(3.0, radius * 0.20))
    arc_penalty = float(min(1.0, near_band * 1.5)) if radial_spread < radius * 1.2 else 0.0
    blob_penalty = float(max(0.0, 1.0 - line_likeness))

    return {
        "line_likeness": line_likeness,
        "radial_spread": radial_spread,
        "length_score": length_score,
        "arc_penalty": arc_penalty,
        "blob_penalty": blob_penalty,
    }


def _component_endpoints(comp: np.ndarray) -> List[Point]:
    ys, xs = np.where(comp > 0)
    pixels = set(zip(xs.tolist(), ys.tolist()))
    endpoints: List[Point] = []
    for x, y in pixels:
        n = 0
        for dy in (-1, 0, 1):
            for dx in (-1, 0, 1):
                if dx == 0 and dy == 0:
                    continue
                if (x + dx, y + dy) in pixels:
                    n += 1
        if n <= 1:
            endpoints.append((int(x), int(y)))
    return endpoints


def _path_between_component_points(comp: np.ndarray, start: Point, end: Point, offset: Tuple[int, int]) -> List[Point]:
    h, w = comp.shape[:2]
    sx, sy = start
    ex, ey = end
    q = deque([(sx, sy)])
    parent: Dict[Point, Optional[Point]] = {(sx, sy): None}
    neighbours = [(-1, -1), (0, -1), (1, -1), (-1, 0), (1, 0), (-1, 1), (0, 1), (1, 1)]

    while q:
        x, y = q.popleft()
        if (x, y) == (ex, ey):
            break
        for dx, dy in neighbours:
            nx, ny = x + dx, y + dy
            if not (0 <= nx < w and 0 <= ny < h):
                continue
            if comp[ny, nx] == 0 or (nx, ny) in parent:
                continue
            parent[(nx, ny)] = (x, y)
            q.append((nx, ny))

    if (ex, ey) not in parent:
        return [(start[0] + offset[0], start[1] + offset[1]), (end[0] + offset[0], end[1] + offset[1])]

    path = []
    cur: Optional[Point] = (ex, ey)
    while cur is not None:
        path.append((cur[0] + offset[0], cur[1] + offset[1]))
        cur = parent[cur]
    path.reverse()
    if len(path) > 80:
        step = max(1, len(path) // 80)
        path = path[::step]
        if path[-1] != (end[0] + offset[0], end[1] + offset[1]):
            path.append((end[0] + offset[0], end[1] + offset[1]))
    return path


def _component_trace_on_mask(mask: np.ndarray, balloon: BalloonCandidate, *, method: str, stage: int) -> LeaderTrace:
    cx, cy = balloon.center
    radius = max(1.0, float(balloon.radius))
    h, w = mask.shape[:2]

    work = mask.copy()
    cv2.circle(work, (int(round(cx)), int(round(cy))), int(round(radius * 0.82)), 0, -1)

    pad = int(max(radius * 6.0, 90))
    x1 = max(0, int(cx - pad))
    y1 = max(0, int(cy - pad))
    x2 = min(w, int(cx + pad))
    y2 = min(h, int(cy + pad))
    local = work[y1:y2, x1:x2]

    num, labels, stats, _centroids = cv2.connectedComponentsWithStats(local, connectivity=8)
    best = None

    for label_id in range(1, num):
        area = int(stats[label_id, cv2.CC_STAT_AREA])
        if area < max(8, radius * 0.18):
            continue

        comp = (labels == label_id).astype(np.uint8)
        ys, xs = np.where(comp > 0)
        xs_global = xs.astype(np.float32) + x1
        ys_global = ys.astype(np.float32) + y1
        points = np.column_stack([xs_global, ys_global])

        dx = xs_global - cx
        dy = ys_global - cy
        dists = np.sqrt(dx * dx + dy * dy)
        boundary_tol = max(4.0, radius * 0.35)
        boundary_pixels = np.abs(dists - radius) < boundary_tol
        boundary_contact = float(min(1.0, boundary_pixels.sum() / max(radius * 0.25, 1.0)))
        min_boundary_delta = float(np.min(np.abs(dists - radius))) if len(dists) else 9999.0
        if min_boundary_delta > radius * 0.90:
            continue

        outward_extension = float(min(1.0, max(0.0, dists.max() - radius) / max(radius * 3.0, 1.0)))
        metrics = _component_line_metrics(points, (cx, cy), radius)

        comp_local = comp * 255
        endpoints = _component_endpoints(comp_local)
        endpoint_score = 0.0
        source_local: Optional[Point] = None
        target_local: Optional[Point] = None
        if endpoints:
            endpoint_arr = np.array(endpoints, dtype=np.float32)
            endpoint_global = endpoint_arr + np.array([[x1, y1]], dtype=np.float32)
            ed = np.sqrt((endpoint_global[:, 0] - cx) ** 2 + (endpoint_global[:, 1] - cy) ** 2)
            near_idx = int(np.argmin(np.abs(ed - radius)))
            far_idx = int(np.argmax(ed))
            source_local = endpoints[near_idx]
            target_local = endpoints[far_idx]
            near_ok = abs(float(ed[near_idx]) - radius) < radius * 0.90
            far_ok = float(ed[far_idx]) > radius * 1.35
            endpoint_score = (0.5 if near_ok else 0.0) + (0.5 if far_ok else 0.0)
        else:
            far_idx = int(np.argmax(dists))
            target_local = (int(xs[far_idx]), int(ys[far_idx]))

        score = (
            2.0 * boundary_contact
            + 2.5 * outward_extension
            + 2.0 * metrics["line_likeness"]
            + 1.5 * endpoint_score
            + 1.0 * metrics["length_score"]
            - 2.0 * metrics["arc_penalty"]
            - 1.0 * metrics["blob_penalty"]
        )

        debug = {
            "component_area": area,
            "boundary_contact_score": round(boundary_contact, 3),
            "min_boundary_delta": round(min_boundary_delta, 2),
            "outward_extension_score": round(outward_extension, 3),
            "line_likeness_score": round(metrics["line_likeness"], 3),
            "radial_spread": round(metrics["radial_spread"], 2),
            "endpoint_score": round(endpoint_score, 3),
            "length_score": round(metrics["length_score"], 3),
            "arc_penalty": round(metrics["arc_penalty"], 3),
            "blob_penalty": round(metrics["blob_penalty"], 3),
            "total_score": round(float(score), 3),
        }

        if best is None or score > best["score"]:
            best = {
                "score": float(score),
                "component_id": label_id,
                "comp": comp_local,
                "source_local": source_local,
                "target_local": target_local,
                "debug": debug,
            }

    hough_candidate = _trace_mask_hough(local, (x1, y1), balloon, method=method, stage=stage)

    if best is None:
        if hough_candidate is not None:
            return hough_candidate
        return LeaderTrace(
            balloon_candidate_id=balloon.candidate_id,
            polyline=[],
            target_endpoint=None,
            source_endpoint=None,
            component_id=None,
            confidence=0.0,
            status="not_found",
            method=method,
            fallback_stage=stage,
            debug_metrics={"reason": "no line-like component near balloon"},
        )

    target_local = best["target_local"]
    source_local = best["source_local"] or target_local
    target = (int(target_local[0] + x1), int(target_local[1] + y1))
    source = (int(source_local[0] + x1), int(source_local[1] + y1))
    polyline = _path_between_component_points(best["comp"], source_local, target_local, (x1, y1))
    confidence = float(max(0.0, min(1.0, (best["score"] + 1.0) / 8.0)))
    status = "found" if confidence >= FOUND_THRESHOLD else "low_confidence"

    if hough_candidate is not None and hough_candidate.confidence > confidence:
        return hough_candidate

    return LeaderTrace(
        balloon_candidate_id=balloon.candidate_id,
        polyline=polyline,
        target_endpoint=target,
        source_endpoint=source,
        component_id=int(best["component_id"]),
        confidence=confidence,
        status=status,
        method=method,
        fallback_stage=stage,
        debug_metrics=best["debug"],
    )


def _roi_bounds(shape: Tuple[int, int], balloon: BalloonCandidate) -> Tuple[int, int, int, int]:
    h, w = shape[:2]
    cx, cy = balloon.center
    radius = max(1.0, float(balloon.radius))
    pad = int(max(radius * 6.0, 90))
    return (
        max(0, int(cx - pad)),
        max(0, int(cy - pad)),
        min(w, int(cx + pad)),
        min(h, int(cy + pad)),
    )


def _build_weak_color_mask(image: np.ndarray, base_mask: np.ndarray, balloon: BalloonCandidate) -> np.ndarray:
    x1, y1, x2, y2 = _roi_bounds(base_mask.shape, balloon)
    roi = image[y1:y2, x1:x2]
    if roi.size == 0:
        return base_mask.copy()

    hsv = cv2.cvtColor(roi, cv2.COLOR_BGR2HSV)
    lab = cv2.cvtColor(roi, cv2.COLOR_BGR2LAB)
    base_roi = base_mask[y1:y2, x1:x2]

    if np.count_nonzero(base_roi) > 0:
        hue_vals = hsv[:, :, 0][base_roi > 0]
        med_hue = int(np.median(hue_vals))
    else:
        med_hue = 150

    hue_tol = 34
    sat_min = 10
    val_min = 25
    lo_h = (med_hue - hue_tol) % 180
    hi_h = (med_hue + hue_tol) % 180
    if lo_h <= hi_h:
        hue_mask = cv2.inRange(hsv, np.array([lo_h, sat_min, val_min], dtype=np.uint8), np.array([hi_h, 255, 255], dtype=np.uint8))
    else:
        hue_mask = (
            cv2.inRange(hsv, np.array([lo_h, sat_min, val_min], dtype=np.uint8), np.array([179, 255, 255], dtype=np.uint8))
            | cv2.inRange(hsv, np.array([0, sat_min, val_min], dtype=np.uint8), np.array([hi_h, 255, 255], dtype=np.uint8))
        )

    # LAB fallback: magenta/red annotations usually sit away from the
    # neutral a/b center. This catches faded low-saturation strokes.
    a_chan = lab[:, :, 1].astype(np.int16)
    b_chan = lab[:, :, 2].astype(np.int16)
    chroma = np.sqrt((a_chan - 128) ** 2 + (b_chan - 128) ** 2)
    lab_mask = ((chroma > 10) & (hsv[:, :, 2] > 35)).astype(np.uint8) * 255

    weak_roi = cv2.bitwise_or(hue_mask, lab_mask)
    weak_roi = cv2.bitwise_or(weak_roi, base_roi)
    kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3))
    weak_roi = cv2.morphologyEx(weak_roi, cv2.MORPH_CLOSE, kernel, iterations=2)
    weak_roi = cv2.dilate(weak_roi, kernel, iterations=1)

    weak_mask = np.zeros_like(base_mask)
    weak_mask[y1:y2, x1:x2] = weak_roi
    return weak_mask


def _trace_grayscale_hough(image: np.ndarray, balloon: BalloonCandidate) -> LeaderTrace:
    x1, y1, x2, y2 = _roi_bounds(image.shape[:2], balloon)
    roi = image[y1:y2, x1:x2]
    cx, cy = balloon.center
    radius = max(1.0, float(balloon.radius))
    if roi.size == 0:
        return LeaderTrace(
            balloon_candidate_id=balloon.candidate_id,
            polyline=[],
            target_endpoint=None,
            source_endpoint=None,
            component_id=None,
            confidence=0.0,
            status="not_found",
            method="grayscale_hough",
            fallback_stage=3,
            debug_metrics={"reason": "empty_roi", "grayscale_hough_no_candidate": True},
        )

    gray = cv2.cvtColor(roi, cv2.COLOR_BGR2GRAY)
    gray = cv2.GaussianBlur(gray, (3, 3), 0)
    edges = cv2.Canny(gray, 60, 180)
    cv2.circle(edges, (int(round(cx - x1)), int(round(cy - y1))), int(round(radius * 0.90)), 0, -1)

    min_len = max(18, int(radius * 0.65))
    lines = cv2.HoughLinesP(
        edges,
        rho=1,
        theta=np.pi / 180.0,
        threshold=max(12, int(radius * 0.30)),
        minLineLength=min_len,
        maxLineGap=max(6, int(radius * 0.20)),
    )
    if lines is None:
        return LeaderTrace(
            balloon_candidate_id=balloon.candidate_id,
            polyline=[],
            target_endpoint=None,
            source_endpoint=None,
            component_id=None,
            confidence=0.0,
            status="not_found",
            method="grayscale_hough",
            fallback_stage=3,
            debug_metrics={"reason": "no_hough_lines", "grayscale_hough_no_candidate": True},
        )

    candidates: List[Dict[str, Any]] = []
    rejected: List[Dict[str, Any]] = []
    boundary_tol = max(5.0, radius * 0.38)
    for idx, raw in enumerate(lines[:, 0, :], start=1):
        lx1, ly1, lx2, ly2 = [int(v) for v in raw]
        ax, ay = lx1 + x1, ly1 + y1
        bx, by = lx2 + x1, ly2 + y1
        d1 = float(np.hypot(ax - cx, ay - cy))
        d2 = float(np.hypot(bx - cx, by - cy))
        length = float(np.hypot(bx - ax, by - ay))
        raw_candidate = {
            "method": "grayscale_hough",
            "stage": 3,
            "line_id": idx,
            "line": [[int(ax), int(ay)], [int(bx), int(by)]],
            "length": round(length, 2),
            "accepted": False,
        }
        if length < min_len:
            rejected.append({**raw_candidate, "score": 0.0, "rejection_reason": "too_short"})
            continue
        if d1 < radius * 0.70 and d2 < radius * 0.70:
            rejected.append({**raw_candidate, "score": 0.0, "rejection_reason": "grayscale_hough_circle_chord_rejected"})
            continue

        near_delta_1 = abs(d1 - radius)
        near_delta_2 = abs(d2 - radius)
        source = (int(ax), int(ay)) if near_delta_1 <= near_delta_2 else (int(bx), int(by))
        target = (int(bx), int(by)) if near_delta_1 <= near_delta_2 else (int(ax), int(ay))
        source_dist = min(near_delta_1, near_delta_2)
        target_dist = max(d1, d2)
        center_to_line = _line_point_distance(cx, cy, ax, ay, bx, by)
        if source_dist > boundary_tol and center_to_line > boundary_tol:
            rejected.append({**raw_candidate, "score": 0.0, "rejection_reason": "grayscale_hough_direction_inconsistent"})
            continue
        if target_dist < radius * 1.25:
            rejected.append({**raw_candidate, "score": 0.0, "rejection_reason": "grayscale_hough_circle_chord_rejected"})
            continue

        if center_to_line < radius * 0.22 and min(d1, d2) < radius * 1.15:
            rejected.append({**raw_candidate, "score": 0.0, "rejection_reason": "interior_crossing_rejected"})
            continue

        vx, vy = target[0] - source[0], target[1] - source[1]
        rx, ry = target[0] - cx, target[1] - cy
        denom = max(float(np.hypot(vx, vy) * np.hypot(rx, ry)), 1e-6)
        radial_direction = max(0.0, (vx * rx + vy * ry) / denom)
        near_boundary_score = max(0.0, 1.0 - source_dist / boundary_tol)
        outward_length_score = min(1.0, max(0.0, target_dist - radius) / max(radius * 4.0, 1.0))
        endpoint_distance_score = min(1.0, target_dist / max(radius * 5.0, 1.0))
        tangent_like = center_to_line > radius * 0.82 and min(near_delta_1, near_delta_2) > radius * 0.18
        if tangent_like and radial_direction < 0.35:
            rejected.append({**raw_candidate, "score": 0.0, "rejection_reason": "grayscale_hough_tangent_rejected"})
            continue

        direction_inconsistent = radial_direction < 0.25 or target_dist <= min(d1, d2) + radius * 0.25
        if direction_inconsistent:
            rejected.append({**raw_candidate, "score": 0.0, "rejection_reason": "grayscale_hough_direction_inconsistent"})
            continue

        drawing_line_penalty = 0.55 if length > radius * 6.0 else 0.0
        if length > radius * 8.0 and near_boundary_score < 0.85:
            rejected.append({**raw_candidate, "score": 0.0, "rejection_reason": "grayscale_hough_long_drawing_line_penalty"})
            continue
        score = (
            2.0 * near_boundary_score
            + 1.8 * outward_length_score
            + 1.6 * radial_direction
            + 1.0 * endpoint_distance_score
            - drawing_line_penalty
        )
        candidates.append({
            "score": score,
            "line_id": idx,
            "source": source,
            "target": target,
            "length": length,
            "near_boundary_score": near_boundary_score,
            "outward_length_score": outward_length_score,
            "radial_direction_score": radial_direction,
            "endpoint_distance_score": endpoint_distance_score,
            "drawing_line_penalty": drawing_line_penalty,
            "center_to_line": center_to_line,
            "accepted": True,
        })

    if not candidates:
        return LeaderTrace(
            balloon_candidate_id=balloon.candidate_id,
            polyline=[],
            target_endpoint=None,
            source_endpoint=None,
            component_id=None,
            confidence=0.0,
            status="not_found",
            method="grayscale_hough",
            fallback_stage=3,
            debug_metrics={
                "reason": "no_candidate_after_geometry_filters",
                "grayscale_hough_no_candidate": True,
                "top_rejected_candidates": rejected[:5],
            },
        )

    candidates.sort(key=lambda c: c["score"], reverse=True)
    best = candidates[0]
    ambiguity_delta = float(best["score"] - candidates[1]["score"]) if len(candidates) > 1 else 999.0
    ambiguous = len(candidates) > 1 and ambiguity_delta < 0.35
    endpoints = np.array([c["target"] for c in candidates[:5]], dtype=np.float32)
    endpoint_unstable = False
    if len(endpoints) >= 3:
        endpoint_spread = float(np.mean(np.linalg.norm(endpoints - endpoints.mean(axis=0), axis=1)))
        endpoint_unstable = endpoint_spread > radius * 1.8
    else:
        endpoint_spread = 0.0
    confidence_cap = 0.45 if ambiguous else 0.72
    confidence = float(max(0.0, min(confidence_cap, best["score"] / 6.0)))
    if endpoint_unstable:
        confidence = min(confidence, 0.48)
        ambiguous = True
    status = "ambiguous" if ambiguous else ("found" if confidence >= 0.52 else "low_confidence")
    top_candidates = []
    for cand in candidates[:5]:
        top_candidates.append({
            "method": "grayscale_hough",
            "stage": 3,
            "status": "accepted",
            "line": [list(cand["source"]), list(cand["target"])],
            "score": round(float(cand["score"]), 3),
            "source_endpoint": list(cand["source"]),
            "target_endpoint": list(cand["target"]),
            "near_boundary_score": round(float(cand["near_boundary_score"]), 3),
            "outward_length_score": round(float(cand["outward_length_score"]), 3),
            "radial_direction_score": round(float(cand["radial_direction_score"]), 3),
            "endpoint_distance_score": round(float(cand["endpoint_distance_score"]), 3),
            "drawing_line_penalty": round(float(cand["drawing_line_penalty"]), 3),
            "ambiguity_delta_from_best": round(float(best["score"] - cand["score"]), 3),
            "accepted": cand is best,
            "rejection_reason": "" if cand is best else "lower_scoring_candidate",
        })
    for cand in rejected[:5]:
        top_candidates.append(cand)
    return LeaderTrace(
        balloon_candidate_id=balloon.candidate_id,
        polyline=[best["source"], best["target"]],
        target_endpoint=best["target"],
        source_endpoint=best["source"],
        component_id=int(best["line_id"]),
        confidence=confidence,
        status=status,
        method="grayscale_hough",
        fallback_stage=3,
        debug_metrics={
            "candidate_count": len(candidates),
            "grayscale_hough_ambiguous": ambiguous,
            "grayscale_hough_endpoint_unstable": endpoint_unstable,
            "endpoint_spread": round(endpoint_spread, 2),
            "ambiguity_delta": round(ambiguity_delta, 3) if ambiguity_delta != 999.0 else None,
            "line_length": round(float(best["length"]), 2),
            "near_boundary_score": round(float(best["near_boundary_score"]), 3),
            "outward_length_score": round(float(best["outward_length_score"]), 3),
            "radial_direction_score": round(float(best["radial_direction_score"]), 3),
            "endpoint_distance_score": round(float(best["endpoint_distance_score"]), 3),
            "drawing_line_penalty": round(float(best["drawing_line_penalty"]), 3),
            "total_score": round(float(best["score"]), 3),
            "top_candidates": top_candidates,
        },
    )


def trace_leader(mask: np.ndarray, balloon: BalloonCandidate, image: Optional[np.ndarray] = None) -> LeaderTrace:
    stage_debug: Dict[str, Any] = {}
    stage1 = _component_trace_on_mask(mask, balloon, method="color_component", stage=1)
    stage_debug["stage1"] = {
        "status": stage1.status,
        "confidence": round(float(stage1.confidence), 4),
        "method": stage1.method,
        "metrics": stage1.debug_metrics,
    }
    if stage1.status == "found" and stage1.confidence >= FOUND_THRESHOLD:
        stage1.debug_metrics = {**stage1.debug_metrics, "stages": stage_debug}
        return stage1

    if image is not None:
        weak_mask = _build_weak_color_mask(image, mask, balloon)
        stage2 = _component_trace_on_mask(weak_mask, balloon, method="weak_color_component", stage=2)
        stage_debug["stage2"] = {
            "status": stage2.status,
            "confidence": round(float(stage2.confidence), 4),
            "method": stage2.method,
            "metrics": stage2.debug_metrics,
        }
        stage2_outward = float(stage2.debug_metrics.get("outward_extension_score", 0.0) or 0.0)
        stage2_arc = float(stage2.debug_metrics.get("arc_penalty", 0.0) or 0.0)
        if (stage2.status == "found"
                and stage2.confidence >= FOUND_THRESHOLD
                and stage2_outward >= 0.18
                and stage2_arc < 0.75):
            stage2.debug_metrics = {**stage2.debug_metrics, "weak_color_fallback_used": True, "stages": stage_debug}
            return stage2

        stage3 = _trace_grayscale_hough(image, balloon)
        stage_debug["stage3"] = {
            "status": stage3.status,
            "confidence": round(float(stage3.confidence), 4),
            "method": stage3.method,
            "metrics": stage3.debug_metrics,
        }
        if stage3.status in ("found", "low_confidence", "ambiguous"):
            stage3.debug_metrics = {
                **stage3.debug_metrics,
                "grayscale_hough_fallback_used": True,
                "color_trace_failed_but_hough_succeeded": stage3.status == "found",
                "stages": stage_debug,
            }
            return stage3

    if stage1.status == "not_found" and mask is not None and int(np.count_nonzero(mask)) < 100:
        stage1.status = "no_color_evidence"
    stage1.debug_metrics = {**stage1.debug_metrics, "stages": stage_debug}
    return stage1


def trace_leaders(mask: np.ndarray, balloons: List[BalloonCandidate], image: Optional[np.ndarray] = None) -> List[LeaderTrace]:
    return [trace_leader(mask, balloon, image=image) for balloon in balloons]
