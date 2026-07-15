# Demo Assets

This page contains supporting demo artifacts for the Drawing Bubble Detection milestone.

## Demo Videos

The video files are stored as GitHub Release assets instead of being committed directly to the repository. This keeps the repository lightweight while still making the demos easy to download.

| Demo | What It Shows | Link |
|---|---|---|
| Auto Annotation Demo | Auto annotation workflow and pipeline behavior | [Download MP4](https://github.com/sonianayak66/DrawingBubbleDetection/releases/download/showcase-milestone-v1/auto-annotation-demo-2026-05-05.mp4) |
| Balloon Detection Demo | Balloon detection and result inspection workflow | [Download MP4](https://github.com/sonianayak66/DrawingBubbleDetection/releases/download/showcase-milestone-v1/balloon-detection-demo-2026-05-09.mp4) |

## Result Logs

The result logs PDF is committed in this repository:

[View ResultLogs.pdf](./ResultLogs.pdf)

It contains supporting output/log evidence from the detection workflow.

## Showcase Summary

Current milestone highlights:

- Offline CPU-only detection pipeline.
- Bubble/balloon detection from engineering drawings.
- OCR-based dimension and callout extraction.
- Leader-line tracing and bubble-to-dimension assignment.
- Annotated result image generation.
- Live logs and benchmark reporting.

Known limitation:

- Photo or monitor-recorded images are still less reliable than clean CAD exports or clean screenshots. For production, the next recommended step is a labelled dataset plus trained offline region detectors for bubbles, leaders, arrowheads, and dimension text.
