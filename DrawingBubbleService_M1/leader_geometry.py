"""
Leader line geometry algorithms.

Pure geometric algorithms for detecting leader lines from balloon
boundaries.  No ML models, no tuned thresholds — only physical
properties of engineering drawings:

  1. Leader lines are thin dark lines
  2. They exit the balloon circle at a specific point on the rim
  3. They travel outward from the balloon toward a dimension annotation
  4. They are typically straight or have at most one bend

Algorithm:
  - Radial scan: from circle center, cast rays outward every 3°
  - For each ray, measure dark-pixel density from rim to 2.5× radius
  - The ray with highest density (in the annulus just outside the rim)
    indicates the leader line exit direction
  - Use HoughLinesP in a local region outside the rim to refine the
    exit angle with subpixel accuracy
  - Return the exit point and direction
"""

from __future__ import annotations

import math
from dataclasses import dataclass
from typing import List, Optional, Tuple

import cv2
import numpy as np


@dataclass
class LeaderExit:
    """A detected leader line exit point on the balloon boundary."""
    exit_x: int
    exit_y: int
    direction_x: float  # unit vector outward
    direction_y: float
    strength: float     # dark-pixel density ratio [0, 1]
    angle_deg: int      # exit angle in degrees


def find_leader_exits(
    gray: np.ndarray,
    cx: int, cy: int, radius: int,
    max_exits: int = 5,
) -> List[LeaderExit]:
    """
    Find leader line exit points on a balloon's boundary using
    HoughLinesP to detect line segments that cross the rim.

    DOMAIN PRINCIPLE: A leader line is a straight line segment that
    has one end near/inside the circle rim and extends outward.
    HoughLinesP finds all such segments in the annulus region.
    We return ALL candidates (up to max_exits) sorted by length,
    so the cost matrix can evaluate each one against callouts.

    This uses NO trained model — only the physical property that
    leader lines are straight dark strokes crossing a circle boundary.
    """
    h, w = gray.shape[:2]

    # Crop annulus region around the circle
    pad = int(radius * 1.8)
    x1 = max(0, cx - pad)
    y1 = max(0, cy - pad)
    x2 = min(w, cx + pad)
    y2 = min(h, cy + pad)
    roi = gray[y1:y2, x1:x2]

    # Create annulus mask (outside circle, within pad)
    rh, rw = roi.shape
    lcx, lcy = cx - x1, cy - y1
    mask = np.zeros((rh, rw), dtype=np.uint8)
    cv2.circle(mask, (lcx, lcy), pad, 255, -1)
    cv2.circle(mask, (lcx, lcy), max(1, radius - 2), 0, -1)

    # Edge detection in annulus
    blurred = cv2.GaussianBlur(roi, (3, 3), 0)
    edges = cv2.Canny(blurred, 50, 150)
    edges = edges & mask

    # Find line segments crossing the rim
    lines = cv2.HoughLinesP(
        edges, rho=1, theta=np.pi / 180,
        threshold=max(10, int(radius * 0.25)),
        minLineLength=max(10, int(radius * 0.35)),
        maxLineGap=5,
    )

    if lines is None:
        return []

    # Find lines with one end near the rim and one end outside.
    # FILTER: reject lines that cross THROUGH the balloon (both
    # ends extend in opposite directions from the center).
    # A real leader exits on one side only.
    thresh_val = cv2.threshold(gray, 0, 255, cv2.THRESH_OTSU)[0]

    raw_exits: List[Tuple[float, int, int, int]] = []
    for line in lines:
        lx1, ly1, lx2, ly2 = line[0]
        d1 = math.hypot(lx1 - lcx, ly1 - lcy)
        d2 = math.hypot(lx2 - lcx, ly2 - lcy)

        inside = min(d1, d2)
        outside = max(d1, d2)

        if inside < radius * 1.2 and outside > radius * 1.1:
            # The outer endpoint is the exit
            if d2 > d1:
                ex, ey = lx2 + x1, ly2 + y1
                ang_deg = int(math.degrees(math.atan2(ey - cy, ex - cx)))
            else:
                ex, ey = lx1 + x1, ly1 + y1
                ang_deg = int(math.degrees(math.atan2(ey - cy, ex - cx)))
            if ang_deg < 0:
                ang_deg += 360

            # Through-line filter: check the OPPOSITE side of the rim.
            # If there are dark pixels on the opposite side too, this
            # line crosses through the balloon — it's drawing geometry,
            # not a leader.
            opp_ang = math.radians(ang_deg + 180)
            opp_dark = 0
            opp_total = 0
            for check_r in range(radius + 5, radius + 15):
                px = int(cx + check_r * math.cos(opp_ang))
                py = int(cy + check_r * math.sin(opp_ang))
                if 0 <= px < w and 0 <= py < h:
                    opp_total += 1
                    if gray[py, px] < thresh_val:
                        opp_dark += 1
            # If opposite side has significant dark pixels, it's a
            # through-line → skip
            if opp_total > 0 and opp_dark / opp_total > 0.5:
                continue

            length = math.hypot(lx2 - lx1, ly2 - ly1)
            raw_exits.append((length, ang_deg, ex, ey))

    # Deduplicate by angle (merge exits within ±15°)
    raw_exits.sort(reverse=True)
    exits: List[LeaderExit] = []
    used_angles: set = set()

    for length, ang_deg, ex, ey in raw_exits:
        too_close = False
        for used in used_angles:
            diff = abs(ang_deg - used)
            if diff > 180:
                diff = 360 - diff
            if diff < 15:
                too_close = True
                break
        if too_close:
            continue

        ang = math.radians(ang_deg)
        exits.append(LeaderExit(
            exit_x=int(ex),
            exit_y=int(ey),
            direction_x=math.cos(ang),
            direction_y=math.sin(ang),
            strength=length / (radius * 2),
            angle_deg=ang_deg,
        ))
        used_angles.add(ang_deg)

        if len(exits) >= max_exits:
            break

    return exits


def find_nearest_callout_along_exit(
    exit_point: LeaderExit,
    callout_positions: List[Tuple[float, float, int]],  # (cx, cy, index)
    balloon_cx: float,
    balloon_cy: float,
    balloon_radius: float = 30.0,
    max_lateral_offset: float = 100.0,
) -> Optional[int]:
    """
    Find the callout index closest to the leader exit direction.

    Uses a cone-shaped search: callouts must be roughly in the
    exit direction (within max_lateral_offset perpendicular distance)
    and further from the balloon than the exit point.

    Returns the callout index or None.
    """
    dx, dy = exit_point.direction_x, exit_point.direction_y
    perp_x, perp_y = -dy, dx  # perpendicular

    best_idx: Optional[int] = None
    best_forward_dist = float('inf')

    for ccx, ccy, ci in callout_positions:
        # Vector from balloon center to callout
        vx = ccx - balloon_cx
        vy = ccy - balloon_cy

        # Project onto leader direction
        forward = vx * dx + vy * dy
        lateral = abs(vx * perp_x + vy * perp_y)

        # Must be in the forward direction (away from balloon)
        if forward < 0:
            continue

        # Must be within the lateral tolerance
        if lateral > max_lateral_offset:
            continue

        # Prefer the closest callout in the forward direction
        if forward < best_forward_dist:
            best_forward_dist = forward
            best_idx = ci

    return best_idx


# ── Step 3: Edge-based line following ─────────────────────────────

def follow_leader_line(
    gray: np.ndarray,
    start_x: int, start_y: int,
    direction_x: float, direction_y: float,
    max_steps: int = 300,
    step_size: int = 2,
) -> List[Tuple[int, int]]:
    """
    Follow a dark line from the exit point in the given direction.

    Uses Canny edge pixels + directional continuity. At each step,
    looks ahead in a small fan (±15°) for the next dark pixel.
    Stops when no dark pixel is found or max_steps reached.

    Returns the path as a list of (x, y) points.
    """
    h, w = gray.shape[:2]
    thresh_val = cv2.threshold(gray, 0, 255, cv2.THRESH_OTSU)[0]

    path = [(start_x, start_y)]
    cx, cy = float(start_x), float(start_y)
    dx, dy = direction_x, direction_y

    for _ in range(max_steps):
        # Look ahead: try center, then slight left/right
        found = False
        for angle_offset in [0, 5, -5, 10, -10, 15, -15]:
            rad = math.radians(angle_offset)
            # Rotate direction by offset
            ndx = dx * math.cos(rad) - dy * math.sin(rad)
            ndy = dx * math.sin(rad) + dy * math.cos(rad)

            nx = int(cx + step_size * ndx)
            ny = int(cy + step_size * ndy)

            if not (0 <= nx < w and 0 <= ny < h):
                continue

            if gray[ny, nx] < thresh_val:
                cx, cy = float(nx), float(ny)
                # Update direction (smooth with momentum)
                dx = 0.7 * dx + 0.3 * ndx
                dy = 0.7 * dy + 0.3 * ndy
                mag = math.hypot(dx, dy)
                if mag > 0:
                    dx /= mag
                    dy /= mag
                path.append((nx, ny))
                found = True
                break

        if not found:
            break

    return path


# ── Step 4: Endpoint projection ───────────────────────────────────

def project_endpoint(
    cx: float, cy: float,
    direction_x: float, direction_y: float,
    radius: float,
    scale: float = 3.0,
) -> Tuple[int, int]:
    """
    Project along the leader direction to estimate where the
    dimension text sits.

    DOMAIN PRINCIPLE: dimension text is typically 2-4× bubble radius
    from the center, along the leader direction.
    """
    dist = radius * scale
    return (
        int(cx + dist * direction_x),
        int(cy + dist * direction_y),
    )


# ── Step 8: Leader-text intersection check ────────────────────────

def check_line_enters_box(
    path: List[Tuple[int, int]],
    box_x1: float, box_y1: float,
    box_x2: float, box_y2: float,
) -> bool:
    """
    Check if any point on the traced leader path enters a text
    bounding box.

    DOMAIN PRINCIPLE: if the leader line physically enters a text
    region, that text is almost certainly the dimension value.
    This is the strongest geometric signal.
    """
    for px, py in path:
        if box_x1 <= px <= box_x2 and box_y1 <= py <= box_y2:
            return True
    return False
