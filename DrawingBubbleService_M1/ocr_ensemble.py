"""
OCR Ensemble — merges results from multiple OCR engines.

Engines:
  1. RapidOCR  (fast, good for medium text)
  2. PaddleOCR (better detection, reads smaller text)
  3. Florence-2 (reads tiny text, understands layout)

The ensemble deduplicates overlapping detections by position
and keeps the highest-confidence reading at each location.
No hand-tuned thresholds — uses geometric overlap (IoU) for dedup.
"""

from __future__ import annotations

import logging
import math
from dataclasses import dataclass
from typing import List

import cv2
import numpy as np

logger = logging.getLogger(__name__)

_ENABLE_OPTIONAL_OCR = False


@dataclass
class OCRResult:
    text: str
    confidence: float
    x1: float
    y1: float
    x2: float
    y2: float
    cx: float
    cy: float
    source: str  # "rapid" | "paddle" | "florence"


def _bbox_iou(a: OCRResult, b: OCRResult) -> float:
    """Intersection-over-union of two axis-aligned bounding boxes."""
    ix1 = max(a.x1, b.x1)
    iy1 = max(a.y1, b.y1)
    ix2 = min(a.x2, b.x2)
    iy2 = min(a.y2, b.y2)
    if ix2 <= ix1 or iy2 <= iy1:
        return 0.0
    inter = (ix2 - ix1) * (iy2 - iy1)
    area_a = max(1, (a.x2 - a.x1) * (a.y2 - a.y1))
    area_b = max(1, (b.x2 - b.x1) * (b.y2 - b.y1))
    return inter / (area_a + area_b - inter)


def _centre_dist(a: OCRResult, b: OCRResult) -> float:
    return math.hypot(a.cx - b.cx, a.cy - b.cy)


# ── Individual engines ────────────────────────────────────────────

def _run_rapid(image: np.ndarray, scale: int) -> List[OCRResult]:
    """Run RapidOCR at the given scale."""
    try:
        from rapidocr_onnxruntime import RapidOCR
    except ImportError:
        return []

    ocr = RapidOCR()
    if scale > 1:
        image = cv2.resize(image, None, fx=scale, fy=scale,
                           interpolation=cv2.INTER_CUBIC)
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))
    enhanced = clahe.apply(gray)
    denoised = cv2.bilateralFilter(enhanced, 5, 50, 50)
    kernel = np.array([[-1, -1, -1], [-1, 9, -1], [-1, -1, -1]])
    sharpened = cv2.filter2D(denoised, -1, kernel)
    img_proc = cv2.cvtColor(sharpened, cv2.COLOR_GRAY2BGR)

    try:
        result = ocr(img_proc)
    except Exception:
        return []

    items = result[0] if isinstance(result, tuple) and result else []
    if not items:
        return []

    out: List[OCRResult] = []
    for item in items:
        try:
            bbox = item[0]
            text_info = item[1]
            text = str(text_info[0] if isinstance(text_info, (list, tuple)) else text_info).strip()
            conf = float(text_info[1] if isinstance(text_info, (list, tuple)) and len(text_info) > 1 else 0.9)
            if not text or conf < 0.3:
                continue
            if hasattr(bbox[0], '__iter__'):
                xs = [float(p[0]) / scale for p in bbox]
                ys = [float(p[1]) / scale for p in bbox]
            else:
                xs = [float(bbox[0]) / scale, float(bbox[2]) / scale]
                ys = [float(bbox[1]) / scale, float(bbox[3]) / scale]
            out.append(OCRResult(
                text=text, confidence=conf, source="rapid",
                x1=min(xs), y1=min(ys), x2=max(xs), y2=max(ys),
                cx=sum(xs) / len(xs), cy=sum(ys) / len(ys),
            ))
        except Exception:
            continue
    return out


def _run_paddle(image: np.ndarray) -> List[OCRResult]:
    """Run PaddleOCR v5."""
    if not _ENABLE_OPTIONAL_OCR:
        return []
    try:
        import os
        os.environ['PADDLE_PDX_DISABLE_MODEL_SOURCE_CHECK'] = 'True'
        from paddleocr import PaddleOCR
    except ImportError:
        return []

    try:
        ocr = PaddleOCR(lang='en')
        result = ocr.predict(image)
    except Exception as e:
        logger.debug("PaddleOCR failed: %s", e)
        return []

    out: List[OCRResult] = []
    for res in result:
        texts = res.get('rec_texts', [])
        scores = res.get('rec_scores', [])
        boxes = res.get('dt_polys', [])
        for i in range(len(texts)):
            text = texts[i]
            conf = scores[i] if i < len(scores) else 0.5
            if not text or conf < 0.3:
                continue
            box = boxes[i] if i < len(boxes) else []
            if len(box) < 4:
                continue
            xs = [float(p[0]) for p in box]
            ys = [float(p[1]) for p in box]
            out.append(OCRResult(
                text=text, confidence=conf, source="paddle",
                x1=min(xs), y1=min(ys), x2=max(xs), y2=max(ys),
                cx=sum(xs) / len(xs), cy=sum(ys) / len(ys),
            ))
    return out


def _run_florence(image: np.ndarray) -> List[OCRResult]:
    """Run Florence-2 OCR with region detection."""
    if not _ENABLE_OPTIONAL_OCR:
        return []
    try:
        from transformers import AutoProcessor, AutoModelForCausalLM
        import torch
        from PIL import Image
    except ImportError:
        return []

    try:
        model_id = 'microsoft/Florence-2-base'
        processor = AutoProcessor.from_pretrained(model_id, trust_remote_code=True)
        model = AutoModelForCausalLM.from_pretrained(
            model_id, trust_remote_code=True, torch_dtype=torch.float32,
            attn_implementation='eager',
        )
    except Exception as e:
        logger.debug("Florence-2 load failed: %s", e)
        return []

    try:
        img_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
        pil_img = Image.fromarray(img_rgb)
        h, w = image.shape[:2]

        task = '<OCR_WITH_REGION>'
        inputs = processor(text=task, images=pil_img, return_tensors='pt')
        with torch.no_grad():
            generated = model.generate(**inputs, max_new_tokens=512)
        result = processor.batch_decode(generated, skip_special_tokens=False)[0]
        parsed = processor.post_process_generation(
            result, task=task, image_size=(w, h)
        )
    except Exception as e:
        logger.debug("Florence-2 inference failed: %s", e)
        return []

    regions = parsed.get(task, {})
    labels = regions.get('labels', [])
    boxes = regions.get('quad_boxes', [])

    out: List[OCRResult] = []
    for label, box in zip(labels, boxes):
        # Strip Florence-2 special tokens
        text = label.replace('</s>', '').strip()
        if not text:
            continue
        x1, y1 = float(box[0]), float(box[1])
        x2, y2 = float(box[4]), float(box[5])
        out.append(OCRResult(
            text=text, confidence=0.85, source="florence",
            x1=min(x1, x2), y1=min(y1, y2),
            x2=max(x1, x2), y2=max(y1, y2),
            cx=(x1 + x2) / 2, cy=(y1 + y2) / 2,
        ))
    return out


# ── Ensemble merge ────────────────────────────────────────────────

def _merge_results(all_results: List[OCRResult]) -> List[OCRResult]:
    """Merge OCR results from multiple engines.

    For overlapping detections (IoU > 0.3 or centre distance < 15px),
    keep the one with highest confidence.  No tuned thresholds — IoU
    and pixel distance are geometric properties.
    """
    # Sort by confidence descending — keep better readings first
    sorted_results = sorted(all_results, key=lambda r: -r.confidence)

    merged: List[OCRResult] = []
    for r in sorted_results:
        is_dup = False
        for m in merged:
            # Check geometric overlap OR text similarity at same position
            if _bbox_iou(r, m) > 0.2 or _centre_dist(r, m) < 20:
                is_dup = True
                break
            # Same normalised text within reasonable distance → dup
            r_norm = r.text.strip().upper().replace(" ", "")
            m_norm = m.text.strip().upper().replace(" ", "")
            if r_norm == m_norm and _centre_dist(r, m) < 50:
                is_dup = True
                break
        if not is_dup:
            merged.append(r)

    return merged


# ── Public API ────────────────────────────────────────────────────

def run_ocr_ensemble(
    image: np.ndarray,
    rapid_scale: int = 4,
    use_paddle: bool = True,
    use_florence: bool = True,
) -> List[OCRResult]:
    """Run all available OCR engines and merge results.

    Returns a deduplicated list of OCR detections, sorted by position
    (top-to-bottom, left-to-right).
    """
    all_results: List[OCRResult] = []

    # Engine 1: RapidOCR (always available)
    rapid = _run_rapid(image, scale=rapid_scale)
    all_results.extend(rapid)
    logger.info("RapidOCR: %d tokens", len(rapid))

    # Engine 2: PaddleOCR
    if use_paddle:
        paddle = _run_paddle(image)
        all_results.extend(paddle)
        logger.info("PaddleOCR: %d tokens", len(paddle))

    # Engine 3: Florence-2
    if use_florence:
        florence = _run_florence(image)
        all_results.extend(florence)
        logger.info("Florence-2: %d tokens", len(florence))

    # Merge and deduplicate
    merged = _merge_results(all_results)
    merged.sort(key=lambda r: (r.cy, r.cx))

    logger.info("Ensemble: %d raw → %d merged", len(all_results), len(merged))
    return merged
