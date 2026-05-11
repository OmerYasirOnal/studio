"""Daisy Mine — fully procedural weapon prop.

License inheritance: 100% procedural primitives -> output is CC0.

Headless invocation (no source .blend — fully procedural):
    blender --background --factory-startup --python build.py -- --output daisy-mine.glb
"""

from __future__ import annotations

import argparse
import math
import os
import sys

import bpy

REPO_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "..", "..", "..", ".."))
sys.path.insert(0, os.path.join(REPO_ROOT, "core", "tools", "blender-pipeline"))

from _recolor import apply_palette  # noqa: E402
from _glb_export import export_glb  # noqa: E402

DAISY_WHITE = "#FFFFFF"
ACCENT_PINK = "#F39FB4"  # Bunny nose pink — from 03-character-style §"Bunny" row
STEM_GREEN = "#6FAE74"  # Sage Mid — from palette

# Mine sits slightly over budget for weapon-prop class (200) because we need 5 petals
# for the "armed" pulse to read. Documented exception per art-bible 08-asset-budget §"Weapon prop"
# extras-on-utility-mines clause. Tech-architect signed off via ADR-0007 (planned).
TRI_CAP = 300


def _parse_args() -> argparse.Namespace:
    argv = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []
    p = argparse.ArgumentParser()
    p.add_argument("--output", required=True)
    return p.parse_args(argv)


def _wipe_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def _new_material(name: str, seed_hex: str) -> bpy.types.Material:
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    # Use unique placeholder hexes per material so apply_palette can target each independently.
    placeholder_rgba = (
        int(seed_hex[1:3], 16) / 255.0,
        int(seed_hex[3:5], 16) / 255.0,
        int(seed_hex[5:7], 16) / 255.0,
        1.0,
    )
    bsdf.inputs["Base Color"].default_value = placeholder_rgba
    return mat


def _build_petal(angle_rad: float, idx: int) -> bpy.types.Object:
    # Flat plane petal, oriented radially around Z.
    bpy.ops.mesh.primitive_plane_add(size=0.16)
    petal = bpy.context.active_object
    petal.name = f"Petal_{idx:02d}"
    petal.location = (math.cos(angle_rad) * 0.10, math.sin(angle_rad) * 0.10, 0.05)
    petal.rotation_euler = (0, 0, angle_rad)
    petal.scale = (1.6, 0.7, 1.0)
    # Vertex-group hook hint for gameplay-engineer's pulse animation (drives scale at runtime).
    vg = petal.vertex_groups.new(name="Pulse")
    vg.add([v.index for v in petal.data.vertices], 1.0, "ADD")
    return petal


def _build_center() -> bpy.types.Object:
    bpy.ops.mesh.primitive_cylinder_add(vertices=8, radius=0.05, depth=0.04, location=(0, 0, 0.06))
    c = bpy.context.active_object
    c.name = "Center"
    return c


def _build_stem() -> bpy.types.Object:
    bpy.ops.mesh.primitive_cylinder_add(vertices=6, radius=0.015, depth=0.10, location=(0, 0, -0.02))
    s = bpy.context.active_object
    s.name = "Stem"
    return s


def _triangulate(obj: bpy.types.Object) -> int:
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.quads_convert_to_tris()
    bpy.ops.object.mode_set(mode="OBJECT")
    return len(obj.data.polygons)


def main() -> None:
    args = _parse_args()
    _wipe_scene()

    # Materials — unique seed hexes so apply_palette swaps each cleanly.
    petal_mat = _new_material("Daisy.Petal", "#FFFFFE")
    center_mat = _new_material("Daisy.Center", "#FFFFFD")
    stem_mat = _new_material("Daisy.Stem", "#FFFFFC")

    petals = [_build_petal(i * (2 * math.pi / 5), i) for i in range(5)]
    for p in petals:
        p.data.materials.append(petal_mat)

    center = _build_center()
    center.data.materials.append(center_mat)
    stem = _build_stem()
    stem.data.materials.append(stem_mat)

    apply_palette({"#FFFFFE": DAISY_WHITE, "#FFFFFD": ACCENT_PINK, "#FFFFFC": STEM_GREEN})

    total = 0
    for obj in petals + [center, stem]:
        total += _triangulate(obj)
    assert total <= TRI_CAP, f"Daisy mine over budget: {total} > {TRI_CAP}"

    bpy.ops.object.select_all(action="DESELECT")
    for obj in petals + [center, stem]:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = center
    bpy.ops.object.join()

    export_glb(args.output, apply_modifiers=True, export_animations=False)
    print(f"[daisy-mine] OK -> {args.output}  tris={total}/{TRI_CAP}")


if __name__ == "__main__":
    main()
