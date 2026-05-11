"""Recolor utilities for blender-tech.

Replace principled-BSDF base-color values across all materials in the scene
according to a palette mapping {old_hex: new_hex}.

Usage from a build.py:

    from _recolor import apply_palette
    apply_palette({"#FFFFFF": "#F2D08F", "#000000": "#3F2A1A"})
"""

from __future__ import annotations

try:
    import bpy
except ImportError:  # pragma: no cover
    bpy = None  # type: ignore


def _hex_to_rgba(h: str) -> tuple[float, float, float, float]:
    h = h.lstrip("#")
    if len(h) == 6:
        r, g, b = (int(h[i : i + 2], 16) / 255.0 for i in (0, 2, 4))
        return (r, g, b, 1.0)
    if len(h) == 8:
        r, g, b, a = (int(h[i : i + 2], 16) / 255.0 for i in (0, 2, 4, 6))
        return (r, g, b, a)
    raise ValueError(f"unsupported hex color: {h}")


def _rgba_to_hex(rgba: tuple[float, float, float, float]) -> str:
    r, g, b = rgba[0], rgba[1], rgba[2]
    return "#{:02X}{:02X}{:02X}".format(int(r * 255), int(g * 255), int(b * 255))


def apply_palette(mapping: dict[str, str], tolerance: float = 0.01) -> int:
    """Swap principled BSDF base colors. Returns count of swaps performed."""
    if bpy is None:
        raise RuntimeError("apply_palette must run inside Blender")
    targets = {k.upper(): _hex_to_rgba(v) for k, v in mapping.items()}
    swaps = 0
    for mat in bpy.data.materials:
        if mat is None or not mat.use_nodes:
            continue
        for node in mat.node_tree.nodes:
            if node.bl_idname != "ShaderNodeBsdfPrincipled":
                continue
            base_color = node.inputs["Base Color"]
            cur = tuple(base_color.default_value)
            cur_hex = _rgba_to_hex(cur).upper()
            if cur_hex in targets:
                base_color.default_value = targets[cur_hex]
                swaps += 1
            else:
                # Tolerance match.
                for tk, tv in targets.items():
                    t = _hex_to_rgba(tk)
                    if (
                        abs(t[0] - cur[0]) < tolerance
                        and abs(t[1] - cur[1]) < tolerance
                        and abs(t[2] - cur[2]) < tolerance
                    ):
                        base_color.default_value = tv
                        swaps += 1
                        break
    return swaps
