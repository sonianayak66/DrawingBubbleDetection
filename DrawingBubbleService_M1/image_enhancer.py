"""
Real-ESRGAN-anime ONNX image enhancer.

Loads the lightweight `RealESR-AnimeVideo-v3_x4.onnx` model (2.4 MB)
and runs 4x super-resolution + denoise on engineering drawings.
The anime-trained variant is the right choice for CAD output —
unlike photo-trained super-res models it doesn't invent fake texture
on flat regions (white balloon interiors) and preserves thin
strokes (balloon outlines, leader lines) without distortion.

Public API: one method, `RealESRGANEnhancer.enhance(bgr_image)`.

Engineering choices:
  * Tiled inference. Models with dynamic shapes can run on the
    whole image in one shot, but a 1 MP input yields a 16 MP
    output and CPU memory spikes hard. We split into 256x256
    input tiles (1024x1024 output) with 16 px overlap to hide
    seams, and blend tile edges with a feathered alpha mask.
  * Lazy load. The first call pays ~3-5 s for session startup;
    subsequent calls reuse the loaded session.
  * No saturation/contrast boost on the output. The model already
    enhances contrast as part of its restoration; piling a manual
    HSV boost on top distorts OCR character shapes (verified
    earlier in this session — "9" turned into "96").
"""

from __future__ import annotations

import logging
import os
import threading
from typing import Optional

import cv2
import numpy as np
import onnxruntime as ort

logger = logging.getLogger(__name__)


class RealESRGANEnhancer:
    """ONNX-runtime backed 4x super-resolution enhancer.

    Thread-safe: the underlying onnxruntime session can be shared
    across threads, and the loader uses a lock so concurrent first
    calls don't race to create the session.
    """

    # Empirically tuned on this model: input tiles larger than 320
    # use too much memory; smaller than 192 introduce too many
    # seam blends. 256 is the sweet spot.
    DEFAULT_TILE = 256
    # 16 px input overlap = 64 px output overlap at 4x scale, plenty
    # for a feathered seam blend.
    DEFAULT_OVERLAP = 16
    # Native model upscale ratio. Don't change without re-exporting
    # the ONNX from a different .pth.
    SCALE = 4
    # The 4x output is blended in float32 arrays. Keep converted
    # PDF/TIFF pages from turning one enhancement pass into a multi-GB
    # allocation.
    DEFAULT_MAX_OUTPUT_PIXELS = 18_000_000

    def __init__(
        self,
        model_path: str,
        tile: int = DEFAULT_TILE,
        overlap: int = DEFAULT_OVERLAP,
        providers: Optional[list] = None,
        max_output_pixels: int = DEFAULT_MAX_OUTPUT_PIXELS,
    ):
        if not os.path.exists(model_path):
            raise FileNotFoundError(f"ONNX model not found: {model_path}")
        self.model_path = model_path
        self.tile = tile
        self.overlap = overlap
        self.providers = providers or ["CPUExecutionProvider"]
        self.max_output_pixels = max_output_pixels
        self._session: Optional[ort.InferenceSession] = None
        self._lock = threading.Lock()

    def _get_session(self) -> ort.InferenceSession:
        """Lazy-load the ONNX session. Thread-safe."""
        if self._session is not None:
            return self._session
        with self._lock:
            if self._session is not None:
                return self._session
            logger.info("Loading Real-ESRGAN model from %s", self.model_path)
            opts = ort.SessionOptions()
            # Use all available cores; CPU inference is the bottleneck.
            opts.intra_op_num_threads = max(1, (os.cpu_count() or 4) - 1)
            opts.graph_optimization_level = ort.GraphOptimizationLevel.ORT_ENABLE_ALL
            self._session = ort.InferenceSession(
                self.model_path, opts, providers=self.providers,
            )
            in_name = self._session.get_inputs()[0].name
            logger.info("Real-ESRGAN session ready (input='%s', providers=%s)",
                        in_name, self._session.get_providers())
            return self._session

    @staticmethod
    def _bgr_to_input(tile_bgr: np.ndarray) -> np.ndarray:
        """BGR uint8 → RGB float32 [0,1] NCHW tensor."""
        rgb = cv2.cvtColor(tile_bgr, cv2.COLOR_BGR2RGB).astype(np.float32) / 255.0
        # HWC → NCHW
        return np.transpose(rgb, (2, 0, 1))[None, ...]

    @staticmethod
    def _output_to_bgr(arr: np.ndarray) -> np.ndarray:
        """NCHW float32 [0,1] → BGR uint8 HWC."""
        # Drop batch, NCHW → HWC
        rgb = np.transpose(arr[0], (1, 2, 0))
        rgb = np.clip(rgb, 0.0, 1.0)
        rgb_u8 = (rgb * 255.0 + 0.5).astype(np.uint8)
        return cv2.cvtColor(rgb_u8, cv2.COLOR_RGB2BGR)

    def _build_feather_mask(self, h: int, w: int) -> np.ndarray:
        """Linear feather mask used to blend tile seams.

        The mask is 1.0 at the tile centre and ramps down to 0.0
        across `overlap*SCALE` pixels at the boundaries. Applied
        per-tile so overlapping tiles average smoothly.
        """
        fade = max(1, self.overlap * self.SCALE)
        m = np.ones((h, w), dtype=np.float32)
        ramp = np.linspace(0.0, 1.0, fade, dtype=np.float32)
        # Left / right
        m[:, :fade] *= ramp[None, :]
        m[:, -fade:] *= ramp[::-1][None, :]
        # Top / bottom
        m[:fade, :] *= ramp[:, None]
        m[-fade:, :] *= ramp[::-1][:, None]
        return m

    def enhance(self, bgr: np.ndarray) -> np.ndarray:
        """Run 4x super-resolution + denoise on the BGR image."""
        if bgr is None or bgr.size == 0:
            raise ValueError("enhance() received empty image")
        if bgr.ndim != 3 or bgr.shape[2] != 3:
            raise ValueError(f"expected BGR image, got shape {bgr.shape}")

        session = self._get_session()
        input_name = session.get_inputs()[0].name

        h, w = bgr.shape[:2]
        scale = self.SCALE
        out_h, out_w = h * scale, w * scale
        out_pixels = out_h * out_w
        if out_pixels > self.max_output_pixels:
            raise ValueError(
                "Real-ESRGAN output would be too large: "
                f"{out_w}x{out_h} ({out_pixels:,} px > "
                f"{self.max_output_pixels:,} px cap)"
            )

        # Output accumulators — sum of (tile * mask) and sum of mask,
        # then divide at the end. This is the canonical seam-blend
        # approach for tiled image processing.
        out_sum = np.zeros((out_h, out_w, 3), dtype=np.float32)
        weight = np.zeros((out_h, out_w), dtype=np.float32)

        step = self.tile - self.overlap
        if step <= 0:
            raise ValueError("tile must be larger than overlap")

        for y in range(0, h, step):
            for x in range(0, w, step):
                y1, x1 = y, x
                y2 = min(y + self.tile, h)
                x2 = min(x + self.tile, w)
                # If we ran off the edge, shift the window back so
                # we always feed a full `tile`-sized patch (the
                # model handles dynamic shapes, but constant-size
                # tiles execute fastest because graph shape inference
                # only happens once).
                if y2 - y1 < self.tile and y2 == h:
                    y1 = max(0, h - self.tile)
                if x2 - x1 < self.tile and x2 == w:
                    x1 = max(0, w - self.tile)

                tile_bgr = bgr[y1:y2, x1:x2]
                inp = self._bgr_to_input(tile_bgr)
                out_arr = session.run(None, {input_name: inp})[0]
                tile_out = self._output_to_bgr(out_arr).astype(np.float32)

                th, tw = tile_out.shape[:2]
                mask = self._build_feather_mask(th, tw)

                oy1, ox1 = y1 * scale, x1 * scale
                oy2, ox2 = oy1 + th, ox1 + tw
                out_sum[oy1:oy2, ox1:ox2] += tile_out * mask[..., None]
                weight[oy1:oy2, ox1:ox2] += mask

        # Avoid divide-by-zero on any unfilled pixel (shouldn't happen
        # with proper overlap but cheap insurance).
        weight = np.maximum(weight, 1e-6)
        out_bgr = (out_sum / weight[..., None]).clip(0, 255).astype(np.uint8)
        return out_bgr
