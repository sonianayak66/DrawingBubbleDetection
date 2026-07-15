from __future__ import annotations

import argparse
import csv
import json
from pathlib import Path
from typing import Any, Dict, List

from .runner import run_pipeline


IMAGE_EXTS = {".png", ".jpg", ".jpeg", ".bmp", ".tif", ".tiff"}


def _collect_images(path: Path) -> List[Path]:
    if path.is_file():
        return [path] if path.suffix.lower() in IMAGE_EXTS else []
    images: List[Path] = []
    for ext in sorted(IMAGE_EXTS):
        images.extend(path.rglob(f"*{ext}"))
    return sorted(images)


def _read_json(path: Path) -> Dict[str, Any]:
    try:
        with open(path, encoding="utf-8") as f:
            return json.load(f)
    except Exception as exc:
        return {"errors": [f"could_not_read_{path.name}: {exc}"]}


def _safe_rate(num: float, den: float) -> float:
    return round(float(num) / den, 4) if den else 0.0


def evaluate(images_path: str | Path, debug_dir: str | Path) -> Dict[str, Any]:
    images_path = Path(images_path)
    debug_dir = Path(debug_dir)
    debug_dir.mkdir(parents=True, exist_ok=True)

    images = _collect_images(images_path)
    rows: List[Dict[str, Any]] = []
    metrics_list: List[Dict[str, Any]] = []

    for image_path in images:
        image_debug_dir = debug_dir / image_path.stem
        error = ""
        try:
            run_pipeline(image_path, image_debug_dir)
        except Exception as exc:
            error = str(exc)

        metrics_path = image_debug_dir / "metrics.json"
        report_path = image_debug_dir / "failure_report.json"
        metrics = _read_json(metrics_path) if metrics_path.exists() else {
            "image_name": str(image_path),
            "errors": [f"pipeline_error: {error}" if error else "metrics_missing"],
        }
        report = _read_json(report_path) if report_path.exists() else {}
        metrics_list.append(metrics)

        top_failure = ""
        top = report.get("summary", {}).get("top_failure_reasons", [])
        if top:
            top_failure = top[0].get("reason", "")
        row = {
            "image_name": image_path.name,
            "detected_balloon_count": metrics.get("detected_balloon_count", 0),
            "leader_found_count": metrics.get("leader_found_count", 0),
            "leader_found_rate": metrics.get("leader_found_rate", 0.0),
            "assignment_count": metrics.get("assignment_count", 0),
            "assignment_rate": metrics.get("assignment_rate", 0.0),
            "review_required_count": metrics.get("review_required_count", 0),
            "top_failure_reason": top_failure,
            "processing_time_ms": metrics.get("processing_time_ms", 0.0),
        }
        rows.append(row)

    summary_csv = debug_dir / "summary.csv"
    columns = [
        "image_name",
        "detected_balloon_count",
        "leader_found_count",
        "leader_found_rate",
        "assignment_count",
        "assignment_rate",
        "review_required_count",
        "top_failure_reason",
        "processing_time_ms",
    ]
    with open(summary_csv, "w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=columns)
        writer.writeheader()
        writer.writerows(rows)

    total_images = len(rows)
    total_balloons = sum(int(r.get("detected_balloon_count", 0) or 0) for r in rows)
    total_leaders = sum(int(r.get("leader_found_count", 0) or 0) for r in rows)
    total_assignments = sum(int(r.get("assignment_count", 0) or 0) for r in rows)
    total_reviews = sum(int(r.get("review_required_count", 0) or 0) for r in rows)
    total_time = sum(float(r.get("processing_time_ms", 0.0) or 0.0) for r in rows)

    def _sum_metric(key: str) -> int:
        return sum(int(m.get(key, 0) or 0) for m in metrics_list)

    aggregate = {
        "image_count": total_images,
        "detected_balloon_count": total_balloons,
        "leader_found_count": total_leaders,
        "leader_found_rate": _safe_rate(total_leaders, total_balloons),
        # Per-method leader breakdown — tells us how much of the found
        # rate is reliable colour tracing vs the weaker fallbacks.
        "color_component_found_count": _sum_metric("color_component_found_count"),
        "weak_color_component_found_count": _sum_metric("weak_color_component_found_count"),
        "grayscale_hough_found_count": _sum_metric("grayscale_hough_found_count"),
        "ambiguous_leader_count": _sum_metric("ambiguous_leader_count"),
        "low_confidence_leader_count": _sum_metric("low_confidence_leader_count"),
        "no_leader_count": _sum_metric("no_leader_count"),
        "assignment_count": total_assignments,
        "assignment_rate": _safe_rate(total_assignments, total_balloons),
        "review_required_count": total_reviews,
        "average_processing_time_ms": round(total_time / total_images, 2) if total_images else 0.0,
        "summary_csv": str(summary_csv),
    }
    with open(debug_dir / "aggregate_metrics.json", "w", encoding="utf-8") as f:
        json.dump(aggregate, f, indent=2)
    return aggregate


def main() -> None:
    parser = argparse.ArgumentParser(description="Evaluate pipeline_v2 over one image or a folder.")
    parser.add_argument("--images", required=True, help="Image file or folder")
    parser.add_argument("--debug-dir", required=True, help="Evaluation output directory")
    args = parser.parse_args()

    aggregate = evaluate(args.images, args.debug_dir)
    print(json.dumps(aggregate, indent=2))


if __name__ == "__main__":
    main()
