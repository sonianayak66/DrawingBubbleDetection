"""
compare_v1_v2.py - Score the v1 detector and the v2 pipeline against optional
expected values in DrawingBubble/tests/regression_cases.json.

Cases without expected values are skipped, so the repo can keep reference image
metadata without storing balloon-to-dimension answers.

Metrics per case, computed identically for both pipelines:
  * id_recall      - fraction of expected bubble IDs that appear in the
                     pipeline's output (an ID + a non-empty dimension)
  * value_accuracy - fraction of expected IDs whose assigned dimension
                     matches the expected value under the shared matcher

The v1 detector returns bubbles each carrying a dimension; the v2
pipeline returns one assignment per balloon (balloon_id + dimension_text).
Both collapse to a {bubble_id: dimension} map and are scored the same way.

Usage:
    .venv\\Scripts\\python.exe compare_v1_v2.py
    .venv\\Scripts\\python.exe compare_v1_v2.py --json compare_out.json
"""
from __future__ import annotations

import argparse
import json
import os
import sys
import time
from pathlib import Path
from typing import Dict, Tuple

# Force UTF-8 stdout; Windows console defaults can choke on OCR symbols.
sys.stdout.reconfigure(encoding="utf-8")

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)

import cv2

from detector import BubbleDetector, DetectionConfig

# Reuse the regression harness's value matcher and case loader so v1 and v2
# are scored with byte-identical logic. Importing the test module is safe:
# it only sets up sys.path and defines functions at import time.
sys.path.insert(0, os.path.join(HERE, "DrawingBubble", "tests"))
from test_regression import _value_matches, _load_cases, PKG  # noqa: E402

from pipeline_v2.runner import run_pipeline  # noqa: E402


def _v1_config() -> DetectionConfig:
    # Mirror the exact config the regression harness uses for v1.
    return DetectionConfig(
        ocr_scale=0,
        run_multi_scale_ocr=True,
        min_radius=0,
        max_radius=0,
        hough_param2=0,
        min_dist=0,
        enable_seed_trace_assignment=True,
        enable_image_linking=True,
        enable_heavy_path_disambiguation=False,
        enable_annotation=False,
        print_timing=False,
        enable_targeted_endpoint_ocr=True,
    )


def _run_v1(detector: BubbleDetector, image_path: Path) -> Dict[str, str]:
    img = cv2.imread(str(image_path))
    if img is None:
        return {}
    bubbles, _ = detector.detect_from_array(img)
    out: Dict[str, str] = {}
    for b in bubbles:
        out[str(b.bubble_number)] = b.dimension
    return out


def _run_v2(image_path: Path, debug_root: Path) -> Dict[str, str]:
    debug_dir = debug_root / image_path.stem
    result = run_pipeline(image_path, debug_dir)
    out: Dict[str, str] = {}
    for a in result.assignments:
        # Keep the first non-empty dimension per balloon id. An unread
        # balloon falls back to its candidate_id (e.g. "b_001"), which
        # legitimately won't match any ground-truth integer id.
        if a.dimension_text and a.dimension_text != "NO_DIMENSION":
            out.setdefault(str(a.balloon_id), a.dimension_text)
    return out


def _score(detected: Dict[str, str], expected: Dict[str, str]) -> Tuple[int, int, int]:
    total = len(expected)
    found = sum(1 for bid in expected if bid in detected)
    correct = sum(
        1
        for bid, exp_val in expected.items()
        if bid in detected and _value_matches(detected[bid], exp_val)
    )
    return found, correct, total


def main() -> None:
    parser = argparse.ArgumentParser(description="Compare v1 detector vs v2 pipeline on the regression set.")
    parser.add_argument("--json", default=None, help="Optional path to write a JSON report")
    parser.add_argument("--debug-root", default="_pipeline_v2_debug/compare",
                        help="Where v2 writes its (gitignored) debug artifacts")
    args = parser.parse_args()

    cases = _load_cases()
    detector = BubbleDetector(_v1_config())
    debug_root = Path(args.debug_root)

    rows = []
    agg = {
        "v1_found": 0, "v1_correct": 0,
        "v2_found": 0, "v2_correct": 0,
        "total": 0,
    }

    header = (f"{'case':<22} {'exp':>4} | "
              f"{'v1_rec':>7} {'v1_val':>7} | "
              f"{'v2_rec':>7} {'v2_val':>7}")
    print(header)
    print("-" * len(header))

    for case in cases:
        image_path = PKG / case["image_path"]
        expected_raw = case.get("expected") or {}
        if not expected_raw:
            rows.append({
                "case": case["name"],
                "image": case["image_path"],
                "skipped": "no stored expected values",
            })
            print(f"{case['name']:<22} {'skip':>4} | "
                  f"{'-':>7} {'-':>7} | "
                  f"{'-':>7} {'-':>7}")
            continue

        expected = {str(k): v for k, v in expected_raw.items()}

        t0 = time.perf_counter()
        v1_map = _run_v1(detector, image_path)
        v1_ms = (time.perf_counter() - t0) * 1000.0

        t0 = time.perf_counter()
        v2_map = _run_v2(image_path, debug_root)
        v2_ms = (time.perf_counter() - t0) * 1000.0

        v1_found, v1_correct, total = _score(v1_map, expected)
        v2_found, v2_correct, _ = _score(v2_map, expected)

        agg["v1_found"] += v1_found
        agg["v1_correct"] += v1_correct
        agg["v2_found"] += v2_found
        agg["v2_correct"] += v2_correct
        agg["total"] += total

        rows.append({
            "case": case["name"],
            "image": case["image_path"],
            "expected": total,
            "v1": {"recall": v1_found, "value_correct": v1_correct, "ms": round(v1_ms, 1)},
            "v2": {"recall": v2_found, "value_correct": v2_correct, "ms": round(v2_ms, 1)},
        })

        print(f"{case['name']:<22} {total:>4} | "
              f"{v1_found:>3}/{total:<3} {v1_correct:>3}/{total:<3} | "
              f"{v2_found:>3}/{total:<3} {v2_correct:>3}/{total:<3}")

    t = agg["total"]
    print("-" * len(header))
    print(f"{'TOTAL':<22} {agg['total']:>4} | "
          f"{agg['v1_found']:>3}/{agg['total']:<3} {agg['v1_correct']:>3}/{agg['total']:<3} | "
          f"{agg['v2_found']:>3}/{agg['total']:<3} {agg['v2_correct']:>3}/{agg['total']:<3}")
    print()
    if not t:
        print("No comparable cases: regression_cases.json contains no stored expected values.")
    else:
        print(f"v1  recall={agg['v1_found']/t*100:.0f}%  value_accuracy={agg['v1_correct']/t*100:.0f}%")
        print(f"v2  recall={agg['v2_found']/t*100:.0f}%  value_accuracy={agg['v2_correct']/t*100:.0f}%")

    if args.json:
        def _ratio(n: int) -> float | None:
            return round(n / t, 4) if t else None

        report = {
            "aggregate": {
                "total_expected": agg["total"],
                "v1_recall": _ratio(agg["v1_found"]),
                "v1_value_accuracy": _ratio(agg["v1_correct"]),
                "v2_recall": _ratio(agg["v2_found"]),
                "v2_value_accuracy": _ratio(agg["v2_correct"]),
            },
            "cases": rows,
        }
        with open(args.json, "w", encoding="utf-8") as f:
            json.dump(report, f, indent=2)
        print(f"\nwrote {args.json}")


if __name__ == "__main__":
    main()
