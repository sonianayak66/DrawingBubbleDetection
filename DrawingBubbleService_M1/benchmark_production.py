"""Production benchmark runner for the offline drawing-bubble detector.

This script is intentionally separate from detector.py. It may read expected
IDs and dimensions from benchmark_cases.json, but only after detection has
finished. The detector must never read benchmark expected values.

Examples:
    python benchmark_production.py --cases ui_screenshot_1png dense_drawing_3png
    python benchmark_production.py --all --out _pipeline_v2_debug/benchmark_latest
"""
from __future__ import annotations

import argparse
import csv
import json
import logging
import os
import re
import sys
import time
from pathlib import Path
from typing import Any, Dict, Iterable, List, Tuple

import cv2

sys.stdout.reconfigure(encoding="utf-8")

ROOT = Path(__file__).resolve().parent
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from detector import BubbleDetector, DetectionConfig  # noqa: E402
from tiny_digit_recovery import recover_tiny_digits  # noqa: E402
from v1_unified_view import render_unified_view_v1  # noqa: E402


STEP_DONE_RE = re.compile(r"^\[STEP\]\s+done:\s+(?P<name>.*?)\s+\|\s+ms=(?P<ms>[0-9.]+)")
STEP_SKIP_RE = re.compile(r"^\[STEP\]\s+skip:\s+(?P<name>.*?)(?:\s+\|\s+reason=(?P<reason>.*))?$")


class StepTimingHandler(logging.Handler):
    """Collect detector step timings without changing detector code."""

    def __init__(self) -> None:
        super().__init__(level=logging.INFO)
        self.timings: Dict[str, float] = {}
        self.skipped: Dict[str, str] = {}

    def emit(self, record: logging.LogRecord) -> None:
        message = record.getMessage()
        done = STEP_DONE_RE.match(message)
        if done:
            self.timings[done.group("name").strip()] = float(done.group("ms"))
            return
        skipped = STEP_SKIP_RE.match(message)
        if skipped:
            self.skipped[skipped.group("name").strip()] = (skipped.group("reason") or "").strip()


def _norm(text: str) -> str:
    if not text or text == "NO_DIMENSION":
        return ""
    t = str(text).strip().upper()
    t = re.sub(r"\s+", " ", t)
    t = (
        t.replace(",", ".")
         .replace("Ø", "DIA")
         .replace("Ų", "DIA")
         .replace("Φ", "DIA")
         .replace("⌀", "DIA")
         .replace("×", "X")
         .replace("°", "DEG")
         .replace("±", "+/-")
    )
    t = re.sub(r"\s*MM$", "", t)
    t = re.sub(r"\s*/\s*", "/", t)
    t = re.sub(r"\s*X\s*", "X", t)
    t = re.sub(r"\s+", "", t)
    t = re.sub(r"^DIA0(?=\.)", "DIA", t)
    t = re.sub(r"^0(?=\.)", "", t)
    t = re.sub(r"\.0$", "", t)
    return t


def _value_matches(detected: str, expected: str) -> bool:
    d = _norm(detected)
    e = _norm(expected)
    if not d or not e:
        return False
    if d == e:
        return True
    d_noparen = re.sub(r"[()]", "", d)
    e_noparen = re.sub(r"[()]", "", e)
    if d_noparen == e_noparen:
        return True
    d_no_dia = re.sub(r"^DIA", "", d_noparen)
    e_no_dia = re.sub(r"^DIA", "", e_noparen)
    if d_no_dia and d_no_dia == e_no_dia:
        return True
    if len(e) >= 3 and e in d:
        return True
    if re.fullmatch(r"\d+", d_no_dia) and e_no_dia.startswith(d_no_dia + "."):
        return True
    return False


def _load_manifest(path: Path) -> Dict[str, Any]:
    with path.open(encoding="utf-8") as f:
        return json.load(f)


def _select_cases(cases: List[Dict[str, Any]], names: List[str], run_all: bool) -> List[Dict[str, Any]]:
    if run_all:
        return cases
    if not names:
        scored = [
            c for c in cases
            if c.get("expected_bubbles") or c.get("expected_dimensions")
        ]
        return scored or cases[:1]
    wanted = set(names)
    selected = [c for c in cases if c.get("name") in wanted]
    missing = sorted(wanted - {c.get("name") for c in selected})
    if missing:
        raise SystemExit(f"Unknown benchmark case(s): {', '.join(missing)}")
    return selected


def _make_detector(args: argparse.Namespace) -> BubbleDetector:
    return BubbleDetector(DetectionConfig(
        ocr_scale=max(0, int(args.ocr_scale or 0)),
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
        max_edge_blob_recovery_ocr=(
            int(args.max_edge_blob_ocr)
            if args.max_edge_blob_ocr is not None
            else DetectionConfig.max_edge_blob_recovery_ocr
        ),
        enable_rotated_fullpage_ocr=args.rotated_ocr,
        max_targeted_endpoint_ocr=(
            int(args.max_targeted_endpoint_ocr)
            if args.max_targeted_endpoint_ocr is not None
            else DetectionConfig.max_targeted_endpoint_ocr
        ),
        enable_targeted_endpoint_ocr=args.rescue_ocr,
        enable_realesrgan_enhancement=args.realesrgan,
    ))


def _bubble_map(bubbles: Iterable[Any]) -> Dict[str, str]:
    out: Dict[str, str] = {}
    for b in bubbles:
        bid = str(getattr(b, "bubble_number", "")).strip()
        if not bid:
            continue
        out[bid] = str(getattr(b, "dimension", "") or "")
    return out


def _bubble_instances(bubbles: Iterable[Any]) -> List[Dict[str, Any]]:
    instances: List[Dict[str, Any]] = []
    for idx, b in enumerate(bubbles):
        bid = str(getattr(b, "bubble_number", "")).strip()
        if not bid:
            continue
        dim = str(getattr(b, "dimension", "") or "")
        instances.append({
            "instance_index": idx,
            "bubble": bid,
            "dimension": dim,
            "assigned": bool(dim and dim != "NO_DIMENSION"),
            "confidence": round(float(getattr(b, "confidence", 0.0) or 0.0), 4),
            "needs_review": bool(getattr(b, "needs_review", False)),
            "review_reason": str(getattr(b, "review_reason", "") or ""),
            "x": int(round(float(getattr(b, "x", 0) or 0))),
            "y": int(round(float(getattr(b, "y", 0) or 0))),
            "radius": int(round(float(getattr(b, "radius", 0) or 0))),
        })
    return instances


def _duplicate_ids(instances: Iterable[Dict[str, Any]]) -> Dict[str, int]:
    counts: Dict[str, int] = {}
    for item in instances:
        bid = str(item.get("bubble") or "").strip()
        if not bid:
            continue
        counts[bid] = counts.get(bid, 0) + 1
    return {bid: count for bid, count in sorted(counts.items()) if count > 1}


def _top_step_timings(timings: Dict[str, float], limit: int = 5) -> List[Dict[str, Any]]:
    return [
        {"step": name, "ms": round(ms, 1)}
        for name, ms in sorted(timings.items(), key=lambda item: item[1], reverse=True)[:limit]
    ]


def _bubble_evidence(detector: BubbleDetector, bubbles: Iterable[Any]) -> List[Dict[str, Any]]:
    image = getattr(detector, "image", None)
    if image is None:
        return []
    try:
        mask = detector._annotation_hsv_mask(image)
    except Exception:
        mask = None

    traces_by_index = getattr(detector, "_seed_traces_by_index", {}) or {}
    legacy_traces = getattr(detector, "_seed_traces", {}) or {}
    details: List[Dict[str, Any]] = []
    for idx, b in enumerate(bubbles):
        bid = str(getattr(b, "bubble_number", "")).strip()
        if not bid:
            continue
        cx = int(round(float(getattr(b, "x", 0))))
        cy = int(round(float(getattr(b, "y", 0))))
        radius = max(1, int(round(float(getattr(b, "radius", 1)))))
        try:
            evidence = float(detector._compute_bubble_evidence(image, cx, cy, radius))
        except Exception:
            evidence = None
        try:
            rim_score = (
                float(detector._annotation_ring_score(mask, cx, cy, radius))
                if mask is not None else None
            )
        except Exception:
            rim_score = None
        trace = traces_by_index.get(idx) or legacy_traces.get(bid)
        trace_path = (trace or {}).get("path") or []
        quality = (trace or {}).get("quality")
        if hasattr(quality, "to_dict"):
            quality_detail = quality.to_dict()
        elif isinstance(quality, dict):
            quality_detail = quality
        else:
            quality_detail = None
        details.append({
            "bubble": bid,
            "dimension": str(getattr(b, "dimension", "") or ""),
            "x": cx,
            "y": cy,
            "radius": radius,
            "confidence": round(float(getattr(b, "confidence", 0.0) or 0.0), 3),
            "needs_review": bool(getattr(b, "needs_review", False)),
            "review_reason": str(getattr(b, "review_reason", "") or ""),
            "rim_score": round(rim_score, 3) if rim_score is not None else None,
            "bubble_evidence": round(evidence, 3) if evidence is not None else None,
            "trace_points": len(trace_path),
            "trace_quality": quality_detail,
            "leader_first_status": (trace or {}).get("leader_first_status"),
            "leader_first_quality_threshold": (trace or {}).get("leader_first_quality_threshold"),
            "leader_first_candidates": (trace or {}).get("leader_first_candidates", []),
            "endpoint_ocr_status": (trace or {}).get("endpoint_ocr_status"),
            "endpoint_ocr_text": (trace or {}).get("endpoint_ocr_text"),
            "endpoint_ocr_votes": (trace or {}).get("endpoint_ocr_votes"),
        })
    return details


def _score_case(case: Dict[str, Any], detected: Dict[str, str]) -> Dict[str, Any]:
    expected_bubbles = [str(x) for x in case.get("expected_bubbles") or []]
    expected_dimensions = {
        str(k): str(v) for k, v in (case.get("expected_dimensions") or {}).items()
    }
    expected_ids = sorted(set(expected_bubbles) | set(expected_dimensions.keys()))
    detected_ids = sorted(detected.keys())

    missed = [bid for bid in expected_ids if bid not in detected]
    if case.get("exact_bubbles"):
        spurious = [bid for bid in detected_ids if bid not in expected_ids]
    else:
        spurious = []

    wrong_values: List[Tuple[str, str, str]] = []
    value_checked = 0
    value_correct = 0
    for bid, expected in expected_dimensions.items():
        value_checked += 1
        got = detected.get(bid, "")
        if _value_matches(got, expected):
            value_correct += 1
        else:
            wrong_values.append((bid, expected, got))

    recall = (
        (len(expected_ids) - len(missed)) / len(expected_ids)
        if expected_ids else None
    )
    value_accuracy = (
        value_correct / value_checked
        if value_checked else None
    )
    spurious_rate = (
        len(spurious) / max(1, len(expected_ids))
        if expected_ids and case.get("exact_bubbles") else None
    )
    return {
        "expected_ids": expected_ids,
        "detected_ids": detected_ids,
        "missed_ids": missed,
        "spurious_ids": spurious,
        "wrong_values": [
            {"bubble": bid, "expected": exp, "detected": got}
            for bid, exp, got in wrong_values
        ],
        "bubble_recall": recall,
        "value_accuracy": value_accuracy,
        "spurious_rate": spurious_rate,
        "scored": bool(expected_ids or expected_dimensions),
    }


def _write_outputs(
    out_dir: Path,
    case_name: str,
    detector: BubbleDetector,
    bubbles: List[Any],
    annotated: Any,
    *,
    skip_unified: bool = False,
) -> None:
    out_dir.mkdir(parents=True, exist_ok=True)
    if annotated is not None:
        cv2.imwrite(str(out_dir / f"{case_name}_annot.png"), annotated)
    if skip_unified:
        return

    work_img = detector.image
    if work_img is None:
        return
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
    cv2.imwrite(str(out_dir / f"{case_name}_unified.png"), unified)


def _write_labeling_worksheet(
    out_dir: Path,
    rows: List[Dict[str, Any]],
) -> None:
    lines = [
        "# Manual Labeling Worksheet",
        "",
        "Fill `expected_bubbles` and `expected_dimensions` in `benchmark_cases.json` after reviewing each image.",
        "Detector output below is only a hint for review, never ground truth.",
        "",
    ]
    for row in rows:
        if row.get("scored"):
            continue
        lines.append(f"## {row['name']}")
        lines.append("")
        lines.append(f"- Image: `{row['image_path']}`")
        lines.append(f"- Category: `{row.get('category', '')}`")
        lines.append(f"- Annotated: `{row['name']}_annot.png`")
        lines.append(f"- Unified: `{row['name']}_unified.png`")
        lines.append(f"- Current detected IDs: `{', '.join(row.get('detected_ids') or []) or 'none'}`")
        lines.append("")
        lines.append("Expected data to fill:")
        lines.append("")
        lines.append("```json")
        lines.append('"expected_bubbles": [],')
        lines.append('"expected_dimensions": {}')
        lines.append("```")
        lines.append("")
        if row.get("evidence"):
            lines.append("Current detector evidence:")
            lines.append("")
            for ev in row["evidence"]:
                lines.append(
                    f"- `{ev['bubble']}` at ({ev['x']},{ev['y']}) r={ev['radius']} "
                    f"dim=`{ev['dimension']}` rim={ev['rim_score']} "
                    f"evidence={ev['bubble_evidence']} reason=`{ev['review_reason']}`"
                )
            lines.append("")
    (out_dir / "labeling_worksheet.md").write_text(
        "\n".join(lines),
        encoding="utf-8",
    )


def _write_benchmark_readiness_report(
    out_dir: Path,
    rows: List[Dict[str, Any]],
    manifest_cases: List[Dict[str, Any]],
) -> None:
    scored = [r for r in rows if r.get("scored")]
    unlabeled = [r for r in rows if not r.get("scored")]
    failed = [
        r for r in scored
        if (r.get("missed_ids") or r.get("wrong_values") or r.get("spurious_ids"))
    ]
    run_names = {r.get("name") for r in rows}
    not_run = [c for c in manifest_cases if c.get("name") not in run_names]

    by_category: Dict[str, Dict[str, int]] = {}
    for row in rows:
        cat = row.get("category") or "uncategorized"
        bucket = by_category.setdefault(cat, {"total": 0, "scored": 0, "failed": 0})
        bucket["total"] += 1
        if row.get("scored"):
            bucket["scored"] += 1
        if row in failed:
            bucket["failed"] += 1

    lines = [
        "# Benchmark Readiness Report",
        "",
        "This report is generated from detector output only. Unlabeled cases are smoke coverage, not accuracy proof.",
        "",
        "## Summary",
        "",
        f"- Run cases: {len(rows)}",
        f"- Scored cases: {len(scored)}",
        f"- Unlabeled smoke cases: {len(unlabeled)}",
        f"- Scored cases with failures: {len(failed)}",
        f"- Manifest cases not run: {len(not_run)}",
        "",
        "## Category Coverage",
        "",
    ]
    for cat, stats in sorted(by_category.items()):
        lines.append(
            f"- `{cat}`: {stats['scored']}/{stats['total']} scored, "
            f"{stats['failed']} scored failing"
        )

    if failed:
        lines.extend(["", "## Failing Scored Cases", ""])
        for row in failed:
            lines.append(f"### {row['name']}")
            if row.get("missed_ids"):
                lines.append(f"- Missed bubbles: `{', '.join(row['missed_ids'])}`")
            if row.get("spurious_ids"):
                lines.append(f"- Spurious bubbles: `{', '.join(row['spurious_ids'])}`")
            for wrong in row.get("wrong_values") or []:
                lines.append(
                    f"- Wrong value for `{wrong['bubble']}`: "
                    f"expected `{wrong['expected']}`, got `{wrong['detected']}`"
                )
            lines.append("")

    if unlabeled:
        lines.extend(["", "## Label Next", ""])
        for row in unlabeled:
            assigned = len(row.get("assigned_instances") or [])
            detected = len(row.get("detected_instances") or [])
            lines.append(
                f"- `{row['name']}` ({row.get('category', '')}): "
                f"{detected} detected, {assigned} assigned. "
                f"Add `expected_bubbles` and `expected_dimensions`."
            )

    if not_run:
        lines.extend(["", "## Not Run / Missing", ""])
        for case in not_run:
            image_path = ROOT / case.get("image_path", "")
            status = "missing file" if not image_path.exists() else "not selected"
            lines.append(f"- `{case.get('name')}`: `{case.get('image_path')}` ({status})")

    lines.extend([
        "",
        "## Production Gate Recommendation",
        "",
        "- Keep clean scored acceptance separate from full-manifest readiness.",
        "- Promote a smoke case to scored only after manual labeling.",
        "- Do not accept a production change unless scored recall/value improve or stay flat across the full labeled set.",
    ])

    (out_dir / "benchmark_readiness.md").write_text(
        "\n".join(lines),
        encoding="utf-8",
    )


def _fmt_pct(value: Any) -> str:
    if value is None:
        return "n/a"
    return f"{float(value) * 100:.0f}%"


def main() -> int:
    parser = argparse.ArgumentParser(description="Run production benchmark for the detector.")
    parser.add_argument("--manifest", default="benchmark_cases.json")
    parser.add_argument("--out", default="_pipeline_v2_debug/production_benchmark")
    parser.add_argument("--cases", nargs="*", default=[])
    parser.add_argument("--all", action="store_true", help="Run every manifest case, including unscored cases.")
    parser.add_argument("--rescue-ocr", action="store_true", help="Enable targeted endpoint OCR.")
    parser.add_argument("--realesrgan", action="store_true", help="Enable Real-ESRGAN enhancement.")
    parser.add_argument("--ocr-scale", type=int, default=0,
                        help="Override detector OCR scale for benchmark experiments; 0 keeps auto scale.")
    parser.add_argument("--max-edge-blob-ocr", type=int, default=None,
                        help="Override edge-blob local OCR budget for benchmark experiments.")
    parser.add_argument("--rotated-ocr", action="store_true",
                        help="Enable rotated full-page OCR for benchmark experiments.")
    parser.add_argument("--max-targeted-endpoint-ocr", type=int, default=None,
                        help="Override targeted endpoint OCR crop budget for benchmark experiments.")
    parser.add_argument("--skip-unified", action="store_true",
                        help="Skip slow unified debug rendering; annotated images are still written.")
    parser.add_argument("--fail-on-threshold", action="store_true")
    args = parser.parse_args()

    manifest_path = ROOT / args.manifest
    manifest = _load_manifest(manifest_path)
    selected = _select_cases(manifest["cases"], args.cases, args.all)
    out_dir = ROOT / args.out
    out_dir.mkdir(parents=True, exist_ok=True)

    rows: List[Dict[str, Any]] = []
    print(f"{'case':<28}{'bub':>4}{'asg':>4}{'recall':>8}{'values':>8}{'spur':>8}{'ms/mp':>10}  detected", flush=True)
    print("-" * 112, flush=True)

    for case in selected:
        image_path = ROOT / case["image_path"]
        img = cv2.imread(str(image_path))
        if img is None:
            raise SystemExit(f"Cannot load image for case {case['name']}: {image_path}")

        detector = _make_detector(args)
        step_handler = StepTimingHandler()
        detector_logger = logging.getLogger("detector")
        detector_logger.addHandler(step_handler)
        detector_logger.setLevel(logging.INFO)
        t0 = time.perf_counter()
        try:
            bubbles, annotated = detector.detect_from_array(img)
        finally:
            detector_logger.removeHandler(step_handler)
        runtime_ms = (time.perf_counter() - t0) * 1000.0
        megapixels = (img.shape[0] * img.shape[1]) / 1_000_000.0
        runtime_ms_per_mp = runtime_ms / max(megapixels, 0.001)

        detected_instances = _bubble_instances(bubbles)
        duplicate_ids = _duplicate_ids(detected_instances)
        detected = _bubble_map(bubbles)
        evidence = _bubble_evidence(detector, bubbles)
        assigned = {
            bid: dim for bid, dim in detected.items()
            if dim and dim != "NO_DIMENSION"
        }
        assigned_instances = [
            item for item in detected_instances
            if item.get("assigned")
        ]
        score = _score_case(case, detected)
        _write_outputs(
            out_dir,
            case["name"],
            detector,
            bubbles,
            annotated,
            skip_unified=args.skip_unified,
        )

        row = {
            "name": case["name"],
            "image_path": case["image_path"],
            "category": case.get("category", ""),
            "runtime_ms": round(runtime_ms, 1),
            "runtime_ms_per_megapixel": round(runtime_ms_per_mp, 1),
            "detected": detected,
            "assigned": assigned,
            "detected_instances": detected_instances,
            "assigned_instances": assigned_instances,
            "duplicate_ids": duplicate_ids,
            "evidence": evidence,
            "step_timings_ms": {
                name: round(ms, 1)
                for name, ms in step_handler.timings.items()
            },
            "skipped_steps": step_handler.skipped,
            "top_steps": _top_step_timings(step_handler.timings),
            "bubble_count": len(detected_instances),
            "assigned_count": len(assigned_instances),
            **score,
        }
        rows.append(row)

        pairs = ", ".join(f"{k}={v}" for k, v in sorted(assigned.items()))
        print(
            f"{case['name']:<28}{len(detected_instances):>4}{len(assigned_instances):>4}"
            f"{_fmt_pct(score['bubble_recall']):>8}"
            f"{_fmt_pct(score['value_accuracy']):>8}"
            f"{_fmt_pct(score['spurious_rate']):>8}"
            f"{runtime_ms_per_mp:>10.0f}  {pairs[:80]}",
            flush=True,
        )

    summary = _summarize(rows, manifest.get("acceptance") or {})
    report = {
        "manifest": str(manifest_path),
        "options": {
            "realesrgan": bool(args.realesrgan),
            "rescue_ocr": bool(args.rescue_ocr),
            "skip_unified": bool(args.skip_unified),
        },
        "summary": summary,
        "cases": rows,
    }
    (out_dir / "benchmark_report.json").write_text(
        json.dumps(report, indent=2, ensure_ascii=False),
        encoding="utf-8",
    )
    _write_csv(out_dir / "benchmark_report.csv", rows)
    _write_labeling_worksheet(out_dir, rows)
    _write_benchmark_readiness_report(out_dir, rows, selected)

    print("-" * 112)
    print(
        "TOTAL "
        f"recall={_fmt_pct(summary['bubble_recall'])} "
        f"values={_fmt_pct(summary['value_accuracy'])} "
        f"spurious={_fmt_pct(summary['spurious_rate'])} "
        f"avg_ms_per_mp={summary['avg_runtime_ms_per_megapixel']:.0f} "
        f"scored_cases={summary['scored_cases']}/{summary['total_cases']}"
    )
    if summary["unlabeled_cases"]:
        print("Unlabeled cases:", ", ".join(summary["unlabeled_cases"]))

    if args.fail_on_threshold and not summary["passes_acceptance"]:
        print("Benchmark failed acceptance thresholds.")
        return 1
    return 0


def _summarize(rows: List[Dict[str, Any]], acceptance: Dict[str, Any]) -> Dict[str, Any]:
    scored = [r for r in rows if r.get("scored")]
    expected_total = sum(len(r["expected_ids"]) for r in scored)
    missed_total = sum(len(r["missed_ids"]) for r in scored)
    # Use per-case metric averages to avoid one large case hiding one small
    # case. This is intentionally conservative for production readiness.
    recalls = [r["bubble_recall"] for r in scored if r["bubble_recall"] is not None]
    values = [r["value_accuracy"] for r in scored if r["value_accuracy"] is not None]
    spurious = [r["spurious_rate"] for r in scored if r["spurious_rate"] is not None]
    avg_runtime = (
        sum(float(r["runtime_ms_per_megapixel"]) for r in rows) / len(rows)
        if rows else 0.0
    )
    bubble_recall = sum(recalls) / len(recalls) if recalls else None
    value_accuracy = sum(values) / len(values) if values else None
    spurious_rate = sum(spurious) / len(spurious) if spurious else None
    unlabeled = [
        r["name"] for r in rows
        if not r.get("scored")
    ]
    step_totals: Dict[str, float] = {}
    step_counts: Dict[str, int] = {}
    for row in rows:
        for step, ms in (row.get("step_timings_ms") or {}).items():
            step_totals[step] = step_totals.get(step, 0.0) + float(ms)
            step_counts[step] = step_counts.get(step, 0) + 1
    top_bottleneck_steps = [
        {
            "step": step,
            "total_ms": round(total, 1),
            "avg_ms": round(total / max(1, step_counts.get(step, 1)), 1),
            "cases": step_counts.get(step, 0),
        }
        for step, total in sorted(step_totals.items(), key=lambda item: item[1], reverse=True)[:10]
    ]
    passes = True
    if bubble_recall is not None:
        passes = passes and bubble_recall >= float(acceptance.get("min_bubble_recall", 0.0))
    if value_accuracy is not None:
        passes = passes and value_accuracy >= float(acceptance.get("min_value_accuracy", 0.0))
    if spurious_rate is not None:
        passes = passes and spurious_rate <= float(acceptance.get("max_spurious_rate", 1.0))
    passes = passes and avg_runtime <= float(acceptance.get("max_runtime_ms_per_megapixel", 10**12))
    return {
        "total_cases": len(rows),
        "scored_cases": len(scored),
        "unlabeled_cases": unlabeled,
        "expected_bubbles": expected_total,
        "missed_bubbles": missed_total,
        "wrong_value_count": sum(len(r.get("wrong_values", [])) for r in scored),
        "bubble_recall": bubble_recall,
        "value_accuracy": value_accuracy,
        "spurious_rate": spurious_rate,
        "avg_runtime_ms_per_megapixel": avg_runtime,
        "top_bottleneck_steps": top_bottleneck_steps,
        "passes_acceptance": passes,
    }


def _write_csv(path: Path, rows: List[Dict[str, Any]]) -> None:
    fields = [
        "name", "image_path", "category", "bubble_count", "assigned_count",
        "bubble_recall", "value_accuracy", "spurious_rate",
        "runtime_ms", "runtime_ms_per_megapixel",
        "missed_ids", "spurious_ids", "wrong_values", "detected",
        "detected_instances", "assigned_instances", "duplicate_ids", "evidence",
        "step_timings_ms", "top_steps",
    ]
    with path.open("w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=fields)
        writer.writeheader()
        for row in rows:
            writer.writerow({
                field: json.dumps(row.get(field), ensure_ascii=False)
                if isinstance(row.get(field), (dict, list))
                else row.get(field)
                for field in fields
            })


if __name__ == "__main__":
    raise SystemExit(main())
