from __future__ import annotations

import json
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from benchmark_production import _value_matches


def test_benchmark_manifest_paths_and_expected_shape() -> None:
    manifest = json.loads((ROOT / "benchmark_cases.json").read_text(encoding="utf-8"))
    assert manifest["schema_version"] == 1
    assert manifest["cases"]

    scored_cases = 0
    existing_manual_cases = 0
    for case in manifest["cases"]:
        image_path = ROOT / case["image_path"]
        has_expected = bool(case.get("expected_bubbles") or case.get("expected_dimensions"))
        if not image_path.exists():
            assert not has_expected, f"Missing scored benchmark image: {case['image_path']}"
            assert case.get("needs_manual_expected") is True
            continue

        expected_bubbles = {str(x) for x in case.get("expected_bubbles") or []}
        expected_dimensions = {
            str(k): str(v) for k, v in (case.get("expected_dimensions") or {}).items()
        }
        if expected_bubbles or expected_dimensions:
            scored_cases += 1
            assert set(expected_dimensions).issubset(expected_bubbles)
            for bubble_id, expected in expected_dimensions.items():
                assert bubble_id
                assert expected.strip()
        else:
            assert case.get("needs_manual_expected") is True
            existing_manual_cases += 1

    assert scored_cases >= 2
    assert existing_manual_cases >= 1


def test_detector_does_not_read_benchmark_expected_data() -> None:
    detector_source = (ROOT / "detector.py").read_text(encoding="utf-8")
    assert "benchmark_cases.json" not in detector_source
    assert "expected_dimensions" not in detector_source
    assert "expected_bubbles" not in detector_source


def test_benchmark_value_matching_normalizes_common_dimension_forms() -> None:
    assert _value_matches("Ø.800±.001", "Ø.800±.001")
    assert _value_matches("R72.55", "R72.55")
    assert _value_matches("65.05", "65.05")
    assert _value_matches("DIA.800+/- .001", "Ø.800±.001")
    assert not _value_matches("59.5", "9(REF)")
