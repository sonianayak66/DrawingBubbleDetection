from __future__ import annotations

import argparse
import time
from pathlib import Path

import cv2

from .annotation_segmenter import segment_annotation_color
from .associator import associate
from .balloon_detector import detect_balloons
from .balloon_ocr import read_balloon_ids
from .contracts import PipelineV2Result
from .debug_writer import (
    DebugWriter,
    overlay_assignments,
    overlay_balloons,
    overlay_components,
    overlay_leader_candidates,
    overlay_leaders,
    overlay_schema_view,
    overlay_suppressed_regions,
    overlay_text_candidates,
    overlay_unified_view,
)
from .dimension_ocr import detect_dimension_text
from .failure_analyzer import (
    build_failure_report,
    build_leader_candidates_report,
    build_metrics,
    overlay_failure_analysis,
)
from .image_enhancer import enhance_image
from .leader_tracer import trace_leaders
from .region_suppressor import suppress_table_regions


def run_pipeline(image_path: str | Path, debug_dir: str | Path | None = None) -> PipelineV2Result:
    t_start = time.perf_counter()
    image_path = Path(image_path)
    img = cv2.imread(str(image_path))
    if img is None:
        raise ValueError(f"Cannot load image: {image_path}")

    if debug_dir is None:
        debug_dir = Path("debug") / image_path.stem
    writer = DebugWriter(debug_dir)

    writer.write_image("01_input.png", img)

    enhanced, enhancement = enhance_image(img)
    writer.write_image("02_enhanced.png", enhanced)

    mask, segmentation = segment_annotation_color(enhanced)
    writer.write_image("03_magenta_mask.png", mask)
    writer.write_image("04_magenta_components.png", overlay_components(mask))

    balloons = detect_balloons(mask)
    writer.write_image("05_balloons_overlay.png", overlay_balloons(enhanced, balloons))

    crops_dir = Path(debug_dir) / "06_balloon_crops"
    crops_dir.mkdir(parents=True, exist_ok=True)
    for balloon in balloons:
        x1, y1, x2, y2 = balloon.bbox
        crop = enhanced[y1:y2, x1:x2]
        if crop.size:
            cv2.imwrite(str(crops_dir / f"{balloon.candidate_id}.png"), crop)

    balloon_ocr = read_balloon_ids(enhanced, mask, balloons)

    leaders = trace_leaders(mask, balloons, image=enhanced)
    writer.write_image("07_leaders_overlay.png", overlay_leaders(enhanced, leaders))

    text_candidates = detect_dimension_text(enhanced)
    text_candidates, suppressed_regions = suppress_table_regions(enhanced, text_candidates)
    writer.write_image("08_ocr_boxes.png", overlay_text_candidates(enhanced, text_candidates))
    writer.write_image("09_suppressed_regions.png", overlay_suppressed_regions(enhanced, suppressed_regions))

    assignments = associate(balloons, balloon_ocr, leaders, text_candidates)
    writer.write_image("10_association_candidates.png", overlay_text_candidates(overlay_leaders(enhanced, leaders), text_candidates))
    writer.write_image("11_final_assignments.png", overlay_assignments(enhanced, balloons, leaders, assignments))

    result = PipelineV2Result(
        image=str(image_path),
        enhancement=enhancement,
        segmentation=segmentation,
        balloons=balloons,
        balloon_ocr=balloon_ocr,
        leaders=leaders,
        text_candidates=text_candidates,
        assignments=assignments,
    )
    writer.write_result(result)

    processing_time_ms = (time.perf_counter() - t_start) * 1000.0
    metrics = build_metrics(result, processing_time_ms)
    failure_report = build_failure_report(result)
    leader_candidates = build_leader_candidates_report(result)
    writer.write_json("metrics.json", metrics)
    writer.write_json("failure_report.json", failure_report)
    writer.write_json("leader_candidates.json", leader_candidates)
    writer.write_image("12_failure_analysis.png", overlay_failure_analysis(enhanced, result, failure_report))
    writer.write_image("13_leader_candidates_overlay.png", overlay_leader_candidates(enhanced, leader_candidates))
    writer.write_image("14_schema_view.png", overlay_schema_view(
        enhanced, balloons, balloon_ocr, leaders, text_candidates, assignments,
    ))
    writer.write_image("15_unified_view.png", overlay_unified_view(
        enhanced, balloons, balloon_ocr, leaders, text_candidates,
    ))
    return result


def main() -> None:
    parser = argparse.ArgumentParser(description="Run experimental color-first bubble pipeline.")
    parser.add_argument("image", help="Path to drawing image")
    parser.add_argument("--debug-dir", default=None, help="Directory for debug artifacts")
    args = parser.parse_args()

    result = run_pipeline(args.image, args.debug_dir)
    found = sum(1 for leader in result.leaders if leader.status == "found")
    assigned = sum(1 for a in result.assignments if a.dimension_text != "NO_DIMENSION")
    print(f"balloons={len(result.balloons)} leaders_found={found}/{len(result.leaders)} assignments={assigned}/{len(result.assignments)}")
    print(f"debug written for {args.image}")


if __name__ == "__main__":
    main()
