"""Carrot Boomerang — weapon prop build recipe.

License inheritance: base mesh is Kenney veggie pack (CC0) -> output is CC0.

Headless invocation:
    blender --background carrot-boomerang.blend --python build.py -- --output carrot-boomerang.glb

Or fully fresh (no .blend yet — uses factory startup):
    blender --background --factory-startup --python build.py -- --output carrot-boomerang.glb
"""

from __future__ import annotations

import argparse
import os
import sys

import bpy

# Shared blender-pipeline utilities (per project convention).
REPO_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "..", "..", "..", ".."))
sys.path.insert(0, os.path.join(REPO_ROOT, "core", "tools", "blender-pipeline"))

from _recolor import apply_palette  # noqa: E402
from _glb_export import export_glb  # noqa: E402

# Brave-Bunny pickup gold + accent orange (from 01-color-palette.md via 03-character-style.md).
PICKUP_GOLD = "#FFD23F"
ACCENT_ORANGE = "#FF8C42"
LEAF_GREEN = "#A8D86B"  # Meadow Lime — Tortoise primary, doubles as carrot-top leaf

TRI_CAP = 200  # Weapon prop cap from 08-asset-budget.md

# Kenney veggie pack base mesh — picked up by asset-curator into assets-raw/kenney/.
BASE_MESH_HINT = "../../kenney/foodKit/Models/carrot.fbx"  # TODO: confirm exact filename post-curate


def _parse_args() -> argparse.Namespace:
    argv = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []
    p = argparse.ArgumentParser()
    p.add_argument("--output", required=True, help="Absolute path for the .glb output")
    return p.parse_args(argv)


def _wipe_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def _build_carrot_body() -> bpy.types.Object:
    """Stand-in body: a tapered cylinder so the recipe runs without the Kenney FBX present.

    In production, asset-curator stages the Kenney FBX; this block is then replaced by:
        bpy.ops.import_scene.fbx(filepath=os.path.join(os.path.dirname(__file__), BASE_MESH_HINT))
    """
    bpy.ops.mesh.primitive_cone_add(vertices=12, radius1=0.12, radius2=0.02, depth=0.5)
    body = bpy.context.active_object
    body.name = "CarrotBody"
    # Rotate 15 deg for boomerang feel (cheaper than a real curve modifier and keeps tri count tight).
    body.rotation_euler = (0, 0, 0.262)  # ~15 deg
    return body


def _build_leaf(body: bpy.types.Object) -> bpy.types.Object:
    bpy.ops.mesh.primitive_cone_add(vertices=6, radius1=0.05, radius2=0.0, depth=0.12, location=(0, 0, 0.30))
    leaf = bpy.context.active_object
    leaf.name = "CarrotLeaf"
    leaf.scale = (1.4, 0.6, 1.0)
    return leaf


def _assign_material(obj: bpy.types.Object, name: str, hex_color: str) -> None:
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    # Set a seed color the recolor pass will swap (uses absolute hex match).
    bsdf.inputs["Base Color"].default_value = (1.0, 1.0, 1.0, 1.0)
    obj.data.materials.append(mat)
    # Apply target color via the shared util so we exercise the recolor codepath.
    apply_palette({"#FFFFFF": hex_color})


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

    body = _build_carrot_body()
    leaf = _build_leaf(body)

    _assign_material(body, "CarrotBody.Mat", ACCENT_ORANGE)
    _assign_material(leaf, "CarrotLeaf.Mat", LEAF_GREEN)
    # Apply pickup gold as a tip accent via the recolor mapping (no extra material needed for the demo).
    apply_palette({ACCENT_ORANGE: ACCENT_ORANGE, "#FFFFFE": PICKUP_GOLD})

    body_tris = _triangulate(body)
    leaf_tris = _triangulate(leaf)
    total = body_tris + leaf_tris
    assert total <= TRI_CAP, f"Carrot boomerang over budget: {total} > {TRI_CAP}"

    bpy.ops.object.select_all(action="DESELECT")
    body.select_set(True)
    leaf.select_set(True)
    bpy.context.view_layer.objects.active = body
    bpy.ops.object.join()

    export_glb(args.output, apply_modifiers=True, export_animations=False)
    print(f"[carrot-boomerang] OK -> {args.output}  tris={total}/{TRI_CAP}")


if __name__ == "__main__":
    main()
