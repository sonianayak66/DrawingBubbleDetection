"""
Temporary validation harness for new.jpeg (phone-photo-of-monitor input).

Drives the detector with screenshot normalisation + targeted OCR rescue
to see whether a local re-OCR pass can recover bubble #6's `9°(REF)`
value that the global OCR missed.
"""
from __future__ import annotations

import sys
from pathlib import Path

import logging

import cv2

logging.basicConfig(level=logging.INFO, format="%(message)s")

HERE = Path(__file__).resolve().parent
if str(HERE) not in sys.path:
    sys.path.insert(0, str(HERE))

from detector import BubbleDetector, DetectionConfig  # noqa: E402


def main() -> int:
    src = Path("C:/Users/Dell/Downloads/new.jpeg")
    out_dir = Path("C:/Users/Dell/source/repos/AdarshMPI/report_images")
    out_dir.mkdir(parents=True, exist_ok=True)

    img = cv2.imread(str(src))
    if img is None:
        print(f"[FAIL] could not load {src}")
        return 1

    cfg = DetectionConfig()
    cfg.enable_screenshot_normalization = True
    cfg.enable_targeted_endpoint_ocr = False

    det = BubbleDetector(cfg)
    bubbles, annotated = det.detect_from_array(img.copy())

    import numpy as np
    orig = det._original_image

    def chroma_at(x, y, window=30):
        h, w = orig.shape[:2]
        x1 = max(0, x - window); y1 = max(0, y - window)
        x2 = min(w, x + window); y2 = min(h, y + window)
        patch = orig[y1:y2, x1:x2]
        bb = patch[:, :, 0].astype(np.float32)
        gg = patch[:, :, 1].astype(np.float32)
        rr = patch[:, :, 2].astype(np.float32)
        chroma = (np.abs(rr - gg) + np.abs(gg - bb) + np.abs(rr - bb)) / 3
        return float(chroma.mean())

    print(f"Final: {len(bubbles)} bubbles")
    for b in bubbles:
        rev = " [REVIEW]" if getattr(b, "needs_review", False) else ""
        reason = f" ({b.review_reason})" if getattr(b, "review_reason", "") else ""
        c = chroma_at(int(b.x), int(b.y))
        print(
            f"  #{b.bubble_number:<5} dim={b.dimension!r:<28} "
            f"conf={b.confidence:.2f} pos=({int(b.x)},{int(b.y)}) "
            f"r={int(b.radius)} chroma={c:.2f}{rev}{reason}"
        )

    if annotated is not None:
        out_path = out_dir / "new_final_v12.jpg"
        cv2.imwrite(str(out_path), annotated)
        print(f"\nSaved: {out_path}")
    else:
        print("\n[warn] no annotated image returned")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
