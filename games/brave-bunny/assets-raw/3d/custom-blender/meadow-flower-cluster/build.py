"""Meadow Flower Cluster — filler-prop cluster for Meadow chunks.

License inheritance: 100% procedural primitives -> output is CC0.

Builds 5 small flowers (2 color variants) in a deterministic-but-jittered layout
within a 1x1 u area. The output mesh is joined so the level-designer drops a
single instance per chunk corner (the GPU instancer batches them across chunks).

Headless invocation:
    blender --background --factory-startup --python build.py -- --output meadow-flower-cluster.glb
"""

from __future__ import annotations

import argparse
import hashlib
import math
import os
import random
import sys

import bpy

REPO_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "..", "..", "..", ".."))
sys.path.insert(0, os.path.join(REPO_ROOT, "core", "tools", "blender-pipeline"))

from _recolor import apply_palette  # noqa: E402
from _glb_export import export_glb  # noqa: E402

DAISY_YELLOW = "#FFE066"  # cheerful yellow (sits in Meadow palette comfortably)
DAISY_PINK = "#F39FB4"  # accent pink — re-uses bunny nose hex for palette cohesion
STEM_GREEN = "#6FAE74"  # Sage Mid

TRI_CAP = 250  # Filler prop cap (300) with safety margin for the GPU instancer batch budget

# Deterministic seed from entity name -> jitter same every CI run.
SEED = int(hashlib.sha1(b"meadow-flower-cluster").hexdigest()[:8], 16)


def _parse_args() -> argparse.Namespace:
    argv = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []
    p = argparse.ArgumentParser()
    p.add_argument("--output", required=True)
    return p.parse_args(argv)


def _wipe_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def _assign(obj: bpy.types.Object, name: str, seed_hex: str) -> None:
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    rgba = (
        int(seed_hex[1:3], 16) / 255.0,
        int(seed_hex[3:5], 16) / 255.0,
        int(seed_hex[5:7], 16) / 255.0,
        1.0,
    )
    bsdf.inputs["Base Color"].default_value = rgba
    obj.data.materials.append(mat)


def _build_one_flower(x: float, y: float, yaw: float, variant: str) -> list[bpy.types.Object]:
    parts: list[bpy.types.Object] = []
    # Stem
    bpy.ops.mesh.primitive_cylinder_add(vertices=5, radius=0.01, depth=0.10, location=(x, y, 0.05))
    stem = bpy.context.active_object
    _assign(stem, f"FlowerStem.{variant}", "#FFFFFC")
    parts.append(stem)
    # Head (single low-poly plane as 4-petal star — cheap)
    bpy.ops.mesh.primitive_plane_add(size=0.08, location=(x, y, 0.11))
    head = bpy.context.active_object
    head.rotation_euler = (0, 0, yaw)
    placeholder = "#FFFFFE" if variant == "yellow" else "#FFFFFD"
    _assign(head, f"FlowerHead.{variant}", placeholder)
    parts.append(head)
    return parts


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

    rng = random.Random(SEED)
    all_parts: list[bpy.types.Object] = []
    # 5 flowers, alternating variants, jittered inside a 1x1 footprint.
    for i in range(5):
        x = rng.uniform(-0.45, 0.45)
        y = rng.uniform(-0.45, 0.45)
        yaw = rng.uniform(0, math.tau)
        variant = "yellow" if i % 2 == 0 else "pink"
        all_parts.extend(_build_one_flower(x, y, yaw, variant))

    apply_palette({"#FFFFFE": DAISY_YELLOW, "#FFFFFD": DAISY_PINK, "#FFFFFC": STEM_GREEN})

    total = 0
    for obj in all_parts:
        total += _triangulate(obj)
    assert total <= TRI_CAP, f"Flower cluster over budget: {total} > {TRI_CAP}"

    bpy.ops.object.select_all(action="DESELECT")
    for obj in all_parts:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = all_parts[0]
    bpy.ops.object.join()

    export_glb(args.output, apply_modifiers=True, export_animations=False)
    print(f"[meadow-flower-cluster] OK -> {args.output}  tris={total}/{TRI_CAP}")


if __name__ == "__main__":
    main()
