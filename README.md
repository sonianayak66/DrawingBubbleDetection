# Drawing Bubble Detection

Offline CPU-based engineering drawing bubble and dimension detection system.

This project detects numbered balloons/bubbles in engineering drawings, traces leader lines, reads dimension callouts using OCR, and returns bubble-to-dimension assignments with annotated result images and live pipeline diagnostics.

## Current Milestone

This repository is frozen as a project milestone for showcasing the work completed so far.

Validated showcase results:

| Case | Bubble Recall | Value Accuracy | Notes |
|---|---:|---:|---|
| `ui_screenshot_1png` | 100% | 100% | Clean UI screenshot |
| `dense_drawing_3png` | 100% | 100% | Dense drawing with rotated/local OCR recovery |
| `mn_m1` | 100% | 100% | Clean stress-suite image |
| `mn_n1` | 100% | 100% | Clean stress-suite image |

The system is strongest on clean CAD exports, clean screenshots, and selected dense drawings. Photo/monitor captures remain the main production challenge.

## Tech Stack

- Python
- FastAPI
- OpenCV
- RapidOCR
- ONNX Runtime
- NumPy / SciPy
- ASP.NET Core MVC frontend
- REST API integration

## What The System Does

- Detects colored and circular engineering drawing bubbles.
- Reads bubble numbers using OCR and local recovery.
- Groups OCR text into engineering callouts such as dimensions, tolerances, chamfers, radius/diameter values, and notes.
- Traces leader lines from bubbles to dimension text.
- Assigns bubble numbers to the most likely dimension/callout.
- Produces annotated result images.
- Shows live pipeline logs and step timings in the ASP.NET UI.
- Generates benchmark reports for scored validation cases.

## Important Files

| Path | Purpose |
|---|---|
| `DrawingBubbleService_M1/detector.py` | Main detection pipeline |
| `DrawingBubbleService_M1/main.py` | FastAPI service |
| `DrawingBubbleService_M1/benchmark_production.py` | Production benchmark runner |
| `DrawingBubbleService_M1/benchmark_cases.json` | Benchmark manifest |
| `DrawingBubbleService_M1/BENCHMARKING.md` | Benchmarking workflow |
| `DrawingBubbleService_M1/ocr_rules.py` | OCR normalization and token classification |
| `DrawingBubbleService_M1/callout_rules.py` | Dimension/callout grouping logic |
| `DrawingBubbleService_M1/leader_geometry.py` | Leader-line geometry helpers |
| `Controllers/DrawingBubbleController.cs` | ASP.NET controller integration |
| `Views/DrawingBubble/*.cshtml` | ASP.NET UI pages |

## Running The Python Service

```powershell
cd DrawingBubbleService_M1
python -m venv .venv
.\.venv\Scripts\activate
pip install -r requirements.txt
copy .env.example .env
python main.py
```

The API runs locally and is designed to work offline.

## Running Benchmarks

```powershell
cd DrawingBubbleService_M1
python benchmark_production.py --cases ui_screenshot_1png dense_drawing_3png mn_m1 mn_n1 --skip-unified --fail-on-threshold
pytest test_benchmark_manifest.py -q
```

Benchmark outputs include:

- `benchmark_report.json`
- `benchmark_report.csv`
- `benchmark_readiness.md`
- Annotated result images

Generated benchmark/debug outputs are intentionally ignored by Git.

## Demo Assets

Demo recordings and result logs are listed here:

[docs/demo.md](docs/demo.md)

## Current Limitations

The current implementation is mostly rule-based with OCR and geometry heuristics. It works well on clean images but is not yet fully production-ready for all unseen images.

Known difficult cases:

- Phone/monitor photos
- Perspective distortion
- Low contrast or noisy scans
- Very dense drawings
- Missing/partial leader lines
- Rotated or vertical dimension text
- OCR confusion between similar characters or digits

For production deployment, wrong assignments should be treated carefully. Low-confidence results are marked for review where possible.

## Recommended Production Path

To make this production-ready for unseen customer drawings:

1. Build a larger labelled dataset of real engineering drawings.
2. Label bubbles, leader lines, arrowheads, and dimension text regions.
3. Train or fine-tune an offline detector for drawing regions of interest.
4. Use targeted OCR only on detected callout regions instead of relying on full-page OCR.
5. Keep confidence-based validation so uncertain results are sent for manual review instead of forced.
6. Expand benchmark coverage and require regression tests before every change.

## Repository Status

This repository contains the final milestone code and documentation for showcasing the work completed on the offline engineering drawing bubble and dimension detection system.
