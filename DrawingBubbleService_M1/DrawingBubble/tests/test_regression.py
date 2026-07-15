"""
Regression harness for BubbleDetector.

Run from the project root:
    pytest DrawingBubbleService/tests/test_regression.py -v

Or with a short summary:
    pytest DrawingBubbleService/tests/test_regression.py -v --tb=short
"""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path
from typing import Dict, List, Tuple

import cv2
import pytest

SERVICE_ROOT = Path(__file__).resolve().parent.parent.parent
PKG = SERVICE_ROOT / "DrawingBubble"

for entry in (SERVICE_ROOT, SERVICE_ROOT.parent, PKG):
    if str(entry) not in sys.path:
        sys.path.insert(0, str(entry))

from detector import BubbleDetector, DetectionConfig
from mask_topology_assign import apply_topology_override
from tiny_digit_recovery import recover_tiny_digits


def _norm(text: str) -> str:
    """Lightweight normalisation for fuzzy value matching."""
    if not text:
        return ""
    t = text.strip().upper()
    t = re.sub(r"\s+", " ", t)
    t = (
        t.replace("Â·", ".")
         .replace(",", ".")
         .replace("Ã˜", "DIA")
         .replace("Ø", "DIA")
         .replace("âŒ€", "DIA")
         .replace("⌀", "DIA")
    )
    t = (
        t.replace("Â°", "DEG")
         .replace("°", "DEG")
         .replace("Ã—", "X")
         .replace("×", "X")
    )
    t = re.sub(r"(\d)\s*DIA\b", r"\1 DIA", t)
    t = re.sub(r"(\d)\s*([HhGgFf]\d+)\b", r"\1 \2", t)
    t = re.sub(r"(R\d+(?:\.\d+)?)\s*(TYP)\b", r"\1 \2", t)
    t = re.sub(r"(MJ?\d+[Xx]\d+[\.,]\d+)[,\.](\d+[Hh]\d+[Hh])", r"\1 \2", t)
    t = re.sub(r"\s*/\s*", "/", t)
    t = re.sub(r"\b0+(\.\d)", r"\1", t)
    t = re.sub(r"(\d)F\b", r"\1R", t)
    t = re.sub(r"^[-â€“â€”=]+\s*", "", t)
    t = re.sub(r"\s*[-â€“â€”=]+$", "", t)
    t = re.sub(r"\(+", "(", t)
    t = re.sub(r"\)+", ")", t)
    t = re.sub(r"^L(\d)", r"\1", t)
    t = re.sub(r"^Q(\.\d)", r"DIA\1", t)
    t = re.sub(r"\.$", "", t)
    t = re.sub(r"\.0$", "", t)
    t = re.sub(r"\s*MM$", "", t)
    t = re.sub(r"(\d)X(\d)", r"\1 X \2", t)
    t = re.sub(r"DEG\s*TYP", "DEG TYP", t)
    t = re.sub(r"(\d)\s*X\s*(\d)", r"\1 X \2", t)
    t = re.sub(r"^0(\d{2,})", r"DIA\1", t)
    return t


def _value_matches(detected: str, expected: str) -> bool:
    """Return True if the detected dimension text is an acceptable match."""
    if not detected or detected == "NO_DIMENSION":
        return False

    d = _norm(detected)
    e = _norm(expected)

    if d == e:
        return True

    if re.fullmatch(r"\d+", d) and e.startswith(d + "."):
        return True

    # Paren-stripped match: "(8)" == "8", "Ø0.2" == "0.2("
    d_noparen = re.sub(r"[()]", "", d).strip()
    e_noparen = re.sub(r"[()]", "", e).strip()
    if d_noparen and d_noparen == e_noparen:
        return True

    # Combined paren + DIA strip: "0.2(" == "DIA0.2"
    d_clean = re.sub(r"^DIA\s*", "", d_noparen).strip()
    e_clean = re.sub(r"^DIA\s*", "", e_noparen).strip()
    if d_clean and d_clean == e_clean:
        return True

    if len(e) <= 2:
        return False

    # Substring match for expected values 3+ chars, when a case provides them.
    if len(e) >= 3:
        if re.search(r"(?<!\w)" + re.escape(e) + r"(?!\w)", d):
            return True

    d_no_deg = re.sub(r"\s*DEG\s*$", "", d).strip()
    e_no_deg = re.sub(r"\s*DEG\s*$", "", e).strip()
    if d_no_deg == e_no_deg and d_no_deg:
        return True

    d_no_dia = re.sub(r"^DIA\s*", "", d).strip()
    e_no_dia = re.sub(r"^DIA\s*", "", e).strip()
    if d_no_dia == e_no_dia and d_no_dia:
        return True

    # Integer-part match for DIA values: "DIA63" matches "DIA63.1"
    if (re.fullmatch(r"\d+", d_no_dia)
            and e_no_dia.startswith(d_no_dia + ".")):
        return True

    e_words = set(e.split())
    d_words = set(d.split())
    if e_words and e_words.issubset(d_words):
        return True

    if len(e_words) >= 3 and d_words.issubset(e_words):
        coverage = len(d_words & e_words) / len(e_words)
        if coverage >= 0.60:
            return True

    return False


def _run_detector(image_path: Path) -> Dict[str, str]:
    """Run detector on image; return {bubble_id: dimension} dict."""
    img = cv2.imread(str(image_path))
    assert img is not None, f"Cannot load image: {image_path}"

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
        enable_annotation=False,
        print_timing=False,
        enable_targeted_endpoint_ocr=True,
    ))

    bubbles, _ = detector.detect_from_array(img)
    # Surface tiny dim digits (5-8 px tall) the main OCR pass missed.
    # These are needed for drawings with tiny between-arrow dimensions.
    # where the dim value is below RapidOCR's text-height threshold.
    recovered = recover_tiny_digits(detector, bubbles)
    if recovered:
        detector._norm_tokens.extend(recovered)
    # Fallback chain: bbox-sinks topology -> direction-based topology
    # -> v1. Bbox-sinks is silent on cluttered cases; direction-based
    # fills some of those in; v1 is the final fallback.
    apply_topology_override(
        detector, bubbles,
        use_bbox_sinks=True,
        use_direction_fallback=True,
    )
    return {b.bubble_number: b.dimension for b in bubbles}


CASES_FILE = PKG / "tests" / "regression_cases.json"


def _load_cases():
    with open(CASES_FILE, encoding="utf-8") as f:
        data = json.load(f)
    return data["cases"]


def pytest_generate_tests(metafunc):
    if "case" in metafunc.fixturenames:
        metafunc.parametrize("case", _load_cases(), ids=[c["name"] for c in _load_cases()])


@pytest.fixture(scope="session")
def detector_results_cache():
    """Cache detector output per image so we don't re-run for each sub-test."""
    return {}


def _get_results(image_path: Path, cache: dict) -> Dict[str, str]:
    key = str(image_path)
    if key not in cache:
        cache[key] = _run_detector(image_path)
    return cache[key]


def test_recall_and_values(case, detector_results_cache):
    """
    For each test case:
      - Every expected bubble ID must be detected
      - Each detected value must match the expected value, when provided.
      - If exact_bubbles=True, no spurious IDs are allowed
    """
    image_path = PKG / case["image_path"]
    expected: Dict[str, str] | None = case.get("expected")
    if not expected:
        pytest.skip("No stored expected values for this reference image.")
    exact_bubbles: bool = case.get("exact_bubbles", False)

    detected = _get_results(image_path, detector_results_cache)

    missed_ids: List[str] = []
    wrong_values: List[Tuple[str, str, str]] = []
    spurious_ids: List[str] = []

    for bubble_id, exp_val in expected.items():
        if bubble_id not in detected:
            missed_ids.append(bubble_id)
        elif not _value_matches(detected[bubble_id], exp_val):
            wrong_values.append((bubble_id, exp_val, detected[bubble_id]))

    if exact_bubbles:
        expected_set = set(expected.keys())
        for det_id in detected:
            if det_id not in expected_set:
                spurious_ids.append(det_id)

    total_expected = len(expected)
    correctly_found = total_expected - len(missed_ids)
    values_correct = correctly_found - len(wrong_values)
    recall = correctly_found / total_expected if total_expected else 1.0
    value_accuracy = values_correct / total_expected if total_expected else 1.0

    lines = [
        f"\n{'-' * 60}",
        f"  Case         : {case['name']}",
        f"  Image        : {case['image_path']}",
        f"  Expected IDs : {sorted(expected.keys())}",
        f"  Detected IDs : {sorted(detected.keys())}",
        f"  Recall       : {correctly_found}/{total_expected}  ({recall*100:.0f}%)",
        f"  Value acc.   : {values_correct}/{total_expected}  ({value_accuracy*100:.0f}%)",
    ]

    if missed_ids:
        lines.append(f"\n  MISSED bubbles ({len(missed_ids)}):")
        for bid in sorted(missed_ids):
            lines.append(f'    [{bid}]  expected: "{expected[bid]}"')

    if wrong_values:
        lines.append(f"\n  WRONG values ({len(wrong_values)}):")
        for bid, exp_val, got_val in sorted(wrong_values):
            lines.append(f'    [{bid}]  expected: "{exp_val}"  |  got: "{got_val}"')

    if spurious_ids:
        lines.append(f"\n  SPURIOUS IDs ({len(spurious_ids)}): {sorted(spurious_ids)}")

    lines.append("-" * 60)
    report = "\n".join(lines)

    failures = []
    if missed_ids:
        failures.append(f"Missed {len(missed_ids)} bubble(s): {sorted(missed_ids)}")
    if wrong_values:
        failures.append(
            f"{len(wrong_values)} wrong value(s): "
            + ", ".join(f"[{b}]" for b, _, _ in wrong_values)
        )
    if exact_bubbles and spurious_ids:
        failures.append(f"Spurious IDs: {sorted(spurious_ids)}")

    if failures:
        pytest.fail(report + "\n\nFailed checks:\n" + "\n".join(f"  - {f}" for f in failures))

    print(report)


def main() -> None:
    print("Detection strategy: CV_ONLY")

    cases = _load_cases()
    total_exp = total_found = total_correct = 0

    for case in cases:
        if not case.get("expected"):
            print(f"\n[{case['name']}]")
            print("  Skipped: no stored expected values")
            continue
        image_path = PKG / case["image_path"]
        expected = case["expected"]
        detected = _run_detector(image_path)

        found = sum(1 for bid in expected if bid in detected)
        correct = sum(
            1 for bid, ev in expected.items()
            if bid in detected and _value_matches(detected[bid], ev)
        )

        total_exp += len(expected)
        total_found += found
        total_correct += correct

        print(f"\n[{case['name']}]")
        print(f"  Recall:     {found}/{len(expected)}  ({found/len(expected)*100:.0f}%)")
        print(f"  Values:     {correct}/{len(expected)}  ({correct/len(expected)*100:.0f}%)")
        print(f"  Detected:   {sorted(detected.keys())}")
        print(f"  Expected:   {sorted(expected.keys())}")
        spurious = [k for k in detected if k not in expected]
        if spurious:
            print(f"  Spurious:   {sorted(spurious)}")

    print(f"\n{'=' * 50}")
    if total_exp:
        print(f"TOTAL  Recall: {total_found}/{total_exp}  ({total_found/total_exp*100:.0f}%)")
        print(f"TOTAL  Values: {total_correct}/{total_exp}  ({total_correct/total_exp*100:.0f}%)")
    else:
        print("TOTAL  Skipped: no stored expected values")


if __name__ == "__main__":
    main()
