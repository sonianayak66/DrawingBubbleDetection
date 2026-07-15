"""
Production Bubble Detection API — FastAPI service.

Endpoints:
  POST /api/detect         Upload an engineering drawing → get bubble→dimension map
  POST /api/auto-annotate  Generate balloons + leader lines for every dimension
  GET  /api/health         Service health and performance metrics
  GET  /docs               Swagger UI (auto-generated)

The legacy paths `/detect`, `/auto-annotate`, and `/health` are kept as
hidden aliases so older clients keep working — new clients should use
the `/api/...` variants.

Authentication: X-API-Key header (set BUBBLE_API_KEY in .env)
"""

from __future__ import annotations

import asyncio
import base64
import hmac
import logging
import os
import time
import uuid
from contextlib import asynccontextmanager
from typing import Any, Dict, List, Optional
import geometric_utils

import cv2
import numpy as np
import psutil
from dotenv import load_dotenv
from fastapi import Depends, FastAPI, File, HTTPException, Query, Security, UploadFile, Request
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
from fastapi.security.api_key import APIKeyHeader
from pydantic import BaseModel
from fastapi import FastAPI
from fastapi import FastAPI, UploadFile, File


import sys, os
sys.path.insert(0, os.path.dirname(__file__))
from detector import BubbleDetector, DetectionConfig

load_dotenv()

# ─────────────────────────────────────────────────────────────────────────────
# Logging
# ─────────────────────────────────────────────────────────────────────────────

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s  %(levelname)-8s  [%(name)s] %(message)s",
    datefmt="%Y-%m-%d %H:%M:%S",
)
logger = logging.getLogger(__name__)

# Install in-memory ring buffer so the /api/logs endpoint and the /logs
# UI page can read recent log records. This is ADDITIONAL to the stdout
# handler installed by basicConfig — terminal output stays the same.
from log_buffer import attach_buffer, get_logs, get_head_seq, request_context  # noqa: E402
attach_buffer(level=logging.INFO, capacity=2000)

# ─────────────────────────────────────────────────────────────────────────────
# Security
# ─────────────────────────────────────────────────────────────────────────────
# Explicitly load .env from the current folder
load_dotenv(dotenv_path=os.path.join(os.path.dirname(__file__), ".env"))
API_KEY = os.getenv("BUBBLE_API_KEY", "").strip()
# Refuse to boot on an empty key or any of the well-known placeholder
# strings shipped in .env.example. Those tokens are the most common
# "forgot to rotate" mistake and would otherwise let anyone hit the
# detector with the example value.
_API_KEY_BLOCKLIST = (
    "change-this-key",
    "change-me-to",
    "your-key-here",
    "mpcrs-m1-prod-2026",
)
if not API_KEY:
    raise RuntimeError(
        "BUBBLE_API_KEY is not set. Add it to your .env file."
    )
if any(token in API_KEY.lower() for token in _API_KEY_BLOCKLIST):
    raise RuntimeError(
        "BUBBLE_API_KEY still contains a placeholder token "
        f"({_API_KEY_BLOCKLIST!r}). Rotate it to a strong random value "
        "before starting the service."
    )

_api_key_header = APIKeyHeader(name="X-API-Key", auto_error=False)

_START_TIME   = time.time()
_REQUEST_COUNT = 0
_ERROR_COUNT   = 0

# Detector singleton holds per-request state on `self` (image, tokens,
# quality assessment …) so two concurrent detect_from_array() calls
# would race. Serialise access at the FastAPI layer until the detector
# is refactored to use a request-scoped context object.
_detector_lock = asyncio.Lock()


# ─────────────────────────────────────────────────────────────────────────────
# Pydantic models
# ─────────────────────────────────────────────────────────────────────────────

class BubbleDetectionResponse(BaseModel):
    request_id: str
    bubble_count: int
    bubbles: List[Dict[str, Any]]
    processing_time_ms: float
    annotated_image_base64: Optional[str] = None
    diagnostics: Optional[Dict[str, Any]] = None


class HealthResponse(BaseModel):
    status: str
    version: str
    uptime_seconds: float
    memory_usage_mb: float
    request_count: int
    error_count: int


class ErrorResponse(BaseModel):
    error: str
    detail: Optional[str] = None
    request_id: Optional[str] = None


# ─────────────────────────────────────────────────────────────────────────────
# App factory
# ─────────────────────────────────────────────────────────────────────────────

@asynccontextmanager
async def lifespan(app: FastAPI):
    logger.info("Starting Bubble Detection Service …")
    app.state.detector = None
    # Pre-warm: load the detector + OCR model weights at boot so the
    # first real user request doesn't pay the 8–10 s cold-start.
    # Failures here are non-fatal — the service still starts and the
    # next /api/detect call will retry the lazy init.
    try:
        from detector import BubbleDetector, DetectionConfig
        logger.info("Pre-warming detector + OCR models …")
        t0 = time.time()
        # Honour the same env vars here so the prewarmed detector
        # matches the config _get_detector() would have built. Without
        # this the prewarm stashes a default-config detector in
        # app.state and the runtime never re-creates it.
        enable_resrgan = os.getenv("BUBBLE_ENABLE_REALESRGAN", "").lower() in ("1", "true", "yes")
        enable_rescue_ocr = os.getenv("BUBBLE_ENABLE_TARGETED_ENDPOINT_OCR", "").lower() in ("1", "true", "yes")
        enable_photo_pp = os.getenv("BUBBLE_ENABLE_PHOTO_PREPROCESSING", "").lower() in ("1", "true", "yes")
        det = BubbleDetector(DetectionConfig(
            ocr_scale=0, run_multi_scale_ocr=True,
            min_radius=0, max_radius=0,
            hough_param2=0, min_dist=0,
            enable_seed_trace_assignment=True,
            enable_image_linking=True,
            enable_heavy_path_disambiguation=False,
            enable_annotation=True,
            print_timing=False,
            enable_realesrgan_enhancement=enable_resrgan,
            enable_targeted_endpoint_ocr=enable_rescue_ocr,
            enable_photo_preprocessing=enable_photo_pp,
        ))
        # Run a tiny 64x64 detection so the OCR model weights are
        # actually loaded into memory (RapidOCR defers load to first
        # call, so just constructing the detector isn't enough).
        dummy = np.full((64, 64, 3), 255, dtype=np.uint8)
        try:
            det.detect_from_array(dummy)
        except Exception:
            pass  # tiny image may produce no bubbles — that's fine
        app.state.detector = det
        logger.info("Pre-warm complete in %.2f s — first request will be fast.",
                    time.time() - t0)
    except Exception as exc:
        logger.warning("Pre-warm failed (%s) — falling back to lazy init on first request.", exc)
    yield
    logger.info("Shutting down …")
    if getattr(app.state, "detector", None) is not None:
        del app.state.detector


app = FastAPI(
    title="Bubble Detection Service",
    version="4.0.0",
    description=(
        "Detects numbered balloons in engineering drawings and maps each "
        "bubble number to its associated dimension / callout text."
    ),
    lifespan=lifespan,
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["GET", "POST", "OPTIONS"],
    allow_headers=["*"],
)


@app.get("/", include_in_schema=False)
async def root():
    return {"service": "Bubble Detection Service", "version": "4.0.0", "docs": "/docs"}


@app.middleware("http")
async def _track_requests(request: Request, call_next):
    global _REQUEST_COUNT, _ERROR_COUNT
    _REQUEST_COUNT += 1
    t = time.time()
    try:
        response = await call_next(request)
        response.headers["X-Process-Time"] = f"{time.time()-t:.3f}s"
        response.headers["X-Request-ID"]   = str(_REQUEST_COUNT)
        return response
    except Exception as exc:
        _ERROR_COUNT += 1
        logger.error("Request %d failed: %s", _REQUEST_COUNT, exc)
        raise


# ─────────────────────────────────────────────────────────────────────────────
# Helpers
# ─────────────────────────────────────────────────────────────────────────────

ALLOWED_EXTENSIONS = {".png", ".jpg", ".jpeg", ".tiff", ".tif", ".bmp"}
MAX_FILE_BYTES     = 50 * 1024 * 1024   # 50 MB


async def _require_api_key(api_key: str = Security(_api_key_header)) -> str:
    # Constant-time compare to avoid leaking key length / prefix via
    # response-time differences. APIKeyHeader returns None when the
    # header is missing, so guard before passing to compare_digest.
    if not api_key or not hmac.compare_digest(api_key, API_KEY):
        raise HTTPException(status_code=401, detail="Invalid or missing API key.")
    return api_key


def _get_detector() -> BubbleDetector:
    if getattr(app.state, "detector", None) is None:
        logger.info("Initialising BubbleDetector …")
        # Env-var controlled feature flags.
        #   BUBBLE_ENABLE_REALESRGAN: turn on Real-ESRGAN super-resolution
        #     preprocessing for cluttered drawings. Off by default.
        #   BUBBLE_ENABLE_TARGETED_ENDPOINT_OCR: turn on the local
        #     crop-and-retry OCR rescue pass for unresolved / low-conf
        #     bubbles. Adds 1-4 s per drawing depending on rescue count.
        #   BUBBLE_ENABLE_PHOTO_PREPROCESSING: deskew + lighting fix +
        #     bilateral denoise for phone-camera input (incl. photos of
        #     monitor screens). Each sub-step is internally gated so
        #     clean scans pass through untouched.
        # All default off so production behaviour is unchanged unless
        # the operator opts in via the .env file.
        enable_resrgan = os.getenv("BUBBLE_ENABLE_REALESRGAN", "").lower() in ("1", "true", "yes")
        enable_rescue_ocr = os.getenv("BUBBLE_ENABLE_TARGETED_ENDPOINT_OCR", "").lower() in ("1", "true", "yes")
        enable_photo_pp = os.getenv("BUBBLE_ENABLE_PHOTO_PREPROCESSING", "").lower() in ("1", "true", "yes")
        try:
            app.state.detector = BubbleDetector(DetectionConfig(
                ocr_scale=0,
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
                enable_realesrgan_enhancement=enable_resrgan,
                enable_targeted_endpoint_ocr=enable_rescue_ocr,
                enable_photo_preprocessing=enable_photo_pp,
            ))
            logger.info("BubbleDetector ready.")
        except Exception as exc:
            logger.error("BubbleDetector init failed: %s", exc)
            raise HTTPException(status_code=503, detail="Detector unavailable.")
    return app.state.detector


def _decode_image(contents: bytes, filename: str) -> np.ndarray:
    try:
        arr = np.frombuffer(contents, np.uint8)
        img = cv2.imdecode(arr, cv2.IMREAD_COLOR)
        if img is None:
            raise ValueError("cv2.imdecode returned None")
        h, w = img.shape[:2]
        if h < 50 or w < 50:
            raise ValueError(f"Image too small: {w}×{h}")
        if h > 10000 or w > 10000:
            raise ValueError(f"Image too large: {w}×{h}")
        return img
    except Exception as exc:
        raise HTTPException(
            status_code=400,
            detail=f"Cannot decode image '{filename}': {exc}",
        )


# ─────────────────────────────────────────────────────────────────────────────
# Endpoints
# ─────────────────────────────────────────────────────────────────────────────

@app.post("/detect", response_model=BubbleDetectionResponse, include_in_schema=False)
@app.post("/api/detect", response_model=BubbleDetectionResponse)
async def detect(
    file: UploadFile = File(..., description="Engineering drawing (PNG / JPEG / TIFF / BMP)"),
    include_annotated_image: bool = Query(False, description="Return annotated image as base64 PNG"),
    include_diagnostics: bool = Query(False, description="Return detailed diagnostics block"),
    request_id: Optional[str] = Query(None, description="Optional client-supplied request id for live log tailing. If omitted, one is generated."),
    _key: str = Depends(_require_api_key),
) -> JSONResponse:
    """
    Detect bubble numbers and their associated dimensions in an engineering drawing.

    Upload the drawing as a multipart file.  The response contains a list of
    bubbles, each with its number, centroid, radius, assigned dimension text,
    confidence score, and a review flag for low-confidence assignments.
    """
    global _ERROR_COUNT

    # Use caller-supplied id when present (sanitised) so the frontend can
    # tail logs for this exact run via /api/logs?request_id=X. Fall back
    # to a fresh uuid otherwise. The id is trimmed and capped to 64 chars
    # to keep it safe in log lines and URLs.
    if request_id:
        rid = "".join(c for c in request_id.strip() if c.isalnum() or c in "-_")[:64]
        request_id = rid or str(uuid.uuid4())[:8]
    else:
        request_id = str(uuid.uuid4())[:8]
    t_start    = time.time()

    # Validation
    if not file.filename:
        raise HTTPException(status_code=400, detail="No filename provided.")
    ext = os.path.splitext(file.filename)[1].lower()
    if ext not in ALLOWED_EXTENSIONS:
        raise HTTPException(
            status_code=400,
            detail=f"Unsupported file type '{ext}'. Allowed: {', '.join(sorted(ALLOWED_EXTENSIONS))}",
        )

    contents = await file.read()
    if not contents:
        raise HTTPException(status_code=400, detail="Uploaded file is empty.")
    if len(contents) > MAX_FILE_BYTES:
        raise HTTPException(
            status_code=413,
            detail=f"File too large ({len(contents)//1024} KB > {MAX_FILE_BYTES//1024//1024} MB limit).",
        )

    img  = _decode_image(contents, file.filename)
    h, w = img.shape[:2]

    # Every log record emitted inside this block gets tagged with
    # `request_id`, so /api/logs?request_id=X returns just this run.
    with request_context(request_id):
        logger.info("req=%s  file=%s  size=%dx%d  bytes=%d",
                    request_id, file.filename, w, h, len(contents))
        try:
            detector = _get_detector()
            # Detection is CPU-bound and synchronous. Run it in a worker
            # thread so the asyncio event loop stays free to serve other
            # requests (especially /api/logs, which the live UI polls
            # every 1.5 s while detection runs).
            #
            # The detector mutates per-request state on `self`; the lock
            # serialises access so two concurrent uploads can't corrupt
            # each other's tokens / quality assessment / image buffers.
            async with _detector_lock:
                bubbles, annotated_img = await asyncio.to_thread(
                    detector.detect_from_array, img,
                )
        except Exception as exc:
            _ERROR_COUNT += 1
            logger.exception("Detection failed req=%s: %s", request_id, exc)
            raise HTTPException(status_code=500, detail=f"Detection error: {exc}")

        elapsed_ms = (time.time() - t_start) * 1000

        payload: Dict[str, Any] = {
            "request_id":   request_id,
            "bubble_count": len(bubbles),
            "bubbles":      [b.to_dict() for b in bubbles],
            "processing_time_ms": round(elapsed_ms, 2),
        }

        if include_annotated_image:
            ok, buf = cv2.imencode(".png", annotated_img)
            if ok:
                payload["annotated_image_base64"] = base64.b64encode(buf.tobytes()).decode()

        if include_diagnostics:
            payload["diagnostics"] = _build_diagnostics(
                request_id=request_id,
                filename=file.filename,
                width=w, height=h,
                bubbles=bubbles,
                detector=detector,
                elapsed_ms=elapsed_ms,
            )

        logger.info("req=%s  bubbles=%d  ms=%.1f", request_id, len(bubbles), elapsed_ms)
        return JSONResponse(content=payload)


class AutoAnnotateResponse(BaseModel):
    balloon_count: int
    balloons: List[Dict[str, Any]]
    annotated_image_base64: str
    processing_time_ms: float


@app.post("/auto-annotate", response_model=AutoAnnotateResponse, include_in_schema=False)
@app.post("/api/auto-annotate", response_model=AutoAnnotateResponse)
async def auto_annotate_endpoint(
    file: UploadFile = File(..., description="Engineering drawing (PNG / JPEG / TIFF / BMP)"),
    balloon_radius: int = Query(25, description="Radius in pixels for auto-placed balloons"),
    preserve_existing: bool = Query(True, description="Skip dimensions that already have an annotation bubble"),
    _key: str = Depends(_require_api_key),
) -> JSONResponse:
    """Generate balloons + leader lines for every dimension in the
    drawing. Returns the annotated image (base64 PNG) plus the
    ground-truth mapping (number → dimension text, balloon position,
    leader path).

    - Set `preserve_existing=false` to overlay fresh balloons on an
      already-annotated drawing (useful for replacing annotations).
    - The response is ~equivalent to the `run_auto_annotate.py` CLI.
    """
    global _ERROR_COUNT
    from auto_annotate import auto_annotate as _do_auto_annotate

    request_id = str(uuid.uuid4())[:8]
    t_start = time.time()

    if not file.filename:
        raise HTTPException(status_code=400, detail="No filename provided.")
    ext = os.path.splitext(file.filename)[1].lower()
    if ext not in ALLOWED_EXTENSIONS:
        raise HTTPException(
            status_code=400,
            detail=f"Unsupported file type '{ext}'. Allowed: {', '.join(sorted(ALLOWED_EXTENSIONS))}",
        )

    contents = await file.read()
    if not contents:
        raise HTTPException(status_code=400, detail="Uploaded file is empty.")
    if len(contents) > MAX_FILE_BYTES:
        raise HTTPException(
            status_code=413,
            detail=f"File too large ({len(contents)//1024} KB > {MAX_FILE_BYTES//1024//1024} MB limit).",
        )

    img = _decode_image(contents, file.filename)
    h, w = img.shape[:2]

    with request_context(request_id):
        logger.info(
            "auto_annotate req=%s file=%s size=%dx%d",
            request_id, file.filename, w, h,
        )

        try:
            # Same reason as /api/detect: keep the event loop free so
            # /api/logs can be polled while annotation runs.
            annotated, balloons = await asyncio.to_thread(
                _do_auto_annotate, img,
                balloon_radius=balloon_radius,
                preserve_existing=preserve_existing,
            )
        except Exception as exc:
            _ERROR_COUNT += 1
            logger.exception("auto_annotate failed req=%s: %s", request_id, exc)
            raise HTTPException(status_code=500, detail=f"Auto-annotate error: {exc}")

        ok, buf = cv2.imencode(".png", annotated)
        if not ok:
            raise HTTPException(status_code=500, detail="Failed to encode annotated image.")

        elapsed_ms = (time.time() - t_start) * 1000
        payload = {
            "request_id": request_id,
            "balloon_count": len(balloons),
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
            "annotated_image_base64": base64.b64encode(buf.tobytes()).decode(),
            "processing_time_ms": round(elapsed_ms, 2),
        }

        logger.info(
            "auto_annotate req=%s balloons=%d ms=%.1f",
            request_id, len(balloons), elapsed_ms,
        )
        return JSONResponse(content=payload)


@app.get("/health", response_model=HealthResponse, include_in_schema=False)
@app.get("/api/health", response_model=HealthResponse)
async def health() -> HealthResponse:
    """Return service health metrics."""
    proc = psutil.Process()
    return HealthResponse(
        status="ok",
        version="4.0.0",
        uptime_seconds=round(time.time() - _START_TIME, 1),
        memory_usage_mb=round(proc.memory_info().rss / 1024 / 1024, 1),
        request_count=_REQUEST_COUNT,
        error_count=_ERROR_COUNT,
    )


# ─────────────────────────────────────────────────────────────────────────────
# Logs — JSON endpoint and HTML viewer
# ─────────────────────────────────────────────────────────────────────────────

@app.get("/api/logs")
async def api_logs(
    request_id: Optional[str] = Query(None, description="Filter to one detection run"),
    since_seq: Optional[int] = Query(None, description="Only return entries with seq > this (for tailing)"),
    level: Optional[str] = Query(None, description="Minimum level: DEBUG, INFO, WARNING, ERROR"),
    limit: int = Query(500, ge=1, le=2000, description="Max entries to return"),
) -> JSONResponse:
    """Read recent log records captured during detection runs.

    The buffer holds up to 2000 entries (oldest evicted as new ones arrive).
    Each entry has: seq, ts, ts_human, level, logger, request_id, message.

    For the live UI tail: poll every 1-2 seconds passing the `seq` of the
    last entry you saw as `since_seq` to receive only new records.
    """
    entries = get_logs(
        request_id=request_id,
        since_seq=since_seq,
        level=level,
        limit=limit,
    )
    # Disable caching — the live tail page polls this endpoint every
    # 1.5s and any caching (browser, proxy) breaks the tail.
    return JSONResponse(
        content={
            "count": len(entries),
            "entries": entries,
            "head_seq": get_head_seq(),
        },
        headers={
            "Cache-Control": "no-cache, no-store, must-revalidate",
            "Pragma": "no-cache",
            "Expires": "0",
        },
    )


_LOGS_HTML = """<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<title>Bubble Detection — Live Logs</title>
<style>
  body { font: 13px/1.45 ui-monospace, Menlo, Consolas, monospace;
         margin: 0; background: #0d1117; color: #c9d1d9; }
  header { padding: 10px 14px; background: #161b22; border-bottom: 1px solid #30363d;
           position: sticky; top: 0; z-index: 1; display: flex; gap: 10px;
           align-items: center; flex-wrap: wrap; }
  header h1 { font-size: 14px; margin: 0; color: #f0f6fc; font-weight: 600; }
  header label { font-size: 12px; color: #8b949e; }
  header input, header select, header button {
    background: #0d1117; color: #c9d1d9; border: 1px solid #30363d;
    border-radius: 4px; padding: 4px 8px; font: inherit;
  }
  header button { cursor: pointer; }
  header button:hover { background: #1f2937; }
  header .status { margin-left: auto; font-size: 12px; color: #8b949e; }
  #log { padding: 8px 14px; }
  .row { display: grid; grid-template-columns: 170px 60px 100px 1fr;
         column-gap: 12px; padding: 1px 0; word-break: break-word; }
  .row .ts  { color: #6e7681; }
  .row .lvl { font-weight: 600; }
  .row .lvl.INFO     { color: #58a6ff; }
  .row .lvl.WARNING  { color: #d29922; }
  .row .lvl.ERROR    { color: #f85149; }
  .row .lvl.DEBUG    { color: #8b949e; }
  .row .rid { color: #d2a8ff; }
  .row .msg { white-space: pre-wrap; }
  .empty { color: #6e7681; padding: 12px 0; }
</style>
</head>
<body>
<header>
  <h1>Bubble Detection — Live Logs</h1>
  <label>request_id <input id="f-rid" placeholder="any" size="10"></label>
  <label>level
    <select id="f-level">
      <option value="">all</option>
      <option value="DEBUG">DEBUG</option>
      <option value="INFO" selected>INFO</option>
      <option value="WARNING">WARNING</option>
      <option value="ERROR">ERROR</option>
    </select>
  </label>
  <label><input type="checkbox" id="f-tail" checked> tail</label>
  <button id="f-clear">clear</button>
  <span class="status" id="status">connecting…</span>
</header>
<div id="log"></div>
<script>
let lastSeq = 0;
const $log = document.getElementById('log');
const $status = document.getElementById('status');
const $rid = document.getElementById('f-rid');
const $level = document.getElementById('f-level');
const $tail = document.getElementById('f-tail');
document.getElementById('f-clear').onclick = () => {
  $log.innerHTML = '';
  lastSeq = 0;
};
function escape(s) {
  return String(s).replace(/[&<>"']/g, c => ({
    '&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'
  }[c]));
}
function render(rec) {
  const div = document.createElement('div');
  div.className = 'row';
  div.innerHTML =
    `<span class="ts">${escape(rec.ts_human)}</span>` +
    `<span class="lvl ${escape(rec.level)}">${escape(rec.level)}</span>` +
    `<span class="rid">${escape(rec.request_id || '-')}</span>` +
    `<span class="msg">${escape(rec.message)}</span>`;
  $log.appendChild(div);
}
async function poll() {
  const params = new URLSearchParams();
  if ($rid.value.trim()) params.set('request_id', $rid.value.trim());
  if ($level.value) params.set('level', $level.value);
  if (lastSeq) params.set('since_seq', lastSeq);
  params.set('limit', '500');
  try {
    const r = await fetch('/api/logs?' + params.toString(), {cache: 'no-store'});
    if (!r.ok) throw new Error(r.status);
    const data = await r.json();
    if (data.entries.length) {
      for (const rec of data.entries) {
        render(rec);
        if (rec.seq > lastSeq) lastSeq = rec.seq;
      }
      if ($tail.checked) {
        window.scrollTo(0, document.body.scrollHeight);
      }
    }
    $status.textContent = `connected — ${data.count} new — seq ${lastSeq}`;
  } catch (e) {
    $status.textContent = 'disconnected: ' + e.message;
  }
}
$rid.oninput = $level.onchange = () => {
  $log.innerHTML = '';
  lastSeq = 0;
};
poll();
setInterval(poll, 1500);
</script>
</body>
</html>
"""


@app.get("/logs", include_in_schema=False)
async def logs_page():
    """HTML page that tails /api/logs in real time."""
    from fastapi.responses import HTMLResponse
    return HTMLResponse(content=_LOGS_HTML)


# ─────────────────────────────────────────────────────────────────────────────
# Diagnostics helper
# ─────────────────────────────────────────────────────────────────────────────

def _build_diagnostics(
    *,
    request_id: str,
    filename: str,
    width: int,
    height: int,
    bubbles: list,
    detector: BubbleDetector,
    elapsed_ms: float,
) -> Dict[str, Any]:
    seed_traces = getattr(detector, "_seed_traces", {}) or {}

    assigned    = [b for b in bubbles if b.dimension and b.dimension != "NO_DIMENSION"]
    unresolved  = [b.bubble_number for b in bubbles if not assigned or b not in assigned]
    review      = [b.bubble_number for b in bubbles if b.needs_review]
    traced      = [b.bubble_number for b in bubbles if b.bubble_number in seed_traces]
    confidences = [b.confidence for b in bubbles]

    return {
        "request_id": request_id,
        "image": {
            "filename":    filename,
            "width":       width,
            "height":      height,
            "megapixels":  round((width * height) / 1_000_000, 3),
        },
        "image_quality": getattr(detector, "_quality", None),
        "timing_ms": round(elapsed_ms, 2),
        "counts": {
            "total":      len(bubbles),
            "assigned":   len(assigned),
            "unresolved": len(bubbles) - len(assigned),
            "needs_review": len(review),
            "traced":     len(traced),
        },
        "quality": {
            "avg_confidence": round(float(np.mean(confidences)), 3) if confidences else 0.0,
            "min_confidence": round(float(min(confidences)), 3) if confidences else 0.0,
            "assignment_rate": round(len(assigned) / max(len(bubbles), 1), 3),
        },
        "unresolved_bubbles": unresolved,
        "review_bubbles":     review,
        "traced_bubbles":     traced,
    }


# ─────────────────────────────────────────────────────────────────────────────
# Exception handlers
# ─────────────────────────────────────────────────────────────────────────────

@app.exception_handler(HTTPException)
async def _http_handler(request: Request, exc: HTTPException):
    return JSONResponse(
        status_code=exc.status_code,
        content=ErrorResponse(error=f"HTTP {exc.status_code}", detail=exc.detail).model_dump(),
    )


@app.exception_handler(Exception)
async def _generic_handler(request: Request, exc: Exception):
    global _ERROR_COUNT
    _ERROR_COUNT += 1
    logger.exception("Unhandled exception: %s", exc)
    return JSONResponse(
        status_code=500,
        content=ErrorResponse(
            error="Internal server error",
            detail="An unexpected error occurred.",
        ).model_dump(),
    )
