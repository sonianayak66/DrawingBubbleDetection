"""
Image preprocessing for engineering drawings.

Converts input drawings into a clean binarized form where:
  - Background is black
  - Annotations (balloons, text, leader lines) are white
  - Drawing geometry (part outlines, hatching) is also white

This preprocessing improves:
  - OCR accuracy (white text on black background is ideal)
  - Leader line detection (clear white lines)
  - Circle detection (high-contrast balloon outlines)

The preprocessing is automatic — no manual intervention needed.
It handles both light-background and dark-background inputs.
"""

from __future__ import annotations

import cv2
import numpy as np


def preprocess_drawing(image: np.ndarray) -> np.ndarray:
    """Preprocess an engineering drawing for optimal detection.

    Applies adaptive thresholding to produce a clean binary image
    with white foreground on black background.

    Works on:
      - Light background drawings (standard prints)
      - Dark background drawings (already inverted)
      - Photos of drawings (uneven lighting)
      - Scanned documents

    Returns a BGR image (3-channel) with binary pixel values
    suitable for OCR and CV processing.
    """
    # Convert to grayscale
    if len(image.shape) == 3:
        gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    else:
        gray = image.copy()

    # Detect if the image is already inverted (dark background)
    mean_val = float(np.mean(gray))
    if mean_val < 128:
        # Already dark background — just threshold normally
        _, binary = cv2.threshold(gray, 0, 255,
                                  cv2.THRESH_BINARY + cv2.THRESH_OTSU)
    else:
        # Light background — use adaptive threshold + invert
        # Adaptive handles uneven lighting from photos/scans
        binary = cv2.adaptiveThreshold(
            gray, 255,
            cv2.ADAPTIVE_THRESH_GAUSSIAN_C,
            cv2.THRESH_BINARY_INV,
            21, 10,
        )

    # ── Noise removal ──────────────────────────────────────────
    # 1. Remove tiny noise dots (salt noise from scanning/photos)
    kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (2, 2))
    binary = cv2.morphologyEx(binary, cv2.MORPH_OPEN, kernel)

    # 2. Remove small connected components (noise blobs)
    # Only keep components larger than 20 pixels
    num_labels, labels, stats, _ = cv2.connectedComponentsWithStats(
        binary, connectivity=8
    )
    keep_labels = stats[:, cv2.CC_STAT_AREA] >= 20
    keep_labels[0] = False  # background
    binary = (keep_labels[labels].astype(np.uint8)) * 255

    # 3. Close small gaps in lines (broken strokes from scanning)
    close_kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3))
    binary = cv2.morphologyEx(binary, cv2.MORPH_CLOSE, close_kernel)

    # Convert back to BGR for pipeline compatibility
    return cv2.cvtColor(binary, cv2.COLOR_GRAY2BGR)
