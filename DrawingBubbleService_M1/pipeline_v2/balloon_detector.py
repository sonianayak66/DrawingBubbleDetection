from __future__ import annotations

import math
from typing import List

import cv2
import numpy as np

from .contracts import BalloonCandidate


def detect_balloons(mask: np.ndarray) -> List[BalloonCandidate]:
    contours, _ = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    h, w = mask.shape[:2]
    image_area = max(1, h * w)
    candidates: List[BalloonCandidate] = []

    min_area = max(30.0, image_area * 0.000015)
    max_area = image_area * 0.035

    for cnt in contours:
        area = float(cv2.contourArea(cnt))
        if area < min_area or area > max_area:
            continue

        x, y, bw, bh = cv2.boundingRect(cnt)
        if bw <= 3 or bh <= 3:
            continue

        aspect = bw / float(max(bh, 1))
        if aspect < 0.45 or aspect > 1.9:
            continue

        perimeter = float(cv2.arcLength(cnt, True))
        if perimeter <= 1.0:
            continue
        circularity = float(4.0 * math.pi * area / (perimeter * perimeter))

        (cx, cy), radius = cv2.minEnclosingCircle(cnt)
        if radius < 5 or radius > min(h, w) * 0.12:
            continue

        fill_ratio = area / float(max(math.pi * radius * radius, 1.0))
        # Thin rings have low fill ratio after color segmentation, so do
        # not require a filled-circle signature.
        if fill_ratio < 0.03 or fill_ratio > 1.8:
            continue

        shape_score = min(1.0, max(0.0, circularity * 2.4))
        aspect_score = max(0.0, 1.0 - min(abs(1.0 - aspect), 1.0))
        fill_score = min(1.0, fill_ratio / 0.18) if fill_ratio < 0.18 else 1.0
        confidence = float(0.45 * shape_score + 0.35 * aspect_score + 0.20 * fill_score)

        candidates.append(BalloonCandidate(
            candidate_id="",
            bbox=(int(x), int(y), int(x + bw), int(y + bh)),
            center=(float(cx), float(cy)),
            radius=float(radius),
            contour_area=area,
            circularity=circularity,
            aspect_ratio=float(aspect),
            confidence=confidence,
        ))

    candidates.sort(key=lambda c: (c.center[1], c.center[0]))
    for idx, cand in enumerate(candidates, start=1):
        cand.candidate_id = f"b_{idx:03d}"
    return candidates

