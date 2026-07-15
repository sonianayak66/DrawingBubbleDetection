"""
Leader line tracing algorithms.

Standard CV/graph algorithms for tracing leader lines from
balloons to dimension text:

  1. Skeletonization — thin lines to 1px for clean tracing
  2. A* pathfinding — cost-based search that can jump small gaps
  3. Ray casting — project leader endpoint to find the target text box
  4. Segment merging — combine collinear Hough segments

All algorithms are general-purpose, image-agnostic, and use no
tuned thresholds beyond standard CV parameters.
"""

from __future__ import annotations

import heapq
import math
from typing import List, Optional, Tuple

import cv2
import numpy as np


# ── 1. Skeletonization ───────────────────────────────────────────

def skeletonize_mask(mask: np.ndarray) -> np.ndarray:
    """Thin a binary mask to 1-pixel-wide skeleton.

    Uses morphological thinning (iterative erosion with
    connectivity preservation).  Equivalent to Zhang-Suen
    but uses OpenCV's built-in ximgproc if available,
    falling back to iterative morphological thinning.
    """
    if mask.dtype != np.uint8:
        mask = mask.astype(np.uint8)
    binary = (mask > 0).astype(np.uint8)

    # Try OpenCV's thinning (available in opencv-contrib)
    try:
        skeleton = cv2.ximgproc.thinning(
            binary * 255, thinningType=cv2.ximgproc.THINNING_ZHANGSUEN
        )
        return skeleton
    except AttributeError:
        pass

    # Fallback: iterative morphological thinning
    skeleton = np.zeros_like(binary)
    element = cv2.getStructuringElement(cv2.MORPH_CROSS, (3, 3))
    while True:
        eroded = cv2.erode(binary, element)
        dilated = cv2.dilate(eroded, element)
        diff = binary - dilated
        skeleton = cv2.bitwise_or(skeleton, diff)
        binary = eroded.copy()
        if cv2.countNonZero(binary) == 0:
            break
    return skeleton * 255


# ── 2. A* Pathfinding with gap closing ───────────────────────────

def astar_trace(
    gray: np.ndarray,
    start: Tuple[int, int],
    direction: Tuple[float, float],
    balloon_cx: float,
    balloon_cy: float,
    balloon_radius: float,
    callout_boxes: List[Tuple[float, float, float, float]],
    max_steps: int = 500,
) -> Optional[List[Tuple[int, int]]]:
    """Trace a leader line using A* search with gap-closing.

    The cost model:
      - Moving to a DARK pixel costs 1 (following a line)
      - Moving to a LIGHT pixel costs 10 (crossing a gap)
      - Moving BACKWARD (against leader direction) costs 20

    This allows the tracer to jump small gaps (dashed lines,
    scan artifacts) while strongly preferring to follow dark
    ink.  The directional bias prevents backward wandering.

    Returns the path as (x, y) points, or None.
    """
    h, w = gray.shape[:2]
    thresh_val = cv2.threshold(gray, 0, 255, cv2.THRESH_OTSU)[0]

    sx, sy = start
    dx, dy = direction

    if not (0 <= sx < w and 0 <= sy < h):
        return None

    # Suppress balloon interior
    bcx, bcy = int(balloon_cx), int(balloon_cy)
    br = int(balloon_radius * 0.85)

    # A* search
    # State: (cost, x, y)
    open_set: list = []
    heapq.heappush(open_set, (0.0, sx, sy))
    came_from: dict = {}
    g_score: dict = {(sx, sy): 0.0}

    neighbours = [(-1, -1), (0, -1), (1, -1),
                  (-1,  0),          (1,  0),
                  (-1,  1), (0,  1), (1,  1)]

    best_target: Optional[Tuple[int, int]] = None
    best_progress = -1.0
    steps = 0

    while open_set and steps < max_steps:
        _, cx, cy = heapq.heappop(open_set)
        steps += 1

        # Check if we've reached a callout box
        for bx1, by1, bx2, by2 in callout_boxes:
            if bx1 <= cx <= bx2 and by1 <= cy <= by2:
                # Reconstruct path
                path = [(cx, cy)]
                while (cx, cy) in came_from:
                    cx, cy = came_from[(cx, cy)]
                    path.append((cx, cy))
                path.reverse()
                return path

        # Track furthest forward point
        progress = (cx - sx) * dx + (cy - sy) * dy
        if progress > best_progress:
            best_progress = progress
            best_target = (cx, cy)

        for ndx, ndy in neighbours:
            nx, ny = cx + ndx, cy + ndy
            if not (0 <= nx < w and 0 <= ny < h):
                continue

            # Skip balloon interior
            if math.hypot(nx - bcx, ny - bcy) < br:
                continue

            # Cost model
            is_dark = gray[ny, nx] < thresh_val
            # Directional cost: moving forward is cheap
            step_dot = ndx * dx + ndy * dy
            if is_dark:
                move_cost = 1.0
            else:
                move_cost = 10.0  # gap crossing
            if step_dot < 0:
                move_cost += 15.0  # backward penalty

            tentative_g = g_score.get((cx, cy), float('inf')) + move_cost

            if tentative_g < g_score.get((nx, ny), float('inf')):
                g_score[(nx, ny)] = tentative_g
                came_from[(nx, ny)] = (cx, cy)
                # Heuristic: prefer forward movement
                h_val = -step_dot * 0.5
                heapq.heappush(open_set, (tentative_g + h_val, nx, ny))

    # No callout box reached — return path to furthest point
    if best_target is not None:
        path = [best_target]
        cx, cy = best_target
        while (cx, cy) in came_from:
            cx, cy = came_from[(cx, cy)]
            path.append((cx, cy))
        path.reverse()
        return path if len(path) > 3 else None

    return None


# ── 3. Ray casting — project endpoint to bbox ────────────────────

def ray_cast_to_callout(
    endpoint: Tuple[int, int],
    direction: Tuple[float, float],
    callout_boxes: List[Tuple[float, float, float, float, int]],
    max_distance: float = 500.0,
) -> Optional[int]:
    """Project a ray from the trace endpoint and find which callout
    box it hits first.

    The ray starts at `endpoint` and extends in `direction`.
    Returns the callout index or None.

    This is the "snap" rule: if the leader line ends near a text box,
    project forward to find which box it points at.
    """
    ex, ey = endpoint
    dx, dy = direction
    mag = math.hypot(dx, dy)
    if mag < 1e-6:
        return None
    dx, dy = dx / mag, dy / mag

    best_idx: Optional[int] = None
    best_t = max_distance

    for bx1, by1, bx2, by2, ci in callout_boxes:
        # Ray-AABB intersection
        # Parametric: P = (ex + t*dx, ey + t*dy)
        # Find t where ray enters the box

        if abs(dx) > 1e-9:
            t1 = (bx1 - ex) / dx
            t2 = (bx2 - ex) / dx
        else:
            t1 = -1e18 if bx1 <= ex <= bx2 else 1e18
            t2 = t1

        if abs(dy) > 1e-9:
            t3 = (by1 - ey) / dy
            t4 = (by2 - ey) / dy
        else:
            t3 = -1e18 if by1 <= ey <= by2 else 1e18
            t4 = t3

        tmin = max(min(t1, t2), min(t3, t4))
        tmax = min(max(t1, t2), max(t3, t4))

        if tmax < 0 or tmin > tmax:
            continue  # ray misses

        hit_t = tmin if tmin >= 0 else 0
        if hit_t < best_t:
            best_t = hit_t
            best_idx = ci

    return best_idx


# ── 4. Segment merging ───────────────────────────────────────────

def merge_collinear_segments(
    segments: List[Tuple[int, int, int, int]],
    angle_tol: float = 10.0,
    gap_tol: float = 20.0,
) -> List[Tuple[int, int, int, int]]:
    """Merge collinear line segments that are close together.

    Two segments are merged if:
      - Their angles differ by less than angle_tol degrees
      - The gap between their closest endpoints is < gap_tol pixels

    Returns merged segments as (x1, y1, x2, y2) tuples.
    """
    if not segments:
        return []

    def seg_angle(x1, y1, x2, y2):
        return math.degrees(math.atan2(y2 - y1, x2 - x1)) % 180

    def seg_endpoints_dist(s1, s2):
        """Minimum distance between any pair of endpoints."""
        points1 = [(s1[0], s1[1]), (s1[2], s1[3])]
        points2 = [(s2[0], s2[1]), (s2[2], s2[3])]
        return min(
            math.hypot(a[0] - b[0], a[1] - b[1])
            for a in points1 for b in points2
        )

    merged = list(segments)
    changed = True

    while changed:
        changed = False
        new_merged = []
        used = set()

        for i, s1 in enumerate(merged):
            if i in used:
                continue
            current = s1

            for j, s2 in enumerate(merged):
                if j <= i or j in used:
                    continue

                a1 = seg_angle(*current)
                a2 = seg_angle(*s2)
                angle_diff = abs(a1 - a2)
                if angle_diff > 90:
                    angle_diff = 180 - angle_diff

                if angle_diff < angle_tol and seg_endpoints_dist(current, s2) < gap_tol:
                    # Merge: take the two most distant endpoints
                    all_pts = [
                        (current[0], current[1]), (current[2], current[3]),
                        (s2[0], s2[1]), (s2[2], s2[3]),
                    ]
                    best_dist = 0
                    best_pair = (all_pts[0], all_pts[1])
                    for a in all_pts:
                        for b in all_pts:
                            d = math.hypot(a[0] - b[0], a[1] - b[1])
                            if d > best_dist:
                                best_dist = d
                                best_pair = (a, b)

                    current = (best_pair[0][0], best_pair[0][1],
                               best_pair[1][0], best_pair[1][1])
                    used.add(j)
                    changed = True

            new_merged.append(current)
            used.add(i)

        merged = new_merged

    return merged
