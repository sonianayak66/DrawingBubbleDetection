# Production Roadmap

This project milestone demonstrates an offline CPU-based engineering drawing bubble and dimension detection pipeline. The next stage is to move from a mostly rule-based showcase system toward production-grade reliability on unseen customer drawings.

## Priority Work

| Priority | Area | GitHub Issue | Goal |
|---|---|---|---|
| P0 | Benchmarking | [#1](https://github.com/sonianayak66/DrawingBubbleDetection/issues/1) | Build a larger labelled benchmark set with bubbles, leader lines, endpoints, and dimension text annotations. |
| P0 | OCR | [#2](https://github.com/sonianayak66/DrawingBubbleDetection/issues/2) | Add targeted rotated OCR near leader-line endpoints instead of expensive full-page rotated OCR. |
| P0 | Leader tracing | [#3](https://github.com/sonianayak66/DrawingBubbleDetection/issues/3) | Strengthen leader-line tracing so assignments are based on drawing geometry, not nearby text alone. |
| P1 | Confidence handling | [#4](https://github.com/sonianayak66/DrawingBubbleDetection/issues/4) | Mark uncertain results for review instead of forcing weak assignments. |
| P1 | Performance | [#5](https://github.com/sonianayak66/DrawingBubbleDetection/issues/5) | Profile and optimize CPU-only OCR runtime while keeping the system fully offline. |

## Production Direction

The current implementation performs well on clean CAD exports, clean screenshots, and selected dense drawings, but production deployment needs stronger validation across unseen drawing styles. The recommended direction is:

1. Expand the labelled dataset using real drawings from different quality levels.
2. Use geometry-first leader tracing for final assignment decisions.
3. Apply OCR only to targeted regions where the drawing structure indicates relevant text.
4. Add confidence scoring and manual-review states for uncertain cases.
5. Track regressions through repeatable benchmarks before every release.

## Success Criteria

- Bubble recall consistently reaches at least 80% on representative unseen drawings.
- Wrong assignments are reduced by preferring needs-review over forced low-confidence values.
- Runtime remains practical on CPU-only offline deployments.
- Accuracy is measured against a labelled benchmark, not image-specific code paths.
