import cv2
import numpy as np
import math
from typing import List, Tuple

def find_circular_contours(gray: np.ndarray, min_area: int = 200, min_circularity: float = 0.55) -> List[Tuple[int, int, int]]:
    r"""Find circular contours as backup to HoughCircles."""
    contours, _ = cv2.findContours(gray, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    circles = []
    for cnt in contours:
        area = cv2.contourArea(cnt)
        if area < min_area:
            continue
        perimeter = cv2.arcLength(cnt, True)
        if perimeter < 1:
            continue
        circularity = (4 * math.pi * area) / (perimeter ** 2)
        if circularity < min_circularity:
            continue
        (cx, cy), radius = cv2.minEnclosingCircle(cnt)
        cx, cy, r = int(round(cx)), int(round(cy)), int(round(radius))
        if r > 8:
            circles.append((cx, cy, r))
    return circles

def fit_ellipse_to_contour(cnt: np.ndarray) -> Tuple[float, float, float, float]:
    r"""Fit ellipse to contour for occluded circles, project to equiv circle radius."""
    if len(cnt) < 5:
        return 0, 0, 0, 0
    ellipse = cv2.fitEllipse(cnt)
    cx, cy = ellipse[0]
    # Equiv circle radius: geometric mean of major/minor axes
    equiv_r = math.sqrt(ellipse[1][0] * ellipse[1][1]) / 2
    angle = ellipse[2]
    return cx, cy, equiv_r, angle

def ray_intersect_circumference(cx: float, cy: float, r: float, dir_x: float, dir_y: float, n_rays: int = 36) -> List[Tuple[float, float]]:
    r"""Ray cast from center to find leader attach points on circumference."""
    points = []
    for i in range(n_rays):
        angle = 2 * math.pi * i / n_rays
        ray_dir_x = math.cos(angle) * dir_x - math.sin(angle) * dir_y
        ray_dir_y = math.sin(angle) * dir_x + math.cos(angle) * dir_y
        attach_x = cx + r * ray_dir_x
        attach_y = cy + r * ray_dir_y
        points.append((attach_x, attach_y))
    return points

