# Old Boar King — Arena

> Owner: level-designer. Per-phase arena staging for the Old Boar King boss fight in the Meadow biome. Sister docs: `mechanics.md` (attack patterns + phase gates), `../../01-biomes/meadow/layout.md` (base Meadow arena spec), `../../01-biomes/meadow/waves.json` (boss event at t=420, arena_mods block). All frame counts assume 60 fps. All distances in Unity units (u).

## Base arena (inherited from Meadow)

The boss fight uses the **standard Meadow arena** (per `../../01-biomes/meadow/layout.md`) without shrinking the play area. Per `06-biomes.md`, Meadow is the calibration biome — the boss arena layers props *into* the base arena rather than swapping to a custom boss arena.

| Param | Value | Notes |
|---|---|---|
| Playable area | 80 × 80 u | Inherited; no shrink during boss |
| Outer soft boundary | Dense bushes at radius 42 u | Inherited |
| Hard boundary | Invisible collider at radius 45 u | Inherited |
| Camera | Top-down 3/4, FOV 35°, distance 18 u | Inherited |
| Hero props (always present) | Lone tree (+18, +12), wooden well (-16, -14), mushroom cluster (+14, -2) | Inherited; provide positional landmarks during fight |

The boss spawns at the **arena center** (player anchor at fight start), with a 1.5 s entrance animation. The player is briefly pushed to a position 8 u south of the boss during the entrance to clear the line-of-sight for the entrance VFX. Time-dilate to 0.4x for 800 ms during the entrance (per `waves.json` boss-event spec).

## Phase-by-phase staging

### Phase 1 — Awake-and-grumpy (100-66% HP)

**Arena mods**: none.

The arena is the **clean Meadow base**. The three hero props (lone tree, wooden well, mushroom cluster) are the only world geometry. Boss occupies center; player has full 80 × 80 u to work with.

Chunk-by-chunk layout (5 × 5 chunk grid, chunks are 16 × 16 u, origin at world center):

```
chunk grid (x, z)  ->  contents
( -2, +2 )  grass-base, scuff decals
( -1, +2 )  grass-base, flower scatter
(  0, +2 )  grass-base, dirt-path north stub
( +1, +2 )  grass-base, lone-tree (offset +18, +12) — overlaps into chunk (+1, +1)
( +2, +2 )  grass-base, dense-bush outer (visible at edge of play area)

( -2, +1 )  grass-base
( -1, +1 )  grass-base
(  0, +1 )  grass-base, mushroom-cluster (+14, -2) — sits in chunk (+1, 0) actually; this row clean
( +1, +1 )  grass-base, lone-tree overlap from north
( +2, +1 )  grass-base, dense-bush outer

( -2,  0 )  grass-base, dense-bush outer west
( -1,  0 )  grass-base
(  0,  0 )  grass-base, BOSS SPAWN POINT (center)
( +1,  0 )  grass-base, mushroom-cluster
( +2,  0 )  grass-base, dense-bush outer east

( -2, -1 )  grass-base, dense-bush outer
( -1, -1 )  grass-base, wooden-well (-16, -14) — overlaps into chunk (-1, -1)
(  0, -1 )  grass-base
( +1, -1 )  grass-base
( +2, -1 )  grass-base, dense-bush outer

( -2, -2 )  grass-base, dense-bush outer
( -1, -2 )  grass-base, wooden-well overlap from north
(  0, -2 )  grass-base, dirt-path south stub
( +1, -2 )  grass-base
( +2, -2 )  grass-base, dense-bush outer
```

(In practice, the chunks pre-merge per art-bible 04 into 1-2 DC; the per-chunk breakdown above is for level-design clarity, not runtime structure.)

### Phase 2 — Furrowed-brow (66-33% HP)

**Arena mods**: 2 tree-stumps spawn at fixed positions to provide cover.

| Prop | Position (u from center) | Function |
|---|---|---|
| `tree-stump-a` | (-20, +6) — west-northwest | Player can use to break boss line-of-sight; sits between WNW spawner SP_NW and player center |
| `tree-stump-b` | (+22, -8) — east-southeast | Companion cover on opposite side; symmetric break of boss aggro line |

Tree-stumps are **non-traversal-blocking** (player walks through visually) but **occlude boss line-of-sight** for the targeting AI: if a stump is between the boss and the player, the boss's sweep/charge tells re-target with a 0.5 s aim-update delay. This gives the player a usable cover mechanic without making the boss feel "broken" if the player camps a stump.

Tree-stumps spawn with a 0.6 s "pop-in" VFX (small grass-puff) at the phase-2 transition. They persist for the remainder of the fight (carry into phase 3).

### Phase 3 — Fully-cross (33-0% HP)

**Arena mods**: 4 ground-crack decals appear at fixed positions to telegraph the radial stomp shockwave pattern.

| Decal | Position (u from center) | Function |
|---|---|---|
| `ground-crack-n` | (0, +12) — north of boss center | Pre-telegraph for stomp shockwave north arc |
| `ground-crack-e` | (+12, 0) — east | Pre-telegraph for east arc |
| `ground-crack-s` | (0, -12) — south | Pre-telegraph for south arc |
| `ground-crack-w` | (-12, 0) — west | Pre-telegraph for west arc |

Ground-cracks are **cosmetic decals between stomps** (faint brown jagged-line texture, low contrast). They **pulse red** for 0.4 s before each stomp shockwave attack, telegraphing which arcs are about to expand.

Per `mechanics.md`, the stomp shockwave has 2 visible gaps. The gaps are positioned at **opposite arcs** (always N+S OR E+W; the boss randomizes which axis is the gap pair). The ground-cracks make this read possible — the cracks that pulse are the *damage* arcs; the cracks that **don't** pulse are the safe gaps.

The 4 tree-stumps from phase 2 persist; the player now has stump cover + readable gap-windows + the standard hero props (lone tree, wooden well, mushroom cluster) as positional landmarks. The arena is **at its most cluttered** in phase 3, but every clutter element either telegraphs an attack (cracks), enables a defense (stumps), or is a landmark (props).

## Hazard count

| Phase | Active hazards | Hazard sources |
|---|---|---|
| 1 | 0 | Boss attacks only |
| 2 | 0 | Tree-stumps are cover, not hazards |
| 3 | 0 (stomps are boss attacks, not arena hazards) | Ground-cracks are telegraphs, not standalone hazards |

The Meadow boss arena has **zero true arena hazards** per the calibration-biome rule. All damage sources are boss-attack-driven; the arena props enable defense (stumps) or readability (cracks). Subsequent biome bosses (Crab Captain in Beach, Mama Oak in Forest, etc.) add true arena hazards per `06-biomes.md` hazard escalation.

## Boss-fight minion ambient density cap

Per `waves.json`: ambient hop-slime + bee-buzz spawns during the boss fight cap at ~30-50 concurrent. Boss summons (per `mechanics.md` phase 2) add 4 hop-slimes twice. Peak boss-fight concurrent enemies = ~50-60 (well under the 200 cap and well under the 80 ambient threshold from `00-pacing-model.md` beat 8).

The cap exists to keep boss telegraphs **visually readable**. If ambient density crowds the boss's attack tells, players can't react in time. Balance-engineer to verify that the 50-60 concurrent count plus the boss's footprint (~6 u diameter) plus telegraph decals (~12 u radius for stomp) leaves a usable visual frame.

## Cross-references

- Boss attack patterns + telegraphs: `mechanics.md`.
- Base Meadow arena spec: `../../01-biomes/meadow/layout.md`.
- Boss spawn + arena mods JSON: `../../01-biomes/meadow/waves.json` (`boss.arena_mods`).
- Pacing curve for boss-fight beat: `../../00-pacing-model.md` beat 8.
- Tree-stump + ground-crack asset sources: Kenney Nature Kit (stumps) + custom decal (cracks) per `07-art-bible/04-environment-style.md`.
