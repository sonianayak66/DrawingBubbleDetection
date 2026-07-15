# Auto-Annotate Rules

**Scope:** This document is the authoritative spec for `auto_annotate.py` and any
helper it depends on (`callout_rules.py`, `leader_rules.py`, `leader_geometry.py`,
`leader_path_rules.py`, `ocr_rules.py`). Read this before touching any of those
files. If a proposed change conflicts with these rules, stop and surface the
conflict to the user — do not silently relax a rule.

**Reference style:** `ref1.png` and `ref2.png` (the screenshots in the project
root) are the ground truth for visual style. `D3_annotated_big.png`,
`D6_annotated_big.png`, and `D6a_annotated_big.png` are good real outputs.
`D1_annotated_big.png` and `_auto_ref6_v4.png` are cautionary outputs — every
defect they exhibit is forbidden.

---

## 0. The one-line objective

> Place exactly one numbered maroon balloon per real dimension/feature, in
> clean whitespace, with a short non-crossing leader pointing back at the
> dimension. Numbered top-to-bottom, left-to-right. No duplicates. No
> overlaps. No ink underneath the balloon.

If a change makes any clause of that sentence less true, it is a regression.

---

## 1. Hard rejects (any single occurrence is a bug — fail loud, not silent)

| ID | Forbidden state | Concrete check |
|---|---|---|
| H1 | Two balloons with the same number | `len({b.number for b in balloons}) == len(balloons)` |
| H2 | Two balloons whose discs overlap | For every pair `(a,b)`: `dist(a.center, b.center) >= a.r + b.r + 2` |
| H3 | A balloon disc that intersects any ink pixel in the obstacle mask | `obstacle_mask[disc].sum() == 0` after dilation = 2 px |
| H4 | A balloon placed inside the part outline (closed region of the largest ink contour) | Center of disc must be in the largest-white-component returned by the flood-fill |
| H5 | A balloon whose leader crosses another dimension's bbox | Already implemented as the crossing penalty — must remain a hard reject, not just a soft penalty, for the final candidate |
| H6 | A balloon whose leader crosses another balloon's disc | Test segment vs. every other disc |
| H7 | A balloon for a dimension that already has an existing bubble (when `preserve_existing=True`) | Existing-bubble proximity check stays mandatory |
| H8 | A balloon outside the safe drawing area (within `border_margin` of the image edge) | `border_margin = max(balloon_radius * 3, 0.05 * min(w, h))` |
| H9 | A balloon on top of a GD&T frame, datum target, or callout box | These are detected as ink rectangles and added to the obstacle mask |
| H10 | A skipped or out-of-order number | Numbers must be `1..N` contiguous after `_number_balloons` |
| H11 | A leader segment that crosses part geometry / ink | Rasterise the leader polyline onto a blank canvas, AND with the same ink mask `_build_obstacle_mask` produces, count overlap pixels. Violation when count ≥ **20**. |

**H11 exemption notes** (do NOT remove these — they prevent false positives):
1. The first ~3 px of the leader (from the balloon rim) is exempt — the leader has to start somewhere, tiny grazes at the disc circumference are inevitable and not defects.
2. The last ~3 px of the leader (at the dim end) is exempt — the leader is *supposed* to terminate at or next to dim text/arrow ink. That's the whole point.
3. The violation threshold is ≥ 5 overlap pixels in the interior of the path, not ≥ 1. A single aliased pixel from a diagonal leader grazing a centerline is not a violation; a leader routing *through* a part body is.

**Implementation contract:** add a `_validate_output(balloons, image, obstacle_mask)`
function that runs all of H1–H11 and raises `AutoAnnotateError` listing every
violation. Call it at the end of `auto_annotate()` before returning. Tests must
assert it does not raise on the regression suite.

---

## 2. The dedup contract — one balloon per dimension

Rules, in order of application:

1. **OCR pool dedup** (already partially in `_extract_dimensions`): keep
   intersection-over-min ≥ 0.6 as the bbox-overlap threshold. Process
   largest-bbox-first so the engulfer survives.
2. **Same-text proximity dedup**: same normalized text within
   `max(60 px, 1.2 × avg_bbox_diagonal)` collapses to one.
3. **Tolerance-stack collapse**: `1.7 / 1.5` and `1.7, 1.5` as separate
   tokens must collapse to one callout. Driven by `callout_rules.py`'s
   tolerance grouper — do not bypass it.
4. **TYP-label dedup**: `R4`, `C2`, `R2` repeated across multiple fillets →
   keep first occurrence only.
5. **Post-placement dedup**: after balloons are placed, if two balloons end
   up within `2 × balloon_radius` of each other AND their associated
   dimension texts share ≥ 80% character overlap (Levenshtein-normalized),
   drop the lower-confidence one.

**Test:** regression harness must report `len(balloons)` per image and
fail if it exceeds manually-curated truth count by more than 1.

---

## 3. Placement rules (geometry)

### 3.1 Where a balloon may sit

A position is legal iff all hold:
- Disc + 2 px margin contains zero ink pixels (H3)
- Disc center is in the largest-white-component (H4)
- Disc + 2 px margin does not intersect any other balloon (H2)
- Disc center is inside `[border_margin, w − border_margin] × [border_margin, h − border_margin]` (H8)
- Distance from disc center to its target dimension bbox is in `[1.5 × r, 6 × r]`

### 3.2 Candidate generation

For each dimension, generate candidates in this order, keep the first legal
one with the best score:
1. Four cardinal sides (N, E, S, W) of the dim bbox at shaft length `2×r`.
2. Same four sides at shaft length `4×r`.
3. Four diagonals at shaft length `3×r`.
4. Spiral fallback around the dim bbox, step = `0.5×r`, max radius = `8×r`.

### 3.3 Scoring (for the soft case where multiple candidates are legal)

Lower is better. Sum of:
- Ink density at candidate (from the cell grid).
- Squared distance to nearest already-placed balloon (negative — reward spread).
- Distance from dim bbox center (positive — short leaders).
- `1000.0 × (number of other dim bboxes the leader segment crosses)`.
- `500.0 × (1 if leader crosses any drawing geometry, else 0)`.
- `100.0 × (1 if not on the preferred side, else 0)`.

The 1000.0 weight on dim-bbox crossings is load-bearing; do not lower
without explicit user approval.

---

## 4. Leader rules

### 4.1 Geometry
- **Default:** straight line, balloon disc edge → nearest point on dim bbox.
- **L-bend fallback:** single 90° bend, picking the shorter of h-then-v / v-then-h that stays in clear whitespace.
- **Give up:** if both fail → placement is illegal, move to next candidate. Never draw a leader that crosses geometry.

### 4.2 Style
- Solid line, same maroon as balloon, no arrowhead.
- Width: 1 px at low DPI, 2 px at ≥ 1500 px short-side.
- Leader starts on disc circumference, not center.
- Leader endpoint = bare line terminus, no dot/arrow/marker.

---

## 5. Numbering rules
- After all balloons placed, sort by `(row_band, x)` where `row_band = round(y / row_tol)`, `row_tol = max(30, balloon_radius)`.
- Assign 1..N in that order.
- Numbers must be contiguous — no gaps, no letter suffixes.
- Font: bold serif (Times Triplex equivalent). Color: maroon. Size: digit string fills ~70% of disc diameter.

---

## 6. Render style

| Property | Value |
|---|---|
| Ring color (BGR) | maroon-purple, approx. `(80, 30, 110)` |
| Ring thickness | `max(2, round(r / 11))` px |
| Disc fill | none (transparent interior) |
| Number color | same as ring |
| Number font | `cv2.FONT_HERSHEY_TRIPLEX`, bold |
| Number scale | glyph height ≈ `1.1 × r` |
| Default radius | `max(22, round(0.035 × min(w, h)))` px |

Do not change without side-by-side render against ref1/ref2.

---

## 7. OCR-side rules

- Watermark filter stays on.
- Single-digit orphan tokens with no neighbor and no nearby dim line are dropped.
- `°` ↔ `%` repair stays on.
- Bubble-number tokens (inside existing detected rings) excluded from dim pool.
- Rotated-OCR pool stays separate — upright tolerance-grouper must not merge rotated and upright fragments.

---

## 8. preserve_existing mode

1. Detect existing annotation bubbles via Hough + tint check.
2. For each dim, if any existing bubble center is within `3 × balloon_radius` of dim bbox edge → skip.
3. Auto-add bubbles only for residual dimensions.
4. Numbering of new bubbles continues from `max(existing_numbers) + 1`. Existing bubbles are not renumbered.

---

## 9. Per-change validation checklist

- [ ] `python test_regression.py` passes with no new failures.
- [ ] Re-run auto-annotate on the current suite: **D1a, D1b, D1c, D2, D3, D4a, D4b, D6, D6a, ref6.** (D1.jpg was replaced by the three D1{a,b,c} crops — treat those as the D1 coverage.) All 11 hard rejects pass.
- [ ] Total balloon count per image within ±1 of previous run.
- [ ] Eyeball-diff each render for new overlap/on-geometry/duplicate.
- [ ] Scoring-weight changes: one-line justification per weight.
- [ ] `_extract_dimensions` changes: re-run reverse-detection regression (shared OCR plumbing).

### Current suite baseline (H11 threshold = 20)

| Image | Kept | Drop | Violations |
|---|---|---|---|
| D2 | 8 | 0 | 0 |
| D3 | 6 | 0 | 0 |
| D6 | 6 | 0 | 0 |
| D6a | 5 | 0 | 0 |
| D1a | 4 | 0 | 0 |
| D1c | 7 | 0 | 1 H11 |
| D1b | 5 | 5 | 3 (H11×2, H5×1) |
| D4a | 7 | 0 | 2 H11 |
| D4b | 7 | 2 | 4 H11 |
| ref6 | 16 | 1 | 12 (H11×9, H5×3) |

Severe H11 (≥50 px) is concentrated on ref6/D4a/D4b/D1b — tracks back to upstream garbled-OCR bboxes (see §11 parked OCR item).

---

## 10. Ask-before-changing

Stop and ask the user before:
- Lowering any hard-reject (H1–H10) to a soft penalty.
- Changing maroon color, radius formula, font, or stroke width.
- Bypassing `_validate_output`.
- Adding placement candidate strategy that legalizes positions currently rejected.
- Touching `callout_rules.py` tolerance-grouping logic (shared with M1 reverse detection).
- Adding any new dependency.

---

## 11. Known limitations (accepted, not bugs)

- Bare numeric dim next to a circular feature where OCR dropped the Ø (needs geometric classifier, future file `geometric_dim_classifier.py`).
- `Ø40` read as `840` / `Ø34` read as `034` — OCR confusion, mitigated not solved.
- Heavily ballooned title blocks — `preserve_existing` may suppress nearby real dims.
- **ref6 upper-right quadrant: 2–3 H5 floor.** The region around `Ø4 THRU` / `0.75 × 30°` / `0.02` is dense enough that both single-bend leader orientations cross something. Multi-segment L-bends produce visually ugly leaders that snake around the drawing — worse than accepting the crossing. Regression floor: **≤ 3 H5 on ref6, 0 on everything else.**
- **Dense drawings (e.g. D1 shaft with 22+ candidate dims): drop-unresolvable may remove up to ~10 legitimate dimensions** when the available clean space cannot accommodate all of them at the required inter-balloon gap. Known-lost examples on D1: `Ø50`, `190`, `117,5`, `37,5`, `2×45°`, `[+0,014 C]`. The trade-off (12 clean balloons > 22 overlapping) is correct for the training-data and pre-ballooning-aid use cases, but should be revisited if a single-dim-miss becomes a customer complaint.
- **Drop bias — center loses to edges.** The drop-unresolvable pass tiebreaks by `(round(y/row_tol), x)` descending, which means central / lower-right dims are dropped preferentially when conflicting with upper-left ones. Not a random drop pattern. A future refinement could tiebreak on dim confidence or bbox area instead. *Status: quality-tiebreak added later — the most-garbage balloon now drops first. Center-vs-edge bias still applies when qualities tie.*
- **H11 grazing is the floor.** Leaders that clip 5–15 px of a centerline, dim-extension line, or the border frame are visually subtle and fixing them would require either algorithmic L-bend work (often producing uglier leaders) or tighter placement constraints that over-prune real dims. Accepted — H11 threshold is set to **≥ 20 px** so grazes don't trip the validator. See the H11_THRESHOLD constant in `_validate_output`.
- **Severe H11 (≥ 50 px) ties to the parked OCR Ø-restoration item.** On drawings where OCR garbles a token (e.g. `Ø40` read as `'040 OS'` at a bogus position), the dim's bbox ends up in the wrong place and the balloon's leader gets routed hundreds of pixels across the drawing to reach that bogus position. This manifests as severe H11 on D4a #1 (99 px), D4b #2 (160 px), ref6 #7/#11 (157–165 px). The placement algorithm isn't the root cause; the upstream bad bbox is. Fix lives in `geometric_dim_classifier.py` (not yet built) and is PARKED — only unpark when a real consumer is blocked. Do NOT attempt to fix severe H11 by adding placement heuristics that paper over garbled OCR.

---

## 12. File responsibility map

| File | Owns |
|---|---|
| `auto_annotate.py` | Pipeline, placement, scoring, numbering, render, validation |
| `callout_rules.py` | Tolerance grouping, OCR token → callout group |
| `leader_rules.py` | Leader endpoint selection, L-bend geometry, crossing tests |
| `leader_geometry.py` | Low-level segment / bbox intersection primitives |
| `ocr_rules.py` | Token normalization, watermark filter, char-level repair |
| `preprocess.py` | Image preprocessing (binarization, denoise) — shared with detector |

Touching a file outside the row of the feature you're changing should be a deliberate, justified decision.
