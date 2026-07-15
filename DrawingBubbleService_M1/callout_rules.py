from __future__ import annotations

from dataclasses import dataclass
from typing import List
import math
import re

try:
    from .ocr_rules import (
        NormalizedToken,
        token_is_numeric_value,
        token_is_chamfer,
        token_is_thread,
        token_is_diameter,
        token_is_radius,
    )
except ImportError:
    from ocr_rules import (
        NormalizedToken,
        token_is_numeric_value,
        token_is_chamfer,
        token_is_thread,
        token_is_diameter,
        token_is_radius,
    )


# ─────────────────────────────────────────────────────────────────────────────
# Data class
# ─────────────────────────────────────────────────────────────────────────────

@dataclass
class CalloutGroup:
    text: str
    tokens: List[NormalizedToken]
    x1: float
    y1: float
    x2: float
    y2: float
    cx: float
    cy: float
    callout_type: str


# ─────────────────────────────────────────────────────────────────────────────
# Public API
# ─────────────────────────────────────────────────────────────────────────────

def build_callout_groups(
    tokens: List[NormalizedToken],
    scale_factor: float = 1.0,
) -> List[CalloutGroup]:
    """
    Build callout groups from OCR tokens.

    Pipeline (in priority order):
      PRE-PASS  Merge vertically-stacked tokens (tolerance stacks, multi-line notes)
      P1        Complete structured specs  (Ø0.800±.001, MJ5x0.8 4h6h, B2 0.5x45°)
      P2        Major diameter blocks
      P3        Labeled chamfers
      P4        Precision diameter/tolerance pairs
      P5        Numeric pairs (10.0/9.8)
      P6        Radius specifications
      P7        Angle specifications
      P8        Thread specifications
      P9        Keyword groups
      P10       Plain numeric singletons

    scale_factor — pass the image scale multiplier (median_radius / 25.0) for
                   adaptive proximity thresholds on large/small drawings.
    """
    # PRE-PASS 1: Fuse multi-token engineering phrases BEFORE stacking
    # so that phrase tokens don't get absorbed into numeric stacks
    tokens = _fuse_engineering_phrases(tokens, scale_factor=scale_factor)

    # PRE-PASS 2: Merge vertically stacked tokens into combined callouts
    tokens = _merge_vertical_stacks(tokens, scale_factor=scale_factor)

    # Reconstruct structured tokens from fragments
    tokens = _reconstruct_structured_tokens(tokens)

    candidates = [
        t for t in tokens
        if t.token_type in {"dimension", "keyword"}
        and t.semantic_type != "suppressed"
    ]
    candidates = _dedup_tokens(candidates)

    groups: List[CalloutGroup] = []
    used_tokens: set = set()

    def _try_add(group_list: List[CalloutGroup]) -> None:
        for g in group_list:
            # Skip groups whose tokens are already claimed by an earlier pass
            if any(id(t) in used_tokens for t in g.tokens):
                continue
            if _validate_group(g):
                groups.append(g)
                used_tokens.update(id(t) for t in g.tokens)

    _try_add(_build_complete_structured_groups(candidates))
    _try_add(_build_parenthetical_value_groups(candidates))
    _try_add(_build_enhanced_diameter_groups(candidates))
    _try_add(_build_enhanced_chamfer_groups(candidates))
    _try_add(_build_tolerance_groups(candidates))
    _try_add(_build_enhanced_stacked_groups(candidates))
    _try_add(_build_radius_groups(candidates))
    _try_add(_build_angle_groups(candidates))
    _try_add(_build_thread_groups(candidates))
    _try_add(_build_keyword_groups(candidates))

    remaining = [t for t in candidates if id(t) not in used_tokens]
    _try_add(_build_singleton_groups(remaining))

    return groups


def tokens_to_callout_groups_safe(
    tokens: List[NormalizedToken],
    existing_groups: List[CalloutGroup],
) -> List[CalloutGroup]:
    """Build callout groups from recovered tokens, avoiding duplicates."""
    existing_texts = {g.text.strip().lower() for g in existing_groups}
    new_groups = build_callout_groups(tokens)
    return [g for g in new_groups if g.text.strip().lower() not in existing_texts]


# ─────────────────────────────────────────────────────────────────────────────
# Shared heuristics
# ─────────────────────────────────────────────────────────────────────────────

def _are_independent_stacked_decimals(prev_t: str, next_t: str) -> bool:
    """Return True if two stacked tokens are independent dimensions
    (e.g. "120.0" / "80.0"), not a tolerance/precision pair (e.g.
    "1.000" / "1.002", "5.00" / "4.85", "1.7" / "1.5").

    Decision is structural, not numeric-magic-threshold:

      1. **Decimal precision must match.** Engineering convention is to
         quote a max/min tolerance pair at the SAME number of decimal
         places ("5.00 / 4.85", "1.7 / 1.5"). Two stacked numbers with
         different precision were typed for different purposes — they're
         independent dimensions.

      2. **Relative spread must be tolerance-shaped.** Even when both
         numbers share precision, two stacked dimensions on a real
         drawing (e.g. a width of 120.0 above a height of 80.0) sit
         well apart — typically 30%+ of the larger value. Anything
         tighter than that, with matching precision, is a tolerance
         pair (max/min) for ONE feature.

    Both checks are about the structure of the numbers themselves, not
    about absolute units or specific drawings — so the rule generalises
    to any drawing the system hasn't seen before.
    """
    if not prev_t or not next_t:
        return False
    if prev_t[:1] in "+-±" or next_t[:1] in "+-±":
        return False
    # Both must be unsigned numerics (integer OR decimal).
    int_or_dec = re.compile(r"\d+(?:\.\d+)?")
    if not (int_or_dec.fullmatch(prev_t) and int_or_dec.fullmatch(next_t)):
        return False
    try:
        va = float(prev_t)
        vb = float(next_t)
    except ValueError:
        return False
    if va < 1.0 or vb < 1.0:
        return False

    # Decimal precision per token (0 for integers, N for "X.dddd...").
    def _prec(s: str) -> int:
        return len(s.split(".", 1)[1]) if "." in s else 0
    prev_prec = _prec(prev_t)
    next_prec = _prec(next_t)

    # Rule 1: different decimal precision → independent.
    # This now also catches "integer vs decimal" pairs like "536" / "483.2"
    # — engineering tolerance pairs always quote BOTH numbers at the
    # same precision, never mixing integers and decimals.
    if prev_prec != next_prec:
        return True

    # Rule 2: same precision but spread >> typical tolerance → independent.
    return abs(va - vb) / max(va, vb) > 0.30


# ─────────────────────────────────────────────────────────────────────────────
# PRE-PASS: Vertical stack merging
# ─────────────────────────────────────────────────────────────────────────────

def _merge_vertical_stacks(
    tokens: List[NormalizedToken],
    scale_factor: float = 1.0,
) -> List[NormalizedToken]:
    """
    Merge vertically stacked tokens in the same x-column into combined tokens.

    Handles:
      +0.06 / 31.80 / -0.03   (tolerance stacks)
      ALL AROUND / OUTER PROFILE  (multi-line notes)
      MAJOR DIA / 5.00 / 4.85  (diameter specifications)

    scale_factor allows adaptive column/row gap for different drawing scales.
    """
    if len(tokens) < 2:
        return tokens

    col_gap = 24.0 * scale_factor   # max x-distance to be in the "same column"
    # row_gap now applied to the actual bbox gap (y1_next - y2_prev), not centre
    # distance.  Positive = physical gap; negative = overlap.
    # Allows up to 10 px of bbox overlap (JPEG compression / scale jitter) and
    # up to 8 px of gap.  This prevents distant tokens (>8 px apart after their
    # bboxes) from merging even when one token has a very tall bounding box.
    row_gap_max =  5.0 * scale_factor   # max actual bbox gap to allow merge
    row_gap_min = -12.0 * scale_factor  # allow up to 12 px bbox overlap
    min_stack = 2                        # minimum tokens to form a stack

    # Sort by x (column grouping), then by y within a column
    sorted_toks = sorted(tokens, key=lambda t: (round(t.cx / max(col_gap, 1)), t.cy))

    merged: List[NormalizedToken] = []
    used: set = set()

    for i, tok in enumerate(sorted_toks):
        if i in used:
            continue
        # Don't seed stacks with bubble-typed tokens,
        # reclassified single-digit tokens, or fused phrases
        if tok.token_type == "bubble":
            continue
        if tok.semantic_type == "dual_use_digit":
            merged.append(tok)
            used.add(i)
            continue
        if tok.semantic_type == "phrase":
            merged.append(tok)
            used.add(i)
            continue

        # Seed a stack with this token
        stack = [tok]
        used.add(i)

        # Look for tokens in the same column below this one
        for j, other in enumerate(sorted_toks):
            if j in used:
                continue
            # Same column?
            if abs(other.cx - stack[-1].cx) > col_gap:
                continue
            # Don't stack bubble-typed, dual-use, or phrase tokens
            if other.token_type == "bubble":
                continue
            if other.semantic_type in ("dual_use_digit", "phrase"):
                continue
            # Bbox gap: positive = gap between boxes, negative = overlap.
            # Use the LAST token in the stack as the reference for y2.
            bbox_gap = other.y1 - stack[-1].y2
            if row_gap_min <= bbox_gap <= row_gap_max:
                # Two multi-digit pure integers are separate dimensions,
                # not a stacked pair (e.g. "23" / "329" = separate values).
                # Single-digit tokens can merge (part of chamfer: "1 X 45°").
                # Legitimate stacks have decimals, symbols, or keywords.
                prev_t = stack[-1].text.strip()
                next_t = other.text.strip()
                if (len(prev_t) >= 2 and len(next_t) >= 2
                        and re.fullmatch(r"\d+", prev_t)
                        and re.fullmatch(r"\d+", next_t)):
                    continue

                # Two unsigned ≥1.0 decimals whose magnitudes differ
                # by more than 5% of the larger are independent
                # horizontal dimensions, not a tolerance pair (see
                # _are_independent_stacked_decimals).
                # Adjacent diameter specs are independent callouts unless
                # a separate tolerance token explicitly pairs with one of
                # them later. Do not pre-merge "Ø52.28" and "Ø63.1" into
                # one stacked value.
                if token_is_diameter(prev_t) and token_is_diameter(next_t):
                    continue

                if _are_independent_stacked_decimals(prev_t, next_t):
                    continue

                stack.append(other)
                used.add(j)

        if len(stack) < min_stack:
            merged.append(tok)
            continue

        # Merge the stack tokens into one synthetic token.
        # When the stack contains a keyword label (DIA, MAJOR, etc.),
        # place it first regardless of vertical position — engineering
        # convention: "MAJOR DIA 4.85" not "4.85 / MAJOR DIA".
        # Pure-numeric stacks use "/" separator (e.g. "3.2 / 3.0").
        # EXCEPTION: a multi-line parenthesised note (one token starts
        # with "(" and another ends with ")") must preserve top-to-
        # bottom order — e.g. "(1° TAPER LINE STARTING" + "POINT
        # WHERE RADIUS ENDS)" reads naturally top-down, not keyword-
        # first. Without this exception the merged text reverses
        # to "POINT WHERE RADIUS ENDS) (1° TAPER LINE STARTING".
        stack.sort(key=lambda t: t.cy)
        is_paren_note = (
            any(t.text.strip().startswith("(") for t in stack)
            and any(t.text.strip().endswith(")") for t in stack)
        )
        keyword_toks = [t for t in stack if t.token_type == "keyword"]
        value_toks   = [t for t in stack if t.token_type != "keyword"]
        # OCR on rotated dimensions can split one decimal into two
        # vertical fragments, e.g. "28.35" as "0.35" above "28.".
        # Reconstruct only when both fragments look like rotated text
        # boxes (tall and narrow); ordinary horizontal tolerance
        # stacks should keep the slash-separated form.
        if not keyword_toks and len(stack) == 2:
            top, bot = sorted(stack, key=lambda t: t.cy)
            mt = re.fullmatch(r"0\.(\d+)", top.text.strip())
            mb = re.fullmatch(r"(\d+)\.?", bot.text.strip())
            if mt and mb:
                top_h = max(1.0, top.y2 - top.y1)
                top_w = max(1.0, top.x2 - top.x1)
                bot_h = max(1.0, bot.y2 - bot.y1)
                bot_w = max(1.0, bot.x2 - bot.x1)
                if top_h > top_w * 1.2 and bot_h > bot_w * 1.2:
                    combined_text = f"{mb.group(1)}.{mt.group(1)}"
                    xs = [top.x1, top.x2, bot.x1, bot.x2]
                    ys = [top.y1, top.y2, bot.y1, bot.y2]
                    merged.append(NormalizedToken(
                        raw_text=combined_text,
                        text=combined_text,
                        cx=(top.cx + bot.cx) / 2.0,
                        cy=(top.cy + bot.cy) / 2.0,
                        conf=(top.conf + bot.conf) / 2.0,
                        x1=min(xs),
                        y1=min(ys),
                        x2=max(xs),
                        y2=max(ys),
                        token_type="dimension",
                        semantic_type="numeric",
                    ))
                    continue
        if is_paren_note:
            combined_text = " ".join(t.text for t in stack)
        elif keyword_toks:
            ordered = keyword_toks + value_toks
            combined_text = " ".join(t.text for t in ordered)
        else:
            combined_text = " / ".join(t.text for t in stack)
        xs = [t.x1 for t in stack] + [t.x2 for t in stack]
        ys = [t.y1 for t in stack] + [t.y2 for t in stack]
        avg_conf = sum(t.conf for t in stack) / len(stack)

        # Preserve keyword type when all tokens in the stack are keywords
        stack_type = "keyword" if keyword_toks and not value_toks else "dimension"
        merged_tok = NormalizedToken(
            raw_text=combined_text,
            text=combined_text,
            cx=sum(t.cx for t in stack) / len(stack),
            cy=sum(t.cy for t in stack) / len(stack),
            conf=avg_conf,
            x1=min(xs),
            y1=min(ys),
            x2=max(xs),
            y2=max(ys),
            token_type=stack_type,
            semantic_type="stacked",
        )
        merged.append(merged_tok)

        # Also emit each individual token so the assignment solver
        # can consider them as separate callouts.  This handles the
        # case where stacked values belong to different bubbles
        # (e.g. "536.8" → bubble 10 and "483.2" → bubble 9).
        if not keyword_toks and len(value_toks) >= 2:
            for vt in value_toks:
                merged.append(vt)

    return merged


# ─────────────────────────────────────────────────────────────────────────────
# Structure reconstruction
# ─────────────────────────────────────────────────────────────────────────────

def _reconstruct_structured_tokens(tokens: List[NormalizedToken]) -> List[NormalizedToken]:
    """Re-merge fragments that OCR split within a single structured spec."""
    if len(tokens) < 2:
        return tokens

    tokens_sorted = sorted(tokens, key=lambda t: (t.cy, t.cx))
    result: List[NormalizedToken] = []
    used: set = set()

    for i, tok in enumerate(tokens_sorted):
        if i in used:
            continue

        text = tok.text.strip()
        matched = False

        # Pattern: tolerance sign fragment (±, +, -) followed by a number
        if text in ("±", "+", "-", "Ø"):
            for j, other in enumerate(tokens_sorted):
                if j <= i or j in used:
                    continue
                dx = abs(other.cx - tok.x2)
                dy = abs(other.cy - tok.cy)
                if dx < 60 and dy < 20 and re.search(r"\d", other.text):
                    combined = NormalizedToken(
                        raw_text=tok.raw_text + other.raw_text,
                        text=text + other.text,
                        cx=(tok.cx + other.cx) / 2,
                        cy=(tok.cy + other.cy) / 2,
                        conf=min(tok.conf, other.conf),
                        x1=min(tok.x1, other.x1),
                        y1=min(tok.y1, other.y1),
                        x2=max(tok.x2, other.x2),
                        y2=max(tok.y2, other.y2),
                        token_type="dimension",
                        semantic_type=tok.semantic_type,
                    )
                    result.append(combined)
                    used.add(i)
                    used.add(j)
                    matched = True
                    break

        if not matched:
            result.append(tok)

    return result


# ─────────────────────────────────────────────────────────────────────────────
# Engineering phrase fusion
# ─────────────────────────────────────────────────────────────────────────────

# Known multi-word engineering annotations that OCR splits into
# separate tokens.  Longest phrases first for greedy matching.
_ENGINEERING_PHRASES = [
    ["ALL", "AROUND", "OUTER", "PROFILE"],
    ["ALL", "AROUND"],
    ["OUTER", "PROFILE"],
    ["MARK", "PART"],
    ["TRUE", "POSITION"],
]


def _fuse_engineering_phrases(
    tokens: List[NormalizedToken],
    scale_factor: float = 1.0,
) -> List[NormalizedToken]:
    """Fuse consecutive keyword tokens that match known engineering phrases.

    OCR may split multi-word annotations in various ways:
      - "ALL AROUND" + "OUTER PROFILE" (two multi-word tokens)
      - "ALL" + "AROUND" + "OUTER" + "PROFILE" (four single-word tokens)

    This function concatenates the words from nearby keyword tokens
    (same column ±30px, vertical bbox gap ≤60px) and checks if the
    combined text matches a known phrase.
    """
    if len(tokens) < 2:
        return tokens

    col_tol = 30.0 * scale_factor
    row_tol = 60.0 * scale_factor

    # Build the set of known phrase strings for matching
    phrase_strings = [" ".join(p) for p in _ENGINEERING_PHRASES]

    sorted_toks = sorted(tokens, key=lambda t: (t.cy, t.cx))
    result: List[NormalizedToken] = []
    used: set = set()

    for i, tok in enumerate(sorted_toks):
        if i in used:
            continue

        if tok.token_type != "keyword":
            result.append(tok)
            continue

        # Try to build a phrase starting from this token by accumulating
        # nearby keyword tokens (same column, close vertically)
        group_indices = [i]
        group_words = tok.text.strip().upper().split()
        prev = tok

        for j, other in enumerate(sorted_toks):
            if j <= i or j in used:
                continue
            if other.token_type != "keyword":
                continue
            if abs(other.cx - tok.cx) > col_tol:
                continue
            bbox_gap = other.y1 - prev.y2
            if bbox_gap > row_tol:
                continue
            group_indices.append(j)
            group_words.extend(other.text.strip().upper().split())
            prev = other

        # Check if the accumulated words match any known phrase
        combined = " ".join(group_words)
        matched_phrase = None
        for ps in phrase_strings:
            if ps in combined:
                matched_phrase = ps
                break

        if matched_phrase and len(group_indices) > 1:
            phrase_toks = [sorted_toks[idx] for idx in group_indices]
            xs = [t.x1 for t in phrase_toks] + [t.x2 for t in phrase_toks]
            ys = [t.y1 for t in phrase_toks] + [t.y2 for t in phrase_toks]
            fused = NormalizedToken(
                raw_text=matched_phrase,
                text=matched_phrase,
                cx=sum(t.cx for t in phrase_toks) / len(phrase_toks),
                cy=sum(t.cy for t in phrase_toks) / len(phrase_toks),
                conf=min(t.conf for t in phrase_toks),
                x1=min(xs), y1=min(ys),
                x2=max(xs), y2=max(ys),
                token_type="keyword",
                semantic_type="phrase",
            )
            result.append(fused)
            for idx in group_indices:
                used.add(idx)
        else:
            result.append(tok)

    return result


# ─────────────────────────────────────────────────────────────────────────────
# Group builders
# ─────────────────────────────────────────────────────────────────────────────

def _build_complete_structured_groups(candidates: List[NormalizedToken]) -> List[CalloutGroup]:
    """Build groups for fully-specified engineering dimensions."""
    groups = []
    used = set()

    # First: claim already-merged thread tokens (e.g. "MJ5x0.8.4h6h" read as
    # a single tall token).  This prevents the diameter pass from absorbing them.
    for i, tok in enumerate(candidates):
        if i in used:
            continue
        t = tok.text
        if re.search(r"MJ?\d+[Xx\.]\d+", t, re.I) and re.search(r"\d+[Hh]\d+[Hh]", t, re.I):
            groups.append(_make_singleton(tok, "thread"))
            used.add(i)

    patterns = [
        # Thread: MJ5x0.8 4h6h (two separate tokens)
        (r"MJ?\d+[Xx\.]\d+", r"\d+[Hh]\d+[Hh]", "thread"),
        # Diameter + tolerance: Ø0.800±.001
        (r"[ØO]\d+\.?\d*", r"[±]\d+\.?\d*", "diameter_tolerance"),
        # Chamfer: B2 0.5x45°
        (r"[A-Z]\d+", r"\d+\.?\d*[Xx]\d+°", "chamfer"),
    ]

    for lead_pat, follow_pat, gtype in patterns:
        for i, tok in enumerate(candidates):
            if i in used:
                continue
            if gtype == "diameter_tolerance" and "/" in tok.text:
                continue
            if not re.search(lead_pat, tok.text, re.I):
                continue
            for j, other in enumerate(candidates):
                if j <= i or j in used:
                    continue
                if gtype == "diameter_tolerance" and "/" in other.text:
                    continue
                if not re.search(follow_pat, other.text, re.I):
                    continue
                dx = abs(other.cx - tok.cx)
                dy = abs(other.cy - tok.cy)
                if dx < 120 and dy < 50:
                    combined = _merge_token_pair(tok, other, gtype)
                    groups.append(combined)
                    used.add(i)
                    used.add(j)
                    break

    return groups


def _build_parenthetical_value_groups(candidates: List[NormalizedToken]) -> List[CalloutGroup]:
    """Attach adjacent parenthetical qualifiers to a dimension value.

    Examples: "Ø64 (BOSS)", "12.5 (REF)", "R3 (TYP)".
    """
    groups = []
    used = set()
    paren_re = re.compile(r"^\([A-Z0-9][A-Z0-9 ._\-]{1,24}\)$", re.I)

    for i, tok in enumerate(candidates):
        if i in used:
            continue
        if not (
            token_is_diameter(tok.text)
            or token_is_radius(tok.text)
            or token_is_numeric_value(tok.text)
        ):
            continue
        best = None
        best_dist = float("inf")
        for j, other in enumerate(candidates):
            if j == i or j in used:
                continue
            otext = other.text.strip()
            if not paren_re.fullmatch(otext):
                continue
            dx = abs(other.cx - tok.cx)
            dy = abs(other.cy - tok.cy)
            tok_h = max(1.0, tok.y2 - tok.y1)
            other_h = max(1.0, other.y2 - other.y1)
            if dx > 180 or dy > max(70.0, (tok_h + other_h) * 0.9):
                continue
            dist = math.hypot(dx, dy)
            if dist < best_dist:
                best = j
                best_dist = dist
        if best is None:
            continue
        q = candidates[best]
        merged = _merge_token_pair(tok, q, "keyword")
        merged = CalloutGroup(
            text=f"{tok.text} {q.text}",
            tokens=merged.tokens,
            x1=merged.x1,
            y1=merged.y1,
            x2=merged.x2,
            y2=merged.y2,
            cx=merged.cx,
            cy=merged.cy,
            callout_type="keyword",
        )
        groups.append(merged)
        used.add(i)
        used.add(best)
    return groups


def _build_enhanced_diameter_groups(candidates: List[NormalizedToken]) -> List[CalloutGroup]:
    """Group diameter specifications (MAJOR DIA / MIN DIA with values)."""
    groups = []
    used = set()

    for i, tok in enumerate(candidates):
        if i in used:
            continue
        upper = tok.text.upper()
        if "DIA" not in upper and "DIAMETER" not in upper:
            continue

        # Adaptive proximity: use bbox height as a hint for rotated text.
        # Vertical/rotated text (tall bbox) needs a larger y-tolerance
        # because the label and its values span more vertical space.
        tok_h = max(1, tok.y2 - tok.y1)
        y_tol = max(60, int(tok_h * 0.9))

        nearby_vals = [
            (j, other) for j, other in enumerate(candidates)
            if j != i and j not in used
            and abs(other.cx - tok.cx) < 120
            and abs(other.cy - tok.cy) < y_tol
            and re.search(r"\d+\.?\d*", other.text)
            and not token_is_thread(other.text)  # exclude thread specs (MJ5x0.8 4h6h)
            # exclude tokens that already look like complete standalone thread specs
            and not re.search(r"MJ?\d+[Xx\.]\d+", other.text, re.I)
            and other.token_type != "keyword"    # exclude other keyword labels
        ]
        if not nearby_vals:
            groups.append(_make_singleton(tok, "diameter"))
            used.add(i)
        else:
            all_toks = [tok] + [other for _, other in nearby_vals[:2]]
            groups.append(_make_group(all_toks, "diameter"))
            used.add(i)
            for j, _ in nearby_vals[:2]:
                used.add(j)

    return groups


def _build_enhanced_chamfer_groups(candidates: List[NormalizedToken]) -> List[CalloutGroup]:
    """Group chamfer specifications (e.g. B2 0.5x45°)."""
    groups = []
    used = set()

    def _is_chamfer_prefix(text: str) -> bool:
        t = text.strip()
        # Standard letter-prefix: "B2", "C1", etc.
        if re.fullmatch(r"[A-Z]\d+", t, re.I):
            return True
        # Parenthesized-number prefix: "(82)" is a callout-reference
        # prefix on a chamfer dimension like "(82) 0.5x45°". OCR can
        # split the surrounding parens away, so accept the bare
        # number plus partial-paren variants when found ADJACENT to a
        # chamfer token (the chamfer-shape check in the matcher gates
        # against bare-numeric tokens elsewhere in the drawing).
        if re.fullmatch(r"\(?\d+\)?", t):
            return True
        return False

    for i, tok in enumerate(candidates):
        if i in used:
            continue
        if not _is_chamfer_prefix(tok.text):
            continue

        for j, other in enumerate(candidates):
            if j == i or j in used:
                continue
            if not token_is_chamfer(other.text):
                continue
            dx = abs(other.cx - tok.cx)
            dy = abs(other.cy - tok.cy)
            if dx < 120 and dy < 50:
                groups.append(_merge_token_pair(tok, other, "chamfer"))
                used.add(i)
                used.add(j)
                break

    return groups


def _build_tolerance_groups(candidates: List[NormalizedToken]) -> List[CalloutGroup]:
    """Group {val}±{tol} specifications (with or without Ø prefix).

    Pairs a numeric/diameter value with an adjacent ± tolerance token.
    E.g. "184" + "±0.1" → "184 ±0.1" as a single callout.
    """
    groups = []
    used = set()

    pair_candidates = []
    tol_start = re.compile(r"^(?:\u00b1|[+-])\s*\d")
    for i, tok in enumerate(candidates):
        if not (token_is_diameter(tok.text) or token_is_numeric_value(tok.text)):
            continue
        if "\u00b1" in tok.text or "/" in tok.text:
            continue
        stripped = re.sub(r"^[\u00d8\u00f8]\s*", "", tok.text.strip())
        if re.fullmatch(r"0\.\d+", stripped):
            continue
        for j, other in enumerate(candidates):
            if j == i:
                continue
            ot = other.text.strip()
            if "/" in ot or not tol_start.match(ot):
                continue
            dx = abs(other.cx - tok.cx)
            dy = abs(other.cy - tok.cy)
            tok_h = max(1.0, tok.y2 - tok.y1)
            other_h = max(1.0, other.y2 - other.y1)
            max_dy = max(60.0, (tok_h + other_h) * 1.2)
            if dx >= 150 or dy >= max_dy:
                continue
            same_column = dx <= max(45.0, 0.35 * max(tok_h, other_h))
            digit_count = len(re.sub(r"\D", "", tok.text))
            pair_candidates.append((
                0 if same_column else 1,
                math.hypot(dx, dy),
                -digit_count,
                i,
                j,
            ))

    for _col_rank, _dist, _digits, i, j in sorted(pair_candidates):
        if i in used or j in used:
            continue
        groups.append(_merge_token_pair(candidates[i], candidates[j], "diameter_tolerance"))
        used.add(i)
        used.add(j)

    return groups

    # Process longer value tokens first — a full number like "25"
    # must claim its tolerance before a stray OCR fragment "5" can.
    order = sorted(
        range(len(candidates)),
        key=lambda i: -len(re.sub(r"\D", "", candidates[i].text)),
    )

    for i in order:
        tok = candidates[i]
        if i in used:
            continue
        # Match diameter tokens OR plain numeric values
        if not (token_is_diameter(tok.text) or token_is_numeric_value(tok.text)):
            continue
        # Skip if already contains ± (already a complete tolerance spec)
        if "±" in tok.text:
            continue
        # Skip sub-unity decimals ("0.005", "0.003"). These are
        # almost always standalone FCF tolerance values, not main
        # dimensions that need a ± tolerance attached. Pairing one
        # with an unrelated "4 ±0" nearby merges them into a bogus
        # "0.005 4 ±0" callout that confuses downstream balloon
        # placement.
        _stripped = re.sub(r"^[Øø]\s*", "", tok.text.strip())
        if re.fullmatch(r"0\.\d+", _stripped):
            continue
        for j, other in enumerate(candidates):
            if j == i or j in used:
                continue
            # The "tolerance" must START with ± or +/- (a pure
            # fragment like "±0.1", "+0.014"). A token like
            # "2 ±0.1" contains ± but is a complete value-with-
            # tolerance dim on its own — merging it as the
            # tolerance for an unrelated nearby value (e.g.
            # "1.000" + "2 ±0.1" -> bogus "1.000 2 ±0.1").
            ot = other.text.strip()
            if not (re.match(r"^±", ot) or re.match(r"^[+-]\d", ot)):
                continue
            # A vertically merged token like "±0.1 / 184" is not a
            # tolerance operand; it already contains a base value.
            # Pair only with the pure sign+numeric tolerance token.
            if "/" in ot:
                continue
            dx = abs(other.cx - tok.cx)
            dy = abs(other.cy - tok.cy)
            # Adaptive vertical threshold: horizontal text has small
            # bbox height (~20 px) so dy 60 is plenty. Rotated text
            # (vertical dimensions) has tall bboxes — scale with the
            # actual token heights so rotated tolerance stacks still
            # group correctly.
            tok_h = max(1.0, tok.y2 - tok.y1)
            other_h = max(1.0, other.y2 - other.y1)
            max_dy = max(60.0, (tok_h + other_h) * 1.2)
            if dx < 150 and dy < max_dy:
                groups.append(_merge_token_pair(tok, other, "diameter_tolerance"))
                used.add(i)
                used.add(j)
                break

    return groups


def _build_enhanced_stacked_groups(candidates: List[NormalizedToken]) -> List[CalloutGroup]:
    """Group numeric pairs like 10.0/9.8 or vertically stacked numbers."""
    groups = []
    used = set()

    for i, tok in enumerate(candidates):
        if i in used:
            continue
        if tok.semantic_type == "dual_use_digit":
            continue
        if "/" in tok.text:
            if re.search(r"\d", tok.text):
                groups.append(_make_singleton(tok, "numeric_pair"))
                used.add(i)
            continue

        if not token_is_numeric_value(tok.text):
            continue

        for j, other in enumerate(candidates):
            if j == i or j in used:
                continue
            if other.semantic_type == "dual_use_digit":
                continue
            if not token_is_numeric_value(other.text):
                continue
            dx = abs(other.cx - tok.cx)
            dy = abs(other.cy - tok.cy)
            if dx < 80 and dy < 60 and dy > 5:
                # Vertically stacked pair — but skip independent dims
                # (e.g. "120.0" / "110.0") so they stay as separate
                # singletons downstream and each gets its own balloon.
                if _are_independent_stacked_decimals(
                    tok.text.strip(), other.text.strip()
                ):
                    continue
                top = tok if tok.cy < other.cy else other
                bot = other if tok.cy < other.cy else tok
                combined_text = f"{top.text}/{bot.text}"
                all_toks = [top, bot]
                g = _make_group(all_toks, "numeric_pair")
                g = CalloutGroup(
                    text=combined_text,
                    tokens=all_toks,
                    x1=g.x1, y1=g.y1, x2=g.x2, y2=g.y2,
                    cx=g.cx, cy=g.cy,
                    callout_type="numeric_pair",
                )
                groups.append(g)
                used.add(i)
                used.add(j)
                break

    return groups


def _build_radius_groups(candidates: List[NormalizedToken]) -> List[CalloutGroup]:
    groups = []
    used = set()
    for i, tok in enumerate(candidates):
        if i in used:
            continue
        if token_is_radius(tok.text) or re.search(r"R\d|TYP", tok.text, re.I):
            groups.append(_make_singleton(tok, "radius"))
            used.add(i)
    return groups


def _build_angle_groups(candidates: List[NormalizedToken]) -> List[CalloutGroup]:
    groups = []
    used = set()
    for i, tok in enumerate(candidates):
        if i in used:
            continue
        if "°" in tok.text or re.search(r"\d+\s*DEG", tok.text, re.I):
            groups.append(_make_singleton(tok, "angle"))
            used.add(i)
    return groups


def _build_thread_groups(candidates: List[NormalizedToken]) -> List[CalloutGroup]:
    groups = []
    used = set()
    for i, tok in enumerate(candidates):
        if i in used:
            continue
        if token_is_thread(tok.text):
            groups.append(_make_singleton(tok, "thread"))
            used.add(i)
    return groups


def _build_keyword_groups(candidates: List[NormalizedToken]) -> List[CalloutGroup]:
    groups = []
    used = set()
    KEYWORDS = {
        "TYP", "REF", "MAX", "MIN", "NOM", "BSC", "TRUE", "APPROX",
        "THRU", "DEEP", "CBORE", "CSINK", "SPOTFACE", "TT", "NO_DIMENSION",
    }
    for i, tok in enumerate(candidates):
        if i in used:
            continue
        # Single-letter uppercase tokens are datum identifiers
        # (A, B, C, …) referenced by dimensions, not dimensions
        # themselves. Mark as used so they aren't emitted as
        # singletons, and skip the keyword-merge path that would
        # otherwise fuse them with an adjacent numeric (producing
        # groups like "37,5 A" whose combined bbox breaks leader
        # targeting and puts the balloon between the two tokens).
        if re.fullmatch(r"[A-Z]", tok.text.strip()):
            used.add(i)
            continue
        if tok.token_type == "keyword" or tok.text.strip().upper() in KEYWORDS:
            # For fused phrases (e.g. "ALL AROUND OUTER PROFILE"),
            # look for an adjacent numeric value to combine with.
            # Engineering annotations often pair a value with a note:
            # "10 ALL AROUND OUTER PROFILE", "Ø4 THRU", etc.
            # Check both phrase-fused tokens and stacked keywords that
            # match known phrase patterns.
            tok_upper = tok.text.strip().upper()
            is_phrase = (
                tok.semantic_type == "phrase"
                or any(
                    " ".join(p) in tok_upper or tok_upper in " ".join(p)
                    for p in _ENGINEERING_PHRASES
                )
            )
            if is_phrase:
                best_j = None
                best_dist = 999.0
                for j, other in enumerate(candidates):
                    if j == i or j in used:
                        continue
                    if not re.search(r"\d", other.text):
                        continue
                    dx = abs(other.cx - tok.cx)
                    dy = abs(other.cy - tok.cy)
                    d = (dx**2 + dy**2) ** 0.5
                    if dx < 150 and dy < 80 and d < best_dist:
                        best_j = j
                        best_dist = d
                if best_j is not None:
                    num_tok = candidates[best_j]
                    combined = _merge_token_pair(num_tok, tok, "keyword")
                    combined = CalloutGroup(
                        text=f"{num_tok.text} {tok.text}",
                        tokens=combined.tokens,
                        x1=combined.x1, y1=combined.y1,
                        x2=combined.x2, y2=combined.y2,
                        cx=combined.cx, cy=combined.cy,
                        callout_type="keyword",
                    )
                    groups.append(combined)
                    used.add(i)
                    used.add(best_j)
                    continue
            groups.append(_make_singleton(tok, "keyword"))
            used.add(i)
    return groups


def _build_singleton_groups(remaining: List[NormalizedToken]) -> List[CalloutGroup]:
    groups = []
    for tok in remaining:
        if tok.text.strip():
            groups.append(_make_singleton(tok, tok.semantic_type or "numeric"))
    return groups


# ─────────────────────────────────────────────────────────────────────────────
# Helpers
# ─────────────────────────────────────────────────────────────────────────────

def _make_group(toks: List[NormalizedToken], callout_type: str) -> CalloutGroup:
    text = " ".join(t.text for t in toks)
    xs = [t.x1 for t in toks] + [t.x2 for t in toks]
    ys = [t.y1 for t in toks] + [t.y2 for t in toks]
    return CalloutGroup(
        text=text,
        tokens=toks,
        x1=min(xs), y1=min(ys),
        x2=max(xs), y2=max(ys),
        cx=sum(t.cx for t in toks) / len(toks),
        cy=sum(t.cy for t in toks) / len(toks),
        callout_type=callout_type,
    )


def _make_singleton(tok: NormalizedToken, callout_type: str) -> CalloutGroup:
    return CalloutGroup(
        text=tok.text,
        tokens=[tok],
        x1=tok.x1, y1=tok.y1,
        x2=tok.x2, y2=tok.y2,
        cx=tok.cx, cy=tok.cy,
        callout_type=callout_type,
    )


def _merge_token_pair(a: NormalizedToken, b: NormalizedToken, callout_type: str) -> CalloutGroup:
    toks = sorted([a, b], key=lambda t: t.cx)
    return _make_group(toks, callout_type)


def _validate_group(group: CalloutGroup) -> bool:
    if not group.text or not group.text.strip():
        return False
    if len(group.tokens) == 0:
        return False
    if group.callout_type == "diameter_tolerance" and "/" in group.text:
        return False
    if group.callout_type == "tolerance" and re.fullmatch(
        r"(?:\u00b1|[+-])\s*\d+(?:\.\d+)?", group.text.strip()
    ):
        return False
    return True


def _dedup_tokens(tokens: List[NormalizedToken]) -> List[NormalizedToken]:
    seen: set = set()
    out: List[NormalizedToken] = []
    for tok in tokens:
        key = (tok.text.strip().upper(), round(tok.cx), round(tok.cy))
        if key not in seen:
            seen.add(key)
            out.append(tok)
    return out


def _is_well_formed_engineering_text(text: str) -> bool:
    t = text.strip()
    if re.search(r"[ØO]\d+\.?\d*[±]\d+\.?\d*", t):
        return True
    if re.search(r"MJ?\d+[Xx\.]\d+\s+\d+[Hh]\d+[Hh]", t):
        return True
    if re.search(r"[A-Z]\d+\s+\d+\.?\d*[Xx]\d+°", t):
        return True
    return False


def _is_fragmented_pattern(text: str) -> bool:
    t = text.strip()
    if re.fullmatch(r"[+-]?\d+[Hh]", t):
        return True
    if t in ("Ø", "±", "+", "-", "x", "X"):
        return True
    return False
