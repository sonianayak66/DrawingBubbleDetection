from __future__ import annotations

import re
from typing import List

import cv2
import numpy as np

from .contracts import BalloonCandidate, BalloonOcrResult
from .ocr_utils import get_ocr


def _parse_ocr_result(raw):
    if raw is None:
        return []
    if isinstance(raw, tuple):
        raw = raw[0]
    return raw or []


def read_balloon_ids(image: np.ndarray, mask: np.ndarray, balloons: List[BalloonCandidate]) -> List[BalloonOcrResult]:
    ocr = get_ocr()
    results: List[BalloonOcrResult] = []
    h, w = image.shape[:2]

    for balloon in balloons:
        x1, y1, x2, y2 = balloon.bbox
        pad = int(max(6, balloon.radius * 0.18))
        x1p, y1p = max(0, x1 - pad), max(0, y1 - pad)
        x2p, y2p = min(w, x2 + pad), min(h, y2 + pad)
        crop = image[y1p:y2p, x1p:x2p]
        crop_mask = mask[y1p:y2p, x1p:x2p]
        if crop.size == 0:
            results.append(BalloonOcrResult(balloon.candidate_id, "", 0.0, "empty_crop"))
            continue

        # Make colored digits/rings dark on white. The OCR engine sees a
        # clean high-contrast crop without the surrounding drawing clutter.
        binary = np.full(crop_mask.shape, 255, dtype=np.uint8)
        binary[crop_mask > 0] = 0
        binary = cv2.resize(binary, None, fx=3, fy=3, interpolation=cv2.INTER_CUBIC)
        binary = cv2.morphologyEx(binary, cv2.MORPH_CLOSE, np.ones((2, 2), np.uint8), iterations=1)
        crop_bgr = cv2.cvtColor(binary, cv2.COLOR_GRAY2BGR)

        best_text = ""
        best_conf = 0.0
        try:
            for item in _parse_ocr_result(ocr(crop_bgr)):
                if len(item) < 3:
                    continue
                text = str(item[1]).strip()
                conf = float(item[2])
                digits = "".join(re.findall(r"\d+", text))
                if digits and conf > best_conf:
                    best_text = digits
                    best_conf = conf
        except Exception as exc:
            results.append(BalloonOcrResult(balloon.candidate_id, "", 0.0, "ocr_error", {"error": str(exc)}))
            continue

        status = "found" if best_text else "not_found"
        results.append(BalloonOcrResult(
            balloon_candidate_id=balloon.candidate_id,
            text=best_text,
            confidence=best_conf,
            status=status,
            debug={"crop_bbox": [x1p, y1p, x2p, y2p]},
        ))

    return results

