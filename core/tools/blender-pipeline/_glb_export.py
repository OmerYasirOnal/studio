"""Standardized glTF 2.0 binary export options for Unity ingestion.

Usage from a build.py:

    from _glb_export import export_glb
    export_glb("/abs/path/out.glb", apply_modifiers=True)
"""

from __future__ import annotations

try:
    import bpy
except ImportError:  # pragma: no cover - runs only inside Blender
    bpy = None  # type: ignore


def export_glb(out_path: str, apply_modifiers: bool = True, export_animations: bool = True) -> None:
    if bpy is None:
        raise RuntimeError("export_glb must run inside Blender (bpy not available)")
    # Unity conventions: Y-up forward, +Z up, embedded buffers (.glb).
    bpy.ops.export_scene.gltf(
        filepath=out_path,
        export_format="GLB",
        export_apply=apply_modifiers,
        export_yup=True,
        export_animations=export_animations,
        export_image_format="AUTO",
        export_normals=True,
        export_tangents=True,
        export_materials="EXPORT",
        export_skins=True,
    )
