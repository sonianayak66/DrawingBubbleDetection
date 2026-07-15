# grammar_rules_generalized.py
"""
Grammar rules for mechanical drawing interpretation.

Purpose:
- understand OCR tokens as drafting grammar, not just text
- classify special symbols and semantic patterns
- preserve prefixes like B2 / C3
- detect '=' and repeated/shared-value style notation
- support downstream leader/path/linking logic

Design notes:
- deterministic and training-free
- conservative normalization only
- no image-specific assumptions
"""

from __future__ import annotations

from dataclasses import dataclass
from typing import List, Optional, Tuple
import re


DIAMETER = "Ø"
DEGREE = "°"
PLUS_MINUS = "±"


# ── Data Model ────────────────────────────────────────────────────

@dataclass
class GrammarToken:
    raw_text: str
    text: str
    grammar_type: str
    semantic_type: str
    prefix: str = ""
    value: str = ""
    is_reference_like: bool = False
    is_repeat_like: bool = False
    is_shared_like: bool = False


# ── Public API ────────────────────────────────────────────────────

def parse_grammar_token(text: str) -> GrammarToken:
    """
    Convert normalized OCR text into a grammar-aware token.

    grammar_type examples:
      symbol
      prefixed_dimension
      thread_dimension
      diameter_dimension
      radius_dimension
      chamfer_dimension
      angle_dimension
      tolerance_dimension
      numeric_pair
      numeric_single
      keyword
      reference_note
      coordinate_note
      unknown

    semantic_type examples:
      equality
      chamfer
      thread
      diameter
      radius
      angle
      numeric
      pair
      tolerance
      reference
      coordinate
      keyword
      unknown
    """
    raw = (text or "").strip()
    t = _normalize_grammar_text(raw)

    if not t:
        return GrammarToken(
            raw_text=raw,
            text=t,
            grammar_type="unknown",
            semantic_type="unknown",
            value=t,
        )

    # 1. exact / symbol tokens
    if _is_equal_symbol(t):
        return GrammarToken(
            raw_text=raw,
            text=t,
            grammar_type="symbol",
            semantic_type="equality",
            value="=",
            is_repeat_like=True,
            is_shared_like=True,
        )

    # 2. prefixed callouts like B2 0.5x45° / A1 Ø5.0 / C3 10.0/9.8
    pref = _extract_prefix_dimension(t)
    if pref is not None:
        prefix, value = pref
        semantic = _semantic_from_value(value)
        return GrammarToken(
            raw_text=raw,
            text=f"{prefix} {value}",
            grammar_type="prefixed_dimension",
            semantic_type=semantic,
            prefix=prefix,
            value=value,
            is_reference_like=True,
            is_shared_like=_value_implies_shared(value),
        )

    # 3. thread dimension (enhanced)
    if _is_thread_dimension(t):
        # Check for complete thread spec (MJ5x0.8 4h6h)
        if re.search(r"MJ?\d+[xX]\d+(\.\d+)?\s*\d+[hH]\d+[hH]", t):
            return GrammarToken(
                raw_text=raw,
                text=t,
                grammar_type="thread_dimension",
                semantic_type="thread",
                value=t,
                is_reference_like=True,  # Complete specs are often reference-like
            )
        else:
            return GrammarToken(
                raw_text=raw,
                text=t,
                grammar_type="thread_dimension",
                semantic_type="thread",
                value=t,
            )

    # 4. diameter dimension (enhanced)
    if _is_diameter_dimension(t):
        # Check for precision diameter with tolerance (Ø0.800±0.001)
        if re.search(r"Ø\d+\.\d+±\d+\.\d+", t):
            return GrammarToken(
                raw_text=raw,
                text=t,
                grammar_type="diameter_dimension",
                semantic_type="diameter",
                value=t,
                is_reference_like=True,  # Precision specs are often reference-like
            )
        # Check for MAJOR DIA blocks
        elif re.search(r"MAJOR\s*DIA", t, re.IGNORECASE):
            return GrammarToken(
                raw_text=raw,
                text=t,
                grammar_type="diameter_dimension",
                semantic_type="diameter",
                value=t,
                is_reference_like=True,  # MAJOR DIA is reference-like
            )
        else:
            return GrammarToken(
                raw_text=raw,
                text=t,
                grammar_type="diameter_dimension",
                semantic_type="diameter",
                value=t,
                is_shared_like=("TYP" in t.upper()),
            )

    # 5. chamfer dimension (enhanced)
    if _is_chamfer_dimension(t):
        # Check for labeled chamfer (B2 0.5x45°)
        if re.search(r"[A-Z]\d*\s*\d+(\.\d+)?[xX]\d+°", t):
            return GrammarToken(
                raw_text=raw,
                text=t,
                grammar_type="chamfer_dimension",
                semantic_type="chamfer",
                value=t,
                is_reference_like=True,  # Labeled chamfers are reference-like
            )
        else:
            return GrammarToken(
                raw_text=raw,
                text=t,
                grammar_type="chamfer_dimension",
                semantic_type="chamfer",
                value=t,
                is_shared_like=("TYP" in t.upper()),
            )

    # 6. radius dimension
    if _is_radius_dimension(t):
        return GrammarToken(
            raw_text=raw,
            text=t,
            grammar_type="radius_dimension",
            semantic_type="radius",
            value=t,
            is_shared_like=("TYP" in t.upper()),
        )

    # 7. angle / reference angle
    if _is_angle_dimension(t):
        return GrammarToken(
            raw_text=raw,
            text=t,
            grammar_type="angle_dimension",
            semantic_type="angle",
            value=t,
            is_reference_like=("REF" in t.upper()),
        )

    # 8. tolerance-like numeric
    if _is_tolerance_dimension(t):
        return GrammarToken(
            raw_text=raw,
            text=t,
            grammar_type="tolerance_dimension",
            semantic_type="tolerance",
            value=t,
        )

    # 9. numeric pair
    if _is_numeric_pair(t):
        return GrammarToken(
            raw_text=raw,
            text=t,
            grammar_type="numeric_pair",
            semantic_type="pair",
            value=t,
        )

    # 10. numeric single
    if _is_numeric_single(t):
        return GrammarToken(
            raw_text=raw,
            text=t,
            grammar_type="numeric_single",
            semantic_type="numeric",
            value=t,
        )

    # 11. keywords
    if _is_keyword(t):
        tu = t.upper()
        return GrammarToken(
            raw_text=raw,
            text=t,
            grammar_type="keyword",
            semantic_type="keyword",
            value=t,
            is_reference_like=("REF" in tu),
            is_shared_like=("TYP" in tu or "ALL AROUND" in tu),
        )

    # 12. reference-like note
    if _is_reference_note(t):
        tu = t.upper()
        return GrammarToken(
            raw_text=raw,
            text=t,
            grammar_type="reference_note",
            semantic_type="reference",
            value=t,
            is_reference_like=True,
            is_shared_like=("TYP" in tu),
        )

    # 13. coordinate note
    if _is_coordinate_note(t):
        return GrammarToken(
            raw_text=raw,
            text=t,
            grammar_type="coordinate_note",
            semantic_type="coordinate",
            value=t,
            is_reference_like=True,
        )

    return GrammarToken(
        raw_text=raw,
        text=t,
        grammar_type="unknown",
        semantic_type="unknown",
        value=t,
    )


def parse_grammar_tokens(texts: List[str]) -> List[GrammarToken]:
    return [parse_grammar_token(t) for t in texts]


def token_implies_shared_dimension(gt: GrammarToken) -> bool:
    """
    True when a token suggests repeated/shared-value semantics.
    """
    if gt.is_shared_like or gt.is_repeat_like:
        return True

    t = gt.text.upper()
    if "=" in t:
        return True
    if "TYP" in t:
        return True
    if "ALL AROUND" in t:
        return True

    return False


def token_has_prefix_reference(gt: GrammarToken) -> bool:
    """
    True for tokens like B2 0.5x45° where the prefix matters.
    """
    return bool(gt.prefix)


# ── Grammar Normalization ─────────────────────────────────────────

def _normalize_grammar_text(text: str) -> str:
    t = (text or "").strip()
    if not t:
        return ""

    # unify spaces
    t = re.sub(r"\s+", " ", t)

    # normalize multiplication sign conservatively
    t = t.replace("×", "x")
    t = re.sub(r"(?<=\d)\s*[xX]\s*(?=\d)", "x", t)

    # normalize degree variants
    t = t.replace("º", DEGREE)
    t = t.replace("˚", DEGREE)
    t = t.replace("?", DEGREE) if re.search(r"\d\s*[xX]\s*\d+\?$", t) else t

    # normalize slash / plus-minus spacing
    t = re.sub(r"\s*/\s*", "/", t)
    t = re.sub(r"\s*±\s*", PLUS_MINUS, t)

    # normalize commas for coordinate-like notation only
    t = re.sub(r"\s*,\s*", ",", t)

    # normalize OCR typos for diameter marker and DIA keyword
    t = re.sub(r"^O(?=\d)", DIAMETER, t)
    t = re.sub(r"^0(?=\d{1,2}\.\d)", DIAMETER, t) if _looks_like_diameter_prefix_confusion(t) else t
    t = t.replace("D1A", "DIA").replace("D|A", "DIA")

    # normalize OCR typos in thread prefix
    t = re.sub(r"^MJS(?=x)", "MJ5", t, flags=re.I)
    t = re.sub(r"^MJS(?=\d)", "MJ5", t, flags=re.I)
    t = re.sub(r"^M\s+(?=\d)", "M", t, flags=re.I)
    t = re.sub(r"^(MJ\d+)\s*x\s*(\d+(?:\.\d+)?)$", r"\1x\2", t, flags=re.I)
    t = re.sub(r"^(M\d+)\s*x\s*(\d+(?:\.\d+)?)$", r"\1x\2", t, flags=re.I)

    # radius / TYP spacing
    t = re.sub(r"\b(R\d+(?:\.\d+)?)(TYP)\b", r"\1 \2", t, flags=re.I)

    # major/minor dia block spacing
    t = re.sub(r"\bMAJOR\s+DIA\b", "MAJOR DIA", t, flags=re.I)
    t = re.sub(r"\bMINOR\s+DIA\b", "MINOR DIA", t, flags=re.I)
    t = re.sub(r"\bPITCH\s+DIA\b", "PITCH DIA", t, flags=re.I)

    # coordinate + axis format
    t = re.sub(r"^([+\-])\s*([XY])$", lambda m: f"{m.group(1)}{m.group(2).upper()}", t, flags=re.I)
    t = re.sub(r"^(\d+,\d+)\s*([+\-])\s*([XY])$", lambda m: f"{m.group(1)} {m.group(2)}{m.group(3).upper()}", t, flags=re.I)

    # compact duplicate spaces one last time
    t = re.sub(r"\s+", " ", t).strip()
    return t


# ── Core Grammar Predicates ───────────────────────────────────────

def _is_equal_symbol(text: str) -> bool:
    t = text.strip()
    if t == "=":
        return True
    if re.fullmatch(r"=+", t):
        return True
    return False


def _extract_prefix_dimension(text: str) -> Optional[Tuple[str, str]]:
    """
    Match patterns like:
      B2 0.5x45°
      A1 Ø5.0
      C3 10.0/9.8
      D4 M10x1.5
      B2 -9°(REF)

    Important:
    - prefix is treated as meaningful grammar
    - value preserved separately
    - only applies when suffix is truly dimension-like
    """
    t = text.strip()
    m = re.match(r"^([A-Z]\d{1,2})\s+(.+)$", t, flags=re.I)
    if not m:
        return None

    prefix = m.group(1).upper()
    value = m.group(2).strip()

    if _looks_dimension_like(value):
        return prefix, value
    return None


def _is_thread_dimension(text: str) -> bool:
    t = text.strip().upper()

    if re.search(r"\bMJ\d", t):
        return True
    if re.search(r"\bM\d", t):
        return True
    if re.search(r"\d[Hh]\d", t):
        return True
    if "THRU" in t and re.search(r"\bM\d", t):
        return True

    return False


def _is_diameter_dimension(text: str) -> bool:
    t = text.strip().upper()

    if DIAMETER in t:
        return True
    if "DIA" in t:
        return True
    if re.fullmatch(r"\d+(\.\d+)?\s*DIA", t):
        return True
    if re.fullmatch(rf"{re.escape(DIAMETER)}\d+(\.\d+)?{PLUS_MINUS}\d+(\.\d+)?", t):
        return True

    return False


def _is_radius_dimension(text: str) -> bool:
    t = text.strip().upper()
    return bool(
        re.fullmatch(r"R\d+(\.\d+)?(\s+TYP)?", t)
        or re.fullmatch(r"\d+(\.\d+)?R", t)
    )


def _is_chamfer_dimension(text: str) -> bool:
    """
    Matches dimension-like chamfer patterns:
      0.5x45
      0.5x45°
      1x45°
      B2 0.5x45° is handled by prefixed form above

    A digit is always required before and after 'x'.
    """
    t = text.strip()
    return bool(re.fullmatch(r"\d+(\.\d+)?x\d+(?:\.\d+)?°?(?:\s+TYP)?", t, flags=re.I))


def _is_angle_dimension(text: str) -> bool:
    t = text.strip().upper()
    return bool(
        re.fullmatch(rf"[+\-]?\d+(\.\d+)?{DEGREE}(\(REF\))?", t)
        or re.fullmatch(rf"[+\-]?\d+(\.\d+)?{DEGREE}\s*TYP", t)
    )


def _is_tolerance_dimension(text: str) -> bool:
    t = text.strip().upper()
    return bool(
        re.fullmatch(rf"\d+(\.\d+)?{PLUS_MINUS}\d+(\.\d+)?", t)
        or re.fullmatch(r"[+\-]?\d+(\.\d+)?\s*/\s*[+\-]?\d+(\.\d+)?", t)
    )


def _is_numeric_pair(text: str) -> bool:
    t = text.strip()
    return bool(re.fullmatch(r"\d+(\.\d+)?/\d+(\.\d+)?", t))


def _is_numeric_single(text: str) -> bool:
    t = text.strip()
    return bool(re.fullmatch(r"\d+(\.\d+)?", t))


def _is_keyword(text: str) -> bool:
    t = text.strip().upper()
    keywords = {
        "MAJOR",
        "MINOR",
        "PITCH",
        "DIA",
        "MAJOR DIA",
        "MINOR DIA",
        "PITCH DIA",
        "TYP",
        "REF",
        "ALL AROUND",
        "THRU",
        "THROUGH",
        "CBORE",
        "CSINK",
        "DEPTH",
    }
    return t in keywords


def _is_reference_note(text: str) -> bool:
    t = text.strip().upper()

    if "REF" in t:
        return True
    if "TYP" in t:
        return True
    if "ALL AROUND" in t:
        return True

    return False


def _is_coordinate_note(text: str) -> bool:
    t = text.strip().upper()

    if re.fullmatch(r"[+\-][XY]", t):
        return True
    if re.fullmatch(r"[+\-][XY]\s+[+\-][XY]", t):
        return True
    if re.fullmatch(r"\d+,\d+", t):
        return True
    if re.fullmatch(r"\d+,\d+\s+[+\-][XY]", t):
        return True

    return False


def _semantic_from_value(value: str) -> str:
    if _is_chamfer_dimension(value):
        return "chamfer"
    if _is_thread_dimension(value):
        return "thread"
    if _is_diameter_dimension(value):
        return "diameter"
    if _is_radius_dimension(value):
        return "radius"
    if _is_angle_dimension(value):
        return "angle"
    if _is_tolerance_dimension(value):
        return "tolerance"
    if _is_numeric_pair(value):
        return "pair"
    if _is_numeric_single(value):
        return "numeric"
    if _is_coordinate_note(value):
        return "coordinate"
    return "unknown"


# ── Helpers ───────────────────────────────────────────────────────

def _looks_dimension_like(value: str) -> bool:
    return any(
        fn(value)
        for fn in (
            _is_chamfer_dimension,
            _is_thread_dimension,
            _is_diameter_dimension,
            _is_radius_dimension,
            _is_angle_dimension,
            _is_tolerance_dimension,
            _is_numeric_pair,
            _is_numeric_single,
            _is_coordinate_note,
        )
    )


def _value_implies_shared(value: str) -> bool:
    vu = value.upper()
    return "TYP" in vu or "ALL AROUND" in vu


def _looks_like_diameter_prefix_confusion(text: str) -> bool:
    t = text.strip()
    return bool(re.fullmatch(r"0\d{1,2}\.\d+", t))