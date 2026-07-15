"""
apply_topology_override — conservative post-pass that uses topology
candidates to replace v1's needs_review assignments.

mask_topology_assign — bubble->dim assignment by connected-component
analysis of the annotation-colour mask.

The standard v1 pipeline traces leaders pixel-by-pixel via BFS from a
seed point. That trace fails when the BFS gets captured by the bubble
rim and loops, or when the leader has small gaps. In those cases v1's
assignment falls back on OCR proximity heuristics and often picks a
nearby noise fragment instead of the real dimension.

The mask diagnostic view made it visible that the actual leader pixels
ARE present in the colour mask — what we need is an assignment step
that uses that mask as the answer. This module does exactly that:

  1. Build the same saturation-boosted annotation mask the diagnostic
     view uses (catches faint maroon strokes the production mask misses).
  2. Run connectedComponents on it.
  3. Filter "huge" components (>2% of the image) — those are typically
     drawing borders / merged geometry that touch many bubbles and
     would create false connections.
  4. For each bubble, sample component labels along its rim annulus.
  5. For each dim/keyword/bubble OCR token, sample component labels
     inside (and just outside) its bbox.
  6. A bubble is linked to a dim when they share a non-huge component
     label.
  7. Disambiguate by physical distance (closer wins).

Returns {bubble_number: chosen_dim_text} for bubbles where the
topology gives a clear answer. Bubbles with no shared component are
omitted — caller decides whether to keep v1's original assignment.

This module is read-only. It does not modify the detector.
"""
from __future__ import annotations

import math
import re
from typing import Dict, Optional, Set, Tuple

import cv2
import numpy as np


def _build_callout_groups(norm_tokens):
    """Group nearby dim/keyword tokens into combined callout text the
    same way v1 does (so stacked tolerances like "1.7" + "1.5" merge
    into "1.7/1.5", and multi-line callouts like "MJ5x0.8" + "4h6h"
    combine into "MJ5x0.8 4h6h"). Returns a list of CalloutGroup-like
    objects with .text / .x1..y2 attributes, or [] on import failure.
    """
    try:
        from callout_rules import build_callout_groups
    except ImportError:
        return []
    candidates = [
        t for t in norm_tokens
        if getattr(t, "token_type", "") in ("dimension", "keyword")
    ]
    try:
        return list(build_callout_groups(candidates, scale_factor=1.0))
    except Exception:
        return []


def _callout_containing_token(tok, callouts):
    """Find the callout group whose bbox contains the token's centre.
    Used to upgrade a single-token topology answer into the full
    multi-token callout text v1 would have grouped it into."""
    if not callouts or tok is None:
        return None
    tcx = (tok.x1 + tok.x2) / 2.0
    tcy = (tok.y1 + tok.y2) / 2.0
    for cg in callouts:
        if cg.x1 <= tcx <= cg.x2 and cg.y1 <= tcy <= cg.y2:
            return cg
    # Fallback to closest centre within tight tolerance
    best = None
    best_d = float("inf")
    for cg in callouts:
        ccx = (cg.x1 + cg.x2) / 2.0
        ccy = (cg.y1 + cg.y2) / 2.0
        d = math.hypot(ccx - tcx, ccy - tcy)
        if d < best_d:
            best_d = d
            best = cg
    return best if best_d < 30 else None


def _build_boosted_mask(image: np.ndarray, sat_boost: float, sat_min: int) -> np.ndarray:
    hsv = cv2.cvtColor(image, cv2.COLOR_BGR2HSV).astype(np.int32)
    hsv[:, :, 1] = np.clip(hsv[:, :, 1] * sat_boost, 0, 255)
    boosted = cv2.cvtColor(hsv.astype(np.uint8), cv2.COLOR_HSV2BGR)
    hsv_b = cv2.cvtColor(boosted, cv2.COLOR_BGR2HSV)
    m_red = cv2.inRange(
        hsv_b, np.array([0, sat_min, 40]), np.array([12, 255, 255]),
    )
    m_purple = cv2.inRange(
        hsv_b, np.array([125, sat_min, 40]), np.array([180, 255, 255]),
    )
    mask = cv2.bitwise_or(m_red, m_purple)
    return cv2.morphologyEx(
        mask, cv2.MORPH_CLOSE,
        cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (5, 5)),
    )


def topology_assign(
    image: np.ndarray,
    bubbles,           # iterable of v1 BubbleResult (.x .y .radius .bubble_number)
    norm_tokens,       # iterable of NormalizedToken (.token_type .text .x1..y2)
    sat_boost: float = 2.0,
    sat_min: int = 40,
    huge_pct: float = 0.02,
    min_leader_reach: float = 1.1,   # need reach barely past rim — stub direction is enough
    max_endpoint_to_dim: float = 10.0,  # nearest dim along the ray is acceptable
    dilation_iters: int = 1,
    angle_tol_deg: float = 35.0,     # ±tolerance around leader direction
) -> Dict[str, Tuple[str, float, int]]:
    """Per-bubble assignment via leader-component geometry.

    Algorithm:
      1. Build the sat-boosted annotation mask, lightly dilated.
      2. Connected-components on it; filter huge components (>huge_pct
         of the image — those are drawing borders / merged geometry).
      3. For each bubble, find components touching its rim.
      4. For each such component, walk to its FAR endpoint — the
         pixel farthest from the bubble centre.
      5. Reject components whose far endpoint is still within
         `min_leader_reach * r` (those are rim loops / captured
         traces, not real leaders).
      6. Find the dim/keyword OCR token whose bbox-centre is closest
         to that far endpoint. Cap acceptance at
         `max_endpoint_to_dim * r` to avoid wild matches.

    Returns dict bubble_number -> (assigned_dim_text, distance_far_to_dim,
    leader_reach_pixels).
    """
    h, w = image.shape[:2]
    mask = _build_boosted_mask(image, sat_boost, sat_min)
    if dilation_iters > 0:
        mask = cv2.dilate(
            mask,
            cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3)),
            iterations=dilation_iters,
        )
    n_labels, labels = cv2.connectedComponents(mask, connectivity=8)
    label_sizes = np.bincount(labels.ravel())
    huge_threshold = int(h * w * huge_pct)
    huge_labels: Set[int] = {
        int(lbl) for lbl in range(1, n_labels)
        if label_sizes[lbl] > huge_threshold
    }

    # Pre-compute candidate dim/keyword token centres.
    # Includes token_type="bubble" tokens whose text is NOT a detected
    # bubble ID — v1's OCR rule classifies any 1-3 digit integer as
    # "bubble", which can put real numeric dimension values into that
    # bucket. Filtering against the actual detected
    # bubble IDs is the right way to keep those candidates while
    # rejecting actual bubble-label tokens like "6" inside bubble 6.
    detected_bubble_ids = {str(b.bubble_number).strip() for b in bubbles}
    dim_centers = []  # list of (text, cx, cy, token)
    for tok in norm_tokens:
        ttype = getattr(tok, "token_type", "")
        if ttype not in ("dimension", "keyword", "bubble"):
            continue
        text = (getattr(tok, "text", "") or "").strip()
        if ttype == "bubble" and text in detected_bubble_ids:
            continue  # this token IS a bubble ID, not a stray dim
        if not text:
            continue
        x1, y1, x2, y2 = tok.x1, tok.y1, tok.x2, tok.y2
        tcx = (x1 + x2) / 2.0
        tcy = (y1 + y2) / 2.0
        dim_centers.append((text, tcx, tcy, tok))

    # Build v1-style callout groups so we can upgrade single-token
    # answers (e.g. "1.7") into the full multi-token dim text
    # ("1.7/1.5") that lives at the leader endpoint region.
    callouts = _build_callout_groups(norm_tokens)

    result: Dict[str, Tuple[str, float, int]] = {}

    for b in bubbles:
        bnum = str(b.bubble_number)
        cx, cy = float(b.x), float(b.y)
        r = max(1.0, float(b.radius))

        # Sample mask labels just outside the bubble rim.
        rim_labels: Set[int] = set()
        for r_off in (-3, -1, 1, 3, 5, 7):
            sample_r = r + r_off
            if sample_r < 1:
                continue
            for ang_deg in range(0, 360, 3):
                ang = math.radians(ang_deg)
                px = int(cx + sample_r * math.cos(ang))
                py = int(cy + sample_r * math.sin(ang))
                if 0 <= px < w and 0 <= py < h:
                    lbl = int(labels[py, px])
                    if lbl > 0 and lbl not in huge_labels:
                        rim_labels.add(lbl)
        if not rim_labels:
            continue

        # For each rim-touching component, find its far endpoint.
        best_far = None
        best_reach = 0.0
        for lbl in rim_labels:
            ys, xs = np.where(labels == lbl)
            if ys.size == 0:
                continue
            dx = xs.astype(np.float32) - cx
            dy = ys.astype(np.float32) - cy
            dists = np.hypot(dx, dy)
            idx = int(np.argmax(dists))
            reach = float(dists[idx])
            if reach > best_reach:
                best_reach = reach
                best_far = (int(xs[idx]), int(ys[idx]))

        if best_far is None or best_reach < min_leader_reach * r:
            continue  # rim loop / no leader stub at all

        fx, fy = best_far
        # Direction from bubble centre to leader-stub far endpoint.
        # Even a 1.2x reach stub gives a usable direction vector.
        ddx = fx - cx
        ddy = fy - cy
        dnorm = math.hypot(ddx, ddy)
        if dnorm < 1e-3:
            continue
        ux, uy = ddx / dnorm, ddy / dnorm
        cos_tol = math.cos(math.radians(angle_tol_deg))

        exclusion_r = 1.5 * r
        # Find the dim along the ray from bubble centre in (ux,uy)
        # direction. "Along the ray" means: positive projection onto
        # (ux,uy), within angle_tol_deg of it, and we minimise the
        # perpendicular distance / projection distance combination.
        nearest = None
        nearest_score = float("inf")
        for text, tcx, tcy, tok in dim_centers:
            vx = tcx - cx
            vy = tcy - cy
            vd = math.hypot(vx, vy)
            if vd < exclusion_r:
                continue  # inside / very near the bubble
            if vd > max_endpoint_to_dim * r:
                continue
            cos_a = (vx * ux + vy * uy) / vd
            if cos_a < cos_tol:
                continue  # token is not roughly along the leader direction
            # Score = projection distance penalised by off-axis angle.
            perp = vd * math.sqrt(max(0.0, 1.0 - cos_a * cos_a))
            score = vd + 2.0 * perp
            if score < nearest_score:
                nearest_score = score
                nearest = (text, tok, vd)

        if nearest is None:
            continue

        text, tok, dist = nearest
        # Upgrade to full callout text if v1's grouping would have
        # combined this token with neighbours.
        cg = _callout_containing_token(tok, callouts)
        final_text = cg.text if cg else text
        result[bnum] = (final_text, dist, int(round(best_reach)))

    return result


def _looks_like_garbage_dim(dim_text) -> bool:
    """v1 produced a likely-bad dim worth overriding.

    Captures the pattern surfaced by the rescue-OCR work: empty,
    NO_DIMENSION, or a 1-3 character fragment that's almost
    certainly an OCR noise piece rather than a real dim."""
    if not dim_text:
        return True
    text = str(dim_text).strip()
    if not text or text == "NO_DIMENSION":
        return True
    return len(text) <= 3


def _is_weak_current_dim(bubble) -> bool:
    current = str(getattr(bubble, "dimension", "") or "").strip()
    if _looks_like_garbage_dim(current):
        return True
    reason = str(getattr(bubble, "review_reason", "") or "")
    try:
        conf = float(getattr(bubble, "confidence", 0.0) or 0.0)
    except (TypeError, ValueError):
        conf = 0.0
    if conf < 0.20:
        return True
    return (
        reason in {"smart_assign_fallback", "local_rapidocr_rescue"}
        and conf < 0.55
        and not _looks_structured_dim(current)
    )


def _looks_structured_dim(text: str) -> bool:
    t = str(text or "").strip()
    return bool(
        re.search(r"\d", t)
        and (
            len(t) > 3
            or re.search(r"[A-Za-zØΦ±°×x/().]", t)
        )
    )


def _same_text(a, b) -> bool:
    aa = re.sub(r"\s+", "", str(a or "")).upper()
    bb = re.sub(r"\s+", "", str(b or "")).upper()
    return bool(aa) and aa == bb


def _clean_dimension_text(text: str) -> str:
    cleaned = str(text or "").strip()
    cleaned = re.sub(r"(?<=\d)\)\s+(?=\d)", " ", cleaned)
    cleaned = re.sub(r"\)(\d)", r"\1", cleaned)
    m = re.fullmatch(r"0(\d{2,}(?:\.\d+)?)", cleaned)
    if m:
        cleaned = f"Ø{m.group(1)}"
    return cleaned


def _should_apply_direction_override(bubble, new_text: str, dist: float) -> bool:
    """Gate the less-conservative direction topology fallback.

    The direction fallback is useful when the current value is empty or a
    tiny fragment, but it can also downgrade a complete callout to the first
    token along a noisy leader ray. Keep those partial replacements out.
    """
    current = str(getattr(bubble, "dimension", "") or "").strip()
    new_clean = str(new_text or "").strip()
    if not new_clean or _same_text(current, new_clean):
        return True
    if not current or current == "NO_DIMENSION":
        return True

    cur_compact = re.sub(r"[\s()]", "", current).upper()
    new_compact = re.sub(r"[\s()]", "", new_clean).upper()
    if new_compact and new_compact in cur_compact and len(new_compact) < len(cur_compact):
        return False

    radius = max(1.0, float(getattr(bubble, "radius", 1.0) or 1.0))
    if dist <= 2.3 * radius:
        return True
    if _looks_like_garbage_dim(current) and dist <= 4.0 * radius:
        return True
    return False


def _apply_tiny_digit_endpoint_override(detector, bubbles, norm_tokens) -> int:
    """Assign recovered 1-2 digit dimensions near traced leader endpoints."""
    traces = getattr(detector, "_seed_traces", {}) or {}
    if not traces:
        return 0

    candidates = []
    for tok in norm_tokens:
        if getattr(tok, "token_type", "") != "dimension":
            continue
        text = (getattr(tok, "text", "") or "").strip()
        if not re.fullmatch(r"\d{1,2}", text):
            continue
        try:
            conf = float(getattr(tok, "conf", 0.0) or 0.0)
        except (TypeError, ValueError):
            conf = 0.0
        if conf < 0.50:
            continue
        candidates.append(tok)

    applied = 0
    used_tokens: Set[int] = set()
    for tok_idx, tok in enumerate(candidates):
        ranked = []
        for b in bubbles:
            trace = traces.get(str(b.bubble_number)) or traces.get(b.bubble_number)
            path = (trace or {}).get("path") or []
            if not path:
                continue
            ex, ey = path[-1]
            radius = max(1.0, float(getattr(b, "radius", 1.0) or 1.0))
            endpoint_dist = math.hypot(float(tok.cx) - ex, float(tok.cy) - ey)
            center_dist = math.hypot(float(tok.cx) - float(b.x), float(tok.cy) - float(b.y))
            if endpoint_dist > max(95.0, 2.4 * radius):
                continue
            if center_dist > max(150.0, 3.0 * radius):
                continue
            ranked.append((endpoint_dist, center_dist, b))
        if not ranked:
            continue
        ranked.sort(key=lambda item: (item[0], item[1]))
        if len(ranked) > 1 and ranked[1][0] - ranked[0][0] < 15.0:
            continue
        if tok_idx in used_tokens:
            continue
        _endpoint_dist, _center_dist, bubble = ranked[0]
        text = _clean_dimension_text(str(tok.text).strip())
        if _same_text(getattr(bubble, "dimension", ""), text):
            continue
        if not _is_weak_current_dim(bubble):
            old_dim = str(getattr(bubble, "dimension", "") or "").strip()
            if not _looks_structured_dim(old_dim):
                continue
            radius = max(1.0, float(getattr(bubble, "radius", 1.0) or 1.0))
            recipients = []
            for other in bubbles:
                if other is bubble:
                    continue
                if not _is_weak_current_dim(other):
                    continue
                d = math.hypot(
                    float(getattr(other, "x", 0.0)) - float(getattr(bubble, "x", 0.0)),
                    float(getattr(other, "y", 0.0)) - float(getattr(bubble, "y", 0.0)),
                )
                if d <= max(260.0, 5.0 * radius):
                    recipients.append((d, other))
            recipients.sort(key=lambda item: item[0])
            if not recipients:
                continue
            if len(recipients) > 1 and recipients[1][0] - recipients[0][0] < 25.0:
                continue
            recipient = recipients[0][1]
            recipient.dimension = old_dim
            recipient.review_reason = "transferred_from_tiny_endpoint"
            if getattr(recipient, "confidence", 0.0) < 0.50:
                recipient.confidence = 0.50
        bubble.dimension = text
        bubble.review_reason = "tiny_digit_endpoint"
        if getattr(bubble, "confidence", 0.0) < 0.50:
            bubble.confidence = 0.50
        used_tokens.add(tok_idx)
        applied += 1
    return applied


def _propagate_suffix_dimensions(bubbles) -> int:
    """After topology/tiny overrides, keep suffix balloons with their base."""
    by_id = {str(b.bubble_number): b for b in bubbles}
    applied = 0
    for b in bubbles:
        bnum = str(b.bubble_number)
        base = re.sub(r"[A-Za-z]+$", "", bnum)
        if not base or base == bnum:
            continue
        donor = by_id.get(base)
        if donor is None:
            continue
        donor_dim = str(getattr(donor, "dimension", "") or "").strip()
        if not donor_dim or donor_dim == "NO_DIMENSION":
            continue
        current = str(getattr(b, "dimension", "") or "").strip()
        if _same_text(current, donor_dim):
            continue
        displaced = current if current and current != "NO_DIMENSION" else ""
        if current and not _looks_like_garbage_dim(current):
            # Suffix IDs conventionally share the base feature for tiny
            # quantity/location notes like "2". For richer callouts, keep
            # a strong independent suffix assignment.
            donor_is_tiny_shared_value = re.fullmatch(r"\d{1,2}", donor_dim) is not None
            if not donor_is_tiny_shared_value:
                continue
        b.dimension = donor_dim
        b.review_reason = f"shared_dim_suffix_{base}"
        if getattr(b, "confidence", 0.0) < 0.50:
            b.confidence = 0.50
        applied += 1
        if displaced and _looks_structured_dim(displaced):
            radius = max(1.0, float(getattr(b, "radius", 1.0) or 1.0))
            recipients = []
            for other in bubbles:
                if other is b or str(other.bubble_number) == base:
                    continue
                current_other = str(getattr(other, "dimension", "") or "").strip()
                try:
                    other_conf = float(getattr(other, "confidence", 0.0) or 0.0)
                except (TypeError, ValueError):
                    other_conf = 0.0
                if current_other and not _looks_like_garbage_dim(current_other) and other_conf >= 0.35:
                    continue
                d = math.hypot(
                    float(getattr(other, "x", 0.0)) - float(getattr(b, "x", 0.0)),
                    float(getattr(other, "y", 0.0)) - float(getattr(b, "y", 0.0)),
                )
                if d <= max(260.0, 5.0 * radius):
                    recipients.append((d, other))
            recipients.sort(key=lambda item: item[0])
            if recipients and not (len(recipients) > 1 and recipients[1][0] - recipients[0][0] < 25.0):
                recipient = recipients[0][1]
                recipient.dimension = displaced
                recipient.review_reason = f"transferred_from_suffix_{bnum}"
                if getattr(recipient, "confidence", 0.0) < 0.50:
                    recipient.confidence = 0.50
                applied += 1
    return applied


def _apply_nearby_numeric_reference_override(bubbles, norm_tokens) -> int:
    """Repair duplicate topology picks using nearby unclaimed numeric refs.

    Parenthesized reference dimensions like "(8)" are often OCR'd as a
    bare bubble-typed "8". If topology assigns a dimension that another
    bubble already owns, a close unclaimed numeric reference is better
    evidence than duplicating the same distant dimension value.
    """
    detected_ids = {str(b.bubble_number).strip() for b in bubbles}
    dim_counts = {}
    for b in bubbles:
        dim = str(getattr(b, "dimension", "") or "").strip()
        if dim and dim != "NO_DIMENSION":
            dim_counts[dim] = dim_counts.get(dim, 0) + 1

    candidates = []
    for tok in norm_tokens:
        if getattr(tok, "token_type", "") != "bubble":
            continue
        text = str(getattr(tok, "text", "") or "").strip()
        if not re.fullmatch(r"\d{1,3}", text):
            continue
        if text in detected_ids:
            continue
        candidates.append(tok)

    applied = 0
    for b in bubbles:
        if str(getattr(b, "review_reason", "") or "") != "topology_override":
            continue
        current = str(getattr(b, "dimension", "") or "").strip()
        if dim_counts.get(current, 0) < 2:
            continue
        radius = max(1.0, float(getattr(b, "radius", 1.0) or 1.0))
        ranked = []
        for tok in candidates:
            d = math.hypot(float(tok.cx) - float(b.x), float(tok.cy) - float(b.y))
            if d <= max(130.0, 4.0 * radius):
                ranked.append((d, tok))
        ranked.sort(key=lambda item: item[0])
        if not ranked:
            continue
        if len(ranked) > 1 and ranked[1][0] - ranked[0][0] < 20.0:
            continue
        b.dimension = str(ranked[0][1].text).strip()
        b.review_reason = "nearby_numeric_reference"
        if getattr(b, "confidence", 0.0) < 0.50:
            b.confidence = 0.50
        applied += 1
    return applied


def topology_assign_bbox_sinks(
    image: np.ndarray,
    bubbles,
    norm_tokens,
    sat_boost: float = 2.0,
    sat_min: int = 40,
    dilation_iters: int = 2,
    huge_pct: float = 0.05,
) -> Dict[str, Tuple[str, float, int]]:
    """Bubble->dim assignment by painting dim bboxes INTO the mask
    and walking connected components.

    Difference from topology_assign(): the leader-end-to-dim-bbox gap
    that defeats pure colour-mask connectivity is bridged BY
    CONSTRUCTION here — every dim bbox is filled solid in the mask
    before connected-components, so the leader stroke and its target
    dim sit in the same component once dilation closes the small
    physical gap.

    Returns dict bubble_number -> (assigned_dim_text, _, _) for
    EXCLUSIVE links — a component that touches exactly one bubble's
    rim AND lies inside exactly one dim bbox. Ambiguous or
    huge-component cases are dropped (the caller keeps v1's answer).
    """
    h, w = image.shape[:2]
    mask = _build_boosted_mask(image, sat_boost, sat_min)

    # Paint each candidate dim bbox solid into the mask. Skips
    # bubble-typed tokens whose text matches an actual detected
    # bubble id (those are the bubble-label OCR tokens, not dim
    # targets).
    detected_bubble_ids = {str(b.bubble_number).strip() for b in bubbles}
    combined = mask.copy()
    bbox_lookup = []  # (text, x1, y1, x2, y2, token)
    for tok in norm_tokens:
        ttype = getattr(tok, "token_type", "")
        if ttype not in ("dimension", "keyword", "bubble"):
            continue
        text = (getattr(tok, "text", "") or "").strip()
        if not text:
            continue
        if ttype == "bubble" and text in detected_bubble_ids:
            continue
        x1 = max(0, int(round(tok.x1)))
        y1 = max(0, int(round(tok.y1)))
        x2 = min(w, int(round(tok.x2)))
        y2 = min(h, int(round(tok.y2)))
        if x2 <= x1 or y2 <= y1:
            continue
        combined[y1:y2, x1:x2] = 255
        bbox_lookup.append((text, x1, y1, x2, y2, tok))

    # Build v1-style callout groups once so we can upgrade single-
    # token answers into the full multi-token dim text (e.g. "1.7" ->
    # "1.7/1.5", "MJ5x0.8" -> "MJ5x0.8 4h6h").
    callouts = _build_callout_groups(norm_tokens)

    if dilation_iters > 0:
        combined = cv2.dilate(
            combined,
            cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3)),
            iterations=dilation_iters,
        )

    n_labels, labels = cv2.connectedComponents(combined, connectivity=8)
    label_sizes = np.bincount(labels.ravel())
    huge_threshold = int(h * w * huge_pct)
    huge_labels: Set[int] = {
        int(lbl) for lbl in range(1, n_labels)
        if label_sizes[lbl] > huge_threshold
    }

    # Per bubble: collect non-huge labels touching the rim.
    bubble_rim_labels: Dict[str, Set[int]] = {}
    for b in bubbles:
        cx, cy = int(round(b.x)), int(round(b.y))
        r = max(1, int(round(b.radius)))
        s: Set[int] = set()
        for r_off in (-2, 0, 2, 4, 6):
            sample_r = r + r_off
            if sample_r < 1:
                continue
            for ang_deg in range(0, 360, 3):
                ang = math.radians(ang_deg)
                px = int(cx + sample_r * math.cos(ang))
                py = int(cy + sample_r * math.sin(ang))
                if 0 <= px < w and 0 <= py < h:
                    lbl = int(labels[py, px])
                    if lbl > 0 and lbl not in huge_labels:
                        s.add(lbl)
        bubble_rim_labels[str(b.bubble_number)] = s

    # Per dim bbox: the SINGLE label that fills most of its interior
    # (since we painted the bbox solid, its interior pixels should
    # mostly share one label).
    dim_label = []  # (text, x1, y1, x2, y2, label, token)
    for text, x1, y1, x2, y2, tok in bbox_lookup:
        region = labels[y1:y2, x1:x2]
        if region.size == 0:
            continue
        flat = region.ravel()
        nz = flat[flat > 0]
        if nz.size == 0:
            continue
        counts = np.bincount(nz)
        dom = int(np.argmax(counts))
        if dom in huge_labels:
            continue
        dim_label.append((text, x1, y1, x2, y2, dom, tok))

    # Build label -> dim list and label -> bubble list. Exclusive
    # link = one bubble + one dim share a label.
    label_to_bubbles: Dict[int, list] = {}
    for bnum, lset in bubble_rim_labels.items():
        for lbl in lset:
            label_to_bubbles.setdefault(lbl, []).append(bnum)

    label_to_dims: Dict[int, list] = {}
    for entry in dim_label:
        _text, _x1, _y1, _x2, _y2, lbl, _tok = entry
        label_to_dims.setdefault(lbl, []).append(entry)

    result: Dict[str, Tuple[str, float, int]] = {}
    bubble_assigned: Set[str] = set()
    for lbl, bnums in label_to_bubbles.items():
        dims = label_to_dims.get(lbl, [])
        if len(bnums) == 1 and len(dims) == 1:
            bnum = bnums[0]
            text, x1, y1, x2, y2, _, tok = dims[0]
            if bnum not in bubble_assigned:
                # Upgrade single-token answer to full callout text
                # if v1's grouping would have combined this token
                # with neighbouring ones (stacked tolerances,
                # multi-line callouts).
                cg = _callout_containing_token(tok, callouts)
                final_text = cg.text if cg else text
                b = next((bb for bb in bubbles
                          if str(bb.bubble_number) == bnum), None)
                if b is not None:
                    dcx = (x1 + x2) / 2.0
                    dcy = (y1 + y2) / 2.0
                    dist = math.hypot(dcx - b.x, dcy - b.y)
                else:
                    dist = 0.0
                result[bnum] = (final_text, float(dist), int(label_sizes[lbl]))
                bubble_assigned.add(bnum)
    return result


def apply_topology_override(
    detector,
    bubbles,
    use_bbox_sinks: bool = True,
    use_direction_fallback: bool = False,
):
    """Override v1's dim with topology's answer when topology has one.

    Fallback chain when both flags are on:
      1. bbox-sinks topology  (most conservative, exclusive 1:1)
      2. direction-based topology  (uses leader-stub direction,
         can fire on cases bbox-sinks abstains from)
      3. v1's original answer  (whenever both topologies silent —
         no override applied)

    When `use_direction_fallback=False` (default) only stage 1 runs
    and v1 fills in everywhere it's silent. That matches the
    "production-safe" behaviour that produces zero regressions on
    the ref suite.

    Returns (bubbles, applied_count).
    """
    work_img = getattr(detector, "image", None)
    if work_img is None:
        return bubbles, 0
    norm_tokens = getattr(detector, "_norm_tokens", []) or []
    applied = _apply_tiny_digit_endpoint_override(detector, bubbles, norm_tokens)

    primary = (
        topology_assign_bbox_sinks(work_img, bubbles, norm_tokens)
        if use_bbox_sinks
        else topology_assign(work_img, bubbles, norm_tokens)
    )
    secondary = (
        topology_assign(work_img, bubbles, norm_tokens)
        if (use_bbox_sinks and use_direction_fallback)
        else {}
    )

    for b in bubbles:
        bnum = str(b.bubble_number)
        override = primary.get(bnum)
        source = "primary"
        if override is None:
            override = secondary.get(bnum)
            source = "secondary"
        if override is None:
            continue  # both topology layers silent -> keep v1's answer
        new_text, _dist, _reach = override
        new_text = _clean_dimension_text(new_text)
        if not new_text or new_text == b.dimension:
            continue
        if not re.search(r"\d", new_text):
            continue
        if source == "secondary" and not _should_apply_direction_override(
            b, new_text, float(_dist)
        ):
            continue
        b.dimension = new_text
        b.review_reason = "topology_override"
        if getattr(b, "confidence", 0.0) < 0.50:
            b.confidence = 0.50
        applied += 1
    applied += _apply_nearby_numeric_reference_override(bubbles, norm_tokens)
    applied += _propagate_suffix_dimensions(bubbles)
    return bubbles, applied
