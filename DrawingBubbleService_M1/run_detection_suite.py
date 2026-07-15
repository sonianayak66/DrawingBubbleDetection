"""Run the detector on arbitrary image paths or glob patterns.

Examples:
    python run_detection_suite.py Images/*.jpeg
    python run_detection_suite.py path/to/image1.png path/to/image2.jpeg --out debug_run
"""
from __future__ import annotations

import argparse
import glob
import os
import sys
import time
from pathlib import Path

import cv2

sys.stdout.reconfigure(encoding="utf-8")
HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)

from detector import BubbleDetector, DetectionConfig  # noqa: E402
from tiny_digit_recovery import recover_tiny_digits  # noqa: E402
from v1_unified_view import render_unified_view_v1  # noqa: E402


def _expand_inputs(patterns: list[str]) -> list[Path]:
    paths: list[Path] = []
    seen: set[str] = set()
    for pattern in patterns:
        matches = glob.glob(pattern)
        if not matches and Path(pattern).exists():
            matches = [pattern]
        for match in matches:
            path = Path(match)
            if path.is_file():
                key = str(path.resolve())
                if key not in seen:
                    seen.add(key)
                    paths.append(path)
    return paths


def main() -> int:
    parser = argparse.ArgumentParser(description="Run drawing bubble detection on arbitrary images.")
    parser.add_argument("images", nargs="+", help="Image files or glob patterns.")
    parser.add_argument("--out", default="_pipeline_v2_debug/detection_suite",
                        help="Directory for annotated and unified debug images.")
    parser.add_argument("--realesrgan", action="store_true", help="Enable Real-ESRGAN enhancement.")
    parser.add_argument("--rescue-ocr", action="store_true", help="Enable targeted endpoint OCR.")
    args = parser.parse_args()

    images = _expand_inputs(args.images)
    if not images:
        print("No input images matched.")
        return 2

    out_dir = Path(args.out)
    out_dir.mkdir(parents=True, exist_ok=True)

    detector = BubbleDetector(DetectionConfig(
        ocr_scale=0,
        run_multi_scale_ocr=True,
        min_radius=0,
        max_radius=0,
        hough_param2=0,
        min_dist=0,
        enable_seed_trace_assignment=True,
        enable_image_linking=True,
        enable_heavy_path_disambiguation=False,
        enable_annotation=True,
        print_timing=False,
        enable_targeted_endpoint_ocr=args.rescue_ocr,
        enable_realesrgan_enhancement=args.realesrgan,
    ))

    print(f"{'image':<28}{'bub':>4}{'asg':>4}{'rev':>4}{'clut':>6}{'enh':>4}{'ms':>8}   bubble->dim", flush=True)
    print("-" * 98, flush=True)
    for path in images:
        img = cv2.imread(str(path))
        if img is None:
            print(f"{path.name:<28}  cannot load", flush=True)
            continue

        t0 = time.perf_counter()
        bubbles, annotated = detector.detect_from_array(img)
        ms = (time.perf_counter() - t0) * 1000.0

        assigned = [b for b in bubbles if b.dimension and b.dimension != "NO_DIMENSION"]
        review = [b for b in bubbles if b.needs_review]
        if annotated is not None:
            cv2.imwrite(str(out_dir / f"{path.stem}_annot.png"), annotated)

        work_img = detector.image if detector.image is not None else img
        recovered = recover_tiny_digits(detector, bubbles)
        if recovered:
            detector._norm_tokens.extend(recovered)

        prod_traces = getattr(detector, "_seed_traces", {}) or {}
        render_traces = getattr(detector, "_render_seed_traces", {}) or {}
        unified = render_unified_view_v1(
            work_img,
            bubbles,
            getattr(detector, "_norm_tokens", []) or [],
            {**prod_traces, **render_traces},
        )
        cv2.imwrite(str(out_dir / f"{path.stem}_unified.png"), unified)

        quality = getattr(detector, "_quality", None) or {}
        clutter = quality.get("clutter_score", 0)
        enhanced = "Y" if getattr(detector, "_was_upscaled", False) else "-"
        pairs = ", ".join(
            f"{b.bubble_number}={b.dimension}"
            for b in sorted(bubbles, key=lambda x: (len(x.bubble_number), x.bubble_number))
            if b.dimension and b.dimension != "NO_DIMENSION"
        )
        print(f"{path.name:<28}{len(bubbles):>4}{len(assigned):>4}{len(review):>4}"
              f"{clutter:>6.0f}{enhanced:>4}{ms:>8.0f}   {pairs[:110]}", flush=True)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
