"""
Bubble Detection Engine - MPCRS Edition
========================================
OpenCV  →  Hough circle detection
RapidOCR  →  Text / dimension extraction
scipy  →  Hungarian algorithm for optimal bubble-to-dimension matching

Key method: detect_from_array(img: np.ndarray)
  Accepts an already-loaded OpenCV image. No file I/O inside this class.
  Used by main.py HTTP endpoint (image bytes decoded before calling this).
"""

import cv2
import numpy as np
import math
import re
import logging
from dataclasses import dataclass
from typing import List, Tuple

from rapidocr_onnxruntime import RapidOCR
from scipy.optimize import linear_sum_assignment

logger = logging.getLogger(__name__)

# ── OCR Singleton ─────────────────────────────────────────────────
# RapidOCR loads ONNX models on first use. Keep one instance for the process lifetime.
_ocr_instance: RapidOCR = None


def _get_ocr() -> RapidOCR:
    global _ocr_instance
    if _ocr_instance is None:
        _ocr_instance = RapidOCR()
    return _ocr_instance


# ── Data Classes ──────────────────────────────────────────────────

@dataclass
class DetectionConfig:
    # Circle detection (Hough Transform)
    min_radius: int = 18
    max_radius: int = 58
    hough_param1: int = 50
    hough_param2: int = 28
    min_dist: int = 40
    dedup_dist: int = 25
    # Interior content filtering
    min_dark_ratio: float = 0.02
    max_dark_ratio: float = 0.45
    edge_margin: int = 5
    # OCR scaling — 2x improves accuracy on small bubble text
    ocr_scale: int = 2
    # Bubble-to-dimension association max distance (pixels)
    max_assoc_distance: int = 300
    # Keywords near "Part No." labels — used to exclude part number labels from bubble detection
    part_num_keywords: Tuple = ("No.", "PART", "P/N", "DWG")


@dataclass
class TextItem:
    text: str
    cx: float
    cy: float
    conf: float
    x1: float = 0.0
    y1: float = 0.0
    x2: float = 0.0
    y2: float = 0.0


@dataclass
class BubbleResult:
    bubble_number: str
    x: int
    y: int
    radius: int
    dimension: str = ""
    zone: str = ""
    confidence: float = 0.0
    needs_review: bool = False

    def to_dict(self) -> dict:
        return {
            "bubble_number": self.bubble_number,
            "dimension": self.dimension,
            "zone": self.zone,
            "x": self.x,
            "y": self.y,
            "radius": self.radius,
            "confidence": round(self.confidence, 2),
            "needs_review": self.needs_review,
        }


# ── Main Detector Class ───────────────────────────────────────────

class BubbleDetector:

    def __init__(self, config: DetectionConfig = None):
        self.cfg = config or DetectionConfig()
        self.ocr = _get_ocr()

    # ── Public API ────────────────────────────────────────────────

    def detect(self, image_path: str) -> Tuple[List[BubbleResult], np.ndarray]:
        """Load image from file path and run detection. Convenience wrapper."""
        img = cv2.imread(image_path)
        if img is None:
            raise ValueError(f"Cannot load image from path: {image_path}")
        return self.detect_from_array(img)

    def detect_from_array(self, img: np.ndarray) -> Tuple[List[BubbleResult], np.ndarray]:
        """
        Primary detection method.
        Accepts an already-decoded OpenCV BGR image array.
        Returns (list of BubbleResult, annotated image as np.ndarray).

        No file I/O is performed inside this method.
        """
        if img is None:
            raise ValueError("Image array is None.")

        gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        h, w = gray.shape
        logger.info(f"Image size: {w}x{h}")

        # Step 1: OCR the full image to get all text regions
        text_items = self._run_ocr(img)
        logger.info(f"OCR found {len(text_items)} text regions")

        # Step 2: Find candidate circles via Hough transform
        circles = self._find_circles(gray)
        logger.info(f"Found {len(circles)} candidate circles")

        # Step 3: Match circles with bubble number text inside them
        bubbles, used_text = self._identify_bubbles(circles, text_items, w, h)

        # Step 4: Find bubble numbers that have no matching circle (rectangular labels etc.)
        self._find_uncircled_bubbles(bubbles, text_items, used_text, circles)
        logger.info(f"Identified {len(bubbles)} bubbles")

        # Step 5: Classify remaining text as dimension groups or zone labels
        dim_groups, zone_info = self._classify_text(text_items, used_text, w, h)
        logger.info(f"Found {len(dim_groups)} dimension groups")

        # Step 6: Optimal bubble ↔ dimension assignment (Hungarian algorithm)
        self._associate(bubbles, dim_groups)

        # Step 7: Assign zone labels to bubbles
        self._assign_zones(bubbles, zone_info, w, h)

        # Step 8: Sort output by bubble number
        bubbles.sort(key=lambda b: _sort_key(b.bubble_number))

        # Step 9: Draw annotated image
        annotated = self._annotate(img, bubbles, circles, dim_groups)

        return bubbles, annotated

    # ── OCR ───────────────────────────────────────────────────────

    def _run_ocr(self, img: np.ndarray) -> List[TextItem]:
        scale = self.cfg.ocr_scale
        if scale > 1:
            img_scaled = cv2.resize(img, None, fx=scale, fy=scale, interpolation=cv2.INTER_CUBIC)
        else:
            img_scaled = img

        results, _ = self.ocr(img_scaled)
        items = []
        if results:
            for box, text, conf in results:
                xs = [p[0] / scale for p in box]
                ys = [p[1] / scale for p in box]
                items.append(TextItem(
                    text=text.strip(),
                    conf=conf,
                    cx=sum(xs) / 4,
                    cy=sum(ys) / 4,
                    x1=min(xs), y1=min(ys),
                    x2=max(xs), y2=max(ys),
                ))
        return items

    # ── Circle Detection ──────────────────────────────────────────

    def _find_circles(self, gray: np.ndarray) -> List[Tuple[int, int, int]]:
        blurred = cv2.GaussianBlur(gray, (9, 9), 2)
        cfg = self.cfg
        all_c = []

        # Run Hough with three parameter sets to improve recall
        for dp, p2_delta in [(1.2, 0), (1.5, -3), (1.0, 5)]:
            p2 = max(15, cfg.hough_param2 + p2_delta)
            circles = cv2.HoughCircles(
                blurred, cv2.HOUGH_GRADIENT,
                dp=dp, minDist=cfg.min_dist,
                param1=cfg.hough_param1, param2=p2,
                minRadius=cfg.min_radius, maxRadius=cfg.max_radius,
            )
            if circles is not None:
                for c in circles[0]:
                    all_c.append((int(c[0]), int(c[1]), int(c[2])))

        # Deduplicate overlapping detections
        unique = []
        for c in all_c:
            if not any(math.dist(c[:2], u[:2]) < cfg.dedup_dist for u in unique):
                unique.append(c)
        return unique

    # ── Bubble Identification ─────────────────────────────────────

    def _identify_bubbles(
        self,
        circles: List[Tuple[int, int, int]],
        text_items: List[TextItem],
        w: int,
        h: int,
    ) -> Tuple[List[BubbleResult], set]:
        cfg = self.cfg
        part_num_pos = [
            (t.cx, t.cy) for t in text_items
            if any(kw in t.text for kw in cfg.part_num_keywords)
        ]
        bubbles: List[BubbleResult] = []
        used_text: set = set()
        seen_nums: set = set()

        for cx, cy, r in circles:
            # Skip circles touching the image edge
            if (cx - r < cfg.edge_margin or cy - r < cfg.edge_margin or
                    cx + r > w - cfg.edge_margin or cy + r > h - cfg.edge_margin):
                continue

            for i, t in enumerate(text_items):
                if i in used_text:
                    continue
                if math.dist((cx, cy), (t.cx, t.cy)) < r * 1.3:
                    if _is_bubble_number(t.text):
                        norm = _normalize(t.text)
                        # Exclude part number title block labels
                        if any(math.dist((t.cx, t.cy), p) < 120 for p in part_num_pos):
                            used_text.add(i)
                            continue
                        if norm not in seen_nums:
                            seen_nums.add(norm)
                            bubbles.append(BubbleResult(
                                bubble_number=norm,
                                x=cx, y=cy, radius=r,
                                confidence=t.conf,
                            ))
                        used_text.add(i)
                        break

        return bubbles, used_text

    def _find_uncircled_bubbles(
        self,
        bubbles: List[BubbleResult],
        text_items: List[TextItem],
        used_text: set,
        circles: List[Tuple[int, int, int]],
    ):
        """Detect balloon numbers that appear in rectangular boxes or without a circle."""
        cfg = self.cfg
        part_num_pos = [
            (t.cx, t.cy) for t in text_items
            if any(kw in t.text for kw in cfg.part_num_keywords)
        ]
        seen = {b.bubble_number for b in bubbles}

        for i, t in enumerate(text_items):
            if i in used_text or not _is_bubble_number(t.text):
                continue
            norm = _normalize(t.text)
            if norm in seen:
                continue
            if any(math.dist((t.cx, t.cy), p) < 120 for p in part_num_pos):
                continue
            near_circle = any(
                math.dist((cx, cy), (t.cx, t.cy)) < 60
                for cx, cy, _ in circles
            )
            if near_circle or _is_isolated(t, text_items, i):
                seen.add(norm)
                bubbles.append(BubbleResult(
                    bubble_number=norm,
                    x=int(t.cx), y=int(t.cy),
                    radius=35,
                    confidence=t.conf,
                ))
                used_text.add(i)

    # ── Text Classification ───────────────────────────────────────

    def _classify_text(
        self,
        text_items: List[TextItem],
        used_text: set,
        w: int,
        h: int,
    ) -> Tuple[list, List[TextItem]]:
        SKIP = {"ZONE", "No.", "of1", "A1", "--", "*"}
        ZONE_LABELS = {"b4", "c4", "c5", "c6", "d5", "d4", "a1"}
        TITLE_BLOCK_PATTERNS = (
            r"^PER\s", r"GTPS", r"REV\b", r"SCALE", r"SHEET",
            r"DRAWN", r"CHECKED", r"APPROVED", r"DATE", r"MATERIAL",
        )

        dim_texts: List[TextItem] = []
        zone_info: List[TextItem] = []

        for i, t in enumerate(text_items):
            if i in used_text:
                continue
            text = t.text.strip()
            if not text or t.conf < 0.3 or text in SKIP:
                continue
            if text.lower() in ZONE_LABELS:
                zone_info.append(t)
                continue
            # Skip right-margin title block text without numbers
            if t.cx > w * 0.88 and not re.search(r"\d", text):
                continue
            if any(re.search(p, text, re.I) for p in TITLE_BLOCK_PATTERNS):
                continue
            # Skip bottom title block area unless clearly a numeric dimension
            if t.cy > h * 0.95 and not re.search(r"^\d+\.?\d*$", text):
                continue
            if _is_dimension_text(text):
                dim_texts.append(t)

        dim_groups = self._group_dimensions(dim_texts)
        return dim_groups, zone_info

    def _group_dimensions(
        self,
        dim_texts: List[TextItem],
        v_gap: int = 45,
        h_gap: int = 35,
    ) -> list:
        """Group vertically stacked dimension texts (e.g. tolerance pairs like 1.7 / 1.5)."""
        used: set = set()
        groups: list = []
        sorted_d = sorted(enumerate(dim_texts), key=lambda x: (x[1].cx, x[1].cy))

        for idx, dt in sorted_d:
            if idx in used:
                continue
            group = [dt]
            used.add(idx)

            for idx2, dt2 in sorted_d:
                if idx2 in used:
                    continue
                if abs(dt.cx - dt2.cx) < h_gap and abs(dt.cy - dt2.cy) < v_gap:
                    group.append(dt2)
                    used.add(idx2)

            # Pull in nearby MAJOR DIA labels
            for idx2, dt2 in sorted_d:
                if idx2 in used:
                    continue
                if "MAJOR" in dt2.text and math.dist((dt.cx, dt.cy), (dt2.cx, dt2.cy)) < 120:
                    group.append(dt2)
                    used.add(idx2)

            groups.append(group)

        result = []
        for group in groups:
            gcx = sum(d.cx for d in group) / len(group)
            gcy = sum(d.cy for d in group) / len(group)
            gtext = _format_dim_group(group)
            if gtext:
                result.append((group, gcx, gcy, gtext))
        return result

    # ── Association (Hungarian Algorithm) ────────────────────────

    def _associate(self, bubbles: List[BubbleResult], dim_groups: list):
        """Optimally assign dimension groups to bubbles using the Hungarian algorithm."""
        if not bubbles or not dim_groups:
            for b in bubbles:
                b.needs_review = True
            return

        n_b = len(bubbles)
        n_d = len(dim_groups)
        size = max(n_b, n_d)
        BIG = 100_000.0

        cost = np.full((size, size), BIG, dtype=float)
        bpos = [(b.x, b.y, b.radius) for b in bubbles]

        for i, b in enumerate(bubbles):
            for j, (_, gcx, gcy, _) in enumerate(dim_groups):
                d = math.dist((b.x, b.y), (gcx, gcy))
                if d < b.radius * 0.8 or d > self.cfg.max_assoc_distance:
                    continue
                # Reject if dim center falls inside another bubble
                inside = any(
                    math.dist((gcx, gcy), (p[0], p[1])) < p[2] * 1.2
                    for k, p in enumerate(bpos) if k != i
                )
                if inside:
                    continue
                cost[i][j] = d

        row_ind, col_ind = linear_sum_assignment(cost)
        for i, j in zip(row_ind, col_ind):
            if i < n_b and j < n_d and cost[i][j] < BIG:
                bubbles[i].dimension = dim_groups[j][3]
            elif i < n_b:
                bubbles[i].needs_review = True

    # ── Zone Assignment ───────────────────────────────────────────

    def _assign_zones(
        self,
        bubbles: List[BubbleResult],
        zone_info: List[TextItem],
        w: int,
        h: int,
    ):
        if not zone_info:
            return
        for b in bubbles:
            for z in zone_info:
                if abs(b.y - z.cy) < 100:
                    b.zone = z.text.strip()
                    break

    # ── Annotation ────────────────────────────────────────────────

    def _annotate(
        self,
        img: np.ndarray,
        bubbles: List[BubbleResult],
        circles: List[Tuple[int, int, int]],
        dim_groups: list,
    ) -> np.ndarray:
        out = img.copy()
        bubble_positions = {(b.x, b.y) for b in bubbles}

        # Draw rejected circles faintly
        for cx, cy, r in circles:
            if (cx, cy) not in bubble_positions:
                cv2.circle(out, (cx, cy), r, (180, 180, 220), 1)

        # Draw confirmed bubbles
        for b in bubbles:
            color = (0, 200, 0) if b.dimension else (0, 180, 255)
            cv2.circle(out, (b.x, b.y), b.radius, color, 3)

            # Bubble number label above circle
            label = b.bubble_number
            (tw, th), _ = cv2.getTextSize(label, cv2.FONT_HERSHEY_SIMPLEX, 0.7, 2)
            lx = b.x - tw // 2
            ly = b.y - b.radius - 12
            cv2.rectangle(out, (lx - 3, ly - th - 3), (lx + tw + 3, ly + 5), (255, 255, 255), -1)
            cv2.putText(out, label, (lx, ly), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 120, 0), 2)

            # Dimension label below circle
            if b.dimension:
                dim = b.dimension[:35]
                (dw, dh), _ = cv2.getTextSize(dim, cv2.FONT_HERSHEY_SIMPLEX, 0.38, 1)
                dx = b.x - dw // 2
                dy = b.y + b.radius + 16
                cv2.rectangle(out, (dx - 2, dy - dh - 2), (dx + dw + 2, dy + 4), (200, 255, 200), -1)
                cv2.putText(out, dim, (dx, dy), cv2.FONT_HERSHEY_SIMPLEX, 0.38, (0, 80, 0), 1)

        return out


# ── Helper Functions ──────────────────────────────────────────────

def _is_bubble_number(text: str) -> bool:
    """Return True if text looks like a balloon/bubble number (1-99 or 1A-99Z)."""
    text = text.strip()
    if not text or len(text) > 3:
        return False
    if text.isdigit() and 1 <= int(text) <= 99:
        return True
    m = re.match(r"^(\d{1,2})([A-Za-z])$", text)
    return bool(m and 1 <= int(m.group(1)) <= 99)


def _normalize(text: str) -> str:
    """Normalize bubble number — strip leading zeros, uppercase alpha suffix."""
    text = text.strip().upper()
    return str(int(text)) if text.isdigit() else text


def _sort_key(text: str) -> Tuple:
    m = re.match(r"^(\d+)([A-Z]?)$", text)
    return (int(m.group(1)), m.group(2)) if m else (999, text)


def _is_isolated(item: TextItem, all_items: List[TextItem], idx: int) -> bool:
    """Return True if no other text items are within 50px of this item."""
    return (
        sum(
            1 for i, t in enumerate(all_items)
            if i != idx and math.dist((item.cx, item.cy), (t.cx, t.cy)) < 50
        ) <= 1
    )


def _is_dimension_text(text: str) -> bool:
    """Return True if text looks like a dimension, tolerance, or thread spec."""
    patterns = [
        r"\d+\.?\d*",       # plain number: 4.85, 10, 1.7
        r"\d+[xX]\d+",      # chamfer: 0.5x45
        r"DIA",             # diameter label
        r"MAJOR",           # MAJOR DIA
        r"MJ\d+",           # thread: MJ5x0.8
        r"\d+[hH]\d+",      # tolerance class: 4h6h
        r"\d+°",            # angle
    ]
    return any(re.search(p, text.strip()) for p in patterns)


def _format_dim_group(group: List[TextItem]) -> str:
    """Combine a group of related dimension texts into a single readable string."""
    texts = [d.text.strip() for d in sorted(group, key=lambda d: d.cy)]
    cleaned = []
    for t in texts:
        # Strip leading dashes (OCR artefacts)
        t = re.sub(r"^[-—–=]+", "", t).strip()
        # Fix OCR merge: "B20.5x45" → "0.5x45"
        m = re.match(r"^[A-Z]\d(\d+\.?\d*[xX].+)$", t)
        if m:
            t = m.group(1)
        if t and t not in ("--",):
            cleaned.append(t)

    if not cleaned:
        return ""

    # Two plain numbers → tolerance pair: "1.7/1.5"
    if len(cleaned) == 2:
        if (re.match(r"^\d+\.?\d*$", cleaned[0]) and
                re.match(r"^\d+\.?\d*$", cleaned[1])):
            return f"{cleaned[0]}/{cleaned[1]}"

    return " ".join(cleaned)
