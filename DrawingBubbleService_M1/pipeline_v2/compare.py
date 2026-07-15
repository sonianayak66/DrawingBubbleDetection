"""
pipeline_v2.compare — measurement-only v1 vs v2 comparison.

Runs the same image set through the legacy v1 detector and the v2
pipeline, collects comparable counts, and writes a report bucketing
images into likely improvements / likely regressions.

This is a PROXY comparison: without per-image ground truth it compares
output *counts* (bubbles detected, dimensions assigned), not correctness.
For ground-truth-scored accuracy on the 8 reference cases, use the
separate compare_v1_v2.py at the service root.

Usage:
    python -m pipeline_v2.compare --images DrawingBubble \\
        --debug-dir _pipeline_v2_debug/compare_v1_v2
"""
from __future__ import annotations

import argparse
import csv
import json
import os
import sys
import time
from pathlib import Path
from typing import Any, Dict, List, Optional

import cv2

from .failure_analyzer import build_failure_report, build_metrics
from .runner import run_pipeline

IMAGE_EXTS = {".png", ".jpg", ".jpeg", ".bmp", ".tif", ".tiff"}


def _collect_images(path: Path) -> List[Path]:
    if path.is_file():
        return [path] if path.suffix.lower() in IMAGE_EXTS else []
    images: List[Path] = []
    for ext in sorted(IMAGE_EXTS):
        images.extend(path.rglob(f"*{ext}"))
    return sorted(set(images))


# ── v1 adapter ──────────────────────────────────────────────────────────────
# v1 lives one directory up (detector.py). Import it lazily and behind an
# adapter so the v2 package's normal import graph stays clean.

def _load_v1():
    root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    if root not in sys.path:
        sys.path.insert(0, root)
    from detector import BubbleDetector, DetectionConfig  # noqa: E402

    cfg = DetectionConfig(
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
    return BubbleDetector(cfg)


def _run_v1(detector, image_path: Path) -> Dict[str, Any]:
    img = cv2.imread(str(image_path))
    if img is None:
        return {"error": "cannot_load_image", "bubble_count": 0, "assignment_count": 0, "ms": 0.0}
    t0 = time.perf_counter()
    try:
        bubbles, _ = detector.detect_from_array(img)
    except Exception as exc:  # record and continue
        return {"error": f"v1_failed: {exc}", "bubble_count": 0, "assignment_count": 0, "ms": 0.0}
    ms = (time.perf_counter() - t0) * 1000.0
    assigned = sum(
        1 for b in bubbles
        if b.dimension and b.dimension != "NO_DIMENSION"
    )
    return {
        "error": "",
        "bubble_count": len(bubbles),
        "assignment_count": assigned,
        "ms": round(ms, 1),
    }


def _run_v2(image_path: Path, debug_dir: Path) -> Dict[str, Any]:
    t0 = time.perf_counter()
    try:
        result = run_pipeline(image_path, debug_dir)
    except Exception as exc:
        return {"error": f"v2_failed: {exc}"}
    ms = (time.perf_counter() - t0) * 1000.0
    metrics = build_metrics(result, ms)
    report = build_failure_report(result)
    top = report.get("summary", {}).get("top_failure_reasons", [])
    top_reason = top[0].get("reason", "") if top else ""
    return {
        "error": "",
        "bubble_count": metrics["detected_balloon_count"],
        "assignment_count": metrics["assignment_count"],
        "leader_found_count": metrics["leader_found_count"],
        "color_component_found_count": metrics["color_component_found_count"],
        "weak_color_component_found_count": metrics["weak_color_component_found_count"],
        "grayscale_hough_found_count": metrics["grayscale_hough_found_count"],
        "ambiguous_leader_count": metrics["ambiguous_leader_count"],
        "low_confidence_leader_count": metrics["low_confidence_leader_count"],
        "no_leader_count": metrics["no_leader_count"],
        "ms": round(ms, 1),
        "top_failure_reason": top_reason,
    }


def _classify(v1: Dict[str, Any], v2: Dict[str, Any]) -> Dict[str, Any]:
    """Proxy regression / improvement rules (no ground truth)."""
    v1_assign = int(v1.get("assignment_count", 0) or 0)
    v2_assign = int(v2.get("assignment_count", 0) or 0)
    v1_bub = int(v1.get("bubble_count", 0) or 0)
    v2_bub = int(v2.get("bubble_count", 0) or 0)
    ambiguous = int(v2.get("ambiguous_leader_count", 0) or 0)
    no_leader = int(v2.get("no_leader_count", 0) or 0)

    reasons_reg: List[str] = []
    reasons_imp: List[str] = []

    if v2_assign < v1_assign:
        reasons_reg.append(f"v2_fewer_assignments ({v2_assign} < {v1_assign})")
    if v1_bub > 0 and v2_bub < 0.6 * v1_bub:
        reasons_reg.append(f"v2_much_fewer_bubbles ({v2_bub} vs {v1_bub})")
    if (ambiguous + no_leader) > 0 and v1_assign > v2_assign:
        reasons_reg.append(
            f"v2_unresolved_leaders ({ambiguous} ambiguous, {no_leader} none) "
            f"where v1 assigned more"
        )

    if v2_assign > v1_assign:
        reasons_imp.append(f"v2_more_assignments ({v2_assign} > {v1_assign})")
    if v1_assign == 0 and v2_assign > 0:
        reasons_imp.append("v2_assigned_where_v1_empty")

    likely_regression = bool(reasons_reg) and not (v2_assign > v1_assign)
    likely_improvement = bool(reasons_imp)
    return {
        "likely_regression": likely_regression,
        "likely_improvement": likely_improvement,
        "regression_reasons": reasons_reg,
        "improvement_reasons": reasons_imp,
    }


def compare(images_path: str | Path, debug_dir: str | Path) -> Dict[str, Any]:
    images_path = Path(images_path)
    debug_dir = Path(debug_dir)
    debug_dir.mkdir(parents=True, exist_ok=True)

    images = _collect_images(images_path)
    detector = _load_v1()

    rows: List[Dict[str, Any]] = []
    regressions: List[Dict[str, Any]] = []
    improvements: List[Dict[str, Any]] = []

    for image_path in images:
        v1 = _run_v1(detector, image_path)
        v2 = _run_v2(image_path, debug_dir / image_path.stem)

        if v2.get("error"):
            # v2 crashed — record minimal row and continue.
            rows.append({
                "image_name": image_path.name,
                "v1_bubble_count": v1.get("bubble_count", 0),
                "v2_bubble_count": 0,
                "v1_assignment_count": v1.get("assignment_count", 0),
                "v2_assignment_count": 0,
                "v2_leader_found_count": 0,
                "v2_color_component_found_count": 0,
                "v2_weak_color_component_found_count": 0,
                "v2_grayscale_hough_found_count": 0,
                "v2_ambiguous_leader_count": 0,
                "v2_low_confidence_leader_count": 0,
                "v2_no_leader_count": 0,
                "v1_processing_time_ms": v1.get("ms", 0.0),
                "v2_processing_time_ms": 0.0,
                "delta_bubble_count": -int(v1.get("bubble_count", 0) or 0),
                "delta_assignment_count": -int(v1.get("assignment_count", 0) or 0),
                "likely_regression": True,
                "likely_improvement": False,
                "top_v2_failure_reason": v2.get("error", ""),
                "error": v2.get("error", ""),
            })
            continue

        cls = _classify(v1, v2)
        delta_bub = int(v2["bubble_count"]) - int(v1.get("bubble_count", 0) or 0)
        delta_assign = int(v2["assignment_count"]) - int(v1.get("assignment_count", 0) or 0)

        row = {
            "image_name": image_path.name,
            "v1_bubble_count": v1.get("bubble_count", 0),
            "v2_bubble_count": v2["bubble_count"],
            "v1_assignment_count": v1.get("assignment_count", 0),
            "v2_assignment_count": v2["assignment_count"],
            "v2_leader_found_count": v2["leader_found_count"],
            "v2_color_component_found_count": v2["color_component_found_count"],
            "v2_weak_color_component_found_count": v2["weak_color_component_found_count"],
            "v2_grayscale_hough_found_count": v2["grayscale_hough_found_count"],
            "v2_ambiguous_leader_count": v2["ambiguous_leader_count"],
            "v2_low_confidence_leader_count": v2["low_confidence_leader_count"],
            "v2_no_leader_count": v2["no_leader_count"],
            "v1_processing_time_ms": v1.get("ms", 0.0),
            "v2_processing_time_ms": v2["ms"],
            "delta_bubble_count": delta_bub,
            "delta_assignment_count": delta_assign,
            "likely_regression": cls["likely_regression"],
            "likely_improvement": cls["likely_improvement"],
            "top_v2_failure_reason": v2.get("top_failure_reason", ""),
            "error": v1.get("error", ""),
        }
        rows.append(row)

        if cls["likely_regression"]:
            regressions.append({
                "image_name": image_path.name,
                "delta_assignment_count": delta_assign,
                "reasons": cls["regression_reasons"],
                "top_v2_failure_reason": v2.get("top_failure_reason", ""),
            })
        if cls["likely_improvement"]:
            improvements.append({
                "image_name": image_path.name,
                "delta_assignment_count": delta_assign,
                "reasons": cls["improvement_reasons"],
            })

    columns = [
        "image_name",
        "v1_bubble_count", "v2_bubble_count",
        "v1_assignment_count", "v2_assignment_count",
        "v2_leader_found_count",
        "v2_color_component_found_count",
        "v2_weak_color_component_found_count",
        "v2_grayscale_hough_found_count",
        "v2_ambiguous_leader_count",
        "v2_low_confidence_leader_count",
        "v2_no_leader_count",
        "v1_processing_time_ms", "v2_processing_time_ms",
        "delta_bubble_count", "delta_assignment_count",
        "likely_regression", "likely_improvement",
        "top_v2_failure_reason",
    ]
    summary_csv = debug_dir / "comparison_summary.csv"
    with open(summary_csv, "w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=columns, extrasaction="ignore")
        writer.writeheader()
        writer.writerows(rows)

    def _sum(key: str) -> int:
        return sum(int(r.get(key, 0) or 0) for r in rows)

    aggregate = {
        "proxy_metrics": True,
        "note": "Count-based proxy comparison. Ground-truth accuracy: see compare_v1_v2.py.",
        "image_count": len(rows),
        "v1_total_bubbles": _sum("v1_bubble_count"),
        "v2_total_bubbles": _sum("v2_bubble_count"),
        "v1_total_assignments": _sum("v1_assignment_count"),
        "v2_total_assignments": _sum("v2_assignment_count"),
        "total_delta_assignments": _sum("v2_assignment_count") - _sum("v1_assignment_count"),
        "likely_regression_count": sum(1 for r in rows if r.get("likely_regression")),
        "likely_improvement_count": sum(1 for r in rows if r.get("likely_improvement")),
        "average_v1_processing_time_ms": round(
            sum(float(r.get("v1_processing_time_ms", 0.0) or 0.0) for r in rows) / max(len(rows), 1), 2),
        "average_v2_processing_time_ms": round(
            sum(float(r.get("v2_processing_time_ms", 0.0) or 0.0) for r in rows) / max(len(rows), 1), 2),
        "v2_total_color_component_found": _sum("v2_color_component_found_count"),
        "v2_total_weak_color_component_found": _sum("v2_weak_color_component_found_count"),
        "v2_total_grayscale_hough_found": _sum("v2_grayscale_hough_found_count"),
        "v2_total_ambiguous_leaders": _sum("v2_ambiguous_leader_count"),
        "v2_total_low_confidence_leaders": _sum("v2_low_confidence_leader_count"),
        "v2_total_no_leaders": _sum("v2_no_leader_count"),
    }

    with open(debug_dir / "comparison_aggregate.json", "w", encoding="utf-8") as f:
        json.dump(aggregate, f, indent=2)
    regressions.sort(key=lambda r: r["delta_assignment_count"])
    improvements.sort(key=lambda r: r["delta_assignment_count"], reverse=True)
    with open(debug_dir / "regressions.json", "w", encoding="utf-8") as f:
        json.dump(regressions, f, indent=2)
    with open(debug_dir / "improvements.json", "w", encoding="utf-8") as f:
        json.dump(improvements, f, indent=2)

    return {"aggregate": aggregate, "regressions": regressions, "improvements": improvements}


def main() -> None:
    sys.stdout.reconfigure(encoding="utf-8")
    parser = argparse.ArgumentParser(description="Measurement-only v1 vs v2 comparison.")
    parser.add_argument("--images", required=True, help="Image file or folder")
    parser.add_argument("--debug-dir", required=True, help="Output directory (gitignored)")
    args = parser.parse_args()

    out = compare(args.images, args.debug_dir)
    agg = out["aggregate"]
    regressions = out["regressions"]
    improvements = out["improvements"]

    print("=== v1 vs v2 (PROXY: count-based, not correctness) ===")
    print(f"images compared        : {agg['image_count']}")
    print(f"likely improvements    : {agg['likely_improvement_count']}")
    print(f"likely regressions     : {agg['likely_regression_count']}")
    print(f"v1 total assignments   : {agg['v1_total_assignments']}")
    print(f"v2 total assignments   : {agg['v2_total_assignments']}")
    print(f"total delta assignments: {agg['total_delta_assignments']}")
    print(f"avg time v1/v2 (ms)    : {agg['average_v1_processing_time_ms']} / {agg['average_v2_processing_time_ms']}")
    print()
    print("top 5 regression images (most negative assignment delta):")
    for r in regressions[:5]:
        print(f"  {r['image_name']:<28} delta={r['delta_assignment_count']:>4}  {r.get('top_v2_failure_reason','')}")
    print()
    print("top 5 improvement images:")
    if not improvements:
        print("  (none)")
    for r in improvements[:5]:
        print(f"  {r['image_name']:<28} delta={r['delta_assignment_count']:>4}")


if __name__ == "__main__":
    main()
