"""
Bubble Detection FastAPI Service - MPCRS Edition
=================================================
Stateless. Secure. Minimal.

Two endpoints only:
  GET  /api/health   - Health check (no auth required)
  POST /api/detect   - Send image bytes, receive bubbles JSON + annotated image base64

Security:
  POST /api/detect requires header:  X-API-Key: <value from .env>

Responsibilities:
  - This service ONLY does image processing and OCR.
  - File storage, PDF conversion, and DB saving are handled by MPCRS (C#).
  - One image per request. C# sends images one at a time (including per-page for PDFs).
"""

import os
import base64
import logging

import cv2
import numpy as np
from dotenv import load_dotenv
from fastapi import FastAPI, UploadFile, File, HTTPException, Security, Depends
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
from fastapi.security.api_key import APIKeyHeader

from .detector import BubbleDetector, DetectionConfig

# ── Startup ──────────────────────────────────────────────────────
load_dotenv()

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s  %(levelname)-8s  %(message)s",
    datefmt="%Y-%m-%d %H:%M:%S",
)
logger = logging.getLogger(__name__)

# ── API Key Auth ─────────────────────────────────────────────────
API_KEY = os.getenv("BUBBLE_API_KEY", "")
if not API_KEY:
    raise RuntimeError(
        "BUBBLE_API_KEY is not set in the .env file. "
        "Cannot start service without a key."
    )

_api_key_header = APIKeyHeader(name="X-API-Key", auto_error=False)


async def require_api_key(api_key: str = Security(_api_key_header)):
    if not api_key or api_key != API_KEY:
        logger.warning("Rejected request — invalid or missing API key.")
        raise HTTPException(status_code=401, detail="Invalid or missing API key.")
    return api_key


# ── FastAPI App ───────────────────────────────────────────────────
app = FastAPI(
    title="Bubble Detection Service - MPCRS",
    version="3.0.0",
    docs_url=None,    # Swagger UI disabled in production
    redoc_url=None,
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost", "http://127.0.0.1"],
    allow_methods=["GET", "POST"],
    allow_headers=["*"],
)

# Allow any Host header — this service only binds to 127.0.0.1 so it's
# already unreachable from outside.  On deployed servers the C# HttpClient
# may send a Host header that doesn't match "127.0.0.1" (e.g. the server's
# hostname or IP), which uvicorn / Starlette rejects as "Invalid host".
from starlette.middleware.trustedhost import TrustedHostMiddleware
app.add_middleware(TrustedHostMiddleware, allowed_hosts=["*"])

ALLOWED_EXTENSIONS = {".png", ".jpg", ".jpeg", ".tiff", ".tif", ".bmp"}

# ── Detector Singleton ────────────────────────────────────────────
# Loaded once on first request. OCR model load takes ~2-3 seconds.
_detector: BubbleDetector = None


def get_detector() -> BubbleDetector:
    global _detector
    if _detector is None:
        logger.info("Loading BubbleDetector and OCR model (first request only)...")
        _detector = BubbleDetector(DetectionConfig())
        logger.info("BubbleDetector ready.")
    return _detector


# ── Endpoints ─────────────────────────────────────────────────────

@app.get("/api/health")
async def health():
    """
    Public health check — no API key required.
    MPCRS can call this on startup to confirm the Python service is running.
    """
    return {
        "status": "ok",
        "version": "3.0.0",
        "engine": "rapidocr",
    }


@app.post("/api/detect", dependencies=[Depends(require_api_key)])
async def detect(file: UploadFile = File(...)):
    """
    Accepts a single image file (PNG, JPG, TIFF, BMP).

    Returns JSON:
    {
        "bubble_count": <int>,
        "bubbles": [
            {
                "bubble_number": "1",
                "dimension": "4.85/5.00",
                "zone": "",
                "x": 312,
                "y": 208,
                "radius": 28,
                "confidence": 0.97,
                "needs_review": false
            },
            ...
        ],
        "annotated_image_base64": "<PNG encoded as base64 string>"
    }

    The annotated_image_base64 is a PNG of the original drawing with
    detected bubbles highlighted. C# should decode and save this as a .png file.

    PDF handling: C# must convert PDF pages to images before sending.
    Send one image per request (one page at a time).
    """
    filename = file.filename or "upload"
    ext = os.path.splitext(filename)[1].lower()

    if ext not in ALLOWED_EXTENSIONS:
        raise HTTPException(
            status_code=400,
            detail=(
                f"Unsupported file type '{ext}'. "
                f"Allowed types: {', '.join(sorted(ALLOWED_EXTENSIONS))}. "
                f"For PDFs, convert to image pages on the C# side before sending."
            ),
        )

    try:
        contents = await file.read()

        if len(contents) == 0:
            raise HTTPException(status_code=400, detail="Uploaded file is empty.")

        # Decode image directly from bytes — no temp file written to disk
        nparr = np.frombuffer(contents, np.uint8)
        img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

        if img is None:
            raise HTTPException(
                status_code=400,
                detail="Could not decode image. File may be corrupted or in an unsupported format.",
            )

        h, w = img.shape[:2]
        logger.info(f"Processing '{filename}'  |  size: {w}x{h}  |  bytes: {len(contents):,}")

        detector = get_detector()
        bubbles, annotated_img = detector.detect_from_array(img)

        # Encode annotated image → PNG bytes → base64 string
        encode_success, buffer = cv2.imencode(".png", annotated_img)
        if not encode_success:
            raise HTTPException(status_code=500, detail="Failed to encode annotated image.")

        annotated_b64 = base64.b64encode(buffer.tobytes()).decode("utf-8")

        logger.info(f"Done '{filename}'  |  bubbles found: {len(bubbles)}")

        return JSONResponse(content={
            "bubble_count": len(bubbles),
            "bubbles": [b.to_dict() for b in bubbles],
            "annotated_image_base64": annotated_b64,
        })

    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Detection failed for '{filename}': {e}", exc_info=True)
        raise HTTPException(
            status_code=500,
            detail=f"Detection error: {str(e)}",
        )
