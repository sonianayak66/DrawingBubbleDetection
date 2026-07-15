"""
Photo-quality preprocessing for engineering drawings captured with a
phone camera. Three independent steps, each gated to fire only when
the image actually needs it (so scanner-quality input passes through
untouched):

  1. detect_and_correct_skew   — rotate the drawing back to upright
                                  when its dominant lines are tilted
                                  more than ~2° from horizontal /
                                  vertical (cheap HoughLinesP estimate).

  2. normalize_lighting        — CLAHE on the V channel when the image
                                  has uneven brightness (glare patches,
                                  shadowed corners). Skipped on
                                  already-even images.

  3. denoise_for_ocr          — light bilateral denoise to reduce JPEG
                                  ringing without blurring text edges,
                                  applied only when the image's noise
                                  floor exceeds a calibrated bound.

Each function is a pure transform: BGR ndarray in, BGR ndarray out.
None of them touch any global state; the detector calls them at the
top of detect_from_array if the photo-input config flag is on.
"""
from __future__ import annotations

import math
from typing import Tuple

import cv2
import numpy as np


# ─────────────────────────────────────────────────────────────────────────────
# 1. Skew correction
# ─────────────────────────────────────────────────────────────────────────────


def detect_and_correct_skew(
    image: np.ndarray,
    min_correction_deg: float = 2.0,
    max_correction_deg: float = 15.0,
) -> Tuple[np.ndarray, float]:
    """Rotate `image` so its dominant line directions are aligned with
    the horizontal / vertical axes.

    Engineering drawings have horizontal & vertical dimension lines,
    grid markers, and balloon-leader stubs that — when the page sits
    square in the camera — produce HoughLinesP segments at 0° / 90°.
    A phone photo taken at a tilt rotates all of these by the same
    amount; the median deviation from {0°, 90°} is the skew.

    Returns (corrected_image, applied_angle_degrees). The applied angle
    is 0.0 when no rotation was performed.

    Conservative gating:
      - At least 20 line segments must vote on the angle (we want a
        stable median, not a guess from 2-3 noisy lines).
      - Median deviation must be ≥ min_correction_deg (default 2°). A
        truly upright scan has near-zero deviation; rotating it by a
        fraction of a degree only adds resampling blur for no gain.
      - Cap the rotation at max_correction_deg (default 15°). Larger
        deviations are likely a page-orientation problem (e.g., the
        photo is sideways or upside-down) and need a different fix
        than a small affine rotate.
    """
    if image is None or image.size == 0:
        return image, 0.0
    h, w = image.shape[:2]
    if h < 100 or w < 100:
        return image, 0.0

    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY) if image.ndim == 3 else image
    # Downscale large images for the angle estimate — Hough on the full
    # high-res photo wastes ~5x more time for no benefit on the median.
    scale = 1.0
    if max(h, w) > 1600:
        scale = 1600.0 / max(h, w)
        small = cv2.resize(
            gray,
            (int(w * scale), int(h * scale)),
            interpolation=cv2.INTER_AREA,
        )
    else:
        small = gray

    edges = cv2.Canny(small, 50, 150, apertureSize=3)
    min_line_len = max(40, int(min(small.shape[:2]) * 0.08))
    lines = cv2.HoughLinesP(
        edges,
        rho=1,
        theta=np.pi / 180,
        threshold=80,
        minLineLength=min_line_len,
        maxLineGap=10,
    )
    if lines is None or len(lines) < 20:
        return image, 0.0

    deviations = []
    for x1, y1, x2, y2 in lines[:, 0]:
        dx, dy = x2 - x1, y2 - y1
        if dx == 0 and dy == 0:
            continue
        ang_deg = math.degrees(math.atan2(dy, dx))
        # Fold into [-45, 45] so horizontal AND vertical both report
        # 0° when upright — we don't care which axis a segment hugs.
        dev = ang_deg % 90.0
        if dev > 45.0:
            dev -= 90.0
        deviations.append(dev)

    if len(deviations) < 20:
        return image, 0.0

    deviations.sort()
    median_dev = deviations[len(deviations) // 2]

    if abs(median_dev) < min_correction_deg:
        return image, 0.0
    if abs(median_dev) > max_correction_deg:
        return image, 0.0

    # Rotate by `-median_dev` to bring deviation to zero. Use INTER_CUBIC
    # to preserve line crispness; reflect border so corners don't go
    # black (black corners would break downstream "find a callout box"
    # logic that scans the image edges).
    centre = (w / 2.0, h / 2.0)
    rot = cv2.getRotationMatrix2D(centre, median_dev, 1.0)
    corrected = cv2.warpAffine(
        image,
        rot,
        (w, h),
        flags=cv2.INTER_CUBIC,
        borderMode=cv2.BORDER_REPLICATE,
    )
    return corrected, float(median_dev)


# ─────────────────────────────────────────────────────────────────────────────
# 2. Lighting normalisation
# ─────────────────────────────────────────────────────────────────────────────


def normalize_lighting(
    image: np.ndarray,
    contrast_floor: float = 35.0,
) -> np.ndarray:
    """Apply CLAHE on the V channel when the image has uneven lighting.

    `contrast_floor` is the minimum standard deviation of the V channel
    we expect on a well-lit scan. Below that, CLAHE is applied to flatten
    glare patches and pull shadowed corners back into a readable range.
    Above the floor the image is already evenly lit and we return it
    untouched.

    CLAHE on V (rather than on a grayscale conversion) preserves the
    maroon annotation hue — the downstream split-stream relies on that
    hue to distinguish bubble labels from dim text, so we must NOT
    crush colour information here.
    """
    if image is None or image.size == 0 or image.ndim != 3:
        return image
    hsv = cv2.cvtColor(image, cv2.COLOR_BGR2HSV)
    v = hsv[:, :, 2]
    if float(v.std()) >= contrast_floor:
        return image
    clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))
    hsv[:, :, 2] = clahe.apply(v)
    return cv2.cvtColor(hsv, cv2.COLOR_HSV2BGR)


# ─────────────────────────────────────────────────────────────────────────────
# 3. Bilateral denoise (light)
# ─────────────────────────────────────────────────────────────────────────────


def denoise_for_ocr(
    image: np.ndarray,
    noise_threshold: float = 12.0,
) -> np.ndarray:
    """Apply bilateral filter when the image has significant noise
    (phone-photo JPEG ringing, sensor grain). Preserves edges; doesn't
    blur balloon outlines or dim digits.

    The noise estimate is the std of a Laplacian-filtered grayscale
    crop of the image's centre — a cheap classical proxy for "how much
    high-frequency junk is in this image."
    """
    if image is None or image.size == 0:
        return image
    h, w = image.shape[:2]
    if h < 100 or w < 100:
        return image
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY) if image.ndim == 3 else image
    cy, cx = h // 2, w // 2
    crop = gray[max(0, cy - 200):cy + 200, max(0, cx - 200):cx + 200]
    if crop.size == 0:
        return image
    lap_std = float(cv2.Laplacian(crop, cv2.CV_64F).std())
    if lap_std < noise_threshold:
        return image
    # Mild parameters — strong enough to soften JPEG ringing without
    # smoothing out a 5-px digit stroke.
    return cv2.bilateralFilter(image, d=5, sigmaColor=20, sigmaSpace=20)


# ─────────────────────────────────────────────────────────────────────────────
# 4. Small-image upscale
# ─────────────────────────────────────────────────────────────────────────────


def upscale_small_image(
    image: np.ndarray,
    target_short_side: int = 900,
) -> Tuple[np.ndarray, float]:
    """Upscale image so its short side reaches `target_short_side` px,
    if the input is smaller. RapidOCR's text-detection branch struggles
    with characters below ~10-12 px tall; phone photos of a full
    drawing page often have small text after the resolution loss.

    Bicubic interpolation. Caps the upscale factor at 3× to avoid
    excessive runtime on already-small inputs that wouldn't gain much
    from extreme magnification.

    Returns (image, applied_scale_factor). 1.0 means no change.
    """
    if image is None or image.size == 0:
        return image, 1.0
    h, w = image.shape[:2]
    short = min(h, w)
    if short >= target_short_side:
        return image, 1.0
    scale = min(3.0, float(target_short_side) / float(short))
    new_w = int(round(w * scale))
    new_h = int(round(h * scale))
    return cv2.resize(image, (new_w, new_h), interpolation=cv2.INTER_CUBIC), scale


# ─────────────────────────────────────────────────────────────────────────────
# Pipeline
# ─────────────────────────────────────────────────────────────────────────────


def preprocess_photo_input(image: np.ndarray) -> Tuple[np.ndarray, dict]:
    """Apply all three preprocessing steps in order. Returns the
    processed image and a dict of what was actually applied (for
    logging / diagnostics). Each step is internally gated — feeding
    in a clean scan returns the image untouched and the dict reports
    all-no-ops.
    """
    diag = {"skew_deg": 0.0, "clahe_applied": False,
            "denoise_applied": False, "upscale": 1.0}

    img, scale = upscale_small_image(image)
    diag["upscale"] = scale

    img, angle = detect_and_correct_skew(img)
    diag["skew_deg"] = angle

    before_clahe_v_std = (
        float(cv2.cvtColor(img, cv2.COLOR_BGR2HSV)[:, :, 2].std())
        if img.ndim == 3 else 0.0
    )
    img = normalize_lighting(img)
    if img.ndim == 3:
        after_v_std = float(cv2.cvtColor(img, cv2.COLOR_BGR2HSV)[:, :, 2].std())
        diag["clahe_applied"] = abs(after_v_std - before_clahe_v_std) > 1.0

    before_denoise_lap = (
        float(cv2.Laplacian(
            cv2.cvtColor(img, cv2.COLOR_BGR2GRAY) if img.ndim == 3 else img,
            cv2.CV_64F).std())
    )
    img = denoise_for_ocr(img)
    after_lap = float(cv2.Laplacian(
        cv2.cvtColor(img, cv2.COLOR_BGR2GRAY) if img.ndim == 3 else img,
        cv2.CV_64F).std())
    diag["denoise_applied"] = abs(after_lap - before_denoise_lap) > 0.5

    return img, diag
