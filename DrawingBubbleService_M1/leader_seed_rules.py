"""
Leader seed detection and local trace rules.

Design goals:
- Detect the balloon-boundary exit point using HoughLinesP (v2, primary)
  OR the original color-matching annulus (v1, fallback)
- Trace only connected pixels from that seed in a local window
- Stop as soon as the traced path reaches the first valid callout region
- No image-specific or color assumptions in v2

Architecture:
  detect_leader_seed_v2()  -> HoughLinesP-based, style-agnostic  [PRIMARY]
  detect_leader_seed()     -> color-matching annulus              [FALLBACK]
  trace_leader_from_seed() -> BFS path tracer                    [UNCHANGED]
"""

from __future__ import annotations

from collections import deque
from dataclasses import dataclass
from typing import Dict, List, Optional, Tuple
import math

import cv2
import numpy as np

try:
    from .callout_rules import CalloutGroup
except ImportError:
    from callout_rules import CalloutGroup


# ─────────────────────────────────────────────────────────────────────────────
# Data classes
# ─────────────────────────────────────────────────────────────────────────────

@dataclass
class LeaderSeed:
    contact_x: int
    contact_y: int
    dir_x: float
    dir_y: float
    confidence: float
    source: str   # "hough_v2" | "annulus_component" | "fallback"


@dataclass
class BalloonGeometry:
    cx: float
    cy: float
    radius: float
    contour: Optional[np.ndarray] = None


# ─────────────────────────────────────────────────────────────────────────────
# Public API — seed detection
# ─────────────────────────────────────────────────────────────────────────────

def detect_leader_seed_v2(
    image: np.ndarray,
    balloon_geom: BalloonGeometry,
    search_scale: float = 3.0,
) -> Optional[LeaderSeed]:
    """
    PRIMARY leader seed detection using adaptive thresholding + HoughLinesP.

    Does NOT rely on color matching — works across drawing styles, scan
    quality levels, JPEG compression, and blueprint vs. print formats.
    Falls back to None if no plausible line segment is found; the caller
    should then try detect_leader_seed() (color-based fallback).
    """
    cx = int(round(balloon_geom.cx))
    cy = int(round(balloon_geom.cy))
    r  = max(1, int(round(balloon_geom.radius)))
    h, w = image.shape[:2]

    pad = int(r * search_scale)
    rx1 = max(0, cx - pad)
    ry1 = max(0, cy - pad)
    rx2 = min(w, cx + pad)
    ry2 = min(h, cy + pad)
    roi = image[ry1:ry2, rx1:rx2]
    if roi.size == 0:
        return None

    gray = cv2.cvtColor(roi, cv2.COLOR_BGR2GRAY) if len(roi.shape) == 3 else roi.copy()
    blurred = cv2.GaussianBlur(gray, (3, 3), 0)

    # Adaptive binarization — handles dark-on-light and light-on-dark drawings
    mean_val = float(np.mean(gray))
    if mean_val > 128:
        thresh = cv2.adaptiveThreshold(
            blurred, 255,
            cv2.ADAPTIVE_THRESH_GAUSSIAN_C, cv2.THRESH_BINARY_INV, 11, 4
        )
    else:
        thresh = cv2.adaptiveThreshold(
            blurred, 255,
            cv2.ADAPTIVE_THRESH_GAUSSIAN_C, cv2.THRESH_BINARY, 11, 4
        )

    # Remove balloon interior so we don't trace the circle body
    local_cx = cx - rx1
    local_cy = cy - ry1
    interior = np.ones_like(thresh)
    cv2.circle(interior, (local_cx, local_cy), max(1, r - 2), 0, -1)
    thresh = thresh & interior

    # HoughLinesP — parameters scale with bubble radius
    min_len  = max(8,  int(r * 0.45))
    max_gap  = max(4,  int(r * 0.25))
    h_thresh = max(8,  int(r * 0.35))

    lines = cv2.HoughLinesP(
        thresh, rho=1, theta=np.pi / 180,
        threshold=h_thresh,
        minLineLength=min_len,
        maxLineGap=max_gap,
    )
    if lines is None:
        return None

    best_seed:  Optional[LeaderSeed] = None
    best_score: float = float("inf")

    for line in lines:
        lx1, ly1, lx2, ly2 = line[0]
        # Convert local ROI coords → global image coords
        gx1, gy1 = lx1 + rx1, ly1 + ry1
        gx2, gy2 = lx2 + rx1, ly2 + ry1

        d1 = math.hypot(gx1 - cx, gy1 - cy)
        d2 = math.hypot(gx2 - cx, gy2 - cy)
        target = r * 1.08  # just outside the rim

        # Pick the endpoint nearer the balloon boundary as the contact
        if abs(d1 - target) <= abs(d2 - target):
            contact_x, contact_y = gx1, gy1
            far_x, far_y = gx2, gy2
            contact_d = d1
            prox_score = abs(d1 - target)
        else:
            contact_x, contact_y = gx2, gy2
            far_x, far_y = gx1, gy1
            contact_d = d2
            prox_score = abs(d2 - target)

        # Contact must sit in the annulus [0.5r, 2.5r] from center
        if contact_d < r * 0.5 or contact_d > r * 2.5:
            continue

        seg_len = math.hypot(far_x - contact_x, far_y - contact_y)
        if seg_len < 5:
            continue

        # Prefer longer lines (more evidence) over short spurious ones
        adjusted = prox_score - seg_len * 0.1

        if adjusted < best_score:
            best_score = adjusted
            dx = far_x - contact_x
            dy = far_y - contact_y
            norm = math.hypot(dx, dy)
            conf = max(0.1, min(1.0, 1.0 - prox_score / max(1.0, r * 2.0)))
            best_seed = LeaderSeed(
                contact_x=int(contact_x),
                contact_y=int(contact_y),
                dir_x=dx / norm,
                dir_y=dy / norm,
                confidence=conf,
                source="hough_v2",
            )

    return best_seed


def detect_leader_seed(
    image: np.ndarray,
    balloon_geom: BalloonGeometry,
    annulus_inner_scale: float = 0.92,
    annulus_outer_scale: float = 1.30,
    color_tol: int = 55,
) -> Optional[LeaderSeed]:
    """
    FALLBACK leader seed detection using color-matching in a thin annulus.

    Original approach — works well when the leader line colour closely matches
    the balloon rim. Used as fallback when detect_leader_seed_v2 returns None
    or has low confidence.
    """
    h, w = image.shape[:2]
    cx = float(balloon_geom.cx)
    cy = float(balloon_geom.cy)
    r  = max(1.0, float(balloon_geom.radius))

    ref_color = _sample_rim_color(image, balloon_geom)

    annulus_mask = np.zeros((h, w), dtype=np.uint8)
    cv2.circle(annulus_mask, (int(round(cx)), int(round(cy))),
               int(round(r * annulus_outer_scale)), 255, -1)
    cv2.circle(annulus_mask, (int(round(cx)), int(round(cy))),
               int(round(r * annulus_inner_scale)), 0, -1)

    ys, xs = np.where(annulus_mask > 0)
    if ys.size == 0:
        return None

    pixels = image[ys, xs].astype(np.int16)
    ref = ref_color.astype(np.int16)
    color_diff = np.linalg.norm(pixels - ref, axis=1)

    match_mask = np.zeros((h, w), dtype=np.uint8)
    match_mask[ys[color_diff <= color_tol], xs[color_diff <= color_tol]] = 255

    kernel = np.ones((3, 3), dtype=np.uint8)
    match_mask = cv2.morphologyEx(match_mask, cv2.MORPH_CLOSE, kernel, iterations=1)
    match_mask &= annulus_mask

    num_labels, labels, stats, centroids = cv2.connectedComponentsWithStats(
        match_mask, connectivity=8
    )
    if num_labels <= 1:
        return None

    best: Optional[Tuple[float, LeaderSeed]] = None
    for label in range(1, num_labels):
        area = int(stats[label, cv2.CC_STAT_AREA])
        if area < 3:
            continue

        comp_ys, comp_xs = np.where(labels == label)
        if comp_ys.size == 0:
            continue

        dx = comp_xs.astype(np.float32) - cx
        dy = comp_ys.astype(np.float32) - cy
        dists = np.hypot(dx, dy)

        boundary_touch = np.count_nonzero(np.abs(dists - r) <= 1.35)
        outward_pixels = np.count_nonzero(dists >= r + 1.0)
        if boundary_touch == 0 or outward_pixels == 0:
            continue

        comp_cx, comp_cy = centroids[label]
        dir_x = float(comp_cx - cx)
        dir_y = float(comp_cy - cy)
        mag = math.hypot(dir_x, dir_y)
        if mag < 1e-6:
            continue
        ux, uy = dir_x / mag, dir_y / mag

        outwardness  = float(np.max(dists) - r)
        median_offset = float(np.median(dists) - r)
        elongation   = _component_elongation(comp_xs, comp_ys, ux, uy)
        score = (
            outwardness * 3.0
            + median_offset * 1.2
            + min(area, 24) * 0.12
            + min(elongation, 8.0) * 0.8
            + min(boundary_touch, 10) * 0.25
        )

        contact_x, contact_y = _boundary_contact_point(
            comp_xs=comp_xs, comp_ys=comp_ys,
            center_x=cx, center_y=cy, radius=r,
            dir_x=ux, dir_y=uy,
        )

        seed = LeaderSeed(
            contact_x=int(contact_x),
            contact_y=int(contact_y),
            dir_x=ux,
            dir_y=uy,
            confidence=min(0.98, 0.45 + 0.03 * area + 0.04 * outwardness),
            source="annulus_component",
        )

        if best is None or score > best[0]:
            best = (score, seed)

    return None if best is None else best[1]


def detect_best_leader_seed(
    image: np.ndarray,
    balloon_geom: BalloonGeometry,
) -> Optional[LeaderSeed]:
    """
    Convenience wrapper: try HoughLinesP v2 first, fall back to color-based.
    Always returns the best available seed or None.
    """
    seed = detect_leader_seed_v2(image, balloon_geom)
    if seed is not None and seed.confidence >= 0.25:
        return seed
    # Fallback to original color-based method
    return detect_leader_seed(image, balloon_geom)


# ─────────────────────────────────────────────────────────────────────────────
# Public API — path tracing
# ─────────────────────────────────────────────────────────────────────────────

def trace_leader_from_seed(
    image: np.ndarray,
    balloon_geom: BalloonGeometry,
    seed: LeaderSeed,
    boxes: List[Tuple[float, float, float, float]],
    callouts: Optional[List[CalloutGroup]] = None,
    max_trace_len: int = 500,
    local_radius: int = 200,
    color_tol: int = 80,
) -> Optional[List[Tuple[int, int]]]:
    """
    Trace only the connected annotation pixels that originate at the seed.

    Returns an ordered pixel path. When a valid note box is reached the path
    is truncated there so downstream assignment uses the first reached callout.
    """
    h, w = image.shape[:2]
    boxes = _normalize_note_boxes(callouts=callouts, note_boxes=boxes)
    if not boxes:
        return None

    local_radius = _local_trace_radius(
        image_w=w, image_h=h,
        balloon_geom=balloon_geom,
        max_trace_len=max_trace_len,
    )

    x1 = max(0, int(seed.contact_x) - local_radius)
    y1 = max(0, int(seed.contact_y) - local_radius)
    x2 = min(w, int(seed.contact_x) + local_radius + 1)
    y2 = min(h, int(seed.contact_y) + local_radius + 1)
    if x2 <= x1 or y2 <= y1:
        return None

    crop = image[y1:y2, x1:x2]
    if crop.size == 0:
        return None

    ref_color = _sample_seed_color(image, seed, balloon_geom)
    mask = _build_local_leader_mask(crop=crop, ref_color=ref_color, color_tol=color_tol)

    # The balloon, balloon text, and leader lines share the same
    # annotation color (purple, red, etc.).  The color mask above
    # already captures this.  No additional dark-ink mask needed.

    local_seed_x = int(seed.contact_x) - x1
    local_seed_y = int(seed.contact_y) - y1
    if not (0 <= local_seed_x < mask.shape[1] and 0 <= local_seed_y < mask.shape[0]):
        return None

    if balloon_geom is not None:
        _suppress_balloon_interior(
            mask=mask,
            balloon_geom=balloon_geom,
            roi_origin=(x1, y1),
            keep_point=(local_seed_x, local_seed_y),
            stamp_radius=6,
        )

    if mask[local_seed_y, local_seed_x] == 0:
        recovered = _nearest_mask_pixel(
            mask=mask,
            start=(local_seed_x, local_seed_y),
            dir_vec=(seed.dir_x, seed.dir_y),
            max_radius=12,
        )
        if recovered is None:
            cv2.circle(mask, (local_seed_x, local_seed_y), 8, 255, -1)
            recovered = (local_seed_x, local_seed_y)
        local_seed_x, local_seed_y = recovered

    # Close 1-2px gaps in the leader line (scan artifacts, JPEG compression).
    # Morphological close = dilate then erode — fills breaks without growing
    # the overall shape, so unrelated nearby blobs stay separate.
    gap_kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3))
    mask = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, gap_kernel)

    component = _component_from_seed(mask, (local_seed_x, local_seed_y))
    if component is None:
        component = np.zeros_like(mask, dtype=np.uint8)
        cv2.circle(component, (local_seed_x, local_seed_y), 10, 255, -1)

    component_pixels = int(np.count_nonzero(component))
    if component_pixels <= 0:
        return None

    max_component_pixels = min(
        50000,
        max(100, int(0.20 * component.shape[0] * component.shape[1])),
    )
    if component_pixels > max_component_pixels:
        ch, cw = component.shape
        cy_arr, cx_arr = np.ogrid[:ch, :cw]
        dist_from_seed = np.sqrt((cx_arr - local_seed_x) ** 2 + (cy_arr - local_seed_y) ** 2)
        component = component & (dist_from_seed <= 100).astype(np.uint8)

    component = _trim_component_behind_seed(
        component=component,
        seed_xy=(local_seed_x, local_seed_y),
        dir_vec=(seed.dir_x, seed.dir_y),
    )

    local_boxes = _boxes_to_local(
        boxes=boxes,
        roi_origin=(x1, y1),
        roi_shape=(mask.shape[1], mask.shape[0]),
    )
    if not local_boxes:
        return None

    path_local = _directed_component_trace(
        component=component,
        seed_xy=(local_seed_x, local_seed_y),
        dir_vec=(seed.dir_x, seed.dir_y),
        local_boxes=local_boxes,
        max_trace_len=max_trace_len,
    )
    if not path_local:
        return None

    return [(px + x1, py + y1) for px, py in path_local]


# ─────────────────────────────────────────────────────────────────────────────
# Helpers
# ─────────────────────────────────────────────────────────────────────────────

def _sample_rim_color(image: np.ndarray, geom: BalloonGeometry) -> np.ndarray:
    h, w = image.shape[:2]
    colors: List[np.ndarray] = []
    radius = max(1.0, float(geom.radius))
    for ang_deg in range(0, 360, 12):
        ang = math.radians(ang_deg)
        px = int(round(geom.cx + radius * math.cos(ang)))
        py = int(round(geom.cy + radius * math.sin(ang)))
        if 0 <= px < w and 0 <= py < h:
            colors.append(image[py, px])
    if not colors:
        return np.array([0, 0, 0], dtype=np.float32)
    return np.median(np.asarray(colors, dtype=np.float32), axis=0)


def _sample_seed_color(
    image: np.ndarray,
    seed: LeaderSeed,
    balloon_geom: Optional[BalloonGeometry],
) -> np.ndarray:
    colors: List[np.ndarray] = []
    h, w = image.shape[:2]
    for step in range(-3, 5):
        px = int(round(seed.contact_x + seed.dir_x * step))
        py = int(round(seed.contact_y + seed.dir_y * step))
        if 0 <= px < w and 0 <= py < h:
            colors.append(image[py, px])
    if colors:
        return np.median(np.asarray(colors, dtype=np.float32), axis=0)
    if balloon_geom is not None:
        return _sample_rim_color(image, balloon_geom)
    return image[int(seed.contact_y), int(seed.contact_x)].astype(np.float32)


def _build_local_leader_mask(
    crop: np.ndarray,
    ref_color: np.ndarray,
    color_tol: int,
) -> np.ndarray:
    pixels = crop.astype(np.float32)
    ref = ref_color.astype(np.float32)

    diff = np.linalg.norm(pixels - ref[None, None, :], axis=2)
    mask_dist = diff <= float(color_tol)

    b_bias = float(ref[0] - ref[1])
    r_bias = float(ref[2] - ref[1])
    sb = 1.0 if b_bias >= 0.0 else -1.0
    sr = 1.0 if r_bias >= 0.0 else -1.0

    channel_score = (
        sb * (pixels[:, :, 0] - pixels[:, :, 1])
        + sr * (pixels[:, :, 2] - pixels[:, :, 1])
    )
    ref_bias_score = sb * b_bias + sr * r_bias
    bias_tol = max(8.0, min(22.0, abs(ref_bias_score) * 0.60 + 4.0))
    min_score = max(4.0, ref_bias_score - bias_tol)
    mask_bias = channel_score >= min_score

    mask = (mask_dist | mask_bias).astype(np.uint8) * 255

    # HSV-based annotation layer detection.
    # Engineering drawings commonly use a distinct hue (purple, magenta,
    # red, blue) for annotation layers.  When the reference colour has
    # enough chroma (saturation > threshold), augment the mask with an
    # HSV range filter so thin leader lines with slight colour variation
    # are not lost.
    mask = _augment_mask_with_hsv(crop, ref, mask)

    kernel5 = np.ones((5, 5), dtype=np.uint8)
    mask = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, kernel5, iterations=1)
    mask = cv2.medianBlur(mask, 3)
    return mask


def _augment_mask_with_hsv(
    crop: np.ndarray,
    ref_bgr: np.ndarray,
    mask: np.ndarray,
    min_chroma: float = 15.0,
) -> np.ndarray:
    """
    If the reference colour is chromatic (not gray/black), add pixels
    matching its HSV hue range to the mask.

    Handles the common case of purple/magenta annotation layers where
    thin lines may differ slightly in BGR space but share the same hue.
    """
    b_val, g_val, r_val = float(ref_bgr[0]), float(ref_bgr[1]), float(ref_bgr[2])
    chroma = max(b_val, g_val, r_val) - min(b_val, g_val, r_val)
    if chroma < min_chroma:
        return mask  # achromatic reference — HSV filter won't help

    # Convert reference to HSV
    ref_pixel = np.uint8([[list(ref_bgr.astype(np.uint8))]])
    ref_hsv = cv2.cvtColor(ref_pixel, cv2.COLOR_BGR2HSV)[0, 0]
    ref_h = int(ref_hsv[0])

    # Convert crop to HSV
    hsv = cv2.cvtColor(crop, cv2.COLOR_BGR2HSV)

    # Build hue-based range (±15 hue units in OpenCV's 0-180 scale)
    hue_tol = 15
    lo_h = (ref_h - hue_tol) % 180
    hi_h = (ref_h + hue_tol) % 180
    min_s = max(20, int(ref_hsv[1] * 0.3))  # at least 30% of ref saturation
    min_v = max(40, int(ref_hsv[2] * 0.3))

    if lo_h <= hi_h:
        lower = np.array([lo_h, min_s, min_v], dtype=np.uint8)
        upper = np.array([hi_h, 255, 255], dtype=np.uint8)
        hsv_mask = cv2.inRange(hsv, lower, upper)
    else:
        # Hue wraps around 0/180
        lower1 = np.array([lo_h, min_s, min_v], dtype=np.uint8)
        upper1 = np.array([179, 255, 255], dtype=np.uint8)
        lower2 = np.array([0, min_s, min_v], dtype=np.uint8)
        upper2 = np.array([hi_h, 255, 255], dtype=np.uint8)
        hsv_mask = cv2.inRange(hsv, lower1, upper1) | cv2.inRange(hsv, lower2, upper2)

    # Combine with existing mask
    return mask | hsv_mask


def _suppress_balloon_interior(
    mask: np.ndarray,
    balloon_geom: BalloonGeometry,
    roi_origin: Tuple[int, int],
    keep_point: Tuple[int, int],
    stamp_radius: int = 6,
) -> None:
    local_cx = int(round(balloon_geom.cx)) - roi_origin[0]
    local_cy = int(round(balloon_geom.cy)) - roi_origin[1]
    suppress_r = max(1, int(round(balloon_geom.radius * 0.88)))
    cv2.circle(mask, (local_cx, local_cy), suppress_r, 0, -1)
    cv2.circle(mask, keep_point, stamp_radius, 255, -1)


def _nearest_mask_pixel(
    mask: np.ndarray,
    start: Tuple[int, int],
    dir_vec: Tuple[float, float],
    max_radius: int = 12,
) -> Optional[Tuple[int, int]]:
    h, w = mask.shape
    sx, sy = start
    for step in range(1, max_radius + 1):
        for dy_off in range(-step, step + 1):
            for dx_off in range(-step, step + 1):
                if abs(dx_off) != step and abs(dy_off) != step:
                    continue
                nx, ny = sx + dx_off, sy + dy_off
                if 0 <= nx < w and 0 <= ny < h and mask[ny, nx]:
                    dot = dx_off * dir_vec[0] + dy_off * dir_vec[1]
                    if dot >= 0:
                        return (nx, ny)
    return None


def _component_from_seed(
    mask: np.ndarray,
    seed_xy: Tuple[int, int],
) -> Optional[np.ndarray]:
    h, w = mask.shape
    sx, sy = seed_xy
    if not (0 <= sx < w and 0 <= sy < h):
        return None
    if mask[sy, sx] == 0:
        return None

    num_labels, labels = cv2.connectedComponents(mask, connectivity=8)
    target_label = int(labels[sy, sx])
    if target_label == 0:
        return None

    comp = (labels == target_label).astype(np.uint8) * 255
    return comp


def _trim_component_behind_seed(
    component: np.ndarray,
    seed_xy: Tuple[int, int],
    dir_vec: Tuple[float, float],
) -> np.ndarray:
    ys, xs = np.where(component > 0)
    if ys.size == 0:
        return component

    sx, sy = seed_xy
    dxs = xs.astype(np.float32) - sx
    dys = ys.astype(np.float32) - sy
    dots = dxs * dir_vec[0] + dys * dir_vec[1]

    behind = dots < -6.0
    trimmed = component.copy()
    trimmed[ys[behind], xs[behind]] = 0
    return trimmed


def _boxes_to_local(
    boxes: List[Tuple[float, float, float, float]],
    roi_origin: Tuple[int, int],
    roi_shape: Tuple[int, int],
) -> List[Tuple[int, int, int, int]]:
    ox, oy = roi_origin
    rw, rh = roi_shape
    out = []
    for x1, y1, x2, y2 in boxes:
        lx1 = max(0, int(x1) - ox)
        ly1 = max(0, int(y1) - oy)
        lx2 = min(rw, int(x2) - ox)
        ly2 = min(rh, int(y2) - oy)
        if lx2 > lx1 and ly2 > ly1:
            out.append((lx1, ly1, lx2, ly2))
    return out


def _normalize_note_boxes(
    callouts: Optional[List[CalloutGroup]],
    note_boxes: List[Tuple[float, float, float, float]],
) -> List[Tuple[float, float, float, float]]:
    if callouts is not None:
        return [(c.x1, c.y1, c.x2, c.y2) for c in callouts]
    return list(note_boxes)


def _directed_component_trace(
    component: np.ndarray,
    seed_xy: Tuple[int, int],
    dir_vec: Tuple[float, float],
    local_boxes: List[Tuple[int, int, int, int]],
    max_trace_len: int,
) -> Optional[List[Tuple[int, int]]]:
    h, w = component.shape
    sx, sy = seed_xy
    if not (0 <= sx < w and 0 <= sy < h):
        return None

    visited = np.zeros((h, w), dtype=bool)
    visited[sy, sx] = True

    queue = deque()
    queue.append((sx, sy, 0.0, [(sx, sy)]))

    best_path: Optional[List[Tuple[int, int]]] = None
    best_progress: float = -float("inf")

    neighbours8 = [(-1, -1), (0, -1), (1, -1),
                   (-1,  0),           (1,  0),
                   (-1,  1), (0,  1), (1,  1)]

    while queue:
        cx, cy, progress, path = queue.popleft()

        if len(path) > max_trace_len:
            continue

        for dx, dy in neighbours8:
            nx, ny = cx + dx, cy + dy
            if not (0 <= nx < w and 0 <= ny < h):
                continue
            if visited[ny, nx]:
                continue
            if component[ny, nx] == 0:
                continue

            visited[ny, nx] = True

            step_progress = dx * dir_vec[0] + dy * dir_vec[1]
            new_progress  = progress + step_progress
            new_path      = path + [(nx, ny)]

            # Check if we've entered a callout box
            for bx1, by1, bx2, by2 in local_boxes:
                if bx1 <= nx <= bx2 and by1 <= ny <= by2:
                    return new_path

            if new_progress > best_progress:
                best_progress = new_progress
                best_path = new_path

            queue.append((nx, ny, new_progress, new_path))

    return best_path


def _local_trace_radius(
    image_w: int,
    image_h: int,
    balloon_geom: BalloonGeometry,
    max_trace_len: int,
) -> int:
    r = max(1.0, float(balloon_geom.radius))
    adaptive = int(r * 10)
    return max(60, min(max_trace_len, adaptive, max(image_w, image_h) // 2))


def _component_elongation(
    xs: np.ndarray,
    ys: np.ndarray,
    ux: float,
    uy: float,
) -> float:
    if xs.size < 2:
        return 0.0
    proj = xs * ux + ys * uy
    perp = xs * (-uy) + ys * ux
    proj_range = float(proj.max() - proj.min())
    perp_range = float(perp.max() - perp.min()) + 1e-6
    return proj_range / perp_range


def _boundary_contact_point(
    comp_xs: np.ndarray,
    comp_ys: np.ndarray,
    center_x: float,
    center_y: float,
    radius: float,
    dir_x: float,
    dir_y: float,
) -> Tuple[float, float]:
    dists = np.hypot(comp_xs - center_x, comp_ys - center_y)
    proj  = (comp_xs - center_x) * dir_x + (comp_ys - center_y) * dir_y

    on_boundary = np.abs(dists - radius) <= 2.5
    if np.any(on_boundary & (proj >= 0)):
        mask = on_boundary & (proj >= 0)
    elif np.any(on_boundary):
        mask = on_boundary
    else:
        mask = np.ones(len(comp_xs), dtype=bool)

    best_idx = int(np.argmax(proj[mask]))
    selected_xs = comp_xs[mask]
    selected_ys = comp_ys[mask]
    return float(selected_xs[best_idx]), float(selected_ys[best_idx])


# ─────────────────────────────────────────────────────────────────────────────
# Global color-trace: follow leader line color across the full image
# ─────────────────────────────────────────────────────────────────────────────

def color_trace_to_callout(
    image: np.ndarray,
    balloon_geom: BalloonGeometry,
    callouts: List[CalloutGroup],
    color_tol: int = 50,
    min_chroma: float = 8.0,
    min_leader_pixels: int = 10,
) -> Optional[int]:
    """
    Find the callout index associated with *balloon_geom* by globally
    tracing the leader line's annotation colour.

    Algorithm
    ---------
    1. Sample the bubble rim colour.
    2. Confirm it has non-trivial chroma (not a plain black/gray bubble).
    3. Build a global binary mask of pixels matching that colour.
    4. Morphologically close the mask to bridge small gaps in the line.
    5. Erase the bubble interior so only the outward leader remains.
    6. Find the connected component that touches the bubble boundary.
    7. Identify the pixel in that component farthest from the balloon centre.
    8. Return the index of the callout whose bounding box is nearest to
       that far endpoint.

    Returns None when:
      - The rim is achromatic (gray/black circle).
      - No coloured connected component extends beyond the bubble.
      - No callout is within a reasonable proximity of the endpoint.
    """
    if not callouts:
        return None

    h, w = image.shape[:2]
    cx = float(balloon_geom.cx)
    cy = float(balloon_geom.cy)
    r  = max(4.0, float(balloon_geom.radius))

    # ── 1. Sample rim colour ──────────────────────────────────────────
    ref_bgr = _sample_rim_color(image, balloon_geom)   # float32 [B, G, R]

    # ── 2. Chroma check ──────────────────────────────────────────────
    b_val, g_val, r_val = float(ref_bgr[0]), float(ref_bgr[1]), float(ref_bgr[2])
    chroma = max(b_val, g_val, r_val) - min(b_val, g_val, r_val)
    if chroma < min_chroma:
        return None   # achromatic bubble — colour trace won't help

    # ── 3. Global colour mask ─────────────────────────────────────────
    img_f = image.astype(np.float32)
    diff  = np.linalg.norm(img_f - ref_bgr[None, None, :], axis=2)
    mask  = (diff <= float(color_tol)).astype(np.uint8) * 255

    # Augment with HSV hue matching for chromatic annotations
    mask = _augment_mask_with_hsv(image, ref_bgr, mask)

    # ── 4. Close gaps (leader lines can be dashed / thin) ────────────
    kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (5, 5))
    mask = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, kernel, iterations=2)

    # ── 5. Erase balloon interior + small safety margin ──────────────
    icx, icy = int(round(cx)), int(round(cy))
    ir = max(1, int(round(r * 0.85)))
    cv2.circle(mask, (icx, icy), ir, 0, -1)

    # ── 6. Find the component that touches the balloon boundary ───────
    #    Sample a ring of points just outside the bubble rim and pick
    #    whichever labelled component has the most rim votes.
    num_labels, labels = cv2.connectedComponents(mask, connectivity=8)
    if num_labels <= 1:
        return None

    rim_vote: Dict[int, int] = {}
    for ang_deg in range(0, 360, 6):
        ang = math.radians(ang_deg)
        px = int(round(cx + r * 1.05 * math.cos(ang)))
        py = int(round(cy + r * 1.05 * math.sin(ang)))
        if 0 <= px < w and 0 <= py < h:
            lbl = int(labels[py, px])
            if lbl > 0:
                rim_vote[lbl] = rim_vote.get(lbl, 0) + 1

    if not rim_vote:
        return None

    best_lbl = max(rim_vote, key=lambda k: rim_vote[k])
    comp_mask = (labels == best_lbl).astype(np.uint8) * 255

    comp_ys, comp_xs = np.where(comp_mask > 0)
    if comp_ys.size < min_leader_pixels:
        return None   # component too small — probably noise

    # ── 7. Farthest point from bubble centre ─────────────────────────
    dists_from_center = np.hypot(comp_xs - cx, comp_ys - cy)
    far_idx  = int(np.argmax(dists_from_center))
    far_x    = float(comp_xs[far_idx])
    far_y    = float(comp_ys[far_idx])
    far_dist = float(dists_from_center[far_idx])

    # Must extend meaningfully beyond the rim
    if far_dist < r * 2.0:
        return None

    # ── 8. Nearest callout to the far endpoint ────────────────────────
    best_idx: Optional[int] = None
    best_gap: float = float("inf")
    max_gap  = max(80.0, r * 4.0)   # accept if endpoint is within this of callout box

    for ci, c in enumerate(callouts):
        # Distance from far_x/y to the callout bounding box
        box_dx = max(c.x1 - far_x, 0.0, far_x - c.x2)
        box_dy = max(c.y1 - far_y, 0.0, far_y - c.y2)
        gap    = math.hypot(box_dx, box_dy)
        if gap < best_gap:
            best_gap = gap
            best_idx = ci

    if best_idx is None or best_gap > max_gap:
        return None

    return best_idx
