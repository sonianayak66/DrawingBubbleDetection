from __future__ import annotations

from typing import List, Tuple

import cv2
import numpy as np

from .contracts import TextCandidate


def suppress_table_regions(image: np.ndarray, text_candidates: List[TextCandidate]) -> Tuple[List[TextCandidate], List[Tuple[int, int, int, int]]]:
    """Simple title/table-block suppression.

    The first version uses left/bottom dense document areas as penalties.
    Later this can be replaced by grid-line region detection.
    """
    h, w = image.shape[:2]
    suppressed_regions: List[Tuple[int, int, int, int]] = []

    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    edges = cv2.Canny(gray, 80, 180)
    left_roi = edges[:, : int(w * 0.34)]
    if np.count_nonzero(left_roi) > left_roi.size * 0.015:
        suppressed_regions.append((0, 0, int(w * 0.34), h))
    bottom_roi = edges[int(h * 0.78) :, :]
    if np.count_nonzero(bottom_roi) > bottom_roi.size * 0.02:
        suppressed_regions.append((0, int(h * 0.78), w, h))

    for cand in text_candidates:
        cx, cy = cand.center
        for x1, y1, x2, y2 in suppressed_regions:
            if x1 <= cx <= x2 and y1 <= cy <= y2:
                cand.suppressed = True
                cand.suppression_reason = "table_or_title_region"
                break

    return text_candidates, suppressed_regions

