# Crab Captain — Arena

> Owner: level-designer. Per-phase arena staging for the Crab Captain boss fight in the Beach biome. Sister docs: `mechanics.md` (attack patterns + phase gates), `../../01-biomes/beach/layout.md` (base Beach arena spec), `../../01-biomes/beach/waves.json` (boss event at t=420, `arena_mods` block). All frame counts assume 60 fps. All distances in Unity units (u).

## Base arena (inherited from Beach)

The boss fight uses the **standard Beach arena** (per `../../01-biomes/beach/layout.md`) — 80 × 80 u, no shrink. Per `06-biomes.md` and ADR-0013, biome boss arenas layer props *into* the base footprint rather than swapping to a custom arena. Spawn ring radius for any boss-fight summons follows the **ADR-0013 invariant** (`arena_half_extent - 8` = 40 - 8 = **32 u**); sand-puff swarm summons spawn at boss-feet anchor, not the spawn ring.

| Param | Value | Notes |
|---|---|---|
| Playable area | 80 × 80 u | Inherited; no shrink during boss |
| Outer soft boundary | Dune ring N/E/W + wave-lap S at radius 42 u | Inherited |
| Hard boundary | Invisible collider at radius 45 u | Inherited |
| Camera | Top-down 3/4, FOV 35°, distance 18 u | Inherited (P3 zoom-out — see Camera notes) |
| Hero props (always present) | Palm tree (+20, +10), thatched hut (-18, -12), coconut pile (+14, -4) | Inherited; coconut pile **guaranteed broken** at t=418 per layout |

Boss spawns at arena center (player anchor at fight start) with a 1.5 s entrance + bottle-cap settle animation. Player is briefly pushed 8 u south during the entrance to clear the LoS for the entrance VFX. Time-dilate to 0.4x for 800 ms during the entrance.

## Phase-by-phase staging

### Phase 1 — Patrolling (100-66% HP)

**Arena mods**: none beyond the standard Beach delta. The 3 hero props + the broken coconut pile are the only world geometry. Boss side-scuttles laterally across the arena; player has full 80 × 80 u to work with. Pre-existing biome sand-trap patches remain **suppressed during phase 1** (they relocate at the phase-2 gate) so the player can read the captain's lateral pacing without surface friction.

### Phase 2 — Summoning (66-33% HP)

**Arena mods**: the 2 existing Beach sand-trap patches **relocate** from their ambient positions to fixed fight-relevant positions, becoming always-active (no telegraph; the patches just sit there per the boss-arena delta in `layout.md`).

| Sand-trap | Position (u from center) | Function |
|---|---|---|
| `sand-trap-a` | (+8, +5) — east-northeast | Cover + obstacle; sits between SP_NE corner and boss center. Player can pull captain's strafe across the patch to slow his approach. |
| `sand-trap-b` | (-8, -6) — west-southwest | Companion patch on opposite axis; symmetric break of captain's side-scuttle line. |

Sand-traps are **movement-penalty obstacles** (movespeed × 0.65 inside per `layout.md` Hazards) but **non-LoS-blocking** — the captain still aims through them. They reward the bunny for kiting the captain through the patches: he is **immune to his own sand** per the boss-arena delta, but his targeting AI has a 0.4 s aim-update delay when the player crosses a patch (a small **clever-play opening** the level rewards).

Sand-puff summon adds (per `mechanics.md` phase 2) spawn at the **boss-feet anchor**, not at the SP_* ring — they share the captain's footprint so the player's positional read is "the summon is *under* him." Summon-marker decal pulses orange during the wind-up.

### Phase 3 — Cornered (33-0% HP)

**Arena mods**: 4 **tide pools** appear at fixed positions, telegraphing the scuttle-charge / pincer-slam damage zones. These are **damage zones** during the captain's slam + dash telegraphs, not standalone hazards between attacks.

| Tide pool | Position (u from center) | Function |
|---|---|---|
| `tide-pool-n` | (0, +14) — north | Pre-telegraph zone for pincer-slam north impact |
| `tide-pool-e` | (+14, 0) — east | Pre-telegraph zone for east-arc forward dash |
| `tide-pool-s` | (0, -14) — south | Pre-telegraph zone for south slam |
| `tide-pool-w` | (-14, 0) — west | Pre-telegraph zone for west-arc forward dash |

Tide pools are **cosmetic shallow-water decals between captain attacks** (faint cyan ripple, low contrast — they read as "wet sand," not "lava"). They **pulse red** for 0.6 s before each pincer-slam / forward-dash, marking the impact arc. The captain randomizes which two pools fire per attack (always opposite or adjacent pair); the **non-pulsing pools are the safe stand-zones**.

The 2 sand-traps from phase 2 persist. The arena is at its most cluttered in phase 3: 2 sand-traps (movement penalty) + 4 tide pools (telegraphed damage zones) + 3 hero props + broken coconut pile = 10 elements. Every element is either a defense affordance (sand-trap kite), a telegraph (pool), or a landmark (props) — no decorative-only clutter.

## Hazard count

| Phase | Active hazards | Hazard sources |
|---|---|---|
| 1 | 0 | Boss attacks only; ambient sand-traps suppressed |
| 2 | 2 | 2 always-active sand-traps (movement penalty, not damage) |
| 3 | 2 + 4 telegraphed pools | Sand-traps persist; tide pools fire as boss-attack telegraphs (not standalone) |

Beach is the **first biome to add a true arena modifier** to the boss fight (movement penalty), per `06-biomes.md` hazard escalation. Tide pools count as boss-driven damage zones, not standalone hazards — they only fire on telegraph.

## Boundary handling

Same as Beach base — dune ring (N/E/W) + wave-lap (S) at soft 42 u, invisible hard collider at 45 u. Captain's **forward dash** in phase 3 can run him into the soft boundary; per `mechanics.md`, hitting the boundary adds +1.0 s self-stagger (a **designer-rewarded clever-play opening** — the bunny baits the dash toward the dune ring). The wave-lap south edge is *not* a kill-plane for the bunny, and the captain will not dash into the ocean (his behavior tree forbids the southern arc for dash).

## Camera notes

- Phases 1-2: standard Beach camera (FOV 35°, 18 u distance, -55° pitch, fixed yaw). No camera tweak.
- Phase 3: **slight zoom-out** — camera distance lerps from 18 u → 21 u over the phase-2-to-3 transition (1.0 s), then holds. The wider frame is needed to fit all 4 tide pools + the captain's expanded slam/dash AOEs into the player's visual budget. FOV unchanged. Snap back to 18 u over 0.8 s on defeat.
- Camera **shake** on each pincer-slam impact: 6-frame shake at 0.3 amplitude (matches Old Boar King stomp shake intensity per `../old-boar-king/arena.md` convention).

## Boss-fight minion ambient density cap

Per `waves.json`: ambient crab + gull spawns during the boss fight cap at ~30-40 concurrent. Boss summons (per `mechanics.md` phase 2) add 3 sand-puffs per summon, up to 2 summons in phase 2 + 1 per ~12 s in phase 3 — peak boss-fight concurrent enemies = ~50-60 (well under 200 cap, well under 80 ambient threshold).

The cap exists to keep captain telegraphs **visually readable** through the tide pools, sand-traps, and minions. Balance-engineer to verify that the 60-enemy peak + 4 tide-pool pulses + 2 sand-trap glints + captain's ~5 u footprint leaves a usable visual frame.

## Triangle / draw-call cost

Per `06-tech-spec/05-performance-budget.md` (250k tri / 80 DC cap on-screen):
- Beach base arena: ~120k tri, ~22 DC steady-state (chunks merged per art-bible 04).
- Boss-fight prop additions: 2 sand-trap decals (~400 tri, 1 DC merged) + 4 tide-pool decals (~600 tri, 1 DC merged) + boss model + ~30-60 minions (pooled, ~40k tri peak, ~6 DC instanced).
- Peak phase-3 draw call estimate: ~32 DC, ~165k tri — **40% headroom** under the 80 DC / 250k tri budget. Safe.

## Cross-references

- Boss attack patterns + telegraphs: `mechanics.md`.
- Base Beach arena spec: `../../01-biomes/beach/layout.md`.
- Boss spawn + `arena_mods` JSON: `../../01-biomes/beach/waves.json` (`boss.arena_mods`).
- Pacing curve for boss-fight beat: `../../00-pacing-model.md` beat 8.
- Spawn-radius invariant: `../../../decisions/0013-arena-spawn-radius-invariant.md`.
- Sand-trap + tide-pool asset sources: Kenney Nature Kit (sand) + custom decal (tide pool) per `07-art-bible/04-environment-style.md`.
