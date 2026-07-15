import logging
import time
from typing import Dict, List
from dataclasses import dataclass
from pathlib import Path

@dataclass
class ProcessingMetrics:
    image_name: str
    timestamp: str
    total_bubbles: int
    successful_assignments: int
    failed_assignments: int
    ocr_misses: int
    ranking_failures: int
    candidate_generation_failures: int
    avg_runtime: float
    confidence_distribution: Dict[str, int]
    failure_reasons: Dict[str, int]

@dataclass
class BubbleMetrics:
    bubble_number: str
    x: float
    y: float
    dimension: str
    confidence: float
    needs_review: bool
    review_reason: str
    candidate_count: int
    top_candidate_score: float
    processing_time: float

class ProductionLogger:
    def __init__(self, log_dir="logs"):
        self.log_dir = Path(log_dir)
        self.log_dir.mkdir(exist_ok=True)
        self.logger = logging.getLogger("production")
        self.session_metrics: List[ProcessingMetrics] = []
        self.bubble_metrics: List[BubbleMetrics] = []
        self.session_start = time.time()

    def log_image_processing_start(self, image_name):
        self.logger.info("Processing: %s", image_name)

    def log_bubble_result(self, metrics: BubbleMetrics):
        self.bubble_metrics.append(metrics)

    def log_session_metrics(self, metrics: ProcessingMetrics):
        self.session_metrics.append(metrics)

production_logger = ProductionLogger()
