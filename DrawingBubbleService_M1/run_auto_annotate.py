"""
CLI to run auto-annotation on a drawing.

Usage:
    python run_auto_annotate.py <input_image> [output_image]

Takes a drawing with dimensions (ideally no existing balloons) and
outputs an annotated version with auto-placed balloons and leaders.

Also prints the ground-truth (balloon → dimension) mapping which
can be used as training data for the reverse detector.
"""

from __future__ import annotations

import json
import sys
from pathlib import Path

SERVICE_ROOT = Path(__file__).resolve().parent
PKG = SERVICE_ROOT / "DrawingBubble"
for entry in (SERVICE_ROOT, SERVICE_ROOT.parent, PKG):
    if str(entry) not in sys.path:
        sys.path.insert(0, str(entry))

import cv2
from auto_annotate import auto_annotate


def main() -> None:
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)

    input_path = Path(sys.argv[1])
    if not input_path.exists():
        print(f"Input image not found: {input_path}")
        sys.exit(1)

    if len(sys.argv) >= 3:
        output_path = Path(sys.argv[2])
    else:
        output_path = input_path.with_name(
            input_path.stem + "_annotated" + input_path.suffix,
        )

    img = cv2.imread(str(input_path))
    if img is None:
        print(f"Failed to load image: {input_path}")
        sys.exit(1)

    print(f"Running auto-annotation on {input_path.name} "
          f"({img.shape[1]}x{img.shape[0]})...")
    annotated, balloons = auto_annotate(img)

    # Save annotated image
    cv2.imwrite(str(output_path), annotated)
    print(f"Annotated image saved to: {output_path}")

    # Print + save ground-truth mapping
    print(f"\nFound {len(balloons)} dimensions:")
    for b in sorted(balloons, key=lambda b: b.number):
        print(f"  [{b.number:3d}]  {b.dimension_text!r:30s}  "
              f"balloon @({b.cx},{b.cy}) r={b.radius}")

    # Save mapping as JSON
    mapping = [
        {
            "number": b.number,
            "balloon": {"cx": b.cx, "cy": b.cy, "radius": b.radius},
            "dimension_text": b.dimension_text,
            "dim_bbox": list(b.dim_bbox),
            "leader_start": list(b.leader_start),
            "leader_end": list(b.leader_end),
        }
        for b in sorted(balloons, key=lambda b: b.number)
    ]
    mapping_path = output_path.with_suffix(".json")
    with open(mapping_path, "w") as f:
        json.dump({"source": str(input_path), "balloons": mapping}, f, indent=2)
    print(f"Ground-truth mapping saved to: {mapping_path}")


if __name__ == "__main__":
    main()
