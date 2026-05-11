# Mama Oak — Arena

> Owner: level-designer. Per-phase arena staging for the Mama Oak boss fight in the Forest biome. Sister docs: `mechanics.md` (attack patterns + phase gates — Mama Oak is the **only stationary boss** in the roster), `../../01-biomes/forest/layout.md` (base Forest arena spec), `../../01-biomes/forest/waves.json` (boss event at t=420, `arena_mods` block). All frame counts assume 60 fps. All distances in Unity units (u).

## Base arena (inherited from Forest)

The boss fight uses the **standard Forest oval arena** (per `../../01-biomes/forest/layout.md`) — 70 × 90 u oval (E/W × N/S), no shrink. Per `06-biomes.md` and ADR-0013, the arena half-extent used for the spawn-ring invariant is the **smaller dimension** (35 u oval-min → spawn ring radius = 35 - 8 = **27 u**) per ADR-0013. The oval bias means any boss-related off-ring spawns favor the N/S long axis; in this fight only the acorn-rain markers spawn off-ring (positions are arena-wide).

| Param | Value | Notes |
|---|---|---|
| Playable area | 70 × 90 u oval | Inherited; no shrink during boss |
| Outer soft boundary | Dense oak-trunk ring at radius 38 u (E/W) / 48 u (N/S) | Inherited |
| Hard boundary | Invisible ovaloid 40 u E/W, 50 u N/S | Inherited |
| Camera | Top-down 3/4, FOV 35°, distance 18 u | Inherited (P3 pull-back — see Camera notes) |
| Hero props (always present) | Ancient oak (0, +24), log bridge (-16, -4), mushroom ring (+14, -12) | Inherited; mushroom ring's +5% pickup-magnet buff **persists during the fight** per `layout.md` boss-arena delta |

Mama Oak occupies **dead center (0, 0)** with a 3.0 u radius trunk collider — the bunny cannot stand on top of her, must circle. Entrance: 2.0 s "rise-from-the-ground" animation (her trunk grows visibly out of the dirt; canopy-shake VFX caps the rise). Time-dilate to 0.4x for 800 ms during the rise.

## Phase-by-phase staging

### Phase 1 — Politely-rooting (100-66% HP)

**Arena mods**: none beyond standard delta. The 3 hero props + the standing Mama Oak trunk are the only world geometry. Ambient root-snare patches **despawn at t=418** (1.5 s before entrance per `layout.md`) — Mama Oak owns snares this fight, ambient would double-punish.

The arena is **open with a hard center obstacle** (the trunk). The player **circles** around Mama Oak; she swivels her face to track. This is the deliberate variety beat from `mechanics.md`: no kite-in-a-straight-line option, only orbital paths.

### Phase 2 — Firmly-rooting (66-33% HP)

**Arena mods**: 4 **root-tendril zones** appear at cardinal positions (N/E/S/W of boss) as persistent damage zones — the Mama Oak `mechanics.md` calls these "root-snare patches"; here the staging concretizes them as cardinal-fixed positions for phase 2 (per the Mama Oak `arena_mods` block).

| Root-tendril zone | Position (u from center) | Function |
|---|---|---|
| `root-tendril-n` | (0, +6) — north of trunk | Damage zone; on contact, roots player 0.6 s |
| `root-tendril-e` | (+6, 0) — east of trunk | Damage zone; same effect |
| `root-tendril-s` | (0, -6) — south of trunk | Damage zone |
| `root-tendril-w` | (-6, 0) — west of trunk | Damage zone |

Root-tendril zones are **persistent for the duration of phase 2** (no cooldown, no respawn — they pulse and re-arm every 8 s but stay in their cardinal positions). They are **non-LoS-blocking** (acorn-toss still arcs over them) but **enforce a navigation puzzle**: the bunny's circle path must offset radially outward from 6 u to ~9-10 u to skirt them, which puts her closer to the soft boundary on acorn-toss landing arcs.

Mama Oak's stationary state means the **player navigation rules ARE the puzzle**. The boss does nothing to chase; the arena does the chasing through these zones + the trunk collider.

### Phase 3 — Done-being-polite (33-0% HP)

**Arena mods**: **canopy-fall arena hazard** activates — 1 acorn-rain shadow marker spawns every 3 seconds at a random arena position (NOT clamped to cardinals; uses the full 70 × 90 u footprint excluding a 4 u no-spawn margin around the trunk and a 6 u no-spawn margin around the mushroom-ring prop so the bunny's pickup-magnet refuge is preserved).

| Hazard | Spawn rate | Telegraph | Effect |
|---|---|---|---|
| Acorn-rain marker | 1 per 3 s during phase 3 | 0.8 s shadow-circle on ground (orange decal) | 22 hp on impact, 0.8 u AOE |

The acorn-rain markers stack with the **persistent 4 root-tendril zones** carried over from phase 2 (which **expand to 5 zones** per `mechanics.md` — the 5th appears at (0, 0) overlapping the trunk, effectively making the immediate trunk-perimeter a no-stand zone too). The phase-3 arena is at its most punishing: **5 root zones + 1-2 acorn markers + the trunk obstacle + the soft boundary** all narrow the safe pathing window.

The **mushroom-ring prop persists** and provides a **+5% pickup-magnet refuge zone** (per `layout.md`) — designed as a safe haven for catching dropped pickups during the dense phase-3 chaos. Acorn-rain markers will NOT spawn within 6 u of the ring per the no-spawn margin rule.

## Hazard count

| Phase | Active hazards | Hazard sources |
|---|---|---|
| 1 | 0 | Boss attacks only; ambient root-snares suppressed |
| 2 | 4 | 4 cardinal root-tendril zones (persistent damage) |
| 3 | 5 + canopy-fall hazard | 5 root zones + acorn-rain markers every 3 s (arena-wide) |

Forest's boss fight has **the highest persistent hazard count of any biome boss** — the stationary boss compensates by saturating the arena with zone-pressure (player can't outrun zones the way they could outrun the Crab Captain's pincer).

## Boundary handling

Same as Forest base — oak-trunk ring at soft 38 u (E/W) / 48 u (N/S), invisible ovaloid hard collider at 40 u (E/W) / 50 u (N/S). Mama Oak's stationary trunk + 5 root zones + acorn markers push the bunny outward against the boundary in phase 3; the oval shape *helps* here — the long N/S axis gives more retreat room than a 70 × 70 square would. The acorn-toss double in phase 2 has its landing-ring spread capped at 30° per `mechanics.md`, so the bunny can always retreat *along* the oval long axis without being walled.

## Camera notes

- Phases 1-2: standard Forest camera (FOV 35°, 18 u distance, -55° pitch, fixed yaw). The dappled-shadow tiles read clearly at this pitch.
- Phase 3: **slight pull-back when the finale starts** — camera distance lerps from 18 u → 20.5 u over the phase-2-to-3 transition (1.2 s), then holds. The pull-back is needed to fit the acorn-rain markers + 5 root zones + the trunk into frame so the player can read the arena state at-a-glance. FOV unchanged. Snap back to 18 u over 1.0 s on defeat.
- **No camera shake** during Mama Oak attacks per tone bible — she is gentle, even when firm. Acorn-rain impacts use a **6-frame screen-tint flicker (warm amber)** instead of shake, preserving the "polite finale" register.
- Camera **does not center on Mama Oak** — it stays player-anchored. The trunk is large enough (3 u collider, ~5 u visual) to remain framed even when the player orbits the far edge.

## Boss-fight minion ambient density cap

Per `waves.json`: ambient bee + spider spawns during the boss fight cap at ~25-35 concurrent (lower than Beach — Mama Oak's arena pressure is already high, ambient adds light flavor only). Mama Oak has **no summon mechanic** — all hazards are arena-driven. Peak boss-fight concurrent enemies = ~30-40 (well under 200 cap and 80 ambient threshold).

The cap exists to keep the **5 root zones + acorn-rain markers + ambient enemies** all readable simultaneously in phase 3. Balance-engineer to verify the 35-enemy peak + 5 root pulses + 1-2 acorn shadows + the trunk leaves a usable visual frame.

## Triangle / draw-call cost

Per `06-tech-spec/05-performance-budget.md` (250k tri / 80 DC cap on-screen):
- Forest base arena: ~140k tri, ~25 DC steady-state (dappled-shadow decals + oak trunks merged per art-bible 04).
- Boss-fight prop additions: Mama Oak boss mesh (~8k tri, 2 DC for trunk + canopy) + 5 root-tendril zone decals (~1000 tri, 1 DC merged) + acorn-rain shadow markers (pooled, 1 DC, ~200 tri each, peak 2 active = 400 tri) + ~30-40 minions (pooled, ~30k tri peak, ~5 DC instanced).
- Peak phase-3 draw call estimate: ~34 DC, ~180k tri — **28% headroom** under the 80 DC / 250k tri budget. Safe but the **dappled-shadow baked decals + 5 zone pulses make this the densest decal frame in the launch roster** — keep an eye on overdraw if any later mechanic adds more.

## Cross-references

- Boss attack patterns + telegraphs: `mechanics.md`.
- Base Forest arena spec: `../../01-biomes/forest/layout.md`.
- Boss spawn + `arena_mods` JSON: `../../01-biomes/forest/waves.json` (`boss.arena_mods`).
- Pacing curve for boss-fight beat: `../../00-pacing-model.md` beat 8.
- Spawn-radius invariant: `../../../decisions/0013-arena-spawn-radius-invariant.md`.
- Root-tendril + acorn-rain asset sources: Kenney Nature Kit (root vines) + custom decal (acorn shadow) per `07-art-bible/04-environment-style.md`.
