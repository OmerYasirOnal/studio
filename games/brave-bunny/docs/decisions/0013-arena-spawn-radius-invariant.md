# ADR 0013 — Arena spawn-radius invariant

**Date:** 2026-05-12
**Status:** accepted
**Owner:** orchestrator (synthesizing level-designer wave-5 observation)

## Context

The 5 biomes have different arena footprints (from `docs/09-level-design/01-biomes/*/layout.md`):

| Biome | Footprint | Half-extent |
|---|---|---|
| Meadow | 80×80 units | 40 |
| Beach | 80×80 units | 40 |
| Forest | 70×90 (oval) | min 35 / max 45 |
| Cavern | 60×60 units | 30 |
| Snow | 90×90 units | 45 |

This variance affects:

- **Spawner ring radius** — enemies spawn at the edge of the playable area
- **Camera-edge culling** — what's "off-screen" depends on arena half-extent
- **Boss arena dimensions** — bosses with charge attacks need known room size
- **Pickup magnet range** — should never reach beyond visible arena

Without an invariant, every biome's spawner math drifts independently and gameplay-engineer has to handle each biome separately. Worse: balance-engineer's TTK calculations assume a consistent enemy-to-player distance window.

## Decision

**Spawn-radius invariant:** every biome's enemy spawn ring is exactly **`arena_half_extent - 8 units`**.

Implementation:

- `BiomeDefinition` ScriptableObject gains `arenaHalfExtent: float` (the smaller dimension on oval arenas)
- `WaveSpawner` reads `arenaHalfExtent - 8f` as the default spawn radius
- Individual `WaveSpawnEntry` may override per-spawn with `radius: <override>` (already in `waves.json` schema), but the default falls back to the invariant

Calculated values:

| Biome | Half-extent | Spawn radius |
|---|---|---|
| Meadow | 40 | **32** |
| Beach | 40 | **32** |
| Forest | 35 (oval min) | **27** |
| Cavern | 30 | **22** |
| Snow | 45 | **37** |

`waves.json` files written in wave 5 use these values implicitly. Existing Meadow `waves.json` already uses radius 30-40 in spawn entries — verify those fall within [`arenaHalfExtent - 12`, `arenaHalfExtent - 4`] for visual variety while staying off-screen-safe at scroll.

## Consequences

- **gameplay-engineer**: `WaveSpawner.GetDefaultSpawnRadius(BiomeDefinition b) => b.arenaHalfExtent - 8f`
- **level-designer**: future `waves.json` files can omit explicit `radius` field for default-ring spawns
- **balance-engineer**: TTK calculations use the per-biome spawn radius for enemy reach-player time
- **camera system**: Cinemachine virtual camera dampening must be tuned per arena size; flagged for tech-architect
- **pickup magnet**: max default magnet radius is bounded by `arenaHalfExtent - 12` (always reach inside the visible arena)

## Alternatives considered

- **Per-biome free-form** — rejected. Forces every consumer (WaveSpawner, camera, magnet) to query biome-specific params; explosion of edge cases.
- **Force all biomes to 80×80** — rejected. Cavern's claustrophobia is a deliberate pacing tool; Snow's openness rewards Owl's magnet range as a build-driver.
- **Variable spawn radius per wave entry only** — partially adopted. The invariant is the *default*; per-entry overrides remain available for special patterns (e.g., flank attacks from a longer distance).

## References

- `docs/09-level-design/01-biomes/*/layout.md` (per-biome footprints)
- `docs/09-level-design/01-biomes/*/waves.json` (spawn entries)
- `docs/06-tech-spec/02-data-model.md` (`BiomeDefinition` SO)
- `docs/06-tech-spec/05-performance-budget.md` (200-enemy ceiling)
- `docs/handoffs/level-designer-wave-5*.md` (observation source)
