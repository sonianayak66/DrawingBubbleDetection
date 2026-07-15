#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# DrawingBubbleService — Linux / macOS startup script
# Usage:  bash start.sh [port]   (default port: 8000)
# ─────────────────────────────────────────────────────────────────────────────
set -e

PORT=${1:-8000}
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo ""
echo "╔══════════════════════════════════════════════════════╗"
echo "║       Drawing Bubble Detection Service v4.0          ║"
echo "╚══════════════════════════════════════════════════════╝"
echo ""

# ── Check Python version ──────────────────────────────────────────────────────
PYTHON=$(command -v python3 || command -v python)
if [ -z "$PYTHON" ]; then
    echo "ERROR: Python not found. Install Python 3.12." >&2
    exit 1
fi

PY_VER=$("$PYTHON" -c "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')")
echo "Python: $PY_VER  ($PYTHON)"

# ── Check .env ────────────────────────────────────────────────────────────────
if [ ! -f "$SCRIPT_DIR/.env" ]; then
    if [ -f "$SCRIPT_DIR/.env.example" ]; then
        echo ""
        echo "WARNING: .env not found. Creating from .env.example ..."
        cp "$SCRIPT_DIR/.env.example" "$SCRIPT_DIR/.env"
        echo ">>> Edit $SCRIPT_DIR/.env and set BUBBLE_API_KEY before production use!"
        echo ""
    else
        echo "ERROR: .env file not found." >&2
        exit 1
    fi
fi

# ── Check dependencies ────────────────────────────────────────────────────────
echo "Checking dependencies ..."
"$PYTHON" -c "import cv2, numpy, rapidocr_onnxruntime, fastapi, uvicorn" 2>/dev/null || {
    echo ""
    echo "Some packages are missing. Installing from requirements.txt ..."
    "$PYTHON" -m pip install -r "$SCRIPT_DIR/requirements.txt" --quiet
}
echo "Dependencies OK"

# ── Start server ──────────────────────────────────────────────────────────────
echo ""
echo "Starting API server on http://0.0.0.0:$PORT"
echo "Swagger UI:  http://localhost:$PORT/docs"
echo "Health:      http://localhost:$PORT/api/health"
echo ""
echo "Press Ctrl+C to stop."
echo ""

cd "$SCRIPT_DIR"
"$PYTHON" -m uvicorn main:app \
    --host 0.0.0.0 \
    --port "$PORT" \
    --workers 1 \
    --log-level info
