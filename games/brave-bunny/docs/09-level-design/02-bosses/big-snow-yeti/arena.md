# Big Snow-yeti — Arena

> Owner: level-designer. Per-phase arena staging for the Big Snow-yeti boss fight in the Snow biome (launch endgame). Sister docs: `mechanics.md` (attack patterns + phase gates — the **highest-HP boss** with passive cold-aura), `../../01-biomes/snow/layout.md` (base Snow arena spec), `../../01-biomes/snow/waves.json` (boss event at t=420, `arena_mods` block). All frame counts assume 60 fps. All distances in Unity units (u).

## Base arena (inherited from Snow)

The boss fight uses the **standard Snow arena** (per `../../01-biomes/snow/layout.md`) — 90 × 90 u, **largest arena in the game**, no shrink. Per ADR-0013, spawn ring radius = 45 (half-extent) - 8 = **37 u**. The yeti himself spawns at center (0, 0); his frost-charge dash is **not** ring-bound but uses his current facing.

| Param | Value | Notes |
|---|---|---|
| Playable area | 90 × 90 u | Inherited; no shrink during boss |
| Player reveal radius | 9.0 u | Inherited — long sight lines |
| Outer soft boundary | Pine-tree ring at radius 46 u | Inherited |
| Hard boundary | Invisible collider at radius 50 u | Inherited (widest in the game) |
| Camera | Top-down 3/4, FOV 35°, distance 18 u | Inherited (P3 pull-back + fog reduction — see Camera notes) |
| Hero props (always present) | Pine tree (+18, +16), ice formation (-18, +12), igloo (-14, -16) | Inherited; **igloo is critical** for phase 3 survival |
| Permanent decor | Frozen pond patch at (+12, -10) | Inherited; permanent 0.4 s drift |

Yeti spawns at center (0, 0) with a 2.0 s entrance — slow shiver + snow-puff burst + low rumble + first cold-aura ring expansion. Time-dilate to 0.4x for 1.0 s during the entrance (slightly longer than other bosses — endgame moment). **All ambient hazards remain active** per `layout.md`: ice-slides + cold-tick + snowdrift. The yeti's passive cold-aura adds to this — **only fight where all 3 ambient hazard classes stack with boss hazards**.

## Phase-by-phase staging

### Phase 1 — Plodding-shiver (100-66% HP)

**Arena mods**: snowdrift hazards freeze in **fixed positions** (no respawn) per `layout.md` boss-arena delta: (+10, +6) and (-8, +10), always-active. Ice-slide patches **persist in their ambient positions** (3-5 visible). The yeti's passive cold-aura (4.0 u radius around him, 1.5 hp/sec DOT per `mechanics.md`) is the only new threat layer in phase 1. Player has the full 90 × 90 u to work with — kite the yeti away from the igloo to keep it as a phase-3 reserve.

### Phase 2 — Frost-charging (66-33% HP)

**Arena mods**: yeti's **ice-stomp shockwave zones** activate — 1 telegraph circle follows the player for 0.6 s before resolving. Per `mechanics.md` phase 2, ice-stomp shockwave upgrades from 3.0 → 4.0 u radius, and the **trailing ice-patch mechanic** adds 1-2 fresh persistent ice patches per stomp along the wave path. Frost-charge adds 3 more ice patches along its 5 u dash path.

| Hazard | Trigger | Telegraph | Effect |
|---|---|---|---|
| Ice-stomp shockwave zone | Per ice-stomp attack | Pulsing orange circle that **tracks the player's position for 0.6 s** then locks (preview becomes the actual impact area) | 40 hp inside expanding wave; persistent ice patch enforces 0.4 s drift for 6 s |
| Frost-charge trailing patches | Per frost-charge attack | Dashed red line previewing the 5 u dash + 3 small expanding rings on the path | 50 contact during dash; ice patches persist 6 s |

The **0.6 s player-tracking telegraph** is the phase-2 set-piece read: the circle slides under the player's feet, telling them "move now or eat it." This is a deliberate test of movement-prediction (most attacks lock at telegraph onset; ice-stomp's tracking adds a *commitment-prediction* layer). Player learns to **commit to a direction early**, not feather-step.

Cold-aura expands from 4.0 → 5.0 u, DOT from 1.5 → 2.0 hp/sec. The yeti's footprint plus aura plus a tracked shockwave zone occupies ~10-12 u of arena center.

### Phase 3 — Blizzard-roused (33-0% HP)

**Arena mods**: **blizzard-howl arena-wide cold-tick** activates as the set-piece — for 4.0 s, the entire arena's cold-tick rate **doubles to 4.0 hp/sec, suppressed only inside the igloo prop's 8 u suppression radius** (per `layout.md` igloo gives 4 s cold-tick suppression on exit, and per `mechanics.md` the blizzard-howl is suppressed by the igloo prop). During howl, the **igloo becomes mandatory cover**.

Plus: ice-stomp upgrades to **ice-stomp-quake** (6.0 u shockwave + 4 fixed-position ice patches scatter per stomp). Snowball-volley upgrades to 5-shot.

| Mechanic | Effect | Player response |
|---|---|---|
| Blizzard-howl (every ~20 s) | 4 s arena-wide 4.0 hp/sec cold-tick | Retreat to within 8 u of igloo; full 1.0 s DPS window after howl ends |
| Ice-stomp-quake | 6.0 u shockwave + 4 secondary ice patches at preset positions | 0.7 s recovery = biggest DPS window of fight |
| Snowball-volley 5-shot | 5 snowballs in fan, 0.5 s apart | Reposition during volley; igloo blocks LoS for some snowballs |

### Igloo as positional puzzle element

The igloo prop is the **only sheltering prop in the launch roster** (per `06-biomes.md` and `layout.md`). Its 8 u suppression radius is the **only** safe zone during blizzard-howl. The puzzle has three nested layers:

1. **Geographic**: igloo is fixed at (-14, -16) — the SW corner. The yeti spawns at center. Retreating to the igloo means moving ~21 u in a straight line — at 6 u/s base movespeed, ~3.5 s travel time. Blizzard-howl telegraph is 0.8 s + 4 s sustain = 4.8 s window. Player must **already be moving toward the igloo on telegraph onset**, not after the howl starts.
2. **Yeti positioning**: the yeti will sometimes block the igloo's LoS with his ~5 u body footprint. Player must path *around* him, which costs ~1 s. **Kiting the yeti to the NE corner BEFORE phase-3 entry is a meta-skill** the level rewards.
3. **Ice-patch interference**: phase-3 ice patches from ice-stomp-quake can land between the player and the igloo. The player has to **choose paths around ice patches without stepping in cold-tick during howl** — the cold-tick rate (4.0 hp/sec) is high enough that 2-3 extra seconds of pathing = real HP loss.

Per `mechanics.md`, the intro line specifically primes the player on this: *"Big Snow-yeti's grumpy. Keep your paws warm."* — the level designer's job is to make that line **mechanically true**.

The pine tree at (+18, +16) also gives 2 s cold-tick suppression on exit (per `layout.md`) — a **secondary** option for players who can't path to the igloo in time, but the 2 s vs 4 s suppression difference is meaningful in phase 3.

## Hazard count

| Phase | Active hazards | Hazard sources |
|---|---|---|
| 1 | 4-7 | 2 fixed snowdrifts + 2-5 ambient ice-slides + cold-aura + ambient cold-tick |
| 2 | 4-7 + tracking shockwave + trailing ice patches | Phase 1 stack + 1-2 ice patches per stomp + 3 patches per frost-charge |
| 3 | 4-7 + ice-stomp-quake patches + blizzard-howl amplification | Phase 2 stack + 4 patches per quake + arena-wide cold-tick spike during howl |

Snow boss fight has the **largest absolute hazard count in the game**, but the **largest arena** absorbs it. Per `06-biomes.md`, Snow is the **only fight where all ambient hazard classes stack with boss hazards** — by design, this is the graduation arena.

## Boundary handling

Same as Snow base — pine ring at soft 46 u, invisible hard collider at 50 u, enemy despawn at 55 u. The yeti's **frost-charge dash (5 u over 1.0 s)** can run him into the soft boundary, but unlike Crab Captain's dash there is **no self-stagger reward** — the yeti is too heavy to bounce. Instead, dashing into the boundary causes him to **stop with a 0.6 s slide-stop recovery** per `mechanics.md` (already documented; this remains the only punish window for frost-charge regardless of boundary).

Importantly: the **igloo is positioned just 4 u inside the soft boundary** (-14, -16 with boundary at radius 46) — the player retreating to the igloo will visually feel "cornered" against the pine-ring edge. This is intentional staging — the safe zone is at the arena edge, the danger zone is at the center where the yeti stands.

## Camera notes

- Phases 1-2: standard Snow camera (FOV 35°, 18 u distance, -55° pitch, fixed yaw). The 9.0 u reveal radius + open sight lines work in-engine.
- Phase 3: **slight pull-back** — camera distance lerps from 18 u → 21 u over the phase-2-to-3 transition (1.5 s, longer than other bosses to telegraph the endgame moment). FOV unchanged. **Snap back to 18 u over 1.5 s on defeat** (matches the longer 1.5 s slow-mo on yeti defeat per `mechanics.md`).
- **Fog distance reduces to 30 u during blizzard-howl** (from the Snow biome default of ~60 u — overcast sky uses a soft fog falloff). The reduced fog distance creates **visible whiteout** during the 4 s howl — the player can literally see less of the arena, increasing reliance on the igloo's silhouette as the navigational anchor. Fog returns to 60 u over 1.5 s after howl ends.
- Camera **shake** on ice-stomp-quake impacts: 8-frame shake at 0.4 amplitude (largest shake in the launch roster — the yeti is the biggest boss). Frost-charge contact uses a 6-frame shake at 0.3. Blizzard-howl uses a **continuous low-amplitude rumble** (~0.1) for the 4 s sustain.
- The **cold-aura pale-cyan ring** (per `mechanics.md` telegraph color cues) renders as a persistent ground decal around the yeti — always visible at the camera pitch, no scaling change with phase.

## Boss-fight minion ambient density cap

Per `waves.json`: ambient snow-wolf + ice-mite spawns during the boss fight cap at ~35-45 concurrent — **highest cap in the launch roster** because the 90 × 90 u arena is large enough to spread minions without crowding the yeti's telegraphs. The yeti has **no summon mechanic** (he is alone in his sad mountain). Peak boss-fight concurrent enemies = ~50-65 (well under 200 cap and 80 ambient threshold).

The cap exists to keep the **tracking shockwave + ice-stomp-quake + 5-shot snowball volley** all readable against the snowy backdrop (white-on-white is a real risk). Balance-engineer to verify that ~60 ambient enemies + ice patches + cold-aura ring + 5 snowballs + 4 quake patches leaves enough visual headroom against the snow tile palette.

## Triangle / draw-call cost

Per `06-tech-spec/05-performance-budget.md` (250k tri / 80 DC cap on-screen):
- Snow base arena: ~165k tri, ~28 DC steady-state (large arena, falling-snow particle layer, hero-footprint dynamic decals, ice-shimmer decals — Snow is **already the heaviest decal biome** at 17-18 decals on-screen).
- Boss-fight prop additions: yeti boss mesh (~12k tri, 2 DC for body + fur shell — large model) + 4-8 persistent ice patches from ice-stomp-quake (~1200 tri, 1 DC merged) + tracking shockwave decal (1 DC, ~300 tri) + 5-snowball projectile pool (~1500 tri, 1 DC) + ~50-65 minions (pooled, ~50k tri peak, ~8 DC instanced) + cold-aura ground ring decal (~400 tri, 1 DC).
- Phase-3 blizzard-howl adds: fog volume change (no tri cost; shader uniform update), whiteout-tint post effect (1 DC for fullscreen quad).
- Peak phase-3 draw call estimate: ~45 DC, ~232k tri — **only 7% headroom** under the 80 DC / 250k tri budget. **This is the closest the launch roster comes to the perf cap.** Build-engineer should profile this fight on iPhone 12 first; if frame drops appear, the first lever is reducing ambient minion cap from 45 → 35 (recovers ~10k tri, ~2 DC).

## Cross-references

- Boss attack patterns + telegraphs: `mechanics.md`.
- Base Snow arena spec: `../../01-biomes/snow/layout.md`.
- Boss spawn + `arena_mods` JSON: `../../01-biomes/snow/waves.json` (`boss.arena_mods`).
- Pacing curve for boss-fight beat: `../../00-pacing-model.md` beat 8.
- Spawn-radius invariant: `../../../decisions/0013-arena-spawn-radius-invariant.md`.
- Igloo + pine + ice-formation asset sources: custom Blender (igloo, ice-formation) + Kenney Nature Kit (pine) per `07-art-bible/04-environment-style.md`.
- Performance budget: `../../../06-tech-spec/05-performance-budget.md`.
