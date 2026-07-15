"""
Balloon → callout linker.

Key improvements over v1:
  1. Angular sector scoring replaces the weak direction_penalty heuristic.
     Uses the leader direction from the seed trace (when available) to give a
     strong bonus to callouts in the leader direction and a heavy penalty to
     callouts behind the balloon.
  2. resolve_assignment_conflicts() enforces exclusive callout ownership after
     all assignment passes — prevents two bubbles from silently claiming the
     same dimension.
  3. Scale-aware: accepts effective_max_distance from the detector's scale
     calibration so thresholds adapt to drawing resolution automatically.
"""

from __future__ import annotations

import re
from collections import defaultdict
from dataclasses import dataclass
from typing import Dict, List, Optional, Tuple
import math

try:
    from .callout_rules import CalloutGroup
    from .ocr_rules import NormalizedToken
    from .leader_rules import LineSegment
    from .leader_path_rules import (
        PathSegment,
        EndpointHint,
        extract_path_segments,
        leader_endpoint_hint,
        ray_intersection_linking,
        shared_dimension_hint_for_balloon,
    )
    from .grammar_rules import parse_grammar_token
except ImportError:
    from callout_rules import CalloutGroup
    from ocr_rules import NormalizedToken
    from leader_rules import LineSegment
    from leader_path_rules import (
        PathSegment,
        EndpointHint,
        extract_path_segments,
        leader_endpoint_hint,
        ray_intersection_linking,
        shared_dimension_hint_for_balloon,
    )
    from grammar_rules import parse_grammar_token


# ─────────────────────────────────────────────────────────────────────────────
# Data models
# ─────────────────────────────────────────────────────────────────────────────

@dataclass
class BalloonNode:
    bubble_number: str
    x: float
    y: float
    radius: float


@dataclass
class LinkCandidate:
    balloon_index: int
    callout_index: int
    score: float
    distance: float
    grammar_type: str
    semantic_type: str
    endpoint_gap: float
    direction_penalty: float
    semantic_penalty: float


@dataclass
class LinkResult:
    balloon_index: int
    callout_index: Optional[int]
    assigned_text: str
    score: float
    needs_review: bool


# ─────────────────────────────────────────────────────────────────────────────
# Public API
# ─────────────────────────────────────────────────────────────────────────────

def link_balloons_to_callouts(
    balloons: List[BalloonNode],
    callouts: List[CalloutGroup],
    leader_segments: List[LineSegment],
    max_assoc_distance: float = 400.0,
    max_accept_cost: float = 500.0,
    leader_bonus_weight: float = 42.0,
    image=None,
    grammar_tokens: Optional[List[NormalizedToken]] = None,
    leader_directions: Optional[Dict[int, Tuple[float, float]]] = None,
) -> List[LinkResult]:
    """
    Link each balloon to the best matching callout group.

    Parameters
    ----------
    leader_directions : dict mapping balloon_index → (dir_x, dir_y)
        Leader exit directions from the seed trace step.  When provided,
        the angular sector score heavily rewards callouts in the leader
        direction and penalises those behind the balloon.
    """
    if not balloons:
        return []

    if not callouts:
        return [
            LinkResult(bi, None, "", 1e9, True)
            for bi in range(len(balloons))
        ]

    tokens = grammar_tokens or []
    ld = leader_directions or {}

    # Shared-dimension hints
    shared_hint_by_balloon: Dict[int, bool] = {
        i: (
            shared_dimension_hint_for_balloon(
                balloon_x=b.x, balloon_y=b.y,
                grammar_tokens=tokens,
                search_radius=85.0,
            ) if tokens else False
        )
        for i, b in enumerate(balloons)
    }

    # Optional path-related hints
    path_segments: List[PathSegment] = []
    endpoint_hints_by_balloon: Dict[int, EndpointHint] = {}
    if image is not None:
        path_segments = extract_path_segments(image)
        if path_segments:
            for i, b in enumerate(balloons):
                hint = leader_endpoint_hint(
                    balloon_x=b.x, balloon_y=b.y, balloon_radius=b.radius,
                    segments=path_segments, image=None,
                )
                if hint is not None and _endpoint_hint_is_reasonable(hint):
                    endpoint_hints_by_balloon[i] = hint

    candidates = _build_candidates(
        balloons=balloons,
        callouts=callouts,
        max_assoc_distance=max_assoc_distance,
        shared_hint_by_balloon=shared_hint_by_balloon,
        endpoint_hints_by_balloon=endpoint_hints_by_balloon,
        leader_directions=ld,
    )

    # Build (balloon_index, callout_index) → score lookup from candidates.
    score_lookup: Dict[Tuple[int, int], float] = {
        (c.balloon_index, c.callout_index): c.score
        for c in candidates
    }

    results: List[LinkResult] = [
        LinkResult(i, None, "", 1e9, True)
        for i in range(len(balloons))
    ]

    # ── Partition: shared-dimension balloons are excluded from the global
    # bipartite solve because they legitimately share a single callout.
    # They are handled afterward by _apply_shared_dimension_propagation.
    exclusive_ids = [
        bi for bi in range(len(balloons))
        if not shared_hint_by_balloon.get(bi, False)
    ]

    # ── Global optimal 1-to-1 assignment via Hungarian algorithm ──────────
    # Replace the old greedy sequential passes.  The cost matrix is small
    # (N_balloons × N_callouts, typically < 25×50) so this is microseconds.
    used_callouts: set = set()
    if exclusive_ids and callouts:
        import numpy as np
        from scipy.optimize import linear_sum_assignment

        _INF = 1e9
        n_b = len(exclusive_ids)
        n_c = len(callouts)
        cost_matrix = np.full((n_b, n_c), _INF, dtype=np.float64)

        for row, bi in enumerate(exclusive_ids):
            for ci in range(n_c):
                key = (bi, ci)
                if key in score_lookup:
                    cost_matrix[row, ci] = score_lookup[key]

        row_ind, col_ind = linear_sum_assignment(cost_matrix)

        for row, ci in zip(row_ind, col_ind):
            bi = exclusive_ids[row]
            cost = cost_matrix[row, ci]
            if cost >= _INF:
                # No viable candidate — leave unresolved for ray override below
                continue
            if cost > max_accept_cost:
                # Cost exists but is too high — flag for review, leave unassigned
                results[bi] = LinkResult(bi, None, "", cost, True)
                continue
            needs_review = cost > max_assoc_distance * 0.6
            results[bi] = LinkResult(
                balloon_index=bi,
                callout_index=ci,
                assigned_text=callouts[ci].text,
                score=cost,
                needs_review=needs_review,
            )
            used_callouts.add(ci)

    # ── Ray-intersection override (high-confidence geometric traces) ───────
    # Run *after* Hungarian so a definitive path trace can correct the cost
    # function when leader-direction data was missing or noisy.
    # Only overrides when: (a) the balloon is currently unresolved, OR
    # (b) the Hungarian assignment was low-confidence (high cost) and the ray
    #     points to a different callout.
    if image is not None and path_segments:
        for bi in range(len(balloons)):
            if shared_hint_by_balloon.get(bi, False):
                continue
            b = balloons[bi]
            hit = ray_intersection_linking(
                balloon_x=b.x, balloon_y=b.y, balloon_radius=b.radius,
                callouts=callouts, segments=path_segments, image=None,
                max_ray_length=max_assoc_distance,
                corridor_half_width=22.0, snap_radius=28.0,
            )
            if hit is None or getattr(hit, "hit_type", "") != "path":
                continue
            try:
                hit_idx = int(getattr(hit, "callout_index", -1))
            except (AttributeError, TypeError):
                hit_idx = -1
            if not (0 <= hit_idx < len(callouts)):
                continue

            current = results[bi]
            unresolved = current.callout_index is None
            low_confidence = (
                current.callout_index != hit_idx
                and current.score > max_assoc_distance * 0.8
            )
            if not (unresolved or low_confidence):
                continue

            # Only take hit_idx if it is not already owned by someone more
            # confident (score < current threshold).
            if hit_idx in used_callouts and not unresolved:
                continue

            # Free the previously held callout before reassigning.
            if current.callout_index is not None:
                used_callouts.discard(current.callout_index)

            results[bi] = LinkResult(
                balloon_index=bi,
                callout_index=hit_idx,
                assigned_text=callouts[hit_idx].text,
                score=-1.0,   # sentinel: geometric certainty
                needs_review=False,
            )
            used_callouts.add(hit_idx)

    # ── Shared-dimension propagation ──────────────────────────────────────
    results = _apply_shared_dimension_propagation(
        results=results,
        balloons=balloons,
        shared_hint_by_balloon=shared_hint_by_balloon,
    )

    return results


def resolve_assignment_conflicts(
    results: List[LinkResult],
    callouts: List[CalloutGroup],
) -> List[LinkResult]:
    """
    Enforce exclusive callout ownership after all assignment passes.

    When two bubbles claim the same callout, the one with the lower score
    (more confident) keeps it; the other is reset to unresolved and flagged
    for review.  This eliminates silent double-assignment errors.
    """
    ownership: Dict[int, List[LinkResult]] = defaultdict(list)
    for res in results:
        if res.callout_index is not None:
            ownership[res.callout_index].append(res)

    for ci, claimants in ownership.items():
        if len(claimants) <= 1:
            continue
        # Sort ascending by score — lowest score = most confident
        claimants.sort(key=lambda r: r.score)
        # Losers give up their claim
        for loser in claimants[1:]:
            loser.callout_index  = None
            loser.assigned_text  = ""
            loser.score          = 1e9
            loser.needs_review   = True

    return results


# ─────────────────────────────────────────────────────────────────────────────
# Candidate generation
# ─────────────────────────────────────────────────────────────────────────────

def _line_crosses_balloon(
    x1: float, y1: float,
    x2: float, y2: float,
    balloons: List[BalloonNode],
    exclude_bi: int,
) -> bool:
    """
    True if the segment (x1,y1)→(x2,y2) passes through any balloon circle
    other than the one at index *exclude_bi*.

    Uses the standard point-to-segment distance formula.
    """
    dx = x2 - x1
    dy = y2 - y1
    seg_len2 = dx * dx + dy * dy
    if seg_len2 < 1.0:
        return False

    for bi2, other in enumerate(balloons):
        if bi2 == exclude_bi:
            continue
        # Project other.centre onto the segment, clamp to [0,1]
        ox = other.x - x1
        oy = other.y - y1
        t = max(0.0, min(1.0, (ox * dx + oy * dy) / seg_len2))
        # Closest point on segment to other centre
        cx = x1 + t * dx - other.x
        cy = y1 + t * dy - other.y
        if cx * cx + cy * cy <= other.radius * other.radius:
            return True
    return False


def _build_candidates(
    balloons: List[BalloonNode],
    callouts: List[CalloutGroup],
    max_assoc_distance: float,
    shared_hint_by_balloon: Dict[int, bool],
    endpoint_hints_by_balloon: Dict[int, EndpointHint],
    leader_directions: Dict[int, Tuple[float, float]],
) -> List[LinkCandidate]:
    out: List[LinkCandidate] = []

    for bi, b in enumerate(balloons):
        endpoint_hint = endpoint_hints_by_balloon.get(bi)
        ld_dir = leader_directions.get(bi)   # (dir_x, dir_y) or None
        extended = max_assoc_distance * 1.5

        local_callouts = [
            (ci, c) for ci, c in enumerate(callouts)
            if math.hypot(c.cx - b.x, c.cy - b.y) <= extended
        ]
        if not local_callouts:
            continue

        local_density = len(local_callouts)

        for ci, c in local_callouts:
            # Skip callouts that contain no digits — these are noise tokens,
            # not engineering dimensions (e.g. "T NØ.", stray labels).
            if not re.search(r"\d", c.text):
                continue

            dx = c.cx - b.x
            dy = c.cy - b.y
            dist = math.hypot(dx, dy)

            gt = parse_grammar_token(c.text)

            try:
                # ── Core geometric score ──────────────────────────────────
                distance_score = dist

                # ── Angular sector score (replaces old direction_penalty) ─
                if ld_dir is not None:
                    dir_score = _angular_sector_score(
                        b.x, b.y, c.cx, c.cy, ld_dir[0], ld_dir[1]
                    )
                else:
                    dir_score = _weak_direction_score(dx, dy)

                # ── Semantic score ────────────────────────────────────────
                sem_score = _semantic_score(c, gt)

                # ── Endpoint / trace bonus ────────────────────────────────
                trace_bonus = 0.0
                endpoint_gap = 999.0
                geo_score = 0.0

                if endpoint_hint is not None:
                    endpoint_gap = _point_to_box_distance(
                        endpoint_hint.end_x, endpoint_hint.end_y,
                        c.x1, c.y1, c.x2, c.y2,
                    )
                    # Bonus scales with endpoint proximity and trace confidence
                    if endpoint_gap < 8.0 and endpoint_hint.confidence > 0.7:
                        trace_bonus -= 200.0
                    elif endpoint_gap < 15.0 and endpoint_hint.confidence > 0.5:
                        trace_bonus -= 120.0
                    elif endpoint_gap < 25.0:
                        trace_bonus -= 60.0
                    elif endpoint_gap < 40.0:
                        trace_bonus -= 20.0

                    # Geometric direction alignment with trace endpoint
                    ep_dx = endpoint_hint.end_x - b.x
                    ep_dy = endpoint_hint.end_y - b.y
                    ep_dist = math.hypot(ep_dx, ep_dy)
                    bc_dist = math.hypot(dx, dy)
                    if ep_dist > 1 and bc_dist > 1:
                        dot = (dx / bc_dist) * (ep_dx / ep_dist) + (dy / bc_dist) * (ep_dy / ep_dist)
                        if dot > 0.8:
                            geo_score -= 100.0
                        elif dot > 0.6:
                            geo_score -= 60.0
                        elif dot > 0.4:
                            geo_score -= 30.0

                # ── Shape and density ─────────────────────────────────────
                box_penalty     = _bbox_shape_penalty(c)
                density_penalty = _region_density_penalty(local_density)

                # ── Line-of-sight: penalise if path crosses another balloon ─
                los_penalty = 0.0
                if len(balloons) > 1:
                    if _line_crosses_balloon(b.x, b.y, c.cx, c.cy, balloons, bi):
                        los_penalty = 120.0   # strong but not absolute

                # ── Shared dimension ─────────────────────────────────────
                shared_bonus = -15.0 if shared_hint_by_balloon.get(bi, False) else 0.0

                score = (
                    distance_score + dir_score + sem_score
                    + trace_bonus + geo_score
                    + box_penalty + density_penalty + los_penalty + shared_bonus
                )

            except Exception:
                score = 999.0
                endpoint_gap = 999.0

            out.append(
                LinkCandidate(
                    balloon_index=bi,
                    callout_index=ci,
                    score=score,
                    distance=dist,
                    grammar_type=gt.grammar_type,
                    semantic_type=gt.semantic_type,
                    endpoint_gap=endpoint_gap,
                    direction_penalty=0.0,
                    semantic_penalty=0.0,
                )
            )

    return out


# ─────────────────────────────────────────────────────────────────────────────
# Scoring helpers
# ─────────────────────────────────────────────────────────────────────────────

def _angular_sector_score(
    balloon_x: float, balloon_y: float,
    callout_cx: float, callout_cy: float,
    leader_dir_x: float, leader_dir_y: float,
) -> float:
    """
    Score a callout based on whether it lies in the leader direction.

    Uses dot product between the balloon→callout vector and the leader
    direction vector.  Returns a negative value (bonus) for alignment and a
    positive value (penalty) for misalignment.
    """
    dx = callout_cx - balloon_x
    dy = callout_cy - balloon_y
    dist = math.hypot(dx, dy)
    if dist < 1.0:
        return 0.0

    dot = (dx / dist) * leader_dir_x + (dy / dist) * leader_dir_y

    if dot > 0.80:
        return -40.0   # strongly in leader direction → big bonus
    if dot > 0.50:
        return -20.0   # good alignment
    if dot > 0.20:
        return -5.0   # loose alignment
    if dot > -0.20:
        return 0.0   # perpendicular → neutral
    if dot > -0.50:
        return 25.0   # partial opposite
    return 55.0   # behind balloon → heavy penalty


def _weak_direction_score(dx: float, dy: float) -> float:
    """
    Weak geometric hint used when no leader direction is available.
    Penalises callouts strongly to the left or purely vertical.
    """
    score = 0.0
    if dx < -20:
        score += min(40.0, abs(dx) * 0.10)
    if abs(dy) > abs(dx) * 2.5:
        score += min(20.0, abs(dy) * 0.05)
    return score


def _semantic_score(callout: CalloutGroup, gt) -> float:
    score = 0.0

    # Reward well-structured engineering dimensions
    if gt.semantic_type in {"diameter", "radius", "thread", "chamfer", "tolerance"}:
        score -= 30.0
    elif gt.semantic_type == "angle":
        score -= 10.0
    elif gt.semantic_type == "numeric":
        score -= 5.0
    else:
        score += 10.0

    # Penalise malformed or ambiguous callouts
    text = callout.text.strip()
    if not text or len(text) < 2:
        score += 50.0
    if callout.callout_type in {"mixed", "unknown"}:
        score += 12.0

    return score


def _bbox_shape_penalty(callout: CalloutGroup) -> float:
    w = max(1.0, callout.x2 - callout.x1)
    h = max(1.0, callout.y2 - callout.y1)
    area = w * h
    if area > 18000:
        return 18.0
    if area > 9000:
        return 8.0
    return 0.0


def _region_density_penalty(local_density: int) -> float:
    if local_density <= 1:
        return 0.0
    return min(10.0, local_density * 0.8)


def _point_to_box_distance(
    px: float, py: float,
    x1: float, y1: float,
    x2: float, y2: float,
) -> float:
    dx = max(x1 - px, 0.0, px - x2)
    dy = max(y1 - py, 0.0, py - y2)
    return math.hypot(dx, dy)


def _endpoint_hint_is_reasonable(hint: EndpointHint) -> bool:
    return hint.confidence >= 0.30 and hint.path_length >= 12.0


# ─────────────────────────────────────────────────────────────────────────────
# Assignment passes
# ─────────────────────────────────────────────────────────────────────────────

def _choose_fast_candidate(
    cand_list: List[LinkCandidate],
    used_callouts: set,
    shared_hint: bool,
    max_assoc_distance: float,
) -> Tuple[Optional[LinkCandidate], bool, bool]:
    """
    Return (chosen, needs_review, is_ambiguous).
    is_ambiguous=True means the case should be sent to the heavier pass.
    """
    available = [c for c in cand_list if c.callout_index not in used_callouts or shared_hint]
    if not available:
        return None, True, False

    best = available[0]

    # Ambiguous: second candidate within 20% score of best
    if len(available) >= 2:
        gap = available[1].score - best.score
        if gap < 20.0:
            return None, True, True

    # Reject extremely poor matches
    if best.score > max_assoc_distance * 2:
        return None, True, False

    needs_review = best.distance > max_assoc_distance * 0.6
    return best, needs_review, False


def _choose_conservative_fallback_candidate(
    cand_list: List[LinkCandidate],
    used_callouts: set,
    shared_hint: bool,
    max_accept_cost: float,
    callouts: List[CalloutGroup],
) -> Optional[LinkCandidate]:
    for c in cand_list:
        if c.callout_index in used_callouts and not shared_hint:
            continue
        if c.score > max_accept_cost:
            continue
        if 0 <= c.callout_index < len(callouts):
            return c
    return None


# ─────────────────────────────────────────────────────────────────────────────
# Shared dimension propagation
# ─────────────────────────────────────────────────────────────────────────────

def _apply_shared_dimension_propagation(
    results: List[LinkResult],
    balloons: List[BalloonNode],
    shared_hint_by_balloon: Dict[int, bool],
) -> List[LinkResult]:
    """
    Propagate the shared-dimension assignment to balloons that share a callout.
    For example bubbles 3 & 4 may both point to 'MJ5x0.8 4h6h'.
    """
    resolved_shared = [
        res for res in results
        if res.callout_index is not None
        and shared_hint_by_balloon.get(res.balloon_index, False)
    ]

    for res in results:
        if res.callout_index is not None:
            continue
        if not shared_hint_by_balloon.get(res.balloon_index, False):
            continue

        b = balloons[res.balloon_index]
        closest: Optional[LinkResult] = None
        closest_dist = float("inf")

        for source_res in resolved_shared:
            if source_res.balloon_index == res.balloon_index:
                continue
            sb = balloons[source_res.balloon_index]
            d = math.hypot(b.x - sb.x, b.y - sb.y)
            if d < closest_dist:
                closest_dist = d
                closest = source_res

        if closest is not None:
            res.callout_index  = closest.callout_index
            res.assigned_text  = closest.assigned_text
            res.score          = closest.score + 0.01
            res.needs_review   = False

    return results
