# Production Benchmarking

This project must not be improved by per-image patches. Every detector change
should be tied to a failure mode and checked against a fixed benchmark suite.

## Files

- `benchmark_cases.json` is the benchmark manifest.
- `benchmark_production.py` runs the detector and writes reports.
- `_pipeline_v2_debug/production_benchmark*/benchmark_report.json` contains
  machine-readable results for each run.
- `_pipeline_v2_debug/production_benchmark*/labeling_worksheet.md` lists
  unlabeled cases with annotated/unified image paths and current detector
  evidence to help manual review.

Expected values in `benchmark_cases.json` are test-only data. `detector.py`
must never read them.

## Metrics

The benchmark tracks:

- `bubble_recall`: expected physical bubble IDs detected.
- `value_accuracy`: expected bubble dimensions correctly assigned.
- `spurious_rate`: extra IDs for cases marked `exact_bubbles=true`.
- `runtime_ms_per_megapixel`: CPU runtime normalized by input size.

Current acceptance targets are stored in `benchmark_cases.json`:

- bubble recall >= 80%
- value accuracy >= 80%
- spurious rate <= 10%
- runtime <= 120000 ms/MP

## Workflow

1. Add or update expected data only after manual review of the image.
2. Run the benchmark before a detector change.
3. Make the smallest failure-mode fix possible.
4. Run the same benchmark after the change.
5. Keep the change only if it improves or preserves the suite metrics.

Useful commands:

```powershell
python benchmark_production.py --cases ui_screenshot_1png dense_drawing_3png --fail-on-threshold
python benchmark_production.py --cases photo_monitor_newjpeg
python benchmark_production.py --all
pytest test_benchmark_manifest.py -q
```

## Current Known Gaps

- Most `m1..n7` cases are present in the manifest but still need manually
  reviewed expected IDs and dimensions.
- Photo/monitor images are weak and slow; `new.jpeg` currently detects only a
  small subset of visible bubbles.
- CPU runtime is far above the production target on the currently labeled
  cases, even when accuracy is correct.
- `benchmark_report.json` includes per-bubble `rim_score`,
  `bubble_evidence`, `trace_points`, confidence, and review reason. Use these
  fields to identify failure modes before changing detector logic.
