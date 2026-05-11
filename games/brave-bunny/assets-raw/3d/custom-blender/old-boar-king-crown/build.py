"""Old Boar King Crown — additive boss prop, exported as a standalone mesh.

License inheritance: 100% procedural primitives -> output is CC0.
The crown is parented to the Quaternius boar boss mesh's head bone IN UNITY,
not in Blender — so this script never touches the boar source mesh.

Headless invocation:
    blender --background --factory-startup --python build.py -- --output old-boar-king-crown.glb
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

GOLD = "#FFD23F"
RUBY = "#FF3B6B"

TRI_CAP = 150  # Boss accessory — small slice of the 8 000 boss tris cap


def _parse_args() -> argparse.Namespace:
    argv = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []
    p = argparse.ArgumentParser()
    p.add_argument("--output", required=True)
    return p.parse_args(argv)


def _wipe_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def _build_ring() -> bpy.types.Object:
    # Short cylinder as the crown band.
    bpy.ops.mesh.primitive_cylinder_add(vertices=10, radius=0.12, depth=0.08, location=(0, 0, 0))
    ring = bpy.context.active_object
    ring.name = "CrownRing"
    return ring


def _build_tooth(angle_rad: float, idx: int) -> bpy.types.Object:
    bpy.ops.mesh.primitive_cone_add(vertices=4, radius1=0.025, radius2=0.0, depth=0.08)
    tooth = bpy.context.active_object
    tooth.name = f"Tooth_{idx:02d}"
    tooth.location = (math.cos(angle_rad) * 0.115, math.sin(angle_rad) * 0.115, 0.07)
    return tooth


def _build_gem() -> bpy.types.Object:
    bpy.ops.mesh.primitive_cylinder_add(vertices=6, radius=0.02, depth=0.02, location=(0.12, 0, 0.0))
    gem = bpy.context.active_object
    gem.name = "Gem"
    gem.rotation_euler = (0, math.pi / 2, 0)
    return gem


def _assign(obj: bpy.types.Object, name: str, seed_hex: str) -> None:
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    seed_rgba = (
        int(seed_hex[1:3], 16) / 255.0,
        int(seed_hex[3:5], 16) / 255.0,
        int(seed_hex[5:7], 16) / 255.0,
        1.0,
    )
    bsdf.inputs["Base Color"].default_value = seed_rgba
    obj.data.materials.append(mat)


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

    ring = _build_ring()
    _assign(ring, "Crown.Ring", "#FFFFFE")
    teeth = [_build_tooth(i * (2 * math.pi / 5), i) for i in range(5)]
    for t in teeth:
        _assign(t, f"{t.name}.Mat", "#FFFFFE")
    gem = _build_gem()
    _assign(gem, "Crown.Gem", "#FFFFFD")

    apply_palette({"#FFFFFE": GOLD, "#FFFFFD": RUBY})

    total = 0
    for obj in [ring] + teeth + [gem]:
        total += _triangulate(obj)
    assert total <= TRI_CAP, f"Crown over budget: {total} > {TRI_CAP}"

    bpy.ops.object.select_all(action="DESELECT")
    for obj in [ring] + teeth + [gem]:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = ring
    bpy.ops.object.join()

    # Scale to fit a boar head (~0.3 u tall total). Cylinder depth + tooth height is ~0.15 -> scale 2.
    bpy.context.active_object.scale = (1.0, 1.0, 1.0)

    export_glb(args.output, apply_modifiers=True, export_animations=False)
    print(f"[old-boar-king-crown] OK -> {args.output}  tris={total}/{TRI_CAP}")


if __name__ == "__main__":
    main()
