from __future__ import annotations

import time
from typing import Dict, Tuple

import cv2
import numpy as np

from .contracts import ImageEnhancementResult


def _quality_report(img: np.ndarray) -> Dict[str, object]:
    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY) if img.ndim == 3 else img
    h, w = gray.shape[:2]
    lap_var = float(cv2.Laplacian(gray, cv2.CV_64F).var())
    contrast = float(gray.std())
    mean = float(gray.mean())
    return {
        "width": int(w),
        "height": int(h),
        "short_side": int(min(w, h)),
        "laplacian_variance": round(lap_var, 2),
        "gray_mean": round(mean, 2),
        "gray_std": round(contrast, 2),
        "is_low_resolution": min(w, h) < 1000,
        "is_blurry": lap_var < 120.0,
        "is_low_contrast": contrast < 35.0,
    }


def enhance_image(
    img: np.ndarray,
    *,
    target_short_side: int = 1500,
    max_scale: float = 3.0,
) -> Tuple[np.ndarray, ImageEnhancementResult]:
    """Conservative enhancement for color annotation extraction.

    This path is intentionally scale-aware and returns the applied scale
    so downstream/debug consumers know which coordinate space is used.
    """
    if img is None or img.size == 0:
        raise ValueError("enhance_image received an empty image")
    if img.ndim != 3 or img.shape[2] != 3:
        raise ValueError(f"expected BGR image, got shape {img.shape}")

    ops = []
    quality = _quality_report(img)
    out = img.copy()
    h0, w0 = out.shape[:2]

    short = min(h0, w0)
    scale = 1.0
    if short < target_short_side:
        t0 = time.perf_counter()
        scale = min(max_scale, target_short_side / float(max(short, 1)))
        out = cv2.resize(out, None, fx=scale, fy=scale, interpolation=cv2.INTER_CUBIC)
        ops.append({
            "op": "upscale",
            "factor": round(scale, 3),
            "ms": round((time.perf_counter() - t0) * 1000.0, 1),
        })

    t0 = time.perf_counter()
    out = cv2.fastNlMeansDenoisingColored(
        out, None, h=4, hColor=4, templateWindowSize=7, searchWindowSize=21
    )
    ops.append({"op": "denoise", "method": "fastNlMeansDenoisingColored", "ms": round((time.perf_counter() - t0) * 1000.0, 1)})

    t0 = time.perf_counter()
    lab = cv2.cvtColor(out, cv2.COLOR_BGR2LAB)
    l_chan, a_chan, b_chan = cv2.split(lab)
    clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))
    l_chan = clahe.apply(l_chan)
    out = cv2.cvtColor(cv2.merge([l_chan, a_chan, b_chan]), cv2.COLOR_LAB2BGR)
    ops.append({"op": "clahe_luminance", "clip_limit": 2.0, "ms": round((time.perf_counter() - t0) * 1000.0, 1)})

    t0 = time.perf_counter()
    blur = cv2.GaussianBlur(out, (0, 0), 1.0)
    out = cv2.addWeighted(out, 1.35, blur, -0.35, 0)
    ops.append({"op": "unsharp_mask", "amount": 0.35, "ms": round((time.perf_counter() - t0) * 1000.0, 1)})

    result = ImageEnhancementResult(
        image_shape=(int(h0), int(w0)),
        enhanced_shape=(int(out.shape[0]), int(out.shape[1])),
        scale_factor=float(scale),
        quality_report=quality,
        ops=ops,
    )
    return out, result

