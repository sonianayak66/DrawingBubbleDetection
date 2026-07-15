"""In-memory log capture for the detection service.

Provides:
  - `attach_buffer(level, capacity)`: install a ring-buffer log handler on
    the root logger so logs continue to print to terminal AND get captured.
  - `get_logs(...)`: read-back filtered by request_id / time / level.
  - `request_context(request_id)`: context manager that tags every log
    record emitted during the block with the given request_id.

Designed so the existing `logging.basicConfig(...)` in main.py keeps writing
to stdout (terminal/client console) unchanged — this module ADDS a second
sink, it doesn't replace the first.
"""
from __future__ import annotations

import logging
import threading
import time
from collections import deque
from contextlib import contextmanager
from contextvars import ContextVar
from typing import Deque, Dict, List, Optional

# Per-async-task / per-thread current request id. Logs emitted inside an
# `async with request_context(rid)` block are tagged with that rid.
_current_request_id: ContextVar[Optional[str]] = ContextVar(
    "_bubble_log_current_request_id", default=None,
)


class _RingBufferHandler(logging.Handler):
    """Logging handler that keeps the last N records in a thread-safe deque."""

    def __init__(self, capacity: int = 1000) -> None:
        super().__init__()
        self._buf: Deque[Dict] = deque(maxlen=capacity)
        self._lock = threading.Lock()
        self._seq = 0  # monotonic record id, used for `since` polling

    def emit(self, record: logging.LogRecord) -> None:
        try:
            msg = record.getMessage()
        except Exception:
            msg = record.msg
        rid = _current_request_id.get()
        with self._lock:
            self._seq += 1
            self._buf.append({
                "seq": self._seq,
                "ts": record.created,
                "ts_human": time.strftime(
                    "%Y-%m-%d %H:%M:%S",
                    time.localtime(record.created),
                ) + f".{int((record.created % 1) * 1000):03d}",
                "level": record.levelname,
                "logger": record.name,
                "request_id": rid,
                "message": msg,
            })

    def head_seq(self) -> int:
        """Return the seq of the most-recently emitted record (0 if empty).

        Used by clients that want to start a live tail without first
        having to receive the whole buffer just to learn the max seq.
        """
        with self._lock:
            return self._seq

    def snapshot(
        self,
        request_id: Optional[str] = None,
        since_seq: Optional[int] = None,
        level: Optional[str] = None,
        limit: int = 500,
    ) -> List[Dict]:
        """Return a filtered copy of the buffer (oldest -> newest)."""
        level_min = (
            getattr(logging, level.upper(), None)
            if level else None
        )
        out: List[Dict] = []
        with self._lock:
            for rec in self._buf:
                if since_seq is not None and rec["seq"] <= since_seq:
                    continue
                if request_id and rec["request_id"] != request_id:
                    continue
                if level_min is not None:
                    rec_level_no = getattr(logging, rec["level"], 0)
                    if rec_level_no < level_min:
                        continue
                out.append(rec)
                if len(out) >= limit:
                    break
        return out


_handler: Optional[_RingBufferHandler] = None


def attach_buffer(level: int = logging.INFO, capacity: int = 1000) -> _RingBufferHandler:
    """Install the ring-buffer handler on the root logger. Idempotent.

    This runs IN ADDITION to whatever stdout/stderr handler `basicConfig`
    already configured — so the terminal output is unchanged.
    """
    global _handler
    if _handler is not None:
        return _handler
    h = _RingBufferHandler(capacity=capacity)
    h.setLevel(level)
    h.setFormatter(logging.Formatter("%(message)s"))
    logging.getLogger().addHandler(h)
    _handler = h
    return h


def get_logs(
    request_id: Optional[str] = None,
    since_seq: Optional[int] = None,
    level: Optional[str] = None,
    limit: int = 500,
) -> List[Dict]:
    if _handler is None:
        return []
    return _handler.snapshot(
        request_id=request_id,
        since_seq=since_seq,
        level=level,
        limit=limit,
    )


def get_head_seq() -> int:
    """Highest seq currently present in the buffer (0 if no handler yet)."""
    return 0 if _handler is None else _handler.head_seq()


@contextmanager
def request_context(request_id: str):
    """Tag every log record emitted inside this block with `request_id`."""
    token = _current_request_id.set(request_id)
    try:
        yield
    finally:
        _current_request_id.reset(token)
