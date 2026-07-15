#!/usr/bin/env python3
"""
test_api.py — Quick smoke test for the Bubble Detection API.

Usage:
    python test_api.py [image_path] [api_key] [port]

Examples:
    python test_api.py path/to/image.jpeg my-secret-key 8000
    python test_api.py                         # uses defaults
"""

import sys
import os
import json
import urllib.request
import urllib.error

# ─────────────────────────────────────────────────────────────────────────────
# Configuration — populated by _parse_args() when run as a CLI. Defined at
# module scope as safe defaults so importers (e.g. pytest collection)
# don't blow up reading sys.argv at import time.
# ─────────────────────────────────────────────────────────────────────────────

IMAGE_PATH = "sample.jpeg"
API_KEY    = os.environ.get("BUBBLE_API_KEY", "")
PORT       = 8000
HOST       = f"http://localhost:{PORT}"


def _parse_args() -> None:
    """Read CLI args into the module-level config. Called only from main()."""
    global IMAGE_PATH, API_KEY, PORT, HOST
    if len(sys.argv) > 1:
        IMAGE_PATH = sys.argv[1]
    if len(sys.argv) > 2:
        API_KEY = sys.argv[2]
    if len(sys.argv) > 3:
        PORT = int(sys.argv[3])
    HOST = f"http://localhost:{PORT}"


def _request(method: str, url: str, *, headers=None, data=None):
    req = urllib.request.Request(url, method=method, headers=headers or {}, data=data)
    try:
        with urllib.request.urlopen(req, timeout=60) as resp:
            return resp.status, json.loads(resp.read())
    except urllib.error.HTTPError as e:
        return e.code, json.loads(e.read())
    except Exception as e:
        return 0, {"error": str(e)}


def check_health():
    print("── Health check ─────────────────────────────────")
    status, body = _request("GET", f"{HOST}/api/health")
    if status == 200:
        print(f"  Status   : {body.get('status')}")
        print(f"  Version  : {body.get('version')}")
        print(f"  Uptime   : {body.get('uptime_seconds')}s")
        print(f"  Memory   : {body.get('memory_usage_mb')} MB")
    else:
        print(f"  FAILED ({status}): {body}")
    return status == 200


def detect_image(path: str):
    print(f"\n── Detect: {path} ───────────────────────────────")

    if not os.path.isfile(path):
        print(f"  ERROR: file not found: {path}")
        return

    with open(path, "rb") as f:
        image_data = f.read()

    # Build multipart/form-data manually (no external deps)
    boundary = "----BubbleBoundary7MA4YWxkTrZu0gW"
    filename  = os.path.basename(path)
    ext       = os.path.splitext(filename)[1].lower()
    ct_map    = {".jpg": "image/jpeg", ".jpeg": "image/jpeg",
                 ".png": "image/png",  ".bmp": "image/bmp",
                 ".tif": "image/tiff", ".tiff": "image/tiff"}
    content_type = ct_map.get(ext, "application/octet-stream")

    body_parts = [
        f"--{boundary}\r\n".encode(),
        f'Content-Disposition: form-data; name="file"; filename="{filename}"\r\n'.encode(),
        f"Content-Type: {content_type}\r\n\r\n".encode(),
        image_data,
        f"\r\n--{boundary}--\r\n".encode(),
    ]
    body = b"".join(body_parts)

    headers = {
        "X-API-Key": API_KEY,
        "Content-Type": f"multipart/form-data; boundary={boundary}",
        "Content-Length": str(len(body)),
    }

    url = f"{HOST}/api/detect?include_diagnostics=true"
    status, result = _request("POST", url, headers=headers, data=body)

    if status != 200:
        print(f"  ERROR ({status}): {result}")
        return

    bubbles = result.get("bubbles", [])
    elapsed = result.get("processing_time_ms", 0)
    print(f"  Bubbles detected : {result.get('bubble_count')}")
    print(f"  Processing time  : {elapsed:.1f} ms")
    print()

    # Print results table
    header = f"  {'#':<6} {'Dimension':<30} {'Conf':>6}  {'Review':<8}  Position"
    print(header)
    print("  " + "-" * 75)
    for b in sorted(bubbles, key=lambda x: (len(str(x.get("bubble_number",""))), str(x.get("bubble_number","")))):
        num   = b.get("bubble_number", "?")
        dim   = b.get("dimension", "NO_DIMENSION")[:28]
        conf  = b.get("confidence", 0)
        rev   = "⚠ review" if b.get("needs_review") else "ok"
        x, y  = b.get("x", 0), b.get("y", 0)
        print(f"  {str(num):<6} {dim:<30} {conf:>6.2f}  {rev:<8}  ({x}, {y})")

    diag = result.get("diagnostics", {})
    if diag:
        q = diag.get("quality", {})
        c = diag.get("counts", {})
        print()
        print(f"  Assignment rate  : {q.get('assignment_rate', 0)*100:.0f}%")
        print(f"  Avg confidence   : {q.get('avg_confidence', 0):.3f}")
        print(f"  Traced bubbles   : {c.get('traced', 0)} / {c.get('total', 0)}")
        if diag.get("review_bubbles"):
            print(f"  Needs review     : {diag['review_bubbles']}")


def main():
    _parse_args()
    print()
    print("╔══════════════════════════════════════════════════╗")
    print("║      Bubble Detection API — Smoke Test           ║")
    print("╚══════════════════════════════════════════════════╝")
    print(f"  Host    : {HOST}")
    print(f"  API key : {API_KEY[:8]}...")
    print()

    if not check_health():
        print("\nServer is not reachable. Make sure the service is running:")
        print("  Linux/Mac : bash DrawingBubbleService/start.sh")
        print("  Windows   : DrawingBubbleService\\run.bat")
        sys.exit(1)

    detect_image(IMAGE_PATH)

    print("\n✓ Test complete.\n")


if __name__ == "__main__":
    main()
