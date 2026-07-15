"""
Annotation layer extraction for engineering drawings.

Engineering drawings typically use a distinct chromatic color (purple,
red, blue, etc.) for annotations: balloons, balloon text, and leader
lines.  The drawing geometry (dimension lines, hatching, part outlines)
is black/dark gray.

This module extracts the annotation-color layer as a binary mask,
enabling reliable leader-line tracing that ignores drawing geometry.

Algorithm:
  1. Convert to HSV.
  2. Filter for pixels with saturation > threshold (chromatic pixels).
  3. Build a hue histogram of all chromatic pixels.
  4. Identify the dominant hue cluster (the annotation color).
  5. Create a binary mask of pixels within ±hue_tol of that hue.
  6. Morphological cleanup (close small gaps, remove noise).

The result is a clean mask where ONLY the annotation layer (balloons,
leader lines, annotation text) is white — everything else is black.
"""

from __future__ import annotations

import math
from dataclasses import dataclass
from typing import Optional, Tuple

import cv2
import numpy as np


@dataclass
class AnnotationLayer:
    """Result of annotation layer extraction."""
    mask: np.ndarray           # Binary mask (uint8, 0/255)
    dominant_hue: int          # OpenCV hue (0–180) of the annotation color
    dominant_sat: int          # Median saturation
    dominant_val: int          # Median value
    pixel_count: int           # Number of annotation pixels
    confidence: float          # 0–1: how distinct the annotation layer is


def extract_annotation_layer(
    image: np.ndarray,
    min_saturation: int = 30,
    hue_tolerance: int = 18,
    min_annotation_pixels: int = 200,
) -> Optional[AnnotationLayer]:
    """
    Extract the chromatic annotation layer from an engineering drawing.

    Returns None if the image has no clear chromatic annotation layer
    (e.g. a pure black-and-white drawing).

    Parameters
    ----------
    image : BGR image
    min_saturation : Minimum HSV saturation to consider a pixel chromatic.
    hue_tolerance : Half-width of the hue band around the dominant hue.
    min_annotation_pixels : Minimum pixels required for a valid layer.
    """
    h, w = image.shape[:2]
    hsv = cv2.cvtColor(image, cv2.COLOR_BGR2HSV)

    # Step 1: Find all chromatic pixels
    sat_channel = hsv[:, :, 1]
    chroma_mask = sat_channel > min_saturation

    n_chromatic = int(np.count_nonzero(chroma_mask))
    if n_chromatic < min_annotation_pixels:
        # Try lower threshold for faded images
        chroma_mask = sat_channel > max(15, min_saturation // 2)
        n_chromatic = int(np.count_nonzero(chroma_mask))
        if n_chromatic < min_annotation_pixels:
            return None

    # Step 2: Build hue histogram (18 bins × 10° each = 180°)
    chromatic_hues = hsv[:, :, 0][chroma_mask]
    hue_hist, bin_edges = np.histogram(chromatic_hues, bins=18, range=(0, 180))

    # Step 3: Find the dominant hue bin
    dominant_bin = int(np.argmax(hue_hist))
    dominant_count = int(hue_hist[dominant_bin])

    if dominant_count < min_annotation_pixels:
        return None

    # Step 4: Compute confidence — ratio of dominant cluster to total chromatic
    total_chromatic = max(1, int(np.sum(hue_hist)))
    confidence = dominant_count / total_chromatic

    # Step 5: Get precise hue/sat/val stats from the dominant cluster
    bin_lo = dominant_bin * 10
    bin_hi = (dominant_bin + 1) * 10
    cluster_mask = chroma_mask & (hsv[:, :, 0] >= bin_lo) & (hsv[:, :, 0] < bin_hi)
    cluster_hsv = hsv[cluster_mask]

    med_hue = int(np.median(cluster_hsv[:, 0]))
    med_sat = int(np.median(cluster_hsv[:, 1]))
    med_val = int(np.median(cluster_hsv[:, 2]))

    # Step 6: Build the annotation mask using the dominant hue ± tolerance
    lo_h = (med_hue - hue_tolerance) % 180
    hi_h = (med_hue + hue_tolerance) % 180
    min_s = max(15, med_sat // 3)
    min_v = max(30, med_val // 3)

    if lo_h <= hi_h:
        lower = np.array([lo_h, min_s, min_v], dtype=np.uint8)
        upper = np.array([hi_h, 255, 255], dtype=np.uint8)
        mask = cv2.inRange(hsv, lower, upper)
    else:
        # Hue wraps around 0/180 (common for red)
        lower1 = np.array([lo_h, min_s, min_v], dtype=np.uint8)
        upper1 = np.array([179, 255, 255], dtype=np.uint8)
        lower2 = np.array([0, min_s, min_v], dtype=np.uint8)
        upper2 = np.array([hi_h, 255, 255], dtype=np.uint8)
        mask = cv2.inRange(hsv, lower1, upper1) | cv2.inRange(hsv, lower2, upper2)

    # Step 7: Morphological cleanup
    # Close small gaps in leader lines (dashed lines, thin strokes)
    kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (5, 5))
    mask = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, kernel, iterations=2)
    # Remove small noise blobs
    kernel_small = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3))
    mask = cv2.morphologyEx(mask, cv2.MORPH_OPEN, kernel_small, iterations=1)

    pixel_count = int(np.count_nonzero(mask))

    return AnnotationLayer(
        mask=mask,
        dominant_hue=med_hue,
        dominant_sat=med_sat,
        dominant_val=med_val,
        pixel_count=pixel_count,
        confidence=confidence,
    )


def trace_annotation_path(
    annotation_mask: np.ndarray,
    start_x: int,
    start_y: int,
    direction: Tuple[float, float],
    balloon_cx: float,
    balloon_cy: float,
    balloon_radius: float,
    callout_boxes: list,
    max_steps: int = 600,
) -> Optional[list]:
    """
    Trace a leader line on the annotation layer mask starting from
    (start_x, start_y) heading in the given direction.

    This is the core improvement: because the mask contains ONLY
    annotation-color pixels (no drawing geometry), the trace follows
    the actual leader line, not dimension lines or hatching.

    Returns a list of (x, y) pixel coordinates along the path,
    or None if the trace fails to start.
    """
    h, w = annotation_mask.shape[:2]

    # Ensure start is on the mask
    if not (0 <= start_x < w and 0 <= start_y < h):
        return None

    # If start pixel is not on mask, search nearby
    if annotation_mask[start_y, start_x] == 0:
        found = False
        for r in range(1, 15):
            for dy in range(-r, r + 1):
                for dx in range(-r, r + 1):
                    if abs(dx) != r and abs(dy) != r:
                        continue
                    nx, ny = start_x + dx, start_y + dy
                    if 0 <= nx < w and 0 <= ny < h and annotation_mask[ny, nx] > 0:
                        # Prefer forward direction
                        dot = dx * direction[0] + dy * direction[1]
                        if dot >= 0:
                            start_x, start_y = nx, ny
                            found = True
                            break
                if found:
                    break
            if found:
                break
        if not found:
            return None

    # BFS trace along the annotation mask, biased toward the leader direction
    visited = np.zeros((h, w), dtype=bool)
    visited[start_y, start_x] = True

    # Suppress balloon interior
    suppress_r = max(1, int(balloon_radius * 0.85))
    bcx, bcy = int(round(balloon_cx)), int(round(balloon_cy))
    cy_arr, cx_arr = np.ogrid[:h, :w]
    balloon_interior = ((cx_arr - bcx) ** 2 + (cy_arr - bcy) ** 2) < suppress_r ** 2
    visited[balloon_interior] = True
    # But keep the start accessible
    visited[start_y, start_x] = False

    queue = []
    queue.append((0.0, start_x, start_y, [(start_x, start_y)]))

    import heapq
    best_path = [(start_x, start_y)]
    best_progress = -1e9

    neighbours = [(-1, -1), (0, -1), (1, -1),
                  (-1,  0),          (1,  0),
                  (-1,  1), (0,  1), (1,  1)]

    while queue:
        neg_progress, cx, cy, path = heapq.heappop(queue)
        progress = -neg_progress

        if len(path) > max_steps:
            continue

        # Check if we've reached a callout box
        for bx1, by1, bx2, by2 in callout_boxes:
            if bx1 <= cx <= bx2 and by1 <= cy <= by2:
                return path

        for dx, dy in neighbours:
            nx, ny = cx + dx, cy + dy
            if not (0 <= nx < w and 0 <= ny < h):
                continue
            if visited[ny, nx]:
                continue
            if annotation_mask[ny, nx] == 0:
                continue

            visited[ny, nx] = True

            step_progress = dx * direction[0] + dy * direction[1]
            new_progress = progress + step_progress
            new_path = path + [(nx, ny)]

            if new_progress > best_progress:
                best_progress = new_progress
                best_path = new_path

            heapq.heappush(queue, (-new_progress, nx, ny, new_path))

    return best_path if len(best_path) > 1 else None


def trace_leader_corridor(
    image: np.ndarray,
    annotation_mask: Optional[np.ndarray],
    start_x: int,
    start_y: int,
    direction: Tuple[float, float],
    balloon_cx: float,
    balloon_cy: float,
    balloon_radius: float,
    callout_boxes: list,
    corridor_half_width: int = 15,
    max_length: int = 500,
) -> Optional[list]:
    """
    Dual-mode leader line tracer.

    DOMAIN PRINCIPLE: Leader lines in engineering drawings are
    straight (or have at most one bend) and are thin dark lines.
    They may be the same color as the annotation layer (red/purple)
    or plain black ink.

    This tracer works in two modes, tried in order:

    Mode 1 — ANNOTATION COLOR: If the annotation mask has connected
    pixels from the start point, follow them (same as trace_annotation_path).
    This works when leader lines share the balloon color.

    Mode 2 — DARK-INK CORRIDOR: Cast a narrow corridor (±corridor_half_width
    pixels) along the seed direction and follow dark pixels within it.
    This works for black leader lines that the color mask misses.

    The corridor constraint is the key principled element — it
    prevents the trace from wandering into unrelated drawing geometry
    (hatching, dimension lines, outlines) that is also dark.

    Returns a list of (x, y) points or None.
    """
    h, w = image.shape[:2]
    dx, dy = direction

    # ── Mode 1: annotation color trace ───────────────────────────
    if annotation_mask is not None:
        path = trace_annotation_path(
            annotation_mask=annotation_mask,
            start_x=start_x, start_y=start_y,
            direction=direction,
            balloon_cx=balloon_cx, balloon_cy=balloon_cy,
            balloon_radius=balloon_radius,
            callout_boxes=callout_boxes,
            max_steps=max_length,
        )
        # Return the LONGER of annotation trace and corridor trace.
        # DOMAIN PRINCIPLE: the path that travels further from the
        # balloon is more likely to have reached the actual dimension,
        # regardless of whether it followed color or dark ink.
        ann_path = path if path and len(path) > 3 else None

    # ── Mode 2: dark-ink corridor trace ──────────────────────────
    # Build a binary mask of dark pixels (Otsu threshold)
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY) if len(image.shape) == 3 else image
    thresh_val = cv2.threshold(gray, 0, 255, cv2.THRESH_OTSU)[0]
    dark_mask = (gray < thresh_val).astype(np.uint8) * 255

    # Suppress the balloon interior so we don't trace backward
    bcx, bcy = int(round(balloon_cx)), int(round(balloon_cy))
    br = max(1, int(round(balloon_radius * 0.9)))
    cv2.circle(dark_mask, (bcx, bcy), br, 0, -1)

    # Build corridor mask: only keep dark pixels within ±corridor_half_width
    # of the line from start_point in the seed direction.
    # Perpendicular direction:
    perp_x, perp_y = -dy, dx  # 90° rotation

    corridor_mask = np.zeros((h, w), dtype=np.uint8)
    ys, xs = np.where(dark_mask > 0)
    if len(xs) == 0:
        return None

    # Vector from start to each dark pixel
    vx = xs.astype(np.float32) - start_x
    vy = ys.astype(np.float32) - start_y

    # Project onto leader direction and perpendicular
    proj_along = vx * dx + vy * dy         # forward distance
    proj_perp = np.abs(vx * perp_x + vy * perp_y)  # lateral distance

    # Keep pixels that are:
    #   - in the forward direction (proj_along > -5)
    #   - within the corridor width
    #   - not too far ahead (max_length)
    keep = (
        (proj_along > -5)
        & (proj_along < max_length)
        & (proj_perp < corridor_half_width)
    )
    corridor_mask[ys[keep], xs[keep]] = 255

    # Morphological close to bridge small gaps
    kern = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3))
    corridor_mask = cv2.morphologyEx(corridor_mask, cv2.MORPH_CLOSE, kern)

    # Trace along the corridor using BFS
    # Find the connected component from the start point
    if not (0 <= start_x < w and 0 <= start_y < h):
        return None

    # Ensure start is on the mask (search nearby if needed)
    if corridor_mask[start_y, start_x] == 0:
        found = False
        for r in range(1, 10):
            for ddy in range(-r, r + 1):
                for ddx in range(-r, r + 1):
                    nx, ny = start_x + ddx, start_y + ddy
                    if 0 <= nx < w and 0 <= ny < h and corridor_mask[ny, nx] > 0:
                        start_x, start_y = nx, ny
                        found = True
                        break
                if found:
                    break
            if found:
                break
        if not found:
            return None

    # BFS trace along connected dark pixels in the corridor
    visited = np.zeros((h, w), dtype=bool)
    visited[start_y, start_x] = True
    queue = [(start_x, start_y)]
    path = [(start_x, start_y)]
    best_forward = 0.0
    best_point = (start_x, start_y)

    neighbours = [(-1, -1), (0, -1), (1, -1),
                  (-1,  0),          (1,  0),
                  (-1,  1), (0,  1), (1,  1)]

    head = 0
    while head < len(queue):
        cx, cy = queue[head]
        head += 1

        # Check if we've reached a callout box
        for bx1, by1, bx2, by2 in callout_boxes:
            if bx1 <= cx <= bx2 and by1 <= cy <= by2:
                return path[:path.index((cx, cy)) + 1] if (cx, cy) in path else path

        for ddx, ddy in neighbours:
            nx, ny = cx + ddx, cy + ddy
            if not (0 <= nx < w and 0 <= ny < h):
                continue
            if visited[ny, nx]:
                continue
            if corridor_mask[ny, nx] == 0:
                continue
            visited[ny, nx] = True
            queue.append((nx, ny))

            # Track the furthest forward point
            fwd = (nx - start_x) * dx + (ny - start_y) * dy
            if fwd > best_forward:
                best_forward = fwd
                best_point = (nx, ny)
                path.append((nx, ny))

    corridor_path = path if len(path) > 1 else None

    # Return the path that traveled further from the balloon.
    # DOMAIN PRINCIPLE: a longer trace is more likely to have
    # reached the dimension text region.
    if ann_path and corridor_path:
        ann_far = max(
            math.hypot(p[0] - balloon_cx, p[1] - balloon_cy)
            for p in ann_path
        )
        cor_far = max(
            math.hypot(p[0] - balloon_cx, p[1] - balloon_cy)
            for p in corridor_path
        )
        return corridor_path if cor_far > ann_far else ann_path
    return ann_path or corridor_path
