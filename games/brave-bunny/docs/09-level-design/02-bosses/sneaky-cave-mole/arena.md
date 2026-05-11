# Sneaky Cave Mole — Arena

> Owner: level-designer. Per-phase arena staging for the Sneaky Cave Mole boss fight in the Cavern biome. Sister docs: `mechanics.md` (attack patterns + 2-phase structure — mole is the **only 2-phase boss**), `../../01-biomes/cavern/layout.md` (base Cavern arena spec), `../../01-biomes/cavern/waves.json` (boss event at t=420, `arena_mods` block). All frame counts assume 60 fps. All distances in Unity units (u).

## Base arena (inherited from Cavern)

The boss fight uses the **standard Cavern arena** (per `../../01-biomes/cavern/layout.md`) — 60 × 60 u, **smallest arena in the game** (claustrophobic by design), no shrink. Per ADR-0013, spawn ring radius = 30 (half-extent) - 8 = **22 u**. The mole's teleport mechanic is **not** ring-bound; he uses the fixed dig-mound positions documented below.

| Param | Value | Notes |
|---|---|---|
| Playable area | 60 × 60 u | Inherited; no shrink during boss |
| Player reveal radius | 6.5 u (8.0 u within 4 u of a torch) | Inherited — defining biome modifier |
| Outer soft boundary | Stone-wall ring at radius 28 u | Inherited |
| Hard boundary | Invisible collider at radius 30 u | Inherited (tight; player feels the wall) |
| Camera | Top-down 3/4, FOV 35°, distance 18 u, vignette 0.6 | Inherited; vignette + reveal radius = "flashlight" feel |
| Hero props (always present) | Stalactite cluster (0, +14), glow-mushroom patch (-12, -6), gem outcrop (+10, -8) | Inherited |
| Torch props (always present) | 4 torches at NE/SE/SW/NW ~14 u offset | Inherited; **brighter during fight** — intensity 1.2 → 1.6 (per `layout.md` boss-arena delta) — the mole carries a faint glow |

The mole spawns at (0, 0) for the entrance animation, then **immediately burrows** to begin his pop-up rotation. Entrance: 1.2 s "rise + glasses-polish + 'eep!' + dive" sequence. Time-dilate to 0.4x for 800 ms during the entrance. Ambient stalactite hazards **remain active** per `layout.md` — the only fight where ambient hazards stack with boss hazards in Cavern.

## Phase-by-phase staging

### Phase 1 — Twitchy-digger (100-50% HP)

**Arena mods**: none beyond the standard Cavern boss delta. **4 fixed dig-mound marker positions** at (±8, ±8) are the only mole-teleport-valid spawn points. The mole surfaces from one of these per dig-strike pattern (per `mechanics.md`). Mound markers are **always-visible** (small dirt-rise decals at each cardinal-diagonal) so the player can pre-position even before the next telegraph fires.

| Dig-mound | Position (u from center) | Valid teleport target? |
|---|---|---|
| `mound-ne` | (+8, +8) | Yes |
| `mound-se` | (+8, -8) | Yes |
| `mound-sw` | (-8, -8) | Yes |
| `mound-nw` | (-8, +8) | Yes |

The 4 positions are inside the **8 u spawn-ring forbidden zone** (per ADR-0013, the 22 u spawn ring is for ambient enemies; boss teleport uses its own targeting). Mole **never teleports** to:
- (0, 0) — the bunny's anchor (no melt-on-arrival cheap shots)
- The hero-prop footprints (stalactite cluster, glow-mushroom, gem outcrop) — visual readability
- Within 2 u of the bunny's current position — teleport-anti-grief rule (mole pops up *near* the bunny, never *on top of*)
- **Inside the ADR-0013 spawn ring at radius 22 u or beyond** — guarantees the mole always surfaces *inside* the bunny's visible region, never beyond the reveal-radius horizon (which would be unfair given 6.5 u reveal)

### Phase 2 — Sneaky-trickster (50-0% HP)

**Arena mods**: **3 dig-mound decoys appear** alongside the 4 fixed mounds — creating the **visual ambiguity** that defines phase 2. When the mole's `triple-mound` attack fires (per `mechanics.md`), 3 of the 7 total mounds rise simultaneously with identical visuals; **only one is the real mole**, the other two are decoys.

| Decoy mound | Spawn pool (u from center) | Function |
|---|---|---|
| `decoy-n` | (0, +9) — cardinal north | Decoy + visual filler; never the real mole during single-strike attacks |
| `decoy-e` | (+9, 0) — cardinal east | Decoy + visual filler |
| `decoy-s` | (0, -9) — cardinal south | Decoy + visual filler |

The 3 decoys + 4 fixed mounds = **7 mound positions** total in phase 2 (the original 4 stay valid teleport targets for single dig-strikes; the 3 decoys are **only** activated during triple-mound attacks). Decoys are **identical-looking** to real mounds, BUT per `mechanics.md` the real mole-mound has a faint **dirt-puff VFX ~6 frames before the others** — the learnable "git gud" tell.

Boss teleport rules in phase 2:
- Single dig-strike → mole picks one of the 4 fixed mounds (not decoys).
- Triple-mound attack → mole picks 3 of the 7 positions; exactly 1 of the 3 is the real mole; remaining 2 are decoys.
- The mole **never teleports inside the spawn ring at radius 22 u or beyond** (same as phase 1).
- Stalactite-shake attack: mole stays underground; no teleport.

### Reveal radius interaction

The Cavern's **6.5 u reveal radius** is the boss fight's defining tension multiplier. The 4 fixed mound positions at (±8, ±8) sit **just outside** the bunny's reveal radius when she's at (0, 0) — the player sees only the closest 1-2 mounds at any one time unless she repositions. **Torches** at the 4 corners extend reveal locally to 8.0 u within 4.0 u of each torch — the player learns to **path between torches** to keep all 4 mounds visible during the triple-mound mind-game.

The **3 phase-2 decoys at the cardinals (0, ±9) / (±9, 0)** sit at the same radius — also at the edge of reveal. This makes the mound-readability problem worse in phase 2 (more mounds, same reveal radius) without nerfing the player's tools (torches still work).

The interaction with `layout.md`'s "torches extend reveal" rule is what makes phase 2 a positional puzzle: **stand near a torch to see more mounds, but accept that the mole knows where you are**. This is the central tension of the fight.

## Hazard count

| Phase | Active hazards | Hazard sources |
|---|---|---|
| 1 | 1-2 | Ambient stalactite drops (Cavern base hazard) |
| 2 | 1-2 + 3 stalactite-shake zones during boss attack | Ambient + boss-added stalactite zones (peak 5 active per `layout.md`) |

The mole is the **only boss that doesn't add a persistent arena hazard** — his "arena mods" are perceptual (decoy mounds + teleport-position rules), not damage zones. This is by design — the mole's complexity budget is spent on the *read*, not on stacking damage AOEs.

## Boundary handling

Same as Cavern base — stone-wall ring at soft 28 u, invisible hard collider at 30 u, enemy despawn at 35 u. The cramped feel is part of the fight. Mole does NOT have a charge attack; the soft boundary plays no role in offense, only in limiting retreat distance. The 4 fixed mound positions at (±8, ±8) are **well inside** the boundary — the player can always retreat *past* a mound toward the wall and the mole cannot follow (he can only surface from the fixed 4 / decoy 3 positions).

## Camera notes

- Phases 1-2: standard Cavern camera (FOV 35°, 18 u distance, -55° pitch, fixed yaw, **vignette 0.6**). The vignette is inherited from the base biome and **stays on through the fight** — the flashlight feel IS the fight.
- **No camera distance change between phases** — the cramped arena makes pull-back unnecessary; the player's positional read is dominated by the 6.5 u reveal radius, not the camera frame.
- Camera **shake** is reserved for stalactite impacts: 4-frame shake at 0.2 amplitude per impact (subtle — the mole's tone is "shy and twitchy," not "stomping rage"). The mole's own dig-strike does NOT shake the camera (he is small).
- Reveal-radius VFX (the soft edge fade-to-dark at 6.5 u) **renders on top of camera vignette** — the two layers compound visually, making the dark edges feel even darker during the fight.

## Boss-fight minion ambient density cap

Per `waves.json`: ambient bat + cave-spider spawns during the boss fight cap at ~20-30 concurrent — **lowest cap in the launch roster** because the cramped 60 × 60 arena + 6.5 u reveal + decoy-mound cognitive load means too many minions would crowd the read. The mole has **no summon mechanic**. Peak boss-fight concurrent enemies = ~25-35 (well under 200 cap and 80 ambient threshold).

The cap exists to keep the **triple-mound visual ambiguity** readable. Balance-engineer to verify that ~30 ambient enemies + up to 7 mounds (4 fixed + 3 decoys) + up to 5 stalactite zones leaves enough visual headroom to spot the real-mole pre-strike puff VFX.

## Triangle / draw-call cost

Per `06-tech-spec/05-performance-budget.md` (250k tri / 80 DC cap on-screen):
- Cavern base arena: ~100k tri, ~18 DC steady-state (dark palette + simple chunk merge keeps cost low — Cavern is the cheapest biome).
- Boss-fight prop additions: mole boss mesh (~3k tri, 1 DC — small) + 4 fixed dig-mound decals + 3 decoy decals (~700 tri total, 1 DC merged) + dynamic stalactite drop decals (pooled, 1 DC) + ~25-35 minions (pooled, ~25k tri peak, ~5 DC instanced) + 4 torch point-lights (no tri cost; lighting DCs).
- Peak phase-2 draw call estimate: ~26 DC, ~130k tri — **48% headroom** under the 80 DC / 250k tri budget. Easily the **lightest boss fight** in the launch roster.

## Cross-references

- Boss attack patterns + telegraphs: `mechanics.md`.
- Base Cavern arena spec: `../../01-biomes/cavern/layout.md`.
- Boss spawn + `arena_mods` JSON: `../../01-biomes/cavern/waves.json` (`boss.arena_mods`).
- Pacing curve for boss-fight beat: `../../00-pacing-model.md` beat 8.
- Spawn-radius invariant: `../../../decisions/0013-arena-spawn-radius-invariant.md`.
- Dig-mound + torch asset sources: Kenney Nature Kit (rocks/mound) + Quaternius (torch) per `07-art-bible/04-environment-style.md`.
