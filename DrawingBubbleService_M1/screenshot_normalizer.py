"""
Screenshot normalisation pass for the bubble-detection pipeline.

Client uploads are often *screenshots* of engineering drawings rather
than the original raster / PDF. Screenshots add several kinds of noise
that the rest of the pipeline (tuned against clean CAD output) is not
prepared for:

  - JPEG block / ringing artefacts around text edges
  - Sub-native resolution (small short-side) — OCR text density too low
  - Warm / cool colour cast (browser screenshots on a non-calibrated
    display, phone shots of a monitor)
  - Slight rotation / skew (especially phone shots)
  - Anti-aliased glyph fuzz
  - UI chrome (scroll bars, taskbar fragments)

`normalize_screenshot(img)` runs a small suite of cheap, edge-preserving
operations *before* the existing detection pipeline. Each operation is
conservative — anything aggressive enough to risk text smear is gated
behind a confidence check.

Result is a tuple `(normalised_image, info)` where `info` is a dict the
caller can log (and surface in the UI) describing what was done:

    {
      "detected_screenshot": True,
      "ops": [
        {"op": "upscale",        "factor": 2.1,    "ms": 12.4},
        {"op": "white_balance",  "shift_b": -8,    "ms": 1.2},
        {"op": "denoise",        "method": "bilateral", "ms": 18.7},
        {"op": "deskew",         "angle_deg": 1.6, "ms": 9.0},
        ...
      ]
    }

The orchestrator is the only public entry-point; individual operations
are private helpers so callers don't depend on the internal order.
"""
from __future__ import annotations

import logging
import time
from typing import Dict, List, Optional, Tuple

import cv2
import numpy as np

logger = logging.getLogger(__name__)


# ─────────────────────────────────────────────────────────────────────
# Detection — is this image a screenshot that benefits from normalisation?
# ─────────────────────────────────────────────────────────────────────

def _looks_like_screenshot(img: np.ndarray, filename: Optional[str]) -> Tuple[bool, Dict]:
    """Heuristic decision. Returns (is_screenshot, signals).

    Signals are surfaced so we can log *why* we did or didn't normalise.
    Two paths to a positive verdict:

      (a) Strong moiré signal alone — chroma-Laplacian above 5.0 means
          we're looking at a photo of a monitor, regardless of filename
          or resolution.
      (b) Otherwise, ≥ 2 votes from the weaker hints (is_jpeg, small,
          low_detail).

    Filename is *not* required; the function works on the bytes alone.
    Filename-based hints are only used to add a vote when present.
    """
    h, w = img.shape[:2]
    short_side = min(h, w)

    signals: Dict[str, object] = {
        "filename":   filename or "",
        "short_side": int(short_side),
        "is_jpeg":    bool(filename and filename.lower().endswith((".jpg", ".jpeg"))),
        "small":      short_side < 1400,
    }

    # Luminance Laplacian variance — a coarse "is this a low-detail
    # smooth-edged image" heuristic. Low values usually mean heavy JPEG
    # compression.
    try:
        gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY) if img.ndim == 3 else img
        scale = 1024.0 / max(gray.shape)
        sm = (cv2.resize(gray, None, fx=scale, fy=scale,
                         interpolation=cv2.INTER_AREA)
              if scale < 1.0 else gray)
        lap_var = float(cv2.Laplacian(sm, cv2.CV_64F).var())
        signals["lap_variance"] = round(lap_var, 1)
        signals["low_detail"] = lap_var < 500.0
    except Exception:
        signals["lap_variance"] = None
        signals["low_detail"]   = False

    # Moiré signal — the strongest single hint for "phone photo of a
    # screen". Calculated once here and reused later by the orchestrator
    # so we don't pay for it twice.
    moire = _moire_confidence(img)
    signals["moire_confidence"] = round(moire, 3)

    votes = sum(bool(signals.get(k)) for k in ("is_jpeg", "small", "low_detail"))
    signals["votes"] = votes

    # Verdict — moiré on its own is enough; otherwise need 2+ weak votes.
    if moire >= 0.5:
        return True, signals
    return votes >= 2, signals


# ─────────────────────────────────────────────────────────────────────
# Moiré detection + treatment
# ─────────────────────────────────────────────────────────────────────
#
# Photos of computer monitors almost always exhibit moiré — periodic
# rainbow / diagonal banding from the camera CMOS sampling the LCD's
# pixel grid. It shows up in the frequency domain as off-axis peaks
# far from DC, and is wrecked by upscaling: a 5×5 median filter applied
# at the original resolution kills it cheaply; the same filter applied
# after a 2× upscale needs to be 11×11 and blurs text edges with it.
# So we detect + treat moiré BEFORE anything else in the pipeline.


def _moire_confidence(img: np.ndarray) -> float:
    """Confidence (0..1) that the image contains moiré-style noise.

    Signal: variance of the Laplacian of the chroma channels (Cr, Cb)
    in YCrCb space. A clean CAD drawing is overwhelmingly black/white,
    so its chroma channels are nearly flat — combined LapVar ~ 2.
    A phone photo of a monitor adds high-frequency *coloured* banding
    (the rainbow moiré pattern), driving combined LapVar to 10+.

    Measured on representative inputs:
      clean CAD raster     —  combined LapVar ≈ 2.0
      JPEG screenshot      —  combined LapVar ≈ 4-6
      phone photo of LCD   —  combined LapVar ≈ 10-15

    Threshold of 5 places phone photos firmly above and clean drawings
    firmly below.
    """
    try:
        ycc = cv2.cvtColor(img, cv2.COLOR_BGR2YCrCb) if img.ndim == 3 else None
        if ycc is None:
            return 0.0
        cr = ycc[..., 1]
        cb = ycc[..., 2]
        lap_cr = float(cv2.Laplacian(cr, cv2.CV_32F).var())
        lap_cb = float(cv2.Laplacian(cb, cv2.CV_32F).var())
        combined = lap_cr + lap_cb
        # Map 2.0 → 0 (clean), 5 → 0.5 (mild), 10+ → 1.0 (severe).
        confidence = (combined - 2.0) / 8.0
        return float(max(0.0, min(1.0, confidence)))
    except Exception:
        return 0.0


def _remove_moire(img: np.ndarray) -> Tuple[np.ndarray, Dict]:
    """Chroma-blur moiré removal.

    Moiré on a phone photo of a monitor is *coloured* high-frequency
    noise — the rainbow bands. The line work itself (black on white) is
    overwhelmingly luminance information. So we:

      1. Convert to YCrCb.
      2. Strongly blur Cr and Cb (sigma ≈ 4 px) — kills the rainbow
         bands without touching the actual drawing.
      3. Leave Y untouched — text and dimension lines stay sharp.
      4. Convert back to BGR.
      5. Light 3×3 median to clean up any residual luminance ringing.

    This is dramatically more effective than a spatial median on the
    full BGR image because median can't reach the band period without
    blurring text strokes alongside it.
    """
    t0 = time.perf_counter()
    if img.ndim != 3 or img.shape[2] != 3:
        return img, {"op": "remove_moire", "skipped": True,
                     "reason": "not BGR"}

    ycc = cv2.cvtColor(img, cv2.COLOR_BGR2YCrCb)
    y, cr, cb = cv2.split(ycc)
    # Strong chroma blur — sigma 4 covers the typical phone-on-screen
    # moiré band period. Large enough to flatten the colour banding;
    # luminance is untouched.
    cr_blurred = cv2.GaussianBlur(cr, ksize=(0, 0), sigmaX=4.0)
    cb_blurred = cv2.GaussianBlur(cb, ksize=(0, 0), sigmaX=4.0)
    merged = cv2.merge([y, cr_blurred, cb_blurred])
    out = cv2.cvtColor(merged, cv2.COLOR_YCrCb2BGR)
    # Tiny median to suppress luminance ringing from the original JPEG.
    out = cv2.medianBlur(out, 3)
    ms = (time.perf_counter() - t0) * 1000.0
    return out, {"op": "remove_moire", "method": "chroma_blur+median3",
                 "sigma": 4.0, "ms": round(ms, 1)}


# ─────────────────────────────────────────────────────────────────────
# Individual operations
# ─────────────────────────────────────────────────────────────────────

def _upscale_if_small(img: np.ndarray, min_short_side: int = 1600) -> Tuple[np.ndarray, Dict]:
    """Upscale undersized images to ~1600 px short side.

    EMPIRICAL NOTE: end-to-end validation on a representative phone
    photo (new.jpeg, 720×1280) showed that 2.22× upscaling here
    actually *hurt* downstream detection — the Hough circle thresholds
    inside the detector are tuned for 1500–2500 px inputs and don't
    adapt cleanly to ~2800 px. Until the downstream pipeline is made
    scale-aware, this operation is gated behind `_ALLOW_UPSCALE` so it
    can stay in code but be globally disabled. The moiré-cleanup pass
    is the part of normalisation that consistently helps; this one is
    parked.
    """
    if not _ALLOW_UPSCALE:
        return img, {"op": "upscale", "skipped": True,
                     "reason": "disabled — downstream not scale-aware"}
    h, w = img.shape[:2]
    short = min(h, w)
    if short >= min_short_side:
        return img, {"op": "upscale", "skipped": True, "factor": 1.0}
    t0 = time.perf_counter()
    factor = min_short_side / float(short)
    factor = min(factor, 3.0)
    out = cv2.resize(img, None, fx=factor, fy=factor,
                     interpolation=cv2.INTER_LANCZOS4)
    ms = (time.perf_counter() - t0) * 1000.0
    return out, {"op": "upscale", "factor": round(factor, 2),
                 "ms": round(ms, 1), "new_size": [out.shape[1], out.shape[0]]}


# Global switch — flip to True once the downstream Hough / OCR pipeline
# is updated to adapt its size thresholds to the input resolution.
_ALLOW_UPSCALE = False


def _white_balance(img: np.ndarray, threshold: int = 6) -> Tuple[np.ndarray, Dict]:
    """Gray-world white balance. Only applied if the channel means are
    materially uneven — clean drawings already have neutral whites and
    don't need adjustment."""
    if img.ndim != 3 or img.shape[2] != 3:
        return img, {"op": "white_balance", "skipped": True,
                     "reason": "not BGR"}
    means = img.reshape(-1, 3).mean(axis=0)  # [B, G, R]
    gray_target = means.mean()
    deltas = [int(round(m - gray_target)) for m in means]
    if max(abs(d) for d in deltas) < threshold:
        return img, {"op": "white_balance", "skipped": True,
                     "deltas": deltas}
    t0 = time.perf_counter()
    # Per-channel gain to push each channel mean toward the gray target.
    gains = gray_target / means
    out = np.clip(img.astype(np.float32) * gains, 0, 255).astype(np.uint8)
    ms = (time.perf_counter() - t0) * 1000.0
    return out, {"op": "white_balance",
                 "gains": [round(float(g), 3) for g in gains],
                 "deltas": deltas,
                 "ms": round(ms, 1)}


def _denoise(img: np.ndarray, method: str = "bilateral") -> Tuple[np.ndarray, Dict]:
    """Edge-preserving denoise. Default is a mild bilateral filter
    because aggressive denoising blurs the thin strokes that OCR
    depends on to distinguish 0/8/Ø."""
    t0 = time.perf_counter()
    if method == "bilateral":
        # d=5, sigmaColor=35, sigmaSpace=35 — light touch, preserves
        # text edges while killing JPEG ringing.
        out = cv2.bilateralFilter(img, d=5, sigmaColor=35, sigmaSpace=35)
    else:
        out = cv2.fastNlMeansDenoisingColored(img, None, 5, 5, 7, 21)
    ms = (time.perf_counter() - t0) * 1000.0
    return out, {"op": "denoise", "method": method, "ms": round(ms, 1)}


def _estimate_skew(img: np.ndarray) -> Optional[float]:
    """Estimate dominant skew angle in degrees. Returns None if no
    confident horizontal-line cluster is found."""
    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY) if img.ndim == 3 else img
    edges = cv2.Canny(gray, 80, 200)
    h, w = edges.shape
    # Look for horizontal-ish lines (dimension lines, frame borders).
    min_len = max(60, int(w * 0.08))
    lines = cv2.HoughLinesP(edges, 1, np.pi / 360,
                            threshold=80,
                            minLineLength=min_len,
                            maxLineGap=10)
    if lines is None or len(lines) < 8:
        return None
    angles: List[float] = []
    for line in lines[:, 0]:
        x1, y1, x2, y2 = line
        dx, dy = x2 - x1, y2 - y1
        if dx == 0:
            continue
        ang = np.degrees(np.arctan2(dy, dx))
        # Fold to [-45, 45] — we only care about deviation from horizontal.
        if ang > 45:
            ang -= 90
        elif ang < -45:
            ang += 90
        # Reject anything that doesn't look near-horizontal — keeps
        # vertical dimension lines from biasing the estimate.
        if abs(ang) > 10:
            continue
        angles.append(ang)
    if len(angles) < 6:
        return None
    return float(np.median(angles))


def _deskew(img: np.ndarray, min_abs_deg: float = 1.0) -> Tuple[np.ndarray, Dict]:
    angle = _estimate_skew(img)
    if angle is None or abs(angle) < min_abs_deg:
        return img, {"op": "deskew", "skipped": True,
                     "angle_deg": angle if angle is not None else None}
    t0 = time.perf_counter()
    h, w = img.shape[:2]
    M = cv2.getRotationMatrix2D((w / 2.0, h / 2.0), angle, 1.0)
    # Use a white border so introduced corners look like blank paper —
    # they'll be cropped or ignored downstream.
    out = cv2.warpAffine(img, M, (w, h),
                        flags=cv2.INTER_CUBIC,
                        borderMode=cv2.BORDER_CONSTANT,
                        borderValue=(255, 255, 255))
    ms = (time.perf_counter() - t0) * 1000.0
    return out, {"op": "deskew", "angle_deg": round(angle, 2),
                 "ms": round(ms, 1)}


def _crop_drawing_region(img: np.ndarray, pad: int = 12) -> Tuple[np.ndarray, Dict]:
    """Find the largest non-white connected region and crop to it (plus
    a small padding). Removes browser chrome / desktop background that
    sometimes leaks into a screenshot.

    Skipped if the existing bbox is already ~the full image — the
    common case for raster CAD output.
    """
    h, w = img.shape[:2]
    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY) if img.ndim == 3 else img
    _, mask = cv2.threshold(gray, 240, 255, cv2.THRESH_BINARY_INV)
    if mask.sum() == 0:
        return img, {"op": "crop", "skipped": True, "reason": "blank"}
    ys, xs = np.where(mask > 0)
    if len(xs) == 0:
        return img, {"op": "crop", "skipped": True, "reason": "no ink"}
    x1, x2 = int(xs.min()), int(xs.max())
    y1, y2 = int(ys.min()), int(ys.max())
    bbox_w = x2 - x1 + 1
    bbox_h = y2 - y1 + 1
    # If the existing content already fills > 92 % of the image, don't
    # bother cropping — likely a clean drawing.
    if bbox_w / w > 0.92 and bbox_h / h > 0.92:
        return img, {"op": "crop", "skipped": True,
                     "reason": "content fills frame"}
    x1 = max(0, x1 - pad); y1 = max(0, y1 - pad)
    x2 = min(w - 1, x2 + pad); y2 = min(h - 1, y2 + pad)
    out = img[y1:y2 + 1, x1:x2 + 1].copy()
    return out, {"op": "crop", "bbox": [x1, y1, x2, y2],
                 "new_size": [out.shape[1], out.shape[0]]}


# ─────────────────────────────────────────────────────────────────────
# Public entry point
# ─────────────────────────────────────────────────────────────────────

def normalize_screenshot(
    img: np.ndarray,
    filename: Optional[str] = None,
    force: bool = False,
) -> Tuple[np.ndarray, Dict]:
    """Run the normalisation pipeline. Returns (image, info).

    Parameters
    ----------
    img : BGR image
    filename : original filename (used to detect JPEG extension)
    force : skip the screenshot-auto-detect gate and run everything
            anyway (useful for diagnostic runs).
    """
    if img is None:
        return img, {"detected_screenshot": False, "ops": [],
                     "skipped": True, "reason": "no image"}

    is_screenshot, signals = _looks_like_screenshot(img, filename)
    info: Dict = {"detected_screenshot": is_screenshot,
                  "signals": signals,
                  "ops": []}

    if not is_screenshot and not force:
        logger.info(
            "Screenshot normalisation skipped — signals: %s", signals,
        )
        return img, info

    if force and not is_screenshot:
        logger.info("Screenshot normalisation forced — signals: %s", signals)

    out = img

    # Moiré kill — runs FIRST, on the original resolution. Chroma-blur
    # is dramatically more effective at low resolution than after a 2×
    # upscale. Cheap to treat — gated on a confidence threshold so we
    # don't touch clean inputs. Reuse the moiré value that
    # _looks_like_screenshot already computed.
    moire_conf = float(signals.get("moire_confidence", 0.0) or 0.0)
    info["moire_confidence"] = round(moire_conf, 3)
    if moire_conf >= 0.30:
        out, step_info = _remove_moire(out)
        info["ops"].append(step_info)
        logger.info("Screenshot normalisation: moiré removed "
                    "(conf=%.2f) | %s",
                    moire_conf,
                    {k: v for k, v in step_info.items() if k not in ("op", "ms")})
    else:
        info["ops"].append({"op": "remove_moire", "skipped": True,
                            "confidence": round(moire_conf, 3)})

    for fn in (_upscale_if_small, _white_balance, _denoise,
               _deskew, _crop_drawing_region):
        out, step_info = fn(out)
        info["ops"].append(step_info)
        if not step_info.get("skipped"):
            logger.info(
                "Screenshot normalisation: %s applied | %s",
                step_info.get("op"),
                {k: v for k, v in step_info.items() if k not in ("op", "ms")},
            )

    return out, info
