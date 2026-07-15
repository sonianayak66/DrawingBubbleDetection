#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# DrawingBubbleService — Offline installation helper
#
# Step 1 (internet machine):
#   bash offline_install.sh download
#   → downloads all wheels into ./wheels/
#
# Step 2 (offline machine, copy the whole folder):
#   bash offline_install.sh install
#   → installs from ./wheels/ with no internet
# ─────────────────────────────────────────────────────────────────────────────
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
WHEEL_DIR="$SCRIPT_DIR/wheels"
REQ="$SCRIPT_DIR/requirements.txt"

PYTHON=$(command -v python3 || command -v python)
if [ -z "$PYTHON" ]; then
    echo "ERROR: Python not found." >&2; exit 1
fi

ACTION=${1:-install}

case "$ACTION" in
  download)
    echo "Downloading wheels to $WHEEL_DIR ..."
    mkdir -p "$WHEEL_DIR"
    "$PYTHON" -m pip download \
        -r "$REQ" \
        --dest "$WHEEL_DIR" \
        --prefer-binary
    echo ""
    echo "Done. Copy the entire DrawingBubbleService/ folder to the offline machine"
    echo "then run:  bash offline_install.sh install"
    ;;
  install)
    if [ ! -d "$WHEEL_DIR" ]; then
        echo "ERROR: wheels/ directory not found." >&2
        echo "Run 'bash offline_install.sh download' on an internet-connected machine first." >&2
        exit 1
    fi
    echo "Installing from $WHEEL_DIR (offline) ..."
    "$PYTHON" -m pip install \
        --no-index \
        --find-links="$WHEEL_DIR" \
        -r "$REQ"
    echo ""
    echo "Installation complete."
    ;;
  *)
    echo "Usage: bash offline_install.sh [download|install]"
    exit 1
    ;;
esac
