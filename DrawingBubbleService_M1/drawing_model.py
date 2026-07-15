"""
Drawing model — first-class objects representing what's actually IN
an engineering drawing.

The legacy detector (detector.py) operates on flat token lists with
several dozen post-processing passes ("if dim looks weird, clear it";
"if bubble looks like a fragment, suppress it"; etc.) accumulated over
time as workarounds for specific failure cases. Each new image type
risks breaking some of those rules.

This module replaces that with EXPLICIT STRUCTURE. A drawing has
balloons, leaders, and dimension regions, plus the relationships
between them. Detection produces a `Drawing` object directly; there
are no "post-processing patches" — every decision is made in one
place with the full picture.

The contract:

  - Every Balloon, Leader, DimensionRegion has a `confidence` float
    in [0, 1] that propagates upward from its constituent signals
    (OCR confidence, Hough strength, mask-rim overlap, etc.).

  - Relationships are explicit: a Leader references its Balloon and
    (optionally) its target DimensionRegion; a Balloon references
    its Leader; a DimensionRegion references its assigning Leader.
    Nothing is implicit / inferred from text patterns at consumer
    time.

  - There is exactly ONE pass that decides bubble→dim assignment —
    `match_leaders_to_regions` — and it operates on the complete
    population of balloons + dims with all geometric signals
    available simultaneously. No "let me clear this dim because it
    looks garbage" downstream.

This module declares the types only. The detection pipeline that
populates them lives in `principled_detector.py`.
"""
from __future__ import annotations

from dataclasses import dataclass, field
from typing import List, Optional, Tuple


Point = Tuple[float, float]
Rect = Tuple[float, float, float, float]  # (x1, y1, x2, y2)


@dataclass
class Balloon:
    """A numbered annotation balloon: the circle + the digit(s) inside.

    Position and radius are post-detection pixel coordinates. `number`
    is the OCR'd digit string (e.g., "11", "11A", "?" when the digit
    could not be read). `confidence` is the combined detection signal:
    Hough strength × OCR digit confidence × annotation-rim overlap,
    clamped to [0, 1].

    `leader` is set after the trace step. None when no leader was
    found (e.g., a stray circle).
    """
    cx: float
    cy: float
    r: float
    number: str
    confidence: float
    leader: Optional["Leader"] = None

    @property
    def is_numbered(self) -> bool:
        return self.number and self.number != "?"


@dataclass
class Leader:
    """The line connecting a balloon's rim to its dimension region.

    `path` is the ordered list of pixel coordinates the trace found.
    `endpoint` is the trace's last pixel. `target_region` is the
    DimensionRegion the trace points at (set by the matching step).

    `confidence` here measures the TRACE quality: long, monotonic-
    direction paths score high; short rim-loops score low.
    """
    balloon: Balloon
    path: List[Point]
    endpoint: Point
    confidence: float
    target_region: Optional["DimensionRegion"] = None

    @property
    def length(self) -> int:
        return len(self.path)


@dataclass
class DimensionRegion:
    """A spatially-clustered group of OCR tokens forming one dimension.

    `text` is the combined text of the constituent tokens, in reading
    order. `bbox` is the union bounding box of the tokens. `tokens`
    keeps the originals for downstream reference (e.g., subscript
    handling).

    `kind` is a structural classification ("numeric", "diameter",
    "thread", "chamfer", "callout_ref", ...) computed once by the
    region builder. It is NOT a whitelist filter — every region is
    kept; the kind just informs the matching cost.

    `owner_leader` is set by the matching step. A region may serve
    multiple balloons (parent + suffix variant: "11" and "11A"
    share the same dim), but `owner_leader` records the primary
    assignment.
    """
    bbox: Rect
    text: str
    tokens: List["DimToken"]
    kind: str
    confidence: float
    owner_leader: Optional[Leader] = None

    @property
    def cx(self) -> float:
        return (self.bbox[0] + self.bbox[2]) / 2.0

    @property
    def cy(self) -> float:
        return (self.bbox[1] + self.bbox[3]) / 2.0


@dataclass
class DimToken:
    """A single OCR token (one bounding box, one text string).

    These are the building blocks of DimensionRegions. Kept separately
    so the region builder can group tokens spatially and the matcher
    can use individual token positions when refining endpoint searches.
    """
    text: str
    bbox: Rect
    confidence: float

    @property
    def cx(self) -> float:
        return (self.bbox[0] + self.bbox[2]) / 2.0

    @property
    def cy(self) -> float:
        return (self.bbox[1] + self.bbox[3]) / 2.0


@dataclass
class Drawing:
    """The full result of detection on one image.

    `image_shape` is (height, width) of the (possibly preprocessed)
    image the coordinates refer to. `balloons`, `leaders`, and
    `dim_regions` are the populated lists; back-references between
    them are set after matching.

    `unattached_dim_regions` are dims that no leader claimed — useful
    for diagnostics (e.g., "these dims exist in the drawing but no
    balloon points at them; either the user forgot a balloon or our
    trace missed it").
    """
    image_shape: Tuple[int, int]
    balloons: List[Balloon] = field(default_factory=list)
    leaders: List[Leader] = field(default_factory=list)
    dim_regions: List[DimensionRegion] = field(default_factory=list)

    @property
    def unattached_dim_regions(self) -> List[DimensionRegion]:
        return [r for r in self.dim_regions if r.owner_leader is None]

    @property
    def numbered_balloons(self) -> List[Balloon]:
        return [b for b in self.balloons if b.is_numbered]

    def to_legacy_pairs(self) -> List[Tuple[str, str]]:
        """Flatten to the legacy `(bubble_number, dimension_text)` shape
        the existing pipeline consumers expect. Useful while migrating
        — the new detector can stand in for the old API by returning
        this flat list."""
        out: List[Tuple[str, str]] = []
        for b in self.numbered_balloons:
            dim_text = ""
            if b.leader is not None and b.leader.target_region is not None:
                dim_text = b.leader.target_region.text
            out.append((b.number, dim_text))
        return out
