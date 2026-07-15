import os, logging
from enum import Enum
logger = logging.getLogger(__name__)

class OCREngine(Enum):
    RAPID = "rapid"
    AUTO = "auto"

class OCRConfig:
    def __init__(self, preferred_engine="auto"):
        self.preferred_engine = OCREngine(preferred_engine.lower())
        self.actual_engine = None
        self.fallback_used = False
        self.compatibility_checked = False

    def check_rapid_compatibility(self):
        try:
            from rapidocr_onnxruntime import RapidOCR
            import numpy as np
            RapidOCR()(np.zeros((100,100,3), dtype=np.uint8))
            return True
        except:
            return False

    def select_engine(self):
        if self.compatibility_checked:
            return self.actual_engine
        self.compatibility_checked = True
        if self.check_rapid_compatibility():
            self.actual_engine = OCREngine.RAPID
        else:
            raise RuntimeError("RapidOCR not available")
        return self.actual_engine

    def get_engine_info(self):
        return {"actual_engine": self.actual_engine.value if self.actual_engine else None, "fallback_used": self.fallback_used}

_ocr_config = None

def get_ocr_config(preferred_engine="auto"):
    global _ocr_config
    if _ocr_config is None:
        _ocr_config = OCRConfig(preferred_engine)
    return _ocr_config

def get_preferred_ocr_engine():
    return os.environ.get("OCR_ENGINE", "auto").lower()
