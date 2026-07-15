"""
Targeted recovery of tiny single/double-digit dimension values squeezed
between dimension extension arrows (e.g. ←1→, ←2→).

These appear at 5-8 px native height — below RapidOCR's effective minimum
text-height threshold (~10-12 px) — and so vanish from the main detector's
_norm_tokens entirely. Without them, the mask view shows blank space where
the dim value should be, and any bubble that points there gets no callout.

Strategy: walk each detected bubble's leader endpoint, crop a small
rectangle around it, upscale 6x with bicubic interpolation, re-run OCR on
that crop with results filtered to short digit-only strings. Any new digit
not already covered by an existing token (and not sitting inside a bubble
circle) is returned as a NormalizedToken that the caller can append to
detector._norm_tokens.
"""
from __future__ import annotations

from typing import List, Optional, Tuple

import cv2
import numpy as np

from ocr_rules import NormalizedToken


_DIGIT_SET = set("0123456789")


def _bubble_leader_endpoint(detector, bubble) -> Optional[Tuple[int, int]]:
    """Return the (x, y) leader endpoint for `bubble`, or None if unavailable."""
    traces = getattr(detector, "_seed_traces", {}) or {}
    trace = traces.get(bubble.bubble_number)
    if not trace:
        return None
    path = trace.get("path") or []
    if path and len(path) >= 2:
        last = path[-1]
        return (int(last[0]), int(last[1]))
    seed = trace.get("seed")
    if seed is not None:
        return (int(seed[0]), int(seed[1]))
    return None


def _ocr_digits(
    ocr_engine,
    crop: np.ndarray,
    upscale: int,
    min_conf: float,
) -> List[Tuple[str, float, Tuple[int, int, int, int]]]:
    """Run OCR on `crop` upscaled by `upscale`, return (text, conf, bbox)
    triples for digit-only detections. Bboxes are in crop coordinates
    (already downscaled back to native pixels)."""
    h, w = crop.shape[:2]
    if h < 4 or w < 4:
        return []
    big = cv2.resize(
        crop, (w * upscale, h * upscale), interpolation=cv2.INTER_CUBIC,
    )
    try:
        res, _ = ocr_engine(big)
    except Exception:
        return []
    if not res:
        return []

    out: List[Tuple[str, float, Tuple[int, int, int, int]]] = []
    for entry in res:
        if not entry or len(entry) < 3:
            continue
        box, text, conf = entry[0], entry[1], entry[2]
        if not text:
            continue
        cleaned = "".join(c for c in str(text) if c in _DIGIT_SET)
        # Tiny dim values between extension arrows are almost always 1-2
        # digits; anything longer is the regular dim text the main OCR
        # pass already caught, so we skip it.
        if not cleaned or len(cleaned) > 2:
            continue
        try:
            conf_f = float(conf)
        except (TypeError, ValueError):
            continue
        # Two-digit detections are more likely to be FPs on stylised
        # drawing symbols (e.g. "((" arcs reading as "11"). Require a
        # tighter confidence bar for them.
        floor = min_conf if len(cleaned) == 1 else max(min_conf, 0.88)
        if conf_f < floor:
            continue
        xs = [float(p[0]) for p in box]
        ys = [float(p[1]) for p in box]
        x1 = int(min(xs) / upscale)
        y1 = int(min(ys) / upscale)
        x2 = int(max(xs) / upscale)
        y2 = int(max(ys) / upscale)
        if x2 <= x1 or y2 <= y1:
            continue
        # Cap native bbox size — a recovered digit should be a single
        # numeric glyph or pair of glyphs, not a wide chunk of dim text.
        # The cleaned-text filter (len<=2) already rejects multi-char
        # strings; this size cap is a defensive filter against OCR
        # bboxes that snap to large surrounding text. _overlaps_existing
        # handles legit duplicates with main-pass tokens.
        if (y2 - y1) > 60 or (x2 - x1) > 80:
            continue
        out.append((cleaned, conf_f, (x1, y1, x2, y2)))
    return out


def _is_inside_any_bubble(cx: float, cy: float, bubbles, margin: int = 2) -> bool:
    for b in bubbles:
        dx = cx - float(b.x)
        dy = cy - float(b.y)
        r = float(b.radius) + margin
        if dx * dx + dy * dy <= r * r:
            return True
    return False


def _overlaps_existing(
    cx: float,
    cy: float,
    x1: float,
    y1: float,
    x2: float,
    y2: float,
    tokens,
) -> bool:
    """True if the new bbox center sits inside an existing token's bbox,
    or the new bbox overlaps an existing one by more than 30% area."""
    new_area = max(1.0, (x2 - x1) * (y2 - y1))
    for tok in tokens:
        if tok.x1 <= cx <= tok.x2 and tok.y1 <= cy <= tok.y2:
            return True
        ix1 = max(x1, tok.x1)
        iy1 = max(y1, tok.y1)
        ix2 = min(x2, tok.x2)
        iy2 = min(y2, tok.y2)
        if ix2 > ix1 and iy2 > iy1:
            inter = (ix2 - ix1) * (iy2 - iy1)
            if inter / new_area > 0.30:
                return True
    return False


def _anchor_points(detector, bubbles) -> List[Tuple[int, int]]:
    """Collect crop-anchor points: bubble centers + leader endpoints +
    every bubble-typed OCR token center. The last group covers bubbles
    whose Hough circle was missed but whose number was OCR'd."""
    anchors: List[Tuple[int, int]] = []

    for b in bubbles:
        anchors.append((int(b.x), int(b.y)))
        ep = _bubble_leader_endpoint(detector, b)
        if ep is not None:
            anchors.append(ep)

    for tok in getattr(detector, "_norm_tokens", []) or []:
        if getattr(tok, "token_type", "") == "bubble":
            anchors.append((int(tok.cx), int(tok.cy)))

    # De-dupe within 20 px
    deduped: List[Tuple[int, int]] = []
    for x, y in anchors:
        if not any(abs(x - x0) < 20 and abs(y - y0) < 20 for x0, y0 in deduped):
            deduped.append((x, y))
    return deduped


def recover_tiny_digits(
    detector,
    bubbles,
    crop_pads: Tuple[int, ...] = (100, 150),
    upscales: Tuple[int, ...] = (4,),
    min_conf: float = 0.55,
) -> List[NormalizedToken]:
    """Run targeted OCR for tiny dim digits around every anchor point
    (bubble centers + leader endpoints + bubble-typed OCR token centers).
    Returns new NormalizedToken records (token_type='dimension') for any
    digits not already covered by detector._norm_tokens and not sitting
    inside a bubble.

    Caller is responsible for appending the result to
    detector._norm_tokens before re-running the mask render.
    """
    img = getattr(detector, "image", None)
    if img is None or not bubbles:
        return []
    H, W = img.shape[:2]
    existing = list(getattr(detector, "_norm_tokens", []) or [])
    out: List[NormalizedToken] = []
    seen_centers: List[Tuple[float, float]] = []

    for anchor_x, anchor_y in _anchor_points(detector, bubbles):
        per_loc: dict = {}
        # Try each (pad, upscale) combination. RapidOCR's text-detection
        # branch is sensitive to both the crop bounds and the upscale
        # factor — a digit that surfaces with one combo can be missed
        # with another. Iterating a small set covers the common cases
        # without exploding latency.
        for pad in crop_pads:
            x1 = max(0, anchor_x - pad)
            y1 = max(0, anchor_y - pad)
            x2 = min(W, anchor_x + pad)
            y2 = min(H, anchor_y + pad)
            if x2 - x1 < 8 or y2 - y1 < 8:
                continue
            crop = img[y1:y2, x1:x2]
            if crop.size == 0:
                continue
            for s in upscales:
                for text, conf, bbox in _ocr_digits(
                    detector.ocr, crop, upscale=s, min_conf=min_conf,
                ):
                    g_x1 = x1 + bbox[0]
                    g_y1 = y1 + bbox[1]
                    g_x2 = x1 + bbox[2]
                    g_y2 = y1 + bbox[3]
                    key = (g_x1 // 8, g_y1 // 8)
                    if key not in per_loc or conf > per_loc[key][1]:
                        per_loc[key] = (text, conf, (g_x1, g_y1, g_x2, g_y2))

        for text, conf, (ax1, ay1, ax2, ay2) in per_loc.values():
            ax1 = float(ax1)
            ay1 = float(ay1)
            ax2 = float(ax2)
            ay2 = float(ay2)
            cx = (ax1 + ax2) / 2.0
            cy = (ay1 + ay2) / 2.0

            if _is_inside_any_bubble(cx, cy, bubbles):
                continue
            if _overlaps_existing(cx, cy, ax1, ay1, ax2, ay2, existing + out):
                continue
            if any(abs(cx - x0) < 6 and abs(cy - y0) < 6 for x0, y0 in seen_centers):
                continue
            seen_centers.append((cx, cy))

            out.append(NormalizedToken(
                raw_text=text,
                text=text,
                cx=cx, cy=cy,
                conf=conf,
                x1=ax1, y1=ay1, x2=ax2, y2=ay2,
                token_type="dimension",
                semantic_type="numeric",
                dual_use=False,
                is_maroon=False,
            ))

    return out
