from __future__ import annotations

from functools import lru_cache

from rapidocr_onnxruntime import RapidOCR


@lru_cache(maxsize=1)
def get_ocr() -> RapidOCR:
    return RapidOCR(det_use_cuda=False, cls_use_cuda=False, rec_use_cuda=False)

