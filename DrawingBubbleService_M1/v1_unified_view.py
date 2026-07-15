"""
v1_unified_view.py — render a white-on-black "ink" schema from v1's
production detector state.

The v1 detector (DrawingBubbleService_M1/detector.py) produces:
  * BubbleResult list with bubble_number, (x, y, radius), dimension
  * detector._norm_tokens (NormalizedToken with token_type, x1..y2, text)
  * detector._seed_traces (dict bubble_number -> {"seed": ..., "path": [(x,y)...]})

This module renders all of those features on a black canvas in plain
white — colour-agnostic, suitable both as a human-labelling surface
and as a single-channel input for an eventual learned relation model.

Render layers (all WHITE on black):
  * bubble outline + the OCR'd bubble number drawn inside
  * leader polyline (full path) + dot at the trace's far endpoint
  * dimension-token bbox + the OCR'd dimension text drawn inside

Pure function. No detector state is modified.
"""
from __future__ import annotations

import math
from typing import Iterable, Optional

import cv2
import numpy as np


WHITE = (255, 255, 255)


def _annotation_color_mask(img: np.ndarray, sat_min: int = 60) -> np.ndarray:
    """Annotation-colour HSV mask (red ∪ magenta ∪ purple).

    `sat_min` is exposed so the mask view can use a looser threshold
    (e.g. 40) than the production detector's strict 60 — useful for
    faint / anti-aliased annotations that would otherwise be missed."""
    hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)
    m_red = cv2.inRange(
        hsv, np.array([0, sat_min, 40]), np.array([12, 255, 255])
    )
    m_purple = cv2.inRange(
        hsv, np.array([125, sat_min, 40]), np.array([180, 255, 255])
    )
    mask = cv2.bitwise_or(m_red, m_purple)
    return cv2.morphologyEx(
        mask, cv2.MORPH_CLOSE,
        cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (5, 5)),
    )


def _norm_text(s) -> str:
    """Lowercase, strip everything except alphanumerics. For fuzzy
    bubble.dimension <-> token.text matching when the trace failed
    and we need to fall back to v1's assigned-dim link."""
    return "".join(c for c in (s or "").lower() if c.isalnum())


def _find_dim_token_by_text(dim_text, norm_tokens):
    """Find the OCR token whose normalised text best matches `dim_text`.

    Used as the leader-line fallback target: when a bubble's seed
    trace failed, we draw a synthesized line from the bubble to this
    token's bbox so the bubble->dim link is still visible.
    """
    target = _norm_text(dim_text)
    if not target or target == "nodimension":
        return None
    best = None
    best_overlap = 0
    for tok in norm_tokens:
        ttype = getattr(tok, "token_type", "")
        if ttype not in ("dimension", "bubble", "keyword"):
            continue
        ttext = _norm_text(getattr(tok, "text", ""))
        if not ttext:
            continue
        if ttext == target:
            return tok
        if ttext in target or target in ttext:
            overlap = min(len(ttext), len(target))
            if overlap > best_overlap:
                best_overlap = overlap
                best = tok
    return best


def _saturation_boost(img: np.ndarray, factor: float = 1.8) -> np.ndarray:
    """Multiply the S channel by `factor` (clipped to 255).

    Boosts faded annotation pixels so they cross the HSV mask's
    `sat >= 60` threshold. Drawings like m1 have anti-aliased /
    low-saturation maroon strokes that the raw image's mask misses
    entirely; the boosted copy lights them up without affecting hue.
    Only used for masking — the original image is untouched.
    """
    hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV).astype(np.int32)
    hsv[:, :, 1] = np.clip(hsv[:, :, 1] * factor, 0, 255)
    return cv2.cvtColor(hsv.astype(np.uint8), cv2.COLOR_HSV2BGR)


def render_mask_view_v1(
    image: np.ndarray,
    norm_tokens,
    bubbles=None,
    seed_traces: Optional[dict] = None,
    saturation_factor: float = 2.0,
    mask_sat_min: int = 40,
) -> np.ndarray:
    """Render the *actual* annotation-colour mask (every red/magenta/
    maroon pixel from the source) as white-on-black, with dimension
    bboxes and their OCR'd text overlaid.

    This is what the user asked for: the binary "annotation pixels
    only" extraction (bubbles + colored leaders + any colored text)
    drawn as white, plus the black dimension-text boxes layered on
    top so the connection between bubble↔leader↔dim is visible on a
    single canvas.

    Two improvements over the naïve mask:
      * Saturation boost (default 1.8x) on a copy of the image
        before masking — catches faded/anti-aliased annotations that
        a strict mask would miss (m1-class drawings).
      * When the detector found bubble circles via grayscale Hough
        (so they're absent from the colour mask), explicitly stroke
        a white circle for each detected bubble on top of the mask
        pixels — the view then shows EVERY detected bubble, not just
        the ones the colour mask happened to catch.
    """
    h, w = image.shape[:2]
    boosted = _saturation_boost(image, saturation_factor)
    mask = _annotation_color_mask(boosted, sat_min=mask_sat_min)
    # Drop small connected components — JPEG-compression specks /
    # anti-aliasing crumbs that would render as visible grey dots.
    # Threshold ~15 px keeps thin leader strokes (which have many
    # hundreds of pixels) but kills the scattered noise.
    n_lbl, lbl_img, stats, _cent = cv2.connectedComponentsWithStats(mask, connectivity=8)
    if n_lbl > 1:
        keep = np.zeros_like(mask)
        for i in range(1, n_lbl):
            if stats[i, cv2.CC_STAT_AREA] >= 15:
                keep[lbl_img == i] = 255
        mask = keep

    # Mask pixels render as DIM grey so the drawn primitives
    # (bright white below) stand cleanly above them.
    out = np.zeros_like(image)
    DIM = (120, 120, 120)
    out[mask > 0] = DIM

    # Black-fill the inside of every detected bubble so the bubble
    # number renders on a clean black background, not on the dim grey
    # of the colour mask. The white outline + white digit then sit
    # crisply against black — improves readability of small bubbles.
    if bubbles is not None:
        for b in bubbles:
            cx, cy = int(round(b.x)), int(round(b.y))
            r = max(1, int(round(b.radius)))
            inner_r = max(1, r - 2)
            cv2.circle(out, (cx, cy), inner_r, (0, 0, 0), -1)

    # Layer leader lines on top, per bubble. We draw ONLY the actual
    # seed-trace BFS polyline — the path the detector measured by
    # following maroon-ink pixels from the balloon rim outward.
    #
    # The old code also drew a synthetic straight line from the
    # balloon to its matched dim bbox when the trace died short. That
    # was misleading: a straight white line cutting across the
    # drawing read as "this is the leader" when it was actually just
    # the detector's INTENDED link, not anything physically present.
    # We now omit the fallback so any bubble without a visible white
    # polyline is honestly flagged as "no usable leader traced" —
    # making it easy to see which bubbles need better tracing.
    if bubbles is not None:
        for b in bubbles:
            info = (seed_traces or {}).get(b.bubble_number) or {}
            path = info.get("path") or []
            if not path or len(path) < 2:
                continue
            cx, cy = float(b.x), float(b.y)
            ex, ey = path[-1]
            endpoint_dist = math.hypot(ex - cx, ey - cy)
            # Only draw traces that actually exit the rim. The old gate
            # required endpoint > 2× rim radius which filtered out the
            # short stubs (20-40 px past the rim) that ARE the real
            # leader on tight drawings. The replacement gate just asks
            # that the endpoint sit clearly outside the rim AND that
            # the path has progressed beyond just hugging the rim arc.
            if endpoint_dist <= b.radius + 8:
                continue
            # Reject rim-loops: paths whose every point sits within
            # ~1.2× the rim radius are running around the balloon, not
            # leaving it. Check the farthest point along the path.
            max_dist_from_centre = 0.0
            for px, py in path:
                d = math.hypot(px - cx, py - cy)
                if d > max_dist_from_centre:
                    max_dist_from_centre = d
            if max_dist_from_centre <= b.radius * 1.2:
                continue
            # Truncate any trace longer than ~12× the balloon radius:
            # the BFS can occasionally tunnel through a crossing leader
            # at a junction and continue across the drawing. Past 12×r
            # we're certainly off the original leader.
            max_len = max(120.0, 12.0 * b.radius)
            truncated = [path[0]]
            cum = 0.0
            for i in range(1, len(path)):
                px0, py0 = path[i - 1]
                px1, py1 = path[i]
                cum += math.hypot(px1 - px0, py1 - py0)
                truncated.append(path[i])
                if cum >= max_len:
                    break
            pts = np.array(truncated, dtype=np.int32).reshape((-1, 1, 2))
            cv2.polylines(out, [pts], False, WHITE, 2, cv2.LINE_AA)

    if bubbles is not None:
        for b in bubbles:
            cx, cy = int(round(b.x)), int(round(b.y))
            r = max(1, int(round(b.radius)))
            cv2.circle(out, (cx, cy), r, WHITE, 2)
            label = str(b.bubble_number) if getattr(b, "bubble_number", None) else "?"
            (tw, th), _ = cv2.getTextSize(
                label, cv2.FONT_HERSHEY_SIMPLEX, 0.7, 2,
            )
            cv2.putText(out, label, (cx - tw // 2, cy + th // 2),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.7, WHITE, 2, cv2.LINE_AA)

    # Mask the actual text pixels from the source image for EVERY
    # OCR-detected token, not just dimension-typed ones. v1's OCR
    # classifier puts 1-3-digit integers like the "29" at the top of
    # m1 into token_type="bubble" — which then never reaches the
    # render if we filter on dimension. Including all three
    # categories (dimension / bubble / keyword) surfaces every text
    # region the OCR found, including dim values misclassified as
    # bubble labels and descriptive callouts ("DIA", "THRU", etc.).
    # The trade-off is that title-block / table / revision-history
    # text also gets masked — those are already partially visible
    # in the old view and are useful as context.
    src_gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY) if image.ndim == 3 else image
    for tok in norm_tokens:
        if not getattr(tok, "text", "").strip():
            continue
        if getattr(tok, "token_type", "") not in ("dimension", "bubble", "keyword"):
            continue
        x1 = max(0, int(round(tok.x1)))
        y1 = max(0, int(round(tok.y1)))
        x2 = min(w, int(round(tok.x2)))
        y2 = min(h, int(round(tok.y2)))
        if x2 <= x1 or y2 <= y1:
            continue
        crop = src_gray[y1:y2, x1:x2]
        if crop.size == 0:
            continue
        # THRESH_BINARY_INV + Otsu: dark glyphs -> white, light bg -> 0.
        _, text_mask = cv2.threshold(
            crop, 0, 255,
            cv2.THRESH_BINARY_INV + cv2.THRESH_OTSU,
        )
        # Skip bboxes where the threshold produced almost no text
        # pixels — OCR localised a bbox in a near-empty area
        # (false-positive), so rendering the empty rectangle just
        # adds visual noise.
        text_density = float(np.count_nonzero(text_mask)) / float(text_mask.size)
        if text_density < 0.02:
            continue
        # Paint masked text pixels white in the output region.
        region = out[y1:y2, x1:x2]
        region[text_mask > 0] = WHITE
        out[y1:y2, x1:x2] = region
        cv2.rectangle(out, (x1, y1), (x2, y2), WHITE, 1)

    return out


def render_unified_view_v1(
    image: np.ndarray,
    bubbles: Iterable,
    norm_tokens: Iterable,
    seed_traces: Optional[dict] = None,
) -> np.ndarray:
    """Render v1's detector state as a white-on-black unified view.

    Parameters
    ----------
    image
        The image v1 actually ran on (use detector.image after detect_from_array
        so coordinates align with any auto-upscale that happened).
    bubbles
        Iterable of v1 BubbleResult (or anything with .x .y .radius .bubble_number).
    norm_tokens
        Iterable of v1 NormalizedToken-like objects (.token_type, .text, .x1..y2).
        Only tokens with token_type == "dimension" are drawn.
    seed_traces
        Optional dict bubble_number -> {"path": [(x, y), ...], ...}.
        Each path is drawn as a polyline with a dot at its far endpoint.
    """
    out = np.zeros_like(image)
    h, w = out.shape[:2]

    for b in bubbles:
        cx, cy = int(round(b.x)), int(round(b.y))
        r = max(1, int(round(b.radius)))
        cv2.circle(out, (cx, cy), r, WHITE, 2)
        label = str(b.bubble_number) if getattr(b, "bubble_number", None) else "?"
        (tw, th), _ = cv2.getTextSize(label, cv2.FONT_HERSHEY_SIMPLEX, 0.7, 2)
        cv2.putText(out, label, (cx - tw // 2, cy + th // 2),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.7, WHITE, 2, cv2.LINE_AA)

    if seed_traces:
        for bnum, info in seed_traces.items():
            if not info:
                continue
            path = info.get("path") or []
            if len(path) >= 2:
                pts = np.array(path, dtype=np.int32).reshape((-1, 1, 2))
                cv2.polylines(out, [pts], False, WHITE, 2, cv2.LINE_AA)
                ex, ey = path[-1]
                cv2.circle(out, (int(ex), int(ey)), 4, WHITE, -1)

    for tok in norm_tokens:
        if getattr(tok, "token_type", "") != "dimension":
            continue
        x1 = int(round(tok.x1))
        y1 = int(round(tok.y1))
        x2 = int(round(tok.x2))
        y2 = int(round(tok.y2))
        if x2 <= x1 or y2 <= y1:
            continue
        cv2.rectangle(out, (x1, y1), (x2, y2), WHITE, 1)
        text = (tok.text or "").strip()
        if not text:
            continue
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
