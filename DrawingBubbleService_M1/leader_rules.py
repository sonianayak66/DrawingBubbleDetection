"""
General-purpose leader-line evidence rules.

Design goals:
- keep this module a secondary geometric signal, not the primary linker
- stay conservative and fast on unseen mechanical drawings
- prefer thin annotation-like segments over part outlines / long baselines
- avoid image-specific thresholds wherever practical

This module intentionally does NOT perform full global path tracing.
That remains the job of leader_seed_rules / leader_path_rules.
"""

from __future__ import annotations

from dataclasses import dataclass
from typing import List, Tuple
import math

import cv2
import numpy as np


# ── Data Model ────────────────────────────────────────────────────

@dataclass
class LineSegment:
    x1: int
    y1: int
    x2: int
    y2: int
    length: float
    angle_deg: float


# ── Public API ────────────────────────────────────────────────────

def extract_leader_segments(image: np.ndarray) -> List[LineSegment]:
    """
    Extract conservative candidate leader-like line segments.

    Performance-oriented strategy:
    - derive a dark-line mask from the drawing instead of assuming a specific color
    - bias toward thin medium-length strokes
    - reject long baseline-like segments early
    - run a compact set of detectors instead of a broad union of redundant passes
    - aggressively deduplicate and keep only the strongest leader-like candidates

    Output remains intentionally conservative: a compact list of plausible
    leader segments used only as secondary evidence.
    """
    if image is None or image.size == 0:
        return []

    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    h, w = gray.shape[:2]
    short = max(1, min(h, w))

    blur = cv2.GaussianBlur(gray, (3, 3), 0)

    # Robust dark-line mask without assuming a specific annotation color.
    _, otsu_inv = cv2.threshold(
        blur, 0, 255, cv2.THRESH_BINARY_INV + cv2.THRESH_OTSU
    )
    adaptive_block = 31 if short >= 500 else 21
    adaptive_inv = cv2.adaptiveThreshold(
        blur,
        255,
        cv2.ADAPTIVE_THRESH_GAUSSIAN_C,
        cv2.THRESH_BINARY_INV,
        adaptive_block,
        7,
    )
    dark_mask = cv2.bitwise_or(otsu_inv, adaptive_inv)

    # Clean speckle while preserving narrow strokes.
    open3 = cv2.morphologyEx(
        dark_mask,
        cv2.MORPH_OPEN,
        cv2.getStructuringElement(cv2.MORPH_RECT, (3, 3)),
    )
    close3 = cv2.morphologyEx(
        open3,
        cv2.MORPH_CLOSE,
        cv2.getStructuringElement(cv2.MORPH_RECT, (3, 3)),
    )

    # Prefer diagonal-ish leader structure; horizontal/vertical masks are useful
    # but are stronger distractors, so we use them more conservatively.
    diag_mask = cv2.bitwise_or(
        cv2.morphologyEx(close3, cv2.MORPH_OPEN, _diag_kernel(9, slash=True)),
        cv2.morphologyEx(close3, cv2.MORPH_OPEN, _diag_kernel(9, slash=False)),
    )

    hv_mask = cv2.bitwise_or(
        cv2.morphologyEx(
            close3,
            cv2.MORPH_OPEN,
            cv2.getStructuringElement(cv2.MORPH_RECT, (9, 1)),
        ),
        cv2.morphologyEx(
            close3,
            cv2.MORPH_OPEN,
            cv2.getStructuringElement(cv2.MORPH_RECT, (1, 9)),
        ),
    )

    edges = cv2.Canny(blur, 50, 140)

    min_len = max(12, int(round(short * 0.018)))
    max_len = max(min_len + 18, int(round(short * 0.180)))
    max_gap = max(3, int(round(short * 0.008)))

    raw_segments: List[LineSegment] = []

    # PERFORMANCE OPTIMIZATION: Reduce detector configurations and filter early
    # Use fewer, more focused detectors to improve runtime
    
    # Early filter: remove long baseline-like horizontal segments
    filtered_mask = _filter_baseline_segments(close3, short)
    
    # Optimized detector set - fewer passes, higher thresholds
    detector_configs = [
        (diag_mask, 35, min_len, max_gap),  # Higher threshold, fewer segments
        (filtered_mask, 40, min_len, max_gap),  # Filtered input
        (edges, 45, min_len, max_gap),  # Higher threshold
    ]
    
    # Early termination if we get enough good segments
    segment_count = 0
    max_segments_early = 80  # Stop early if we have enough
    
    for mask, threshold, this_min_len, this_gap in detector_configs:
        lines = cv2.HoughLinesP(
            mask,
            rho=1,
            theta=np.pi / 180,
            threshold=threshold,
            minLineLength=this_min_len,
            maxLineGap=this_gap,
        )
        if lines is None:
            continue

        for l in lines:
            x1, y1, x2, y2 = map(int, l[0])
            seg = _make_segment(x1, y1, x2, y2)
            if seg is None:
                continue
            if seg.length < min_len or seg.length > max_len:
                continue
            if not _segment_is_annotation_like(seg, close3, short):
                continue
            
            raw_segments.append(seg)
            segment_count += 1
            
            # Early termination to improve performance
            if segment_count >= max_segments_early:
                break
        
        if segment_count >= max_segments_early:
            break

    # DISABLE LSD for performance - it's expensive and often creates noise
    # Only enable for very small drawings if absolutely needed
    # if short <= 1000 and len(raw_segments) < 20 and hasattr(cv2, "createLineSegmentDetector"):
    #     try:
    #         lsd = cv2.createLineSegmentDetector(0)
    #         lsd_lines = lsd.detect(blur)[0]
    #         if lsd_lines is not None:
    #             for line in lsd_lines:
    #                 x1, y1, x2, y2 = map(int, line[0])
    #                 seg = _make_segment(x1, y1, x2, y2)
    #                 if seg is None:
    #                     continue
    #                 if seg.length < min_len or seg.length > max_len:
    #                     continue
    #                 if not _segment_is_annotation_like(seg, close3, short):
    #                     continue
    #                 raw_segments.append(seg)
    #     except cv2.error:
    #         pass

    deduped = _dedup_segments(raw_segments)

    # Keep only the strongest leader-like candidates. This is critical for runtime:
    # downstream pair scoring should consume a compact evidence set, not hundreds of
    # weak segments.
    ranked = sorted(
        deduped,
        key=lambda s: _segment_priority_score(s, close3, short),
        reverse=True,
    )

    max_keep = _max_segments_to_keep(short)
    return ranked[:max_keep]



def leader_score_for_pair(
    balloon_x: float,
    balloon_y: float,
    balloon_radius: float,
    callout_x1: float,
    callout_y1: float,
    callout_x2: float,
    callout_y2: float,
    segments: List[LineSegment],
) -> float:
    """
    Returns a conservative bonus score in roughly [0, 1].

    Higher means stronger local segment evidence that a balloon points toward a
    callout. This function remains intentionally modest because traced-path logic
    should dominate whenever available.
    """
    if not segments:
        return 0.0

    cx = 0.5 * (callout_x1 + callout_x2)
    cy = 0.5 * (callout_y1 + callout_y2)

    vx = cx - balloon_x
    vy = cy - balloon_y
    pair_dist = math.hypot(vx, vy)
    if pair_dist < 1e-6:
        return 0.0

    ux = vx / pair_dist
    uy = vy / pair_dist

    near_balloon = [
        s for s in segments
        if _segment_touches_balloon(balloon_x, balloon_y, balloon_radius, s)
    ]
    if not near_balloon:
        return 0.0

    callout_diag = math.hypot(callout_x2 - callout_x1, callout_y2 - callout_y1)
    callout_hit_margin = max(8.0, min(28.0, 0.18 * callout_diag + 8.0))

    best = 0.0

    for s in near_balloon:
        start_x, start_y, end_x, end_y = _orient_segment_outward(
            balloon_x=balloon_x,
            balloon_y=balloon_y,
            segment=s,
        )

        svx = end_x - start_x
        svy = end_y - start_y
        slen = math.hypot(svx, svy)
        if slen < 1e-6:
            continue

        sux = svx / slen
        suy = svy / slen

        dir_dot = sux * ux + suy * uy
        if dir_dot < 0.20:
            continue

        start_radial = math.hypot(start_x - balloon_x, start_y - balloon_y)
        end_radial = math.hypot(end_x - balloon_x, end_y - balloon_y)
        outward_gain = end_radial - start_radial
        if outward_gain < max(2.0, 0.08 * balloon_radius):
            continue

        end_box_dist = _point_to_box_distance(
            end_x, end_y, callout_x1, callout_y1, callout_x2, callout_y2
        )
        start_box_dist = _point_to_box_distance(
            start_x, start_y, callout_x1, callout_y1, callout_x2, callout_y2
        )
        approach_gain = start_box_dist - end_box_dist

        corridor_penalty = _distance_point_to_infinite_line(
            end_x, end_y, balloon_x, balloon_y, cx, cy
        )

        seg_angle_abs = _angle_from_vector(svx, svy)
        pair_angle_abs = _angle_from_vector(vx, vy)
        angle_gap = _acute_angle_diff(seg_angle_abs, pair_angle_abs)

        score = 0.0

        # Primary alignment / outward evidence.
        score += 0.42 * max(0.0, dir_dot)
        score += 0.16 * min(1.0, outward_gain / max(8.0, balloon_radius))

        # Endpoint proximity to the real note region matters more than generic corridor closeness.
        if end_box_dist <= callout_hit_margin:
            score += 0.34
        elif end_box_dist <= 36.0:
            score += 0.22
        elif end_box_dist <= 68.0:
            score += 0.10

        # Segment should materially move toward the callout.
        if approach_gain > 30.0:
            score += 0.12
        elif approach_gain > 14.0:
            score += 0.06
        elif approach_gain < 0.0:
            score -= 0.12

        # Medium-length annotation strokes are most trustworthy here.
        if 16.0 <= s.length <= 110.0:
            score += 0.08
        elif s.length > 150.0:
            score -= 0.10

        # Strongly horizontal segments are a common distractor because of dimension baselines.
        if s.length > 40.0 and seg_angle_abs < 9.0:
            score -= 0.16
        elif s.length > 55.0 and seg_angle_abs < 16.0:
            score -= 0.08

        # Strongly vertical segments can also be centerlines / part geometry.
        if s.length > 55.0 and seg_angle_abs > 82.0:
            score -= 0.10

        # Penalize mismatch against balloon->callout direction.
        if angle_gap > 34.0:
            score -= 0.14
        elif angle_gap > 22.0:
            score -= 0.08

        # Keep the segment close to the intended corridor, but not as the primary criterion.
        if corridor_penalty > 54.0:
            score -= 0.16
        elif corridor_penalty > 32.0:
            score -= 0.08

        best = max(best, score)

    return max(0.0, min(1.0, best))


# ── Internal Helpers ──────────────────────────────────────────────

def _make_segment(x1: int, y1: int, x2: int, y2: int) -> LineSegment | None:
    dx = x2 - x1
    dy = y2 - y1
    length = math.hypot(dx, dy)
    if length < 1e-6:
        return None
    angle_deg = _angle_from_vector(dx, dy)
    return LineSegment(
        x1=int(x1),
        y1=int(y1),
        x2=int(x2),
        y2=int(y2),
        length=float(length),
        angle_deg=float(angle_deg),
    )



def _diag_kernel(size: int, slash: bool) -> np.ndarray:
    k = np.zeros((size, size), dtype=np.uint8)
    for i in range(size):
        j = size - 1 - i if slash else i
        k[i, j] = 1
    return k



def _segment_is_annotation_like(seg: LineSegment, fg_mask: np.ndarray, short: int) -> bool:
    # Prefer thin strokes with sparse local support around the segment.
    thickness = _segment_thickness_proxy(seg, fg_mask)
    if thickness > 4.8:
        return False

    occ = _segment_band_occupancy(seg, fg_mask, band_half_width=2)
    if occ < 0.22:
        return False
    if occ > 0.88:
        return False

    angle = seg.angle_deg

    # Very short near-horizontal/vertical fragments are frequently OCR leftovers.
    if seg.length < max(16.0, short * 0.018):
        if angle < 12.0 or angle > 78.0:
            return False

    # Long perfectly horizontal/vertical lines are often dimension baselines,
    # centerlines, or object edges rather than leaders.
    if seg.length > max(42.0, short * 0.055) and angle < 8.0:
        return False
    if seg.length > max(64.0, short * 0.080) and angle > 84.0:
        return False

    # Leaders are often diagonal or mildly slanted. Keep horizontal/vertical
    # only when they are not long.
    if angle < 6.0 and seg.length > max(28.0, short * 0.035):
        return False
    if angle > 86.0 and seg.length > max(40.0, short * 0.050):
        return False

    return True



def _segment_priority_score(seg: LineSegment, fg_mask: np.ndarray, short: int) -> float:
    """
    Rank segments by how annotation-like they appear, so we can cap the pool
    without relying on image-specific assumptions.
    """
    occ = _segment_band_occupancy(seg, fg_mask, band_half_width=2)
    thickness = _segment_thickness_proxy(seg, fg_mask)
    angle = seg.angle_deg

    score = 0.0

    # Prefer medium-length segments.
    ideal_len = max(24.0, short * 0.045)
    len_err = abs(seg.length - ideal_len)
    score += max(0.0, 1.0 - (len_err / max(ideal_len, 1.0)))

    # Prefer thin strokes.
    score += max(0.0, 1.2 - 0.25 * thickness)

    # Prefer diagonal-ish segments over horizontal/vertical distractors.
    diag_pref = 1.0 - abs(angle - 35.0) / 35.0
    score += max(0.0, diag_pref)

    # Prefer moderate occupancy.
    score += max(0.0, 1.0 - abs(occ - 0.55) / 0.55)

    # Penalize baseline-like geometry.
    if angle < 10.0 and seg.length > max(30.0, short * 0.040):
        score -= 1.2
    if angle > 82.0 and seg.length > max(44.0, short * 0.055):
        score -= 0.8

    return score



def _max_segments_to_keep(short: int) -> int:
    # Keep runtime practical while still allowing enough evidence on dense drawings.
    if short < 700:
        return 120
    if short < 1200:
        return 180
    if short < 1800:
        return 240
    return 300



def _segment_thickness_proxy(seg: LineSegment, fg_mask: np.ndarray) -> float:
    # Use a distance transform on the foreground and sample along the segment.
    # For a one-pixel centerline, DT near the center is small; thick outlines are larger.
    if fg_mask.dtype != np.uint8:
        mask = fg_mask.astype(np.uint8)
    else:
        mask = fg_mask
    if mask.max() > 1:
        bin_mask = (mask > 0).astype(np.uint8)
    else:
        bin_mask = mask

    dt = cv2.distanceTransform(bin_mask, cv2.DIST_L2, 3)
    pts = _sample_segment_points(seg, n=7)
    vals = []
    h, w = dt.shape[:2]
    for px, py in pts:
        ix = int(round(px))
        iy = int(round(py))
        if 0 <= ix < w and 0 <= iy < h:
            vals.append(float(dt[iy, ix]))
    if not vals:
        return 999.0
    return 2.0 * float(np.median(vals))



def _segment_band_occupancy(
    seg: LineSegment,
    fg_mask: np.ndarray,
    band_half_width: int = 2,
) -> float:
    h, w = fg_mask.shape[:2]
    band = np.zeros((h, w), dtype=np.uint8)
    cv2.line(
        band,
        (int(seg.x1), int(seg.y1)),
        (int(seg.x2), int(seg.y2)),
        255,
        thickness=max(1, 2 * band_half_width + 1),
    )
    band_n = int(np.count_nonzero(band))
    if band_n <= 0:
        return 0.0
    overlap = cv2.bitwise_and((fg_mask > 0).astype(np.uint8) * 255, band)
    return float(np.count_nonzero(overlap)) / float(band_n)



def _sample_segment_points(seg: LineSegment, n: int = 7) -> List[Tuple[float, float]]:
    if n <= 1:
        return [(0.5 * (seg.x1 + seg.x2), 0.5 * (seg.y1 + seg.y2))]
    pts: List[Tuple[float, float]] = []
    for i in range(n):
        t = i / float(n - 1)
        pts.append((seg.x1 + t * (seg.x2 - seg.x1), seg.y1 + t * (seg.y2 - seg.y1)))
    return pts



def _dedup_segments(segments: List[LineSegment]) -> List[LineSegment]:
    if not segments:
        return []

    ordered = sorted(
        segments,
        key=lambda s: (-s.length, s.angle_deg, s.x1, s.y1, s.x2, s.y2),
    )
    out: List[LineSegment] = []

    for s in ordered:
        keep = True
        sx1, sy1, sx2, sy2 = _canonical_endpoints(s)
        for t in out:
            tx1, ty1, tx2, ty2 = _canonical_endpoints(t)
            if (
                abs(sx1 - tx1) <= 6 and
                abs(sy1 - ty1) <= 6 and
                abs(sx2 - tx2) <= 6 and
                abs(sy2 - ty2) <= 6
            ):
                keep = False
                break
            if _collinear_overlap_ratio(s, t) > 0.70:
                keep = False
                break
        if keep:
            out.append(s)

    return out



def _canonical_endpoints(s: LineSegment) -> Tuple[int, int, int, int]:
    a = (s.x1, s.y1)
    b = (s.x2, s.y2)
    if a <= b:
        return a[0], a[1], b[0], b[1]
    return b[0], b[1], a[0], a[1]



def _collinear_overlap_ratio(a: LineSegment, b: LineSegment) -> float:
    ang_gap = _acute_angle_diff(a.angle_deg, b.angle_deg)
    if ang_gap > 6.0:
        return 0.0

    if (
        _point_to_segment_distance(a.x1, a.y1, b.x1, b.y1, b.x2, b.y2) > 5.0 and
        _point_to_segment_distance(a.x2, a.y2, b.x1, b.y1, b.x2, b.y2) > 5.0
    ):
        return 0.0

    ax = a.x2 - a.x1
    ay = a.y2 - a.y1
    alen = math.hypot(ax, ay)
    if alen < 1e-6:
        return 0.0
    ux = ax / alen
    uy = ay / alen

    def proj(px: float, py: float) -> float:
        return (px - a.x1) * ux + (py - a.y1) * uy

    a0, a1 = 0.0, alen
    b0 = proj(b.x1, b.y1)
    b1 = proj(b.x2, b.y2)
    lo = max(min(a0, a1), min(b0, b1))
    hi = min(max(a0, a1), max(b0, b1))
    overlap = max(0.0, hi - lo)
    denom = max(1.0, min(a.length, b.length))
    return overlap / denom



def _segment_touches_balloon(cx: float, cy: float, r: float, s: LineSegment) -> bool:
    d = _point_to_segment_distance(cx, cy, s.x1, s.y1, s.x2, s.y2)
    rim_tol = max(5.0, 0.30 * r)

    if abs(d - r) <= rim_tol:
        return True

    d1 = math.hypot(s.x1 - cx, s.y1 - cy)
    d2 = math.hypot(s.x2 - cx, s.y2 - cy)
    endpoint_tol = max(6.0, 0.34 * r)
    if abs(d1 - r) <= endpoint_tol or abs(d2 - r) <= endpoint_tol:
        return True

    return False



def _orient_segment_outward(
    balloon_x: float,
    balloon_y: float,
    segment: LineSegment,
) -> Tuple[float, float, float, float]:
    d1 = math.hypot(segment.x1 - balloon_x, segment.y1 - balloon_y)
    d2 = math.hypot(segment.x2 - balloon_x, segment.y2 - balloon_y)
    if d1 <= d2:
        return float(segment.x1), float(segment.y1), float(segment.x2), float(segment.y2)
    return float(segment.x2), float(segment.y2), float(segment.x1), float(segment.y1)



def _angle_from_vector(dx: float, dy: float) -> float:
    angle_deg = abs(math.degrees(math.atan2(dy, dx)))
    return min(angle_deg, 180.0 - angle_deg)



def _acute_angle_diff(a_deg: float, b_deg: float) -> float:
    diff = abs(a_deg - b_deg) % 180.0
    return min(diff, 180.0 - diff)



def _point_to_segment_distance(
    px: float,
    py: float,
    x1: float,
    y1: float,
    x2: float,
    y2: float,
) -> float:
    dx = x2 - x1
    dy = y2 - y1
    seg_len_sq = dx * dx + dy * dy
    if seg_len_sq < 1e-12:
        return math.hypot(px - x1, py - y1)

    u = ((px - x1) * dx + (py - y1) * dy) / seg_len_sq
    u = max(0.0, min(1.0, u))
    ix = x1 + u * dx
    iy = y1 + u * dy
    return math.hypot(px - ix, py - iy)



def _point_to_box_distance(
    px: float,
    py: float,
    x1: float,
    y1: float,
    x2: float,
    y2: float,
) -> float:
    dx = max(x1 - px, 0.0, px - x2)
    dy = max(y1 - py, 0.0, py - y2)
    return math.hypot(dx, dy)



def _distance_point_to_infinite_line(
    px: float,
    py: float,
    x1: float,
    y1: float,
    x2: float,
    y2: float,
) -> float:
    den = math.hypot(x2 - x1, y2 - y1)
    if den < 1e-12:
        return math.hypot(px - x1, py - y1)
    return abs((y2 - y1) * px - (x2 - x1) * py + x2 * y1 - y2 * x1) / den

def _filter_baseline_segments(mask: np.ndarray, short: int) -> np.ndarray:
    """
    Filter out long baseline-like horizontal segments to improve performance.
    
    This removes long horizontal lines that are likely dimension baselines
    rather than leader lines, reducing the number of segments that need to be processed.
    """
    # Create a horizontal kernel to detect long horizontal lines
    h, w = mask.shape
    min_baseline_length = int(w * 0.3)  # At least 30% of image width
    
    # Use morphological operations to find long horizontal structures
    horizontal_kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (min_baseline_length, 1))
    horizontal_lines = cv2.morphologyEx(mask, cv2.MORPH_OPEN, horizontal_kernel)
    
    # Subtract long horizontal lines from the mask
    filtered = cv2.subtract(mask, horizontal_lines)
    
    return filtered