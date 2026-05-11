# Style Overview — Brave Bunny

> Owner: art-director. Cross-refs: `docs/06-tech-spec/` (toon shader + perf budget), `docs/01-research/03-positioning.md` (visual differentiation), `games/brave-bunny/CLAUDE.md` (perf contract: ≤80 draw calls, 250k tris on-screen).

## Style thesis

**Crossy Road meets Cat Quest meets Survivor.io geometry.** Brave Bunny is a saturated low-poly cartoon-mascot world rendered with a 1-tone toon-ramp shader, viewed top-down 3/4. The hero animals are chibi (1.5 head:body), the enemies are puff-blob silhouettes (1:1), the environment is pastel and unthreatening, and the only photographic-grade saturation in the frame is reserved for hero-character highlights, pickups, VFX bursts, and UI accents. Every entity reads from a 32px silhouette test because the game ships to iPhone SE 3 at arm's length. We borrow Crossy Road's voxel-charm proportions, Cat Quest's hand-painted-feeling environment toning, and Survivor.io's swarm-geometry density — then we strip out everything grim about Survivor.io and replace it with smile.

## Moodboard reference table

> CC0-friendly *concepts only*. We name shipped reference games for silhouette/lighting study; we do not lift their art. All actual production assets come from CC0 sources (Quaternius, Kenney) per `core/docs/asset-policy.md`.

| Layer | Reference (study-only) | What we steal | Production source |
|---|---|---|---|
| Hero silhouette | Crossy Road animal cast | Chibi 1.5:1 proportion, micro-bounce idle, single accessory motif | Quaternius Animated Animals (CC0) |
| Hero rig + anim | Quaternius Animated Animals pack | Skinned mesh, 12-anim loop set | Quaternius Animated Animals (CC0) |
| Environment hero shots | Crossy Road open meadow, Cat Quest world map | Pastel ground tint, soft horizon haze, low fog density | Kenney Nature Kit (CC0) |
| Environment props | Kenney Nature Kit (rocks, trees, mushrooms) | Single-material per prop, no PBR | Kenney Nature Kit + recolor |
| Enemy puff-blobs | Survivor.io zombies (silhouette only — not palette) | Density tolerance, swarm-readability rules | Custom low-poly blobs in Blender, ≤200 tris each |
| Boss silhouette | Cat Quest dragon | Cartoon-menacing, not gory; rounded horns/teeth | Quaternius Monsters (CC0) |
| VFX register | Vampire Survivors mobile + Survivor.io upgrade visuals | Hot accent ring on hit, white emissive flash, radial fill on level-up | Built in-engine (URP particles + shader) |
| UI accent | Cat Quest UI gold, Crossy Road title-card chunkiness | Round corners ≥ 16px, thick stroke, drop-shadow at 4px | UI Toolkit + USS |

## Camera spec

| Param | Value | Notes |
|---|---|---|
| Type | Top-down 3/4 perspective | Per `GAME.md` |
| Projection | Perspective (NOT orthographic) | Orthographic kills the toon-ramp depth cue |
| FOV | 35° | Narrow enough for parallax, wide enough for swarm awareness |
| Pitch | 55° down from horizontal | Hero reads from above and slightly behind |
| Distance from focus | 18 Unity units | Hero is the focus point; camera follows with 0.15s smoothing |
| Aspect support | 9:16 to 9:21 (iPhone SE 3 to iPhone 12 Pro Max) | Safe area accounted for in UI layer |

## Proportion rules

| Entity class | Head:body ratio | Total Y-axis height (Unity units) | Silhouette test |
|---|---|---|---|
| Hero animal (chibi) | 1.5 : 1 | 1.0 | Must read from 32px circle at FOV=35°, dist=18u |
| Standard enemy (puff/blob) | 1.0 : 1 (often headless blob) | 0.6 | Same 32px test |
| Elite enemy | 1.2 : 1 | 0.9 | Same, plus distinctive accent ring |
| Boss | 2.0 : 1 (heroic) | 2.4 | Must read from 64px circle |
| Pickup | n/a (icon-style) | 0.3 | Must read at 16px with hot-accent halo |

## Silhouette rule

Every spawnable goes through the **32-pixel circle test** before it leaves art-director sign-off:

1. Render the entity in-engine at gameplay camera distance.
2. Downsample to 32×32px.
3. Convert to pure black-on-white silhouette.
4. If you can't tell what class of thing it is (hero / enemy / boss / pickup / prop), it does not ship.

Bosses get the same test at 64×64px.

## Hand-painted feel directive (cross-ref to tech-spec)

We achieve a hand-painted *feel* on low-poly geometry through a custom URP **toon shader** with these properties — gameplay-engineer and tech-architect will spec the actual implementation:

- **1-tone shadow ramp** (NOT smooth Lambert). Single hard threshold per material, ramp texture authored per biome.
- **Per-vertex paint tint** allowed for cheap variation without extra materials (stays under the 80-DC cap by sharing atlas).
- **No normal maps**, no metallic, no roughness textures. Flat color + ramp + optional emissive.
- **Outline pass: NO**. Outlines blow the draw-call budget on mobile. Silhouette reads via lighting + saturation contrast instead.
- **Static batching ON** for all environment chunks; **GPU instancing ON** for swarm enemies.

## Saturation budget

> The most important visual rule in the entire game.

| Layer | Saturation ceiling | Notes |
|---|---|---|
| Environment ground + props | 80% (pastels) | HSV S ≤ 0.80, V ≥ 0.65 |
| Ambient atmosphere (fog, sky) | 50% | Even softer |
| Standard enemies | 70% | Slightly desaturated so heroes pop |
| Hero character | 100% (max saturation reserved) | Hero MUST be the most saturated thing on screen at all times |
| Pickups (XP gem, gold, heal) | 100% + emissive | Pickups out-saturate everything |
| VFX (hit flash, level-up burst) | 100% + bloom | Brief 100ms peaks only |
| UI accents (level-up buttons, hot-CTA) | 100% | Persistent |

If your screenshot looks busy, the first thing to lower is environment saturation — never hero/pickup/VFX.

## What we don't do

- No gore, blood splatter, bone fragments, or visible wounds.
- No skull motifs (zero — not even on tombstones in the Cavern biome).
- No photorealism. No PBR materials. No normal maps. No realistic skin shaders.
- No dark/grim palettes. The Cavern biome is dim, not dark; plum and coral, not black and red.
- No realistic fire. Fire is stylized orange-yellow flat-shaded with a single 2-frame flicker.
- No anime moe tropes. Heroes are animals, not humans.
- No edgy text styling, no glitch effects, no horror-game UI motifs.
- No real-world brands, logos, real currency symbols on pickups.
- No depth-of-field on the gameplay camera (kills readability — see `02-lighting.md`).
