# leader_path_rules.py
"""
Leader path rules for one drawing style.

Production goal:
- trace actual leader-like paths from balloon boundary
- use traced endpoint as primary ownership signal
- keep API compatible with detector.py / linker_rules.py
"""

from __future__ import annotations

from dataclasses import dataclass
from typing import Dict, List, Optional, Tuple
import math

import cv2
import numpy as np

try:
    from .callout_rules import CalloutGroup
    from .ocr_rules import NormalizedToken
    from .grammar_rules import parse_grammar_token, token_implies_shared_dimension
except ImportError:
    from callout_rules import CalloutGroup
    from ocr_rules import NormalizedToken
    from grammar_rules import parse_grammar_token, token_implies_shared_dimension


# ── Data Models ───────────────────────────────────────────────────

@dataclass
class PathSegment:
    x1: int
    y1: int
    x2: int
    y2: int
    length: float
    angle_deg: float


@dataclass
class LeaderDirection:
    ux: float
    uy: float
    confidence: float
    source: str   # traced_path | boundary_segment | none


@dataclass
class CorridorHit:
    callout_index: int
    projection: float
    lateral_distance: float
    corridor_score: float
    shared_hint: bool


@dataclass
class TracedSegment:
    x1: float
    y1: float
    x2: float
    y2: float
    length: float


@dataclass
class RayHit:
    callout_index: int
    distance: float
    hit_type: str   # path | ray | snap
    confidence: float


@dataclass
class LeaderSeed:
    segment_index: int
    start_x: float
    start_y: float
    ux: float
    uy: float
    confidence: float
    source: str


@dataclass
class EndpointHint:
    end_x: float
    end_y: float
    path_length: float
    confidence: float


# ── Public API ────────────────────────────────────────────────────

def extract_path_segments(image: np.ndarray) -> List[PathSegment]:
    """
    Extract candidate line/path segments from the drawing.

    Parameters adapt to image resolution so results are consistent
    regardless of whether the input is a small scan or a large PDF render.
    """
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    blur = cv2.GaussianBlur(gray, (3, 3), 0)

    _, binary = cv2.threshold(
        blur, 0, 255, cv2.THRESH_BINARY_INV + cv2.THRESH_OTSU
    )
    edges = cv2.Canny(blur, 40, 120)

    # Combine binary + edges into single mask — captures both thick
    # drawing lines (from binary) and thin outlines (from edges).
    # Halves the number of HoughLinesP passes needed.
    combined_mask = cv2.bitwise_or(binary, edges)
    segments: List[PathSegment] = []

    # Scale thresholds with image size so the detector works consistently
    # across different drawing resolutions (small scan vs large PDF render).
    h, w = gray.shape[:2]
    diag = math.hypot(w, h)
    scale = max(0.5, min(3.0, diag / 900.0))

    # Two settings instead of three: strict + permissive covers both
    # clear leaders and faint/broken ones.  The middle setting mostly
    # duplicated the other two in practice.
    hough_settings = [
        (30, int(10 * scale), int(10 * scale), 450.0 * scale),
        (18, int(6 * scale),  int(18 * scale), 900.0 * scale),
    ]

    for threshold, min_len, max_gap, max_len in hough_settings:
        lines = cv2.HoughLinesP(
            combined_mask,
            rho=1,
            theta=np.pi / 180,
            threshold=threshold,
            minLineLength=min_len,
            maxLineGap=max_gap,
        )
        if lines is None:
            continue
        segments.extend(_lines_to_segments(lines, min_len=float(min_len), max_len=max_len))

    if hasattr(cv2, "createLineSegmentDetector"):
        try:
            lsd = cv2.createLineSegmentDetector(0)
            lsd_lines = lsd.detect(blur)[0]
            if lsd_lines is not None:
                for line in lsd_lines:
                    x1, y1, x2, y2 = map(int, line[0])
                    dx = x2 - x1
                    dy = y2 - y1
                    length = math.hypot(dx, dy)
                    if length < 8 or length > 900 * scale:
                        continue
                    angle_deg = abs(math.degrees(math.atan2(dy, dx)))
                    angle_deg = min(angle_deg, 180.0 - angle_deg)
                    segments.append(
                        PathSegment(
                            x1=x1, y1=y1, x2=x2, y2=y2,
                            length=length, angle_deg=angle_deg,
                        )
                    )
        except cv2.error:
            pass

    deduped = _dedup_segments(segments)
    deduped.sort(
        key=lambda s: (
            _is_strong_horizontal_segment(s),
            -min(float(s.length), 220.0),
            abs(s.angle_deg - 45.0),
        )
    )
    return deduped


def ray_intersection_linking(
    balloon_x: float,
    balloon_y: float,
    balloon_radius: float,
    callouts: List[CalloutGroup],
    segments: List[PathSegment],
    image: Optional[np.ndarray] = None,
    max_ray_length: float = 420.0,
    corridor_half_width: float = 24.0,
    snap_radius: float = 34.0,
) -> Optional[RayHit]:
    """
    Primary leader ownership function.

    Behavior:
    1. trace connected path graph from balloon boundary
    2. first grammatically valid grouped callout hit wins
    3. ray and snap are fallback only
    """
    if not callouts:
        return None

    work_segments = segments
    if image is not None:
        local_segments = _extract_local_balloon_segments(
            image=image,
            balloon_x=balloon_x,
            balloon_y=balloon_y,
            balloon_radius=balloon_radius,
            pad=170,
        )
        if local_segments:
            work_segments = _merge_segments(segments, local_segments)

    if not work_segments:
        return None

    graph = _build_segment_graph(work_segments, connect_eps=14.0)
    seeds = _find_leader_seeds(
        balloon_x=balloon_x,
        balloon_y=balloon_y,
        balloon_radius=balloon_radius,
        segments=work_segments,
    )
    if not seeds:
        return None

    best_path_hit: Optional[RayHit] = None
    best_ray_hit: Optional[RayHit] = None
    best_snap_hit: Optional[RayHit] = None

    for seed in seeds:
        paths = _trace_paths_from_seed(
            seed=seed,
            balloon_x=balloon_x,
            balloon_y=balloon_y,
            segments=work_segments,
            graph=graph,
            max_hops=8,
            angle_tol_deg=62.0,
            max_total_length=min(520.0, max_ray_length + 120.0),
        )

        for path in paths:
            hit = _first_callout_hit_along_path(
                path=path,
                callouts=callouts,
                corridor_half_width=corridor_half_width,
            )
            if hit is not None:
                if best_path_hit is None or hit.distance < best_path_hit.distance:
                    best_path_hit = hit

        if best_path_hit is not None:
            continue

        for path in paths:
            hit = _ray_extension_from_path(
                path=path,
                callouts=callouts,
                max_extension=max_ray_length,
                corridor_half_width=corridor_half_width,
            )
            if hit is not None:
                if best_ray_hit is None or hit.distance < best_ray_hit.distance:
                    best_ray_hit = hit

        if best_ray_hit is not None:
            continue

        for path in paths:
            hit = _snap_callout_near_path_endpoint(
                path=path,
                callouts=callouts,
                snap_radius=snap_radius,
            )
            if hit is not None:
                if best_snap_hit is None or hit.distance < best_snap_hit.distance:
                    best_snap_hit = hit

    if best_path_hit is not None:
        return best_path_hit
    if best_ray_hit is not None:
        return best_ray_hit
    return best_snap_hit


def leader_endpoint_hint(
    balloon_x: float,
    balloon_y: float,
    balloon_radius: float,
    segments: List[PathSegment],
    image: Optional[np.ndarray] = None,
) -> Optional[EndpointHint]:
    work_segments = segments
    if image is not None:
        local_segments = _extract_local_balloon_segments(
            image=image,
            balloon_x=balloon_x,
            balloon_y=balloon_y,
            balloon_radius=balloon_radius,
            pad=170,
        )
        if local_segments:
            work_segments = _merge_segments(segments, local_segments)

    if not work_segments:
        return None

    graph = _build_segment_graph(work_segments, connect_eps=14.0)
    seeds = _find_leader_seeds(
        balloon_x=balloon_x,
        balloon_y=balloon_y,
        balloon_radius=balloon_radius,
        segments=work_segments,
    )
    if not seeds:
        return None

    best_path: Optional[List[TracedSegment]] = None
    best_score = -1.0

    for seed in seeds:
        paths = _trace_paths_from_seed(
            seed=seed,
            balloon_x=balloon_x,
            balloon_y=balloon_y,
            segments=work_segments,
            graph=graph,
            max_hops=8,
            angle_tol_deg=62.0,
            max_total_length=520.0,
        )
        for path in paths:
            if not path:
                continue
            total_len = sum(seg.length for seg in path)
            end = path[-1]
            end_dist = math.hypot(end.x2 - balloon_x, end.y2 - balloon_y)
            score = total_len * 0.55 + end_dist * 0.45
            if score > best_score:
                best_score = score
                best_path = path

    if not best_path:
        return None

    total_len = sum(seg.length for seg in best_path)
    last = best_path[-1]
    conf = min(1.0, 0.35 + total_len / 220.0)

    return EndpointHint(
        end_x=last.x2,
        end_y=last.y2,
        path_length=total_len,
        confidence=conf,
    )


def estimate_leader_direction(
    balloon_x: float,
    balloon_y: float,
    balloon_radius: float,
    segments: List[PathSegment],
) -> LeaderDirection:
    seeds = _find_leader_seeds(
        balloon_x=balloon_x,
        balloon_y=balloon_y,
        balloon_radius=balloon_radius,
        segments=segments,
    )
    if not seeds:
        return LeaderDirection(ux=0.0, uy=0.0, confidence=0.0, source="none")

    sx = sy = sw = 0.0
    for s in seeds:
        sx += s.ux * s.confidence
        sy += s.uy * s.confidence
        sw += s.confidence

    norm = math.hypot(sx, sy)
    if norm < 1e-6 or sw < 1e-6:
        return LeaderDirection(ux=0.0, uy=0.0, confidence=0.0, source="none")

    return LeaderDirection(
        ux=sx / norm,
        uy=sy / norm,
        confidence=min(1.0, sw / max(1.0, len(seeds))),
        source="boundary_segment",
    )


def corridor_hits_for_balloon(
    balloon_x: float,
    balloon_y: float,
    balloon_radius: float,
    leader_dir: LeaderDirection,
    callouts: List[CalloutGroup],
    grammar_tokens: Optional[List[NormalizedToken]] = None,
    max_projection: float = 360.0,
    corridor_half_width: float = 42.0,
) -> List[CorridorHit]:
    if leader_dir.confidence <= 0.0:
        return []

    hits: List[CorridorHit] = []
    for i, c in enumerate(callouts):
        vx = c.cx - balloon_x
        vy = c.cy - balloon_y

        projection = vx * leader_dir.ux + vy * leader_dir.uy
        if projection <= balloon_radius * 0.5 or projection > max_projection:
            continue

        lateral = abs(vx * (-leader_dir.uy) + vy * leader_dir.ux)
        if lateral > corridor_half_width:
            continue

        score = 1.0
        score -= min(0.55, lateral / max(1.0, corridor_half_width * 1.8))
        score -= min(0.30, projection / max_projection * 0.30)

        shared_hint = False
        if grammar_tokens:
            shared_hint = _shared_hint_near_callout(c, grammar_tokens)
            if shared_hint:
                score += 0.10

        hits.append(
            CorridorHit(
                callout_index=i,
                projection=projection,
                lateral_distance=lateral,
                corridor_score=max(0.0, score),
                shared_hint=shared_hint,
            )
        )

    hits.sort(key=lambda h: (-h.corridor_score, h.lateral_distance, h.projection))
    return hits


def leader_corridor_bonus(
    balloon_x: float,
    balloon_y: float,
    balloon_radius: float,
    callout: CalloutGroup,
    segments: List[PathSegment],
    grammar_tokens: Optional[List[NormalizedToken]] = None,
) -> Tuple[float, LeaderDirection]:
    direction = estimate_leader_direction(
        balloon_x=balloon_x,
        balloon_y=balloon_y,
        balloon_radius=balloon_radius,
        segments=segments,
    )
    if direction.confidence <= 0.0:
        return 0.0, direction

    hits = corridor_hits_for_balloon(
        balloon_x=balloon_x,
        balloon_y=balloon_y,
        balloon_radius=balloon_radius,
        leader_dir=direction,
        callouts=[callout],
        grammar_tokens=grammar_tokens,
    )
    if not hits:
        return 0.0, direction

    bonus = hits[0].corridor_score * direction.confidence
    return max(0.0, min(1.0, bonus)), direction


def shared_dimension_hint_for_balloon(
    balloon_x: float,
    balloon_y: float,
    grammar_tokens: List[NormalizedToken],
    search_radius: float = 85.0,
) -> bool:
    for t in grammar_tokens:
        gt = parse_grammar_token(t.text)
        if not token_implies_shared_dimension(gt):
            continue
        if math.hypot(t.cx - balloon_x, t.cy - balloon_y) <= search_radius:
            return True
    return False


# ── Segment Extraction Helpers ────────────────────────────────────

def _lines_to_segments(lines, min_len: float, max_len: float) -> List[PathSegment]:
    out: List[PathSegment] = []
    for l in lines:
        x1, y1, x2, y2 = map(int, l[0])
        dx = x2 - x1
        dy = y2 - y1
        length = math.hypot(dx, dy)
        if length < min_len or length > max_len:
            continue
        angle_deg = abs(math.degrees(math.atan2(dy, dx)))
        angle_deg = min(angle_deg, 180.0 - angle_deg)
        out.append(
            PathSegment(
                x1=x1, y1=y1, x2=x2, y2=y2,
                length=length, angle_deg=angle_deg,
            )
        )
    return out


def _dedup_segments(segments: List[PathSegment], eps: float = 6.0) -> List[PathSegment]:
    """
    Deduplicate segments using spatial hashing.

    Previous O(N²) comparison on 2500+ segments caused ~3s overhead on
    typical drawings.  Bucketing by quantized endpoint position brings
    this down to effectively O(N): two near-duplicate segments always
    land in the same bucket (or a neighboring one), so we only compare
    against a handful of candidates per segment.
    """
    # Bucket size matches dedup tolerance.  Check the segment's bucket
    # plus 8 neighbors (3x3 grid) to catch near-boundary duplicates.
    bucket_size = max(1, int(eps))
    buckets: Dict[Tuple[int, int], List[PathSegment]] = {}
    out: List[PathSegment] = []

    def _key(x: float, y: float) -> Tuple[int, int]:
        return (int(x) // bucket_size, int(y) // bucket_size)

    for s in segments:
        key = _key(s.x1, s.y1)
        keep = True
        # Check 3x3 neighborhood of buckets
        for dkx in (-1, 0, 1):
            for dky in (-1, 0, 1):
                nk = (key[0] + dkx, key[1] + dky)
                for t in buckets.get(nk, ()):
                    if _segments_nearly_same(s, t, eps=eps):
                        keep = False
                        break
                if not keep:
                    break
            if not keep:
                break
        # Also check reversed endpoint bucket (for flipped duplicates)
        if keep:
            flip_key = _key(s.x2, s.y2)
            if flip_key != key:
                for dkx in (-1, 0, 1):
                    for dky in (-1, 0, 1):
                        nk = (flip_key[0] + dkx, flip_key[1] + dky)
                        for t in buckets.get(nk, ()):
                            if _segments_nearly_same(s, t, eps=eps):
                                keep = False
                                break
                        if not keep:
                            break
                    if not keep:
                        break
        if keep:
            out.append(s)
            buckets.setdefault(key, []).append(s)
    return out


def _segments_nearly_same(a: PathSegment, b: PathSegment, eps: float = 6.0) -> bool:
    d_same = (
        _pt_dist((a.x1, a.y1), (b.x1, b.y1)) <= eps
        and _pt_dist((a.x2, a.y2), (b.x2, b.y2)) <= eps
    )
    d_flip = (
        _pt_dist((a.x1, a.y1), (b.x2, b.y2)) <= eps
        and _pt_dist((a.x2, a.y2), (b.x1, b.y1)) <= eps
    )
    return d_same or d_flip


def _merge_segments(a: List[PathSegment], b: List[PathSegment]) -> List[PathSegment]:
    return _dedup_segments(list(a) + list(b))


def _extract_local_balloon_segments(
    image: np.ndarray,
    balloon_x: float,
    balloon_y: float,
    balloon_radius: float,
    pad: int = 170,
) -> List[PathSegment]:
    h, w = image.shape[:2]
    x1 = max(0, int(balloon_x - pad))
    y1 = max(0, int(balloon_y - pad))
    x2 = min(w, int(balloon_x + pad))
    y2 = min(h, int(balloon_y + pad))
    crop = image[y1:y2, x1:x2]
    if crop.size == 0:
        return []

    local = extract_path_segments(crop)
    out: List[PathSegment] = []
    for s in local:
        out.append(
            PathSegment(
                x1=s.x1 + x1,
                y1=s.y1 + y1,
                x2=s.x2 + x1,
                y2=s.y2 + y1,
                length=s.length,
                angle_deg=s.angle_deg,
            )
        )
    return out


# ── Graph Construction / Seeds ────────────────────────────────────

def _build_segment_graph(
    segments: List[PathSegment],
    connect_eps: float = 14.0,
) -> Dict[int, List[int]]:
    graph: Dict[int, List[int]] = {i: [] for i in range(len(segments))}
    endpoints = [
        ((float(s.x1), float(s.y1)), (float(s.x2), float(s.y2)))
        for s in segments
    ]

    for i in range(len(segments)):
        p1a, p1b = endpoints[i]
        for j in range(i + 1, len(segments)):
            p2a, p2b = endpoints[j]
            if (
                _pt_dist(p1a, p2a) <= connect_eps
                or _pt_dist(p1a, p2b) <= connect_eps
                or _pt_dist(p1b, p2a) <= connect_eps
                or _pt_dist(p1b, p2b) <= connect_eps
            ):
                graph[i].append(j)
                graph[j].append(i)
    return graph


def _find_leader_seeds(
    balloon_x: float,
    balloon_y: float,
    balloon_radius: float,
    segments: List[PathSegment],
) -> List[LeaderSeed]:
    seeds: List[LeaderSeed] = []
    for i, seg in enumerate(segments):
        if not _segment_touches_ring(balloon_x, balloon_y, balloon_radius, seg):
            continue

        oriented = _orient_segment_for_balloon(seg, balloon_x, balloon_y)
        if oriented is None:
            continue

        ux, uy = _segment_dir(oriented)
        if math.hypot(ux, uy) < 1e-6:
            continue

        start_r = math.hypot(oriented.x1 - balloon_x, oriented.y1 - balloon_y)
        end_r = math.hypot(oriented.x2 - balloon_x, oriented.y2 - balloon_y)
        radial_gain = end_r - start_r
        outward_proj = ((oriented.x2 - oriented.x1) * (oriented.x1 - balloon_x) + (oriented.y2 - oriented.y1) * (oriented.y1 - balloon_y))
        ring_err = abs(start_r - balloon_radius)

        conf = 0.40
        conf += min(0.28, seg.length / 115.0)
        conf += min(0.24, max(0.0, radial_gain) / 70.0)
        conf += min(0.10, max(0.0, outward_proj) / 1200.0)
        conf -= min(0.18, ring_err / max(8.0, balloon_radius * 0.75))
        if _is_strong_horizontal_segment(seg):
            conf -= 0.20
        conf = max(0.0, min(1.0, conf))
        if conf <= 0.12:
            continue

        seeds.append(
            LeaderSeed(
                segment_index=i,
                start_x=oriented.x1,
                start_y=oriented.y1,
                ux=ux,
                uy=uy,
                confidence=conf,
                source="boundary_segment",
            )
        )

    seeds.sort(
        key=lambda s: (
            -s.confidence,
            _is_strong_horizontal_segment(segments[s.segment_index]),
            -segments[s.segment_index].length,
        )
    )
    return seeds[:8]


def _orient_segment_for_balloon(
    seg: PathSegment,
    balloon_x: float,
    balloon_y: float,
) -> Optional[TracedSegment]:
    d1 = math.hypot(seg.x1 - balloon_x, seg.y1 - balloon_y)
    d2 = math.hypot(seg.x2 - balloon_x, seg.y2 - balloon_y)

    if d1 <= d2:
        return TracedSegment(float(seg.x1), float(seg.y1), float(seg.x2), float(seg.y2), float(seg.length))
    return TracedSegment(float(seg.x2), float(seg.y2), float(seg.x1), float(seg.y1), float(seg.length))


# ── Path Tracing ──────────────────────────────────────────────────

def _trace_paths_from_seed(
    seed: LeaderSeed,
    balloon_x: float,
    balloon_y: float,
    segments: List[PathSegment],
    graph: Dict[int, List[int]],
    max_hops: int = 8,
    angle_tol_deg: float = 62.0,
    max_total_length: float = 520.0,
) -> List[List[TracedSegment]]:
    initial_path = [_orient_segment_from_seed(segments[seed.segment_index], seed)]
    visited = {seed.segment_index}
    results: List[List[TracedSegment]] = []

    _dfs_trace(
        current_idx=seed.segment_index,
        current_end=(initial_path[0].x2, initial_path[0].y2),
        prev_dir=(seed.ux, seed.uy),
        path=initial_path,
        total_length=initial_path[0].length,
        graph=graph,
        segments=segments,
        visited=visited,
        results=results,
        max_hops=max_hops,
        angle_tol_deg=angle_tol_deg,
        max_total_length=max_total_length,
        balloon_x=balloon_x,
        balloon_y=balloon_y,
    )

    if not results:
        return [initial_path]

    results.sort(
        key=lambda p: _path_rank_score(
            path=p,
            balloon_x=balloon_x,
            balloon_y=balloon_y,
        ),
        reverse=True,
    )
    return results[:6]


def _dfs_trace(
    current_idx: int,
    current_end: Tuple[float, float],
    prev_dir: Tuple[float, float],
    path: List[TracedSegment],
    total_length: float,
    graph: Dict[int, List[int]],
    segments: List[PathSegment],
    visited: set[int],
    results: List[List[TracedSegment]],
    max_hops: int,
    angle_tol_deg: float,
    max_total_length: float,
    balloon_x: float,
    balloon_y: float,
):
    if len(path) >= max_hops or total_length >= max_total_length:
        results.append(path[:])
        return

    next_choices: List[Tuple[float, int, TracedSegment]] = []

    current_radius = math.hypot(current_end[0] - balloon_x, current_end[1] - balloon_y)

    for nxt in graph.get(current_idx, []):
        if nxt in visited:
            continue

        oriented = _orient_segment_from_point(segments[nxt], current_end, endpoint_eps=14.0)
        if oriented is None:
            continue

        ndir = _segment_dir(oriented)
        angle = _angle_between(prev_dir, ndir)
        if angle > angle_tol_deg:
            continue

        next_end = (oriented.x2, oriented.y2)
        next_radius = math.hypot(next_end[0] - balloon_x, next_end[1] - balloon_y)
        radial_gain = next_radius - current_radius
        backward_penalty = max(0.0, -radial_gain)
        horizontal_penalty = 1.0 if _is_strong_horizontal_traced(oriented) else 0.0
        branch_penalty = max(0.0, len(graph.get(nxt, [])) - 2) * 6.0
        short_penalty = 10.0 if oriented.length < 10.0 else 0.0

        score = angle * 0.75
        score += backward_penalty * 0.85
        score += horizontal_penalty * 18.0
        score += branch_penalty
        score += short_penalty
        score -= min(20.0, max(0.0, radial_gain) * 0.45)
        score -= min(8.0, oriented.length * 0.05)

        next_choices.append((score, nxt, oriented))

    if not next_choices:
        results.append(path[:])
        return

    next_choices.sort(key=lambda x: x[0])
    extended = False

    for _, nxt, oriented in next_choices[:4]:
        extended = True
        visited.add(nxt)
        path.append(oriented)
        _dfs_trace(
            current_idx=nxt,
            current_end=(oriented.x2, oriented.y2),
            prev_dir=_segment_dir(oriented),
            path=path,
            total_length=total_length + oriented.length,
            graph=graph,
            segments=segments,
            visited=visited,
            results=results,
            max_hops=max_hops,
            angle_tol_deg=angle_tol_deg,
            max_total_length=max_total_length,
            balloon_x=balloon_x,
            balloon_y=balloon_y,
        )
        path.pop()
        visited.remove(nxt)

    if not extended:
        results.append(path[:])


def _first_callout_hit_along_path(
    path: List[TracedSegment],
    callouts: List[CalloutGroup],
    corridor_half_width: float = 24.0,
) -> Optional[RayHit]:
    """
    First grammatically valid grouped callout hit along path wins.

    To avoid false ownership from horizontal dimension baselines grazing a note
    box, the effective margin shrinks on strongly horizontal segments and the
    segment must either intersect the box more directly or terminate near it.
    """
    cumulative = 0.0
    best: Optional[RayHit] = None

    for seg in path:
        local_hits: List[Tuple[float, int]] = []
        seg_margin = corridor_half_width * (0.62 if _is_strong_horizontal_traced(seg) else 1.0)

        for ci, c in enumerate(callouts):
            if not _is_valid_callout_for_path_hit(c):
                continue
            if not _segment_hits_box(seg, c, margin=seg_margin):
                continue
            if not _callout_hit_is_plausible(seg, c, margin=seg_margin):
                continue
            proj = _projection_to_box_along_segment(seg, c)
            local_hits.append((proj, ci))

        if local_hits:
            local_hits.sort(key=lambda x: x[0])
            proj, ci = local_hits[0]
            hit = RayHit(
                callout_index=ci,
                distance=cumulative + proj,
                hit_type="path",
                confidence=0.95 if not _is_strong_horizontal_traced(seg) else 0.82,
            )
            if best is None or hit.distance < best.distance:
                best = hit
            break

        cumulative += seg.length

    return best


def _is_valid_callout_for_path_hit(callout: CalloutGroup) -> bool:
    gt = parse_grammar_token(callout.text)

    if gt.semantic_type != "unknown":
        return True

    t = callout.text.upper()
    if any(k in t for k in ("DIA", "MAJOR", "MINOR", "THRU", "TYP", "REF", "MJ", "M")):
        return True

    if "/" in t:
        return True

    if any(ch.isdigit() for ch in t) and len(t.strip()) >= 1:
        return True

    return False


def _ray_extension_from_path(
    path: List[TracedSegment],
    callouts: List[CalloutGroup],
    max_extension: float = 360.0,
    corridor_half_width: float = 24.0,
) -> Optional[RayHit]:
    if not path:
        return None

    last = path[-1]
    ux, uy = _segment_dir(last)
    if math.hypot(ux, uy) < 1e-6:
        return None

    best: Optional[RayHit] = None
    base_len = sum(seg.length for seg in path)

    for ci, c in enumerate(callouts):
        if not _is_valid_callout_for_path_hit(c):
            continue

        proj, lateral = _project_box_onto_ray(
            ox=last.x2,
            oy=last.y2,
            ux=ux,
            uy=uy,
            x1=c.x1,
            y1=c.y1,
            x2=c.x2,
            y2=c.y2,
        )
        if proj is None:
            continue
        if proj < 0 or proj > max_extension:
            continue
        if lateral > corridor_half_width:
            continue

        hit = RayHit(
            callout_index=ci,
            distance=base_len + proj,
            hit_type="ray",
            confidence=0.72,
        )
        if best is None or hit.distance < best.distance:
            best = hit

    return best


def _snap_callout_near_path_endpoint(
    path: List[TracedSegment],
    callouts: List[CalloutGroup],
    snap_radius: float = 34.0,
) -> Optional[RayHit]:
    if not path:
        return None

    last = path[-1]
    base_len = sum(seg.length for seg in path)

    best: Optional[RayHit] = None
    for ci, c in enumerate(callouts):
        if not _is_valid_callout_for_path_hit(c):
            continue

        d = _point_to_box_distance(last.x2, last.y2, c.x1, c.y1, c.x2, c.y2)
        if d > snap_radius:
            continue

        hit = RayHit(
            callout_index=ci,
            distance=base_len + d,
            hit_type="snap",
            confidence=0.56,
        )
        if best is None or hit.distance < best.distance:
            best = hit

    return best


def _path_rank_score(
    path: List[TracedSegment],
    balloon_x: float,
    balloon_y: float,
) -> float:
    if not path:
        return -1e9

    total_len = sum(seg.length for seg in path)
    end = path[-1]
    end_dist = math.hypot(end.x2 - balloon_x, end.y2 - balloon_y)

    bends = 0.0
    horizontal_penalty = 0.0
    backward_penalty = 0.0
    prev_dir: Optional[Tuple[float, float]] = None
    prev_r = math.hypot(path[0].x1 - balloon_x, path[0].y1 - balloon_y)

    for seg in path:
        if _is_strong_horizontal_traced(seg):
            horizontal_penalty += min(28.0, seg.length * 0.30)

        cur_dir = _segment_dir(seg)
        cur_r = math.hypot(seg.x2 - balloon_x, seg.y2 - balloon_y)
        if cur_r < prev_r:
            backward_penalty += min(18.0, (prev_r - cur_r) * 1.2)
        prev_r = cur_r

        if prev_dir is not None:
            bends += _angle_between(prev_dir, cur_dir)
        prev_dir = cur_dir

    return (
        total_len * 0.40
        + end_dist * 0.65
        - bends * 0.28
        - horizontal_penalty
        - backward_penalty
    )


def _is_strong_horizontal_segment(seg: PathSegment) -> bool:
    dx = abs(seg.x2 - seg.x1)
    dy = abs(seg.y2 - seg.y1)
    if seg.length < 12.0:
        return False
    return dy <= max(2.5, seg.length * 0.10) and dx >= max(12.0, seg.length * 0.72)


def _is_strong_horizontal_traced(seg: TracedSegment) -> bool:
    dx = abs(seg.x2 - seg.x1)
    dy = abs(seg.y2 - seg.y1)
    if seg.length < 12.0:
        return False
    return dy <= max(2.5, seg.length * 0.10) and dx >= max(12.0, seg.length * 0.72)


def _callout_hit_is_plausible(
    seg: TracedSegment,
    callout: CalloutGroup,
    margin: float,
) -> bool:
    endpoint_dist = _point_to_box_distance(seg.x2, seg.y2, callout.x1, callout.y1, callout.x2, callout.y2)
    if endpoint_dist <= max(6.0, margin * 0.55):
        return True

    proj = _projection_to_box_along_segment(seg, callout)
    if proj >= seg.length * 0.35:
        return True

    center_dist = _point_to_box_distance(callout.cx, callout.cy, min(seg.x1, seg.x2), min(seg.y1, seg.y2), max(seg.x1, seg.x2), max(seg.y1, seg.y2))
    return center_dist <= max(6.0, margin * 0.45)


# ── Geometry Helpers ──────────────────────────────────────────────

def _orient_segment_from_seed(seg: PathSegment, seed: LeaderSeed) -> TracedSegment:
    d1 = _pt_dist((seg.x1, seg.y1), (seed.start_x, seed.start_y))
    d2 = _pt_dist((seg.x2, seg.y2), (seed.start_x, seed.start_y))
    if d1 <= d2:
        return TracedSegment(float(seg.x1), float(seg.y1), float(seg.x2), float(seg.y2), float(seg.length))
    return TracedSegment(float(seg.x2), float(seg.y2), float(seg.x1), float(seg.y1), float(seg.length))


def _orient_segment_from_point(
    seg: PathSegment,
    pt: Tuple[float, float],
    endpoint_eps: float = 14.0,
) -> Optional[TracedSegment]:
    p = (float(pt[0]), float(pt[1]))
    d1 = _pt_dist((seg.x1, seg.y1), p)
    d2 = _pt_dist((seg.x2, seg.y2), p)

    if d1 <= endpoint_eps or d1 <= d2:
        if d1 <= max(endpoint_eps, d2 + 1e-6):
            return TracedSegment(float(seg.x1), float(seg.y1), float(seg.x2), float(seg.y2), float(seg.length))
    if d2 <= endpoint_eps or d2 < d1:
        return TracedSegment(float(seg.x2), float(seg.y2), float(seg.x1), float(seg.y1), float(seg.length))
    return None


def _segment_dir(seg: TracedSegment) -> Tuple[float, float]:
    dx = seg.x2 - seg.x1
    dy = seg.y2 - seg.y1
    norm = math.hypot(dx, dy)
    if norm < 1e-6:
        return (0.0, 0.0)
    return (dx / norm, dy / norm)


def _angle_between(a: Tuple[float, float], b: Tuple[float, float]) -> float:
    ax, ay = a
    bx, by = b
    na = math.hypot(ax, ay)
    nb = math.hypot(bx, by)
    if na < 1e-6 or nb < 1e-6:
        return 180.0
    dot = max(-1.0, min(1.0, (ax * bx + ay * by) / (na * nb)))
    return math.degrees(math.acos(dot))


def _segment_touches_ring(cx: float, cy: float, r: float, s: PathSegment) -> bool:
    d = _point_to_segment_distance(cx, cy, s.x1, s.y1, s.x2, s.y2)
    if abs(d - r) < 10:
        return True
    d1 = math.hypot(s.x1 - cx, s.y1 - cy)
    d2 = math.hypot(s.x2 - cx, s.y2 - cy)
    return abs(d1 - r) < 10 or abs(d2 - r) < 10


def _projection_to_box_along_segment(seg: TracedSegment, c: CalloutGroup) -> float:
    px = np.array([c.x1, c.x2, c.x2, c.x1], dtype=float)
    py = np.array([c.y1, c.y1, c.y2, c.y2], dtype=float)

    vx = seg.x2 - seg.x1
    vy = seg.y2 - seg.y1
    denom = max(1e-6, math.hypot(vx, vy))
    ux = vx / denom
    uy = vy / denom

    projections = []
    for x, y in zip(px, py):
        proj = (x - seg.x1) * ux + (y - seg.y1) * uy
        projections.append(proj)

    return max(0.0, min(projections))


def _segment_hits_box(seg: TracedSegment, c: CalloutGroup, margin: float = 24.0) -> bool:
    x1 = c.x1 - margin
    y1 = c.y1 - margin
    x2 = c.x2 + margin
    y2 = c.y2 + margin

    if _point_in_box(seg.x1, seg.y1, x1, y1, x2, y2):
        return True
    if _point_in_box(seg.x2, seg.y2, x1, y1, x2, y2):
        return True

    if _segments_intersect(seg.x1, seg.y1, seg.x2, seg.y2, x1, y1, x2, y1):
        return True
    if _segments_intersect(seg.x1, seg.y1, seg.x2, seg.y2, x2, y1, x2, y2):
        return True
    if _segments_intersect(seg.x1, seg.y1, seg.x2, seg.y2, x2, y2, x1, y2):
        return True
    if _segments_intersect(seg.x1, seg.y1, seg.x2, seg.y2, x1, y2, x1, y1):
        return True

    return False


def _project_box_onto_ray(
    ox: float,
    oy: float,
    ux: float,
    uy: float,
    x1: float,
    y1: float,
    x2: float,
    y2: float,
) -> Tuple[Optional[float], float]:
    corners = [
        (x1, y1), (x2, y1), (x2, y2), (x1, y2),
        ((x1 + x2) / 2.0, (y1 + y2) / 2.0),
    ]
    best_proj: Optional[float] = None
    best_lateral = float("inf")

    nx = -uy
    ny = ux

    for px, py in corners:
        vx = px - ox
        vy = py - oy
        proj = vx * ux + vy * uy
        lateral = abs(vx * nx + vy * ny)
        if best_proj is None or (proj >= 0 and proj < best_proj) or (best_proj is not None and best_proj < 0 and proj > best_proj):
            best_proj = proj
            best_lateral = lateral

    return best_proj, best_lateral


def _shared_hint_near_callout(c: CalloutGroup, grammar_tokens: List[NormalizedToken], radius: float = 70.0) -> bool:
    for t in grammar_tokens:
        gt = parse_grammar_token(t.text)
        if not token_implies_shared_dimension(gt):
            continue
        if math.hypot(t.cx - c.cx, t.cy - c.cy) <= radius:
            return True
    return False


def _pt_dist(a: Tuple[float, float], b: Tuple[float, float]) -> float:
    return math.hypot(a[0] - b[0], a[1] - b[1])


def _point_in_box(px: float, py: float, x1: float, y1: float, x2: float, y2: float) -> bool:
    return x1 <= px <= x2 and y1 <= py <= y2


def _point_to_box_distance(px: float, py: float, x1: float, y1: float, x2: float, y2: float) -> float:
    dx = max(x1 - px, 0.0, px - x2)
    dy = max(y1 - py, 0.0, py - y2)
    return math.hypot(dx, dy)


def _point_to_segment_distance(px: float, py: float, x1: float, y1: float, x2: float, y2: float) -> float:
    vx = x2 - x1
    vy = y2 - y1
    wx = px - x1
    wy = py - y1
    seg_len2 = vx * vx + vy * vy
    if seg_len2 < 1e-9:
        return math.hypot(px - x1, py - y1)
    t = max(0.0, min(1.0, (wx * vx + wy * vy) / seg_len2))
    proj_x = x1 + t * vx
    proj_y = y1 + t * vy
    return math.hypot(px - proj_x, py - proj_y)


def _segments_intersect(
    ax: float, ay: float, bx: float, by: float,
    cx: float, cy: float, dx: float, dy: float,
) -> bool:
    def ccw(px, py, qx, qy, rx, ry):
        return (ry - py) * (qx - px) > (qy - py) * (rx - px)

    return (ccw(ax, ay, cx, cy, dx, dy) != ccw(bx, by, cx, cy, dx, dy)) and (
        ccw(ax, ay, bx, by, cx, cy) != ccw(ax, ay, bx, by, dx, dy)
    )
