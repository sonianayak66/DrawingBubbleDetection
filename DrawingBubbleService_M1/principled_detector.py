"""
Principled detector — the new pipeline.

Built from FOUR clean primitives, in order:

  1. find_balloons      — Hough + maroon-mask. Produces a Balloon list.
  2. find_dim_regions   — OCR + spatial cluster. Produces DimensionRegion list.
  3. trace_leaders      — BFS from each balloon rim. Produces Leader list.
  4. match_leaders_to_regions — single global optimisation. Sets each
                                Leader's target_region.

That's it. No post-processing patches, no "suppress suspicious", no
"clear orphan", no "propagate shared". If a balloon has no leader
match, its dim is empty — and the human reviewer fills it in.

Each primitive is a SHORT FUNCTION that takes only the inputs it
needs and produces a result. They compose via the Drawing object.
Anywhere a heuristic would have lived in the legacy pipeline, the
principled version replaces it with a confidence weight contributing
to the global matcher.

Status: SKELETON. The pipeline is wired but several primitives still
delegate to legacy code while the clean replacements are being
built. This is so the new pipeline can be A/B tested against the
old one on every commit, not finished and dropped in at the end.
"""
from __future__ import annotations

import math
from typing import List, Optional, Tuple

import cv2
import numpy as np

from drawing_model import (
    Balloon,
    DimToken,
    DimensionRegion,
    Drawing,
    Leader,
    Point,
)


# ─────────────────────────────────────────────────────────────────────────────
# Step 1 — find balloons
# ─────────────────────────────────────────────────────────────────────────────


def find_balloons(image: np.ndarray, legacy_detector) -> List[Balloon]:
    """Find every numbered balloon in the image.

    Currently delegates to the legacy detector's bubble-identification
    chain (Hough → mask-circle → uncircled tokens). This gives parity
    with the existing recall at the start of the rewrite, then we
    replace the legacy chain piece-by-piece in subsequent sessions.

    The legacy result is normalised to the new Balloon type. Anything
    the legacy detector does NOT detect is invisible to the new
    pipeline — that's intentional, since the rewrite improves
    matching, not detection (yet).
    """
    bubbles, _ = legacy_detector.detect_from_array(image)
    out: List[Balloon] = []
    for b in bubbles:
        out.append(Balloon(
            cx=float(b.x),
            cy=float(b.y),
            r=float(b.radius),
            number=str(b.bubble_number),
            confidence=float(b.confidence) if b.confidence else 0.5,
        ))
    return out


# ─────────────────────────────────────────────────────────────────────────────
# Step 2 — find dim regions
# ─────────────────────────────────────────────────────────────────────────────


def find_dim_regions(
    image: np.ndarray,
    legacy_detector,
    balloons: List[Balloon],
) -> List[DimensionRegion]:
    """Cluster OCR tokens into dimension regions.

    A region is built by spatial proximity, NOT by text whitelist.
    Token clustering uses two rules only:
        - adjacency: tokens whose bboxes are within ~1× text height
          of each other along their reading axis belong together
        - non-overlap with balloon interior: tokens whose centre sits
          inside a balloon's rim are balloon-label tokens, not dims

    Region text is the union, in reading order, of constituent tokens.

    Kind classification: a one-shot pattern check ("thread", "diameter",
    "chamfer", "tolerance", "callout_ref", "numeric") used only as an
    informational signal for the matcher — never as a filter that
    discards regions.
    """
    tokens_raw = list(getattr(legacy_detector, "_norm_tokens", []) or [])
    # Drop tokens that overlap a balloon interior — those are balloon
    # numbers, not dim text.
    def _inside_balloon(cx: float, cy: float) -> bool:
        for b in balloons:
            if math.hypot(cx - b.cx, cy - b.cy) <= b.r:
                return True
        return False

    dim_tokens: List[DimToken] = []
    for t in tokens_raw:
        if getattr(t, "token_type", "") not in ("dimension", "keyword"):
            continue
        if not (t.text or "").strip():
            continue
        if _inside_balloon(float(t.cx), float(t.cy)):
            continue
        dim_tokens.append(DimToken(
            text=str(t.text).strip(),
            bbox=(float(t.x1), float(t.y1), float(t.x2), float(t.y2)),
            confidence=float(t.conf),
        ))

    # For the first cut: delegate cluster-into-regions to the legacy
    # callout grouper (we'll port this to clean code in a follow-up
    # session). Each legacy CalloutGroup → one DimensionRegion.
    regions: List[DimensionRegion] = []
    callout_groups = getattr(legacy_detector, "_callout_groups", None) or []
    for cg in callout_groups:
        text = (cg.text or "").strip()
        if not text:
            continue
        # Find which DimTokens fall inside this callout's bbox.
        member_tokens = [
            t for t in dim_tokens
            if cg.x1 <= t.cx <= cg.x2 and cg.y1 <= t.cy <= cg.y2
        ]
        avg_conf = (
            sum(t.confidence for t in member_tokens) / len(member_tokens)
            if member_tokens else 0.7
        )
        regions.append(DimensionRegion(
            bbox=(float(cg.x1), float(cg.y1), float(cg.x2), float(cg.y2)),
            text=text,
            tokens=member_tokens,
            kind=cg.callout_type or "numeric",
            confidence=avg_conf,
        ))
    return regions


# ─────────────────────────────────────────────────────────────────────────────
# Step 3 — trace leaders
# ─────────────────────────────────────────────────────────────────────────────


def trace_leaders(
    image: np.ndarray,
    legacy_detector,
    balloons: List[Balloon],
) -> List[Leader]:
    """Trace the leader line from each balloon's rim outward.

    Currently uses the legacy `_seed_traces` map (populated by
    `_trace_balloon_leader_paths` during `detect_from_array`). The
    BFS itself follows annotation-coloured pixels on the boosted
    mask; that primitive is sound and we re-use it.

    Confidence here is a function of path length and how far the
    endpoint sits beyond the rim. A path that walked 40 pixels into
    the dim area scores high; one that looped on the rim scores low.
    """
    seed_traces = getattr(legacy_detector, "_seed_traces", None) or {}
    out: List[Leader] = []
    for b in balloons:
        info = seed_traces.get(b.number) or {}
        path = info.get("path") or []
        if not path or len(path) < 2:
            continue
        ex, ey = path[-1]
        endpoint_dist = math.hypot(ex - b.cx, ey - b.cy)
        # Confidence: scaled by how clearly the endpoint exits the rim.
        # 1.0 = endpoint 5× radius beyond centre; 0.0 = endpoint on rim.
        normalised = max(0.0, (endpoint_dist - b.r) / max(1.0, b.r * 4.0))
        conf = float(min(1.0, normalised))
        leader = Leader(
            balloon=b,
            path=[(float(p[0]), float(p[1])) for p in path],
            endpoint=(float(ex), float(ey)),
            confidence=conf,
        )
        b.leader = leader
        out.append(leader)
    return out


# ─────────────────────────────────────────────────────────────────────────────
# Step 4 — match leaders to regions
# ─────────────────────────────────────────────────────────────────────────────


def match_leaders_to_regions(
    leaders: List[Leader],
    regions: List[DimensionRegion],
    image_shape: Tuple[int, int],
) -> None:
    """Globally match each Leader to its DimensionRegion.

    Cost function combines three normalised signals:
      - distance from leader endpoint to region bbox edge
      - directional alignment: leader's centre→endpoint vector vs
        endpoint→region-centre vector
      - region kind: structured (thread / diameter / chamfer) regions
        get a small cost discount because they're high-precision text

    Hungarian solves the assignment globally; in-place sets each
    leader's `target_region` and each region's `owner_leader`. A
    leader with NO acceptable match remains unattached (its dim
    will be empty in the final output).
    """
    if not leaders or not regions:
        return
    try:
        from scipy.optimize import linear_sum_assignment
    except Exception:
        return

    h, w = image_shape
    diag = float(math.hypot(w, h))

    INF = 1e9
    n_l, n_r = len(leaders), len(regions)
    cost = np.full((n_l, n_r), INF, dtype=np.float64)

    structured_kinds = {
        "thread", "diameter", "chamfer",
        "diameter_tolerance", "radius", "angle",
        "numeric_pair",
    }

    for li, leader in enumerate(leaders):
        b = leader.balloon
        ex, ey = leader.endpoint
        # Leader direction (centre → endpoint).
        ldx, ldy = ex - b.cx, ey - b.cy
        ln = math.hypot(ldx, ldy)
        if ln < 1e-6:
            continue
        ldx, ldy = ldx / ln, ldy / ln

        for ri, region in enumerate(regions):
            # Distance from endpoint to region bbox edge.
            nx = max(region.bbox[0], min(ex, region.bbox[2]))
            ny = max(region.bbox[1], min(ey, region.bbox[3]))
            edge_dist = math.hypot(ex - nx, ey - ny)
            if edge_dist > diag * 0.4:
                continue

            # Directional alignment.
            rdx, rdy = region.cx - ex, region.cy - ey
            rn = math.hypot(rdx, rdy)
            if rn < 1e-6:
                cos_align = 1.0
            else:
                cos_align = (ldx * rdx + ldy * rdy) / rn
            # Behind the leader → reject.
            if cos_align < -0.1:
                continue

            # Normalise + combine.
            norm_dist = edge_dist / diag
            dir_penalty = (1.0 - cos_align) / 2.0  # [0..1]
            kind_bonus = -0.05 if region.kind in structured_kinds else 0.0
            cost[li, ri] = norm_dist + dir_penalty * 0.5 + kind_bonus

    if not np.isfinite(cost).any():
        return

    # Hungarian.
    rows, cols = linear_sum_assignment(cost)
    for li, ri in zip(rows, cols):
        if cost[li, ri] >= INF:
            continue
        # A real match must beat a baseline cost — otherwise leave
        # the leader unattached (empty dim is more honest than a
        # speculative far-away pairing).
        if cost[li, ri] > 0.4:
            continue
        leaders[li].target_region = regions[ri]
        regions[ri].owner_leader = leaders[li]


# ─────────────────────────────────────────────────────────────────────────────
# Pipeline entry point
# ─────────────────────────────────────────────────────────────────────────────


def detect(image: np.ndarray, legacy_detector) -> Drawing:
    """Run the principled pipeline on `image`.

    `legacy_detector` is a configured `BubbleDetector` instance. The
    principled pipeline borrows its OCR + Hough + trace primitives
    during the rewrite — those are well-tested low-level building
    blocks. The new pipeline replaces the ORCHESTRATION and the
    HEURISTIC POST-PROCESSING.

    Returns a `Drawing` with balloons, leaders, regions, and their
    relationships fully resolved. Use `.to_legacy_pairs()` to get
    the flat (bubble_number, dim_text) list the rest of the system
    consumes today.
    """
    # Step 1: balloons. This call also populates the legacy detector's
    # state (norm_tokens, callout_groups, seed_traces) which Steps 2-3
    # currently re-use. As we port more primitives, this side effect
    # goes away.
    balloons = find_balloons(image, legacy_detector)
    # Step 2: dim regions.
    regions = find_dim_regions(image, legacy_detector, balloons)
    # Step 3: leaders.
    leaders = trace_leaders(image, legacy_detector, balloons)
    # Step 4: match.
    h, w = image.shape[:2]
    match_leaders_to_regions(leaders, regions, (h, w))

    return Drawing(
        image_shape=(h, w),
        balloons=balloons,
        leaders=leaders,
        dim_regions=regions,
    )
