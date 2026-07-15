"""
Batch auto-annotation: run auto_annotate over a folder of drawings
and emit image + JSON mapping pairs. Intended for generating
training data for an ML-based detector upgrade.

Usage:
    python batch_auto_annotate.py <input_dir> <output_dir> [options]

Options:
    --radius N           Balloon radius in pixels (default 25)
    --preserve           Skip dimensions already covered by an existing
                         annotation bubble (default: off — overlay all)
    --overwrite          Reprocess images even if outputs already exist
    --extensions LIST    Comma-separated extensions to process
                         (default: png,jpg,jpeg,tif,tiff,bmp)

Output (one per input):
    <output_dir>/<stem>_annotated.png     annotated drawing
    <output_dir>/<stem>.json              ground-truth mapping

Summary report is printed at the end (counts, failures).
"""

from __future__ import annotations

import argparse
import json
import sys
import time
from pathlib import Path

SERVICE_ROOT = Path(__file__).resolve().parent
for entry in (SERVICE_ROOT, SERVICE_ROOT.parent):
    if str(entry) not in sys.path:
        sys.path.insert(0, str(entry))

import cv2
from auto_annotate import auto_annotate


def _parse_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(
        description="Batch auto-annotate a folder of drawings.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    p.add_argument("input_dir", type=Path, help="Folder of input drawings")
    p.add_argument("output_dir", type=Path, help="Folder for outputs")
    p.add_argument("--radius", type=int, default=25)
    p.add_argument("--preserve", action="store_true",
                   help="Preserve existing bubbles; only fill gaps")
    p.add_argument("--overwrite", action="store_true",
                   help="Reprocess even if output already exists")
    p.add_argument("--extensions", default="png,jpg,jpeg,tif,tiff,bmp",
                   help="Comma-separated file extensions to process")
    return p.parse_args()


def _process_one(
    img_path: Path,
    out_dir: Path,
    radius: int,
    preserve: bool,
    overwrite: bool,
) -> tuple[str, int | None, str | None]:
    """
    Returns (status, balloon_count, error_message).
    status is one of: "ok", "skipped", "empty", "failed".
    """
    stem = img_path.stem
    out_img = out_dir / f"{stem}_annotated.png"
    out_json = out_dir / f"{stem}.json"

    if out_img.exists() and out_json.exists() and not overwrite:
        return ("skipped", None, None)

    img = cv2.imread(str(img_path))
    if img is None:
        return ("failed", None, f"cv2.imread returned None for {img_path}")

    try:
        annotated, balloons = auto_annotate(
            img, balloon_radius=radius, preserve_existing=preserve,
        )
    except Exception as exc:
        return ("failed", None, f"auto_annotate raised: {exc}")

    if not balloons:
        # Still write an empty JSON so reruns skip; user can delete
        # these if they want to retry.
        with open(out_json, "w", encoding="utf-8") as f:
            json.dump(
                {"source": str(img_path), "balloons": [], "note": "no dimensions detected"},
                f, indent=2,
            )
        cv2.imwrite(str(out_img), annotated)
        return ("empty", 0, None)

    cv2.imwrite(str(out_img), annotated)
    mapping = {
        "source": str(img_path),
        "image_size": [int(img.shape[1]), int(img.shape[0])],
        "balloons": [
            {
                "number": b.number,
                "balloon": {"cx": b.cx, "cy": b.cy, "radius": b.radius},
                "dimension_text": b.dimension_text,
                "dim_bbox": list(b.dim_bbox),
                "leader_points": [list(p) for p in (b.leader_points or [b.leader_start, b.leader_end])],
            }
            for b in sorted(balloons, key=lambda x: x.number)
        ],
    }
    with open(out_json, "w", encoding="utf-8") as f:
        json.dump(mapping, f, indent=2)
    return ("ok", len(balloons), None)


def main() -> int:
    args = _parse_args()
    if not args.input_dir.is_dir():
        print(f"ERROR: input_dir does not exist: {args.input_dir}")
        return 2
    args.output_dir.mkdir(parents=True, exist_ok=True)

    exts = {"." + e.lower().lstrip(".") for e in args.extensions.split(",") if e.strip()}
    images = sorted(
        p for p in args.input_dir.iterdir()
        if p.is_file() and p.suffix.lower() in exts
    )
    if not images:
        print(f"No images found in {args.input_dir} with extensions {sorted(exts)}")
        return 1

    print(f"Found {len(images)} image(s). Output -> {args.output_dir}")
    counts = {"ok": 0, "skipped": 0, "empty": 0, "failed": 0}
    total_balloons = 0
    failures: list[tuple[str, str]] = []
    t_start = time.time()

    for i, img_path in enumerate(images, 1):
        tag = f"[{i:>3}/{len(images)}] {img_path.name}"
        t0 = time.time()
        status, count, err = _process_one(
            img_path, args.output_dir,
            radius=args.radius, preserve=args.preserve, overwrite=args.overwrite,
        )
        elapsed = time.time() - t0
        counts[status] += 1
        if count is not None:
            total_balloons += count
        if status == "ok":
            print(f"{tag:50s}  {count:>3} balloons  ({elapsed:.1f}s)")
        elif status == "empty":
            print(f"{tag:50s}  (no dimensions detected)")
        elif status == "skipped":
            print(f"{tag:50s}  SKIP (outputs exist, --overwrite to force)")
        else:
            print(f"{tag:50s}  FAIL: {err}")
            failures.append((img_path.name, err or "unknown"))

    total = time.time() - t_start
    print()
    print(f"=== Summary ({total:.1f}s total) ===")
    print(f"  ok:      {counts['ok']}")
    print(f"  skipped: {counts['skipped']}")
    print(f"  empty:   {counts['empty']}")
    print(f"  failed:  {counts['failed']}")
    print(f"  total balloons: {total_balloons}")
    if failures:
        print("\nFailures:")
        for name, err in failures:
            print(f"  {name}: {err}")
    return 0 if counts["failed"] == 0 else 1


if __name__ == "__main__":
    sys.exit(main())
