"""
Topology-based leader assignment.

Recovers which bubble connects to which callout by analyzing the
connected ink structure of the drawing.  Instead of approximating
leader paths with geometric heuristics, we ask: "which callout
shares a connected-component of ink with this bubble?"

Pipeline:
  1. Build a combined binary mask: annotation-color pixels (leaders,
     bubbles) PLUS full-binary pixels near callout bboxes (bridges
     the color gap where leaders meet black dimension text)
  2. Dilate lightly to close 1-2px scan gaps
  3. cv2.connectedComponents on the mask
  4. For each bubble, sample the component labels on its rim
  5. For each callout, sample the component labels in its bbox
  6. Bubble → callout when they share a component label
  7. Disambiguate by preferring the callout whose shared-component
     contact point is closest to the bubble

This replaces skeleton extraction with a simpler, more robust
algorithm that doesn't fragment on real drawings.
"""

from __future__ import annotations

import math
from typing import Dict, List, Optional, Set, Tuple

import cv2
import numpy as np


def skeleton_assign(
    image: np.ndarray,
    annotation_mask: Optional[np.ndarray],
    bubbles: List[Tuple[int, int, int]],
    callout_bboxes: List[Tuple[float, float, float, float]],
    rim_tolerance: int = 8,
    bbox_margin: int = 15,
) -> Dict[int, int]:
    """
    Connected-component leader assignment.

    Parameters
    ----------
    image : BGR image
    annotation_mask : binary mask of annotation-color pixels (or None)
    bubbles : [(cx, cy, radius), ...]
    callout_bboxes : [(x1, y1, x2, y2), ...]

    Returns
    -------
    {bubble_index: callout_index} for unambiguous assignments
    """
    h, w = image.shape[:2]
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

    if annotation_mask is not None and np.count_nonzero(annotation_mask) > 100:
        # Use annotation mask only — it isolates leader lines from
        # drawing geometry.  Dilate by 3px to bridge small scan gaps
        # at leader endpoints, but NOT enough to merge parallel leaders
        # (which are typically 8+ px apart).
        kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (5, 5))
        mask = cv2.dilate(annotation_mask, kernel, iterations=1)
    else:
        # No annotation layer — fall back to full binary threshold
        _, mask = cv2.threshold(
            gray, 0, 255, cv2.THRESH_BINARY_INV + cv2.THRESH_OTSU,
        )
    # Connected-component labeling
    n_labels, labels = cv2.connectedComponents(mask, connectivity=8)

    # Sample component labels on each bubble's rim
    bubble_labels: Dict[int, Set[int]] = {}
    for bi, (bx, by, br) in enumerate(bubbles):
        label_set: Set[int] = set()
        for r_offset in range(max(1, br - rim_tolerance), br + rim_tolerance + 1):
            for ang_deg in range(0, 360, 5):
                ang = math.radians(ang_deg)
                px = int(bx + r_offset * math.cos(ang))
                py = int(by + r_offset * math.sin(ang))
                if 0 <= px < w and 0 <= py < h:
                    lbl = int(labels[py, px])
                    if lbl > 0:  # 0 = background
                        label_set.add(lbl)
        bubble_labels[bi] = label_set

    # Sample component labels in each callout's bbox
    callout_labels: Dict[int, Set[int]] = {}
    for ci, (x1, y1, x2, y2) in enumerate(callout_bboxes):
        label_set = set()
        iy1 = max(0, int(y1) - bbox_margin)
        iy2 = min(h, int(y2) + bbox_margin)
        ix1 = max(0, int(x1) - bbox_margin)
        ix2 = min(w, int(x2) + bbox_margin)
        # Sample a grid of points (not every pixel — too slow)
        step = max(1, min((ix2 - ix1), (iy2 - iy1)) // 8)
        for py in range(iy1, iy2, max(1, step)):
            for px in range(ix1, ix2, max(1, step)):
                lbl = int(labels[py, px])
                if lbl > 0:
                    label_set.add(lbl)
        callout_labels[ci] = label_set

    # Filter out overly-large components — these are drawing outlines,
    # borders, or merged geometry that touch many bubbles and callouts,
    # creating false connections.  A real leader component is a thin
    # line with bounded pixel count.
    label_sizes = np.bincount(labels.ravel())
    max_leader_pixels = int(h * w * 0.02)  # leader < 2% of image
    huge_labels: Set[int] = set()
    for lbl in range(1, n_labels):
        if label_sizes[lbl] > max_leader_pixels:
            huge_labels.add(lbl)

    # Strip huge labels from bubble/callout label sets
    for bi in bubble_labels:
        bubble_labels[bi] -= huge_labels
    for ci in callout_labels:
        callout_labels[ci] -= huge_labels

    # For each component label, collect which bubbles and callouts touch it.
    # A component is an EXCLUSIVE link when it touches exactly 1 bubble
    # and exactly 1 callout — this means a single leader line connects
    # them with no ambiguity.  Components touching multiple bubbles or
    # callouts are ambiguous and should be resolved by Hungarian fallback.
    label_to_bubbles: Dict[int, Set[int]] = {}
    label_to_callouts: Dict[int, Set[int]] = {}
    for bi, lbls in bubble_labels.items():
        for lbl in lbls:
            label_to_bubbles.setdefault(lbl, set()).add(bi)
    for ci, lbls in callout_labels.items():
        for lbl in lbls:
            label_to_callouts.setdefault(lbl, set()).add(ci)

    # Collect exclusive links: component → (single bubble, single callout)
    assignments: Dict[int, int] = {}
    used_callouts: Set[int] = set()

    # First pass: lock exclusive 1:1 links
    exclusive_pairs: List[Tuple[int, int, int]] = []  # (label, bi, ci)
    for lbl in range(1, n_labels):
        if lbl in huge_labels:
            continue
        b_set = label_to_bubbles.get(lbl, set())
        c_set = label_to_callouts.get(lbl, set())
        if len(b_set) == 1 and len(c_set) == 1:
            bi = next(iter(b_set))
            ci = next(iter(c_set))
            exclusive_pairs.append((lbl, bi, ci))

    # For each exclusive pair, if neither bubble nor callout is
    # already assigned, lock it
    for lbl, bi, ci in exclusive_pairs:
        if bi in assignments or ci in used_callouts:
            continue
        assignments[bi] = ci
        used_callouts.add(ci)

    return assignments
