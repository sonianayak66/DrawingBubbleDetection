from __future__ import annotations

from typing import Tuple

import cv2
import numpy as np

from annotation_layer import extract_annotation_layer

from .contracts import AnnotationSegmentationResult


def segment_annotation_color(img: np.ndarray) -> Tuple[np.ndarray, AnnotationSegmentationResult]:
    """Extract magenta/maroon annotation pixels as a binary mask."""
    layer = extract_annotation_layer(
        img,
        min_saturation=22,
        hue_tolerance=22,
        min_annotation_pixels=60,
    )
    hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)

    if layer is None:
        # Fallback for known magenta/maroon bands when the dominant-hue
        # detector cannot lock onto a single cluster.
        lower = np.array([120, 18, 35], dtype=np.uint8)
        upper = np.array([178, 255, 255], dtype=np.uint8)
        mask = cv2.inRange(hsv, lower, upper)
        dominant_hue = None
        dominant_sat = None
        dominant_val = None
        confidence = 0.25 if int(np.count_nonzero(mask)) else 0.0
    else:
        mask = layer.mask
        dominant_hue = layer.dominant_hue
        dominant_sat = layer.dominant_sat
        dominant_val = layer.dominant_val
        confidence = layer.confidence

    kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3))
    mask = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, kernel, iterations=2)
    mask = cv2.dilate(mask, kernel, iterations=1)

    pixel_count = int(np.count_nonzero(mask))
    color_stats = {
        "coverage_pct": round(pixel_count * 100.0 / max(mask.shape[0] * mask.shape[1], 1), 4),
    }
    if pixel_count:
        hsv_pixels = hsv[mask > 0]
        color_stats.update({
            "median_hue": int(np.median(hsv_pixels[:, 0])),
            "median_sat": int(np.median(hsv_pixels[:, 1])),
            "median_val": int(np.median(hsv_pixels[:, 2])),
        })

    return mask, AnnotationSegmentationResult(
        dominant_hue=dominant_hue,
        dominant_sat=dominant_sat,
        dominant_val=dominant_val,
        pixel_count=pixel_count,
        confidence=float(confidence),
        color_stats=color_stats,
    )

