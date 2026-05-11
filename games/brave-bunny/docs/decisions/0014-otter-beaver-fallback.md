# ADR-0014: Otter-Beaver fallback for the 8-character launch roster

- **Date**: 2026-05-12
- **Status**: accepted
- **Author**: asset-curator (second-pass fetch follow-up)
- **Supersedes**: —
- **Related**: `assets-raw/INDEX.md` §3D Characters (Otter row, flagged RISK); `assets-raw/fetch-log.md` (Quaternius distribution finding); `docs/02-gdd/03-characters.md` (8-character launch roster); `docs/07-art-bible/09-source-shortlist.md` (per-character source plan)

## Context

The Brave Bunny launch roster (per GDD §03-characters) ships **8 playable animals**: bunny, tortoise, fox, hedgehog, **otter**, panda, badger, owl. Seven of those eight are confirmed-present in the Quaternius Ultimate Animated Animals pack (verified by Quaternius's pack-page imagery and pack metadata: "12 different animals" — 11 of which are common animal species the pack page imagery shows).

The **otter** specifically is flagged in `assets-raw/INDEX.md` as a **risk** because Quaternius's public pack page does not enumerate the per-animal roster, and Quaternius's distribution channel (Google Drive folder — see fetch-log.md second-pass finding) requires manual download by a human before pack contents can be verified. asset-curator cannot inspect pack contents from the CLI; the otter-present-or-absent fact will only be known after the human delivers the ZIP.

The framework's CC0-only asset policy (`core/docs/asset-policy.md` §Allowed sources) forbids commissioning custom 3D meshes from paid AI services (Meshy / Tripo / Hunyuan3D etc.) and the project's "no-Blender-from-scratch for characters" principle (per `docs/07-art-bible/09-source-shortlist.md`) means we cannot author a from-scratch Otter mesh in Blender for launch — only kitbashes / recolors / prop additions on top of existing CC0 meshes are in-scope for blender-tech in Phase 5.

This ADR pre-decides the fallback path so Phase 5 can proceed without a synchronous "wait for human + audit + redecide" stall once the Quaternius pack lands.

## Decision

When the Quaternius Ultimate Animated Animals pack is delivered to `assets-raw/3d/characters/quaternius/`, asset-curator runs an immediate **Otter audit**:

1. **If a mesh whose filename matches `/otter/i` (e.g. `Otter.fbx`, `otter_animated.glb`) exists in the pack**: use it. Recolor to river-brown (`#6B4A3A`) with a belly-shell decal per the art bible. No further action.

2. **If no otter mesh is present**: fall back to **Beaver** (or, if Beaver is also absent, the closest available river/wetland mammal — most likely Otter's body shape is approximated by `Weasel`/`Ferret`/`Beaver`). Specifically:
   - **Keep the gameplay slug `otter`** in `CharacterDefinition` (do not rename to "beaver" — the launch roster is a public-facing fiction the GDD has already committed to).
   - **Point `CharacterDefinition.mesh`** at the Beaver `.fbx` from the same Quaternius pack.
   - **Recolor** to river-brown (`#6B4A3A`) + belly cream — overrides the Beaver's native warm-brown.
   - **blender-tech adds a small paddle-prop** (Otter's distinguishing tail silhouette) bolted to the Beaver tail's root bone via Blender modifier. Tri-budget: ≤30 tris for the paddle.
   - **Re-export** as `otter_animated.fbx` under `assets-raw/3d/characters/custom-blender/` with the original Quaternius rig+animations preserved (Beaver and Otter share quadruped + biped-stand poses in Quaternius's animation library, so anim retargeting is identity).

Both branches preserve the project's two hard constraints:
- **No paid AI mesh generation.**
- **No from-scratch character authoring in Blender** — only kitbash/recolor/prop-add on top of CC0 source.

## Consequences

### Positive

- **No Phase 5 stall**: gameplay-engineer can build the character-pick UI with all 8 slugs hardcoded and a placeholder mesh per slug; the Otter slot will resolve to *either* the native Otter or the Beaver-kitbash without code changes.
- **Roster fiction preserved**: the public-facing launch announcement, app store screenshots, and trailer can all show "8 Brave Bunnies" without rolling back to a 7-character launch.
- **blender-tech scope stays bounded**: paddle-prop is ≤30 tris and reuses the Beaver rig + Quaternius's animation library — no new rigging, no new animation, no skinning rework. Estimated effort: 2-3 hours.

### Negative

- **Silhouette compromise**: the Beaver-with-paddle approximates an otter's profile but isn't perfect. Otter is sleeker; Beaver is rounder. If side-by-side screenshots in marketing show the bunny and otter together, the otter may read as "wrong species" to attentive players. Mitigation: pose Otter in the character-select screen in a swimming/lying-low pose (per the art bible's hero-pose guideline) where the body's roundness reads more "river-mammal" than "beaver".
- **Animation library gaps**: if the Beaver rig in Quaternius's pack lacks a "swim" animation (which Otters need for the Water variants of meadow/beach maps), the Otter character's swim cycle will fall back to a "walk-in-place underwater" placeholder. This is acceptable for launch but is a known polish-pass debt.
- **blender-tech adds a tracked custom asset**: the paddle prop becomes a permanent line-item in `07-art-bible/09-source-shortlist.md` §Gap list, increasing the custom-Blender tally from 6 weapons to 6 weapons + 1 character-prop.

### Neutral

- The Quaternius Animated Animals pack may include 12 species (per pack page) that include some we haven't budgeted character slots for (e.g. Buffalo, Llama, Camel). If the human audit reveals an Otter-like species we hadn't considered (e.g. **Sea Otter** vs **River Otter** model variants), this ADR's decision still applies: prefer native mesh > Beaver-kitbash, and only fall back to kitbash if no obvious river-mammal mesh exists.

## Alternatives considered

### 1. Skip Otter from launch roster (8 → 7 characters)

Drop Otter from the launch character pick. **Rejected** because:
- The GDD §03-characters and `07-art-bible/03-character-palette.md` both ship the Otter; rolling back is a doc-cascade that touches at least 4 spec files and the marketing trailer plan.
- The launch monetization model (per `docs/02-gdd/04-monetization-and-iap.md`) assumes 8 character slots in the Battle Pass — dropping to 7 perturbs the reward-track economy.
- Players read 7 vs 8 as "one less than peer games (Survivor.io = 12)" — psychologically worse than "8 with one slightly off-model" for cozy-roguelite reception.

### 2. Commission a custom Blender Otter from scratch

Author the Otter mesh+rig+animations from zero in Blender. **Rejected** because:
- Violates the `07-art-bible/09-source-shortlist.md` "no-Blender-from-scratch for characters" principle which is itself a hard constraint born from blender-tech's bandwidth budget (Phase 5 wave 2 has ~40 hours of Blender time allocated, fully consumed by 6 custom weapons + ~10 environment props + 4 trash-puff bases).
- The animation work alone (idle, walk, run, attack, hit, victory) is ~12 hours, leaving zero margin for weapon/prop work.

### 3. Use Sketchfab's CC0 Otter listings as the source

Fetch an Otter mesh from Sketchfab (a permitted source under `core/docs/asset-policy.md`) and integrate as the canonical Otter. **Rejected for launch** because:
- Sketchfab CC0 meshes vary wildly in rig topology and animation availability; integrating one means re-rigging to match Quaternius's animation library so the 8 characters share retargeting logic.
- The whole point of using Quaternius for 7/8 characters is the **shared animation library** — a Sketchfab Otter breaks that and forces an Otter-specific animation set, which is ~the same cost as alternative 2.
- Sketchfab Otters with the cozy-low-poly aesthetic in our art bible's tolerance window are rare; spot-search at design time showed 2 candidates, both ~5k tris (above our 2.5k character budget) and both lacking a humanoid-equivalent rig.

This option remains open as a **post-launch polish path** (Phase 7+) if the Beaver-kitbash reads poorly in playtests.

### 4. Use Quaternius's Stylized Animals (older, separate pack) instead of Ultimate Animated Animals

Quaternius has at least three animal packs published over time; the Ultimate Animated Animals is the newest with rigs+anims. **Rejected** because:
- Mixing rigs across packs breaks the shared-animation premise (same problem as alternative 3, internalised).
- The older packs lack the same anim coverage (no attack/hit/victory).

## Implementation notes (for the asset-curator / blender-tech to execute)

When the Quaternius pack lands, run **in order**:

1. `unzip` the pack to `assets-raw/3d/characters/quaternius/animated-animals/` (preserving the pack's internal directory structure).
2. `ls animated-animals/ | grep -iE 'otter|beaver|weasel|ferret|sea_otter|river_otter'` — audit which river-mammal mesh files exist.
3. **Branch A (otter present)**: append a row to `LICENSES.md` for the unzipped pack, register `otter_animated.fbx` in `data/balance/characters.json` under the `otter` slug, mark this ADR as `Resolved — native Otter mesh used`.
4. **Branch B (otter absent, Beaver present)**: copy the Beaver mesh to `assets-raw/3d/characters/custom-blender/otter_from_beaver.blend`, file a follow-up task for blender-tech to add the paddle prop + recolor + re-export as `otter_animated.fbx`, register in `characters.json`. Add a `LICENSES.md` row for the derived work pointing to the Beaver source + CC0 + "Derivative work: paddle prop and recolor by blender-tech, CC0".
5. **Branch C (both absent)**: re-open this ADR with `Status: superseded by NNNN` and re-decide between Sketchfab fetch or roster cut. Notify orchestrator.

## References

- `assets-raw/INDEX.md` — Otter risk flag (§3D Characters, row 5)
- `assets-raw/fetch-log.md` — Quaternius distribution finding (§Second pass, "What I learned about Quaternius")
- `assets-raw/LICENSES.md` — pending row for Quaternius Animated Animals
- `docs/02-gdd/03-characters.md` — 8-character launch roster fiction
- `docs/07-art-bible/03-character-palette.md` — Otter color tokens
- `docs/07-art-bible/09-source-shortlist.md` — "no-Blender-from-scratch for characters" principle
- `core/docs/asset-policy.md` — CC0-only constraint, no paid-AI exclusion
- ADR-0002 (Toon shader source), ADR-0009 (Polymorphic mechanics registry) — both touch CharacterDefinition, no conflict
