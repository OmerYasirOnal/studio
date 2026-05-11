# waves.json — Field Schema

> Owner: level-designer. Documents the fields used in `waves.json` for the Meadow biome (and the per-biome wave-schedule pattern in general). Consumers: gameplay-engineer (wave-spawn runtime), balance-engineer (cross-check), qa-engineer (acceptance tests). Frame counts assume 60 fps.

## Top-level fields

| Field | Type | Required | Notes |
|---|---|---|---|
| `biome` | string | yes | Biome slug, lowercase-kebab (e.g. `meadow`, `beach`). Cross-ref: `02-gdd/06-biomes.md`. |
| `schema_version` | string | yes | Semver-ish for the wave file format. Bump when fields are added/renamed. Current: `1.0`. |
| `duration_seconds` | number | yes | Total intended run length in seconds before forced cleanup. Meadow ships at 480 (8 min canonical per pacing model). |
| `spawner_count` | number | yes | Number of spawner positions in the arena. Meadow = 8 (4 cardinals + 4 corners). Cross-ref: `layout.md`. |
| `max_concurrent_enemies` | number | yes | Hard perf cap. Per `brave-bunny/CLAUDE.md`: 200. Wave runtime must throttle or skip spawns that would breach this. |
| `assumed_avg_enemy_lifetime_seconds` | number | yes | The kill-rate sanity-check constant. Used for the perf cross-check. Meadow = 6 s (peak-build TTK estimate). |
| `notes` | string | optional | Human-readable design notes. |
| `spawns` | array of spawn objects | yes | The chronological spawn schedule. See "Spawn object fields" below. |
| `elite_windows` | array of elite-window objects | optional | Time anchors for scripted elite spawns. Redundancy with `spawns` for tooling/UI ergonomics. |
| `boss` | boss object | yes | The boss event. Single boss per biome (Meadow ships with Old Boar King). |
| `perf_crosscheck` | object | optional | Documents the level-designer's mental math against the perf cap. Reviewed by balance-engineer. |

## Spawn object fields

Every entry in the `spawns` array represents a **single scripted spawn event** at time `t`.

| Field | Type | Required | Notes |
|---|---|---|---|
| `t` | number | yes | Time anchor in seconds from run start (0 = joystick first touch). |
| `beat` | string | optional | Name of the pacing beat this spawn belongs to (e.g. `first-swarm`, `pre-boss-taper`). Cross-ref: `00-pacing-model.md`. For tooling/debug, not runtime-critical. |
| `enemy` | string | one of: `enemy`, `elite`, `boss`, `event` | Enemy archetype slug. Must match `data/balance/scaling.json` entry. |
| `elite` | string | one of | Elite archetype slug. Triggers scripted-spawn rules (telegraph, 60 ms hitstop on kill, guaranteed drops). |
| `boss` | string | one of | Boss archetype slug. Triggers boss-event rules (entrance animation, time-dilate, music swap, arena mods). |
| `event` | string | one of | Non-enemy event slug (e.g. `boss-approach-signal`). Drives VFX/SFX/music without spawning anything. |
| `count` | number | yes (for enemy/elite/boss) | Number of instances to spawn at this `t`. |
| `pattern` | string | yes (for enemy/elite/boss) | Spawn pattern. See "Patterns" below. |
| `radius` | number | optional | Override for spawner ring radius (units). Default per spawner ID. |
| `from` | string | optional | Cardinal/ordinal direction for `stream`, `flank`, `scripted-spawn` patterns. Values: `north`, `east`, `south`, `west`, `northeast`, etc., or `any` (runtime random). |
| `telegraph_seconds` | number | optional | Pre-spawn telegraph duration. Applied to elite/boss; cues VFX + SFX. |
| `during_boss` | boolean | optional | If `true`, this is a boss-fight minion add. Wave runtime should respect boss-fight ambient density cap (80 in Meadow). |
| `post_boss` | boolean | optional | If `true`, this is an outro spawn. Wave runtime should trigger run-end tally check after the wave clears. |
| `vfx`, `sfx`, `music` | string | optional | For `event` entries: VFX prefab slug, SFX cue, music swap signal. |
| `entrance_seconds` | number | optional | For boss spawns: entrance animation duration. |
| `time_dilate_ms` | number | optional | For boss spawns: time-dilate duration in ms during entrance. |
| `time_dilate_factor` | number | optional | For boss spawns: time-dilate factor during entrance (0.4 = 40% speed). |

## Patterns

| Pattern | Behavior | Spawners used |
|---|---|---|
| `ring` | All instances spawn distributed evenly around the player at `radius` (default 35 u). | All 8 |
| `stream` | All instances spawn from a single cardinal/ordinal spawner over a 2-3 s drip. | 1 (specified by `from`) |
| `flank` | All instances spawn from 2 adjacent spawners on the same side, simultaneously. | 2 (e.g. `from: east` uses SP_NE + SP_E + SP_SE adjacency triple, picks 2) |
| `scatter` | Instances spawn at random angular positions on a ring at default radius. | All 8 (random selection per instance) |
| `scripted-spawn` | For elites/bosses: single spawn at a specific direction with telegraph VFX. | 1 (specified by `from`) |
| `center-spawn` | For bosses only: spawn at the arena center (player anchor), with entrance animation. | None (special-case) |

## Elite-window objects

The `elite_windows` array is a tooling/debug convenience that mirrors elite spawns from the `spawns` array. The runtime spawns from `spawns`; this section exists so balance-engineer + qa-engineer can audit elite cadence at a glance.

| Field | Type | Notes |
|---|---|---|
| `t` | number | Time anchor matching the corresponding `spawns` entry. |
| `pool` | array of strings | Elite archetype slugs eligible for this window. Single-element for biomes with one elite. |
| `spawn_direction` | string | Optional cardinal/ordinal override. |

## Boss object fields

| Field | Type | Notes |
|---|---|---|
| `t` | number | Time anchor for boss spawn. Should match the `spawns` entry with `boss` field. |
| `id` | string | Boss archetype slug. Cross-ref: `02-bosses/<boss>/mechanics.md`. |
| `arena_mods` | object | Per-phase arena modifications (props that spawn at phase gates, hazards toggled, etc.). |
| `arena_mods.hazard_count` | number | How many active hazards the boss arena adds. Meadow = 0 (calibration-biome rule). |
| `arena_mods.shrink_radius` | boolean | Whether the playable area shrinks during boss fight. Meadow = false. |
| `arena_mods.phase_2_props` | array | Prop slugs that spawn at phase-2 HP gate. |
| `arena_mods.phase_3_props` | array | Prop slugs that spawn at phase-3 HP gate. |
| `minion_adds` | array | Per-phase summon spec; each entry has `phase`, `enemy`, `count` per summon, and `summon_count` (how many times the boss summons during that phase). |

## Perf cross-check object

A free-form notes object documenting the level-designer's manual perf audit. Not runtime-consumed; reviewed by balance-engineer + tech-architect at PR.

| Field | Type | Notes |
|---|---|---|
| `peak_pre_boss_window` | string | The time window where ambient density peaks pre-boss. |
| `peak_during_boss_window` | string | Boss-fight ambient density window. |
| `method` | string | Math used (rolling 6 s window × spawn rate × avg lifetime). |
| `max_observed_window_concurrent_estimate` | number | The largest projected concurrent count from the method above. |
| `headroom_to_perf_cap` | number | `max_concurrent_enemies - max_observed_window_concurrent_estimate`. Should always be > 0. |

## Validation rules (for qa-engineer / lint)

1. **Monotonic time**: `spawns[i].t >= spawns[i-1].t` for all `i`.
2. **Boss-event consistency**: the `boss.t` must equal the `t` of the `spawns` entry with the `boss` field.
3. **Spawn-direction validity**: `from` values must be one of the cardinal/ordinal set or `any`.
4. **Pattern-direction coherence**: `ring`, `scatter`, `center-spawn` ignore `from`; `stream`, `flank`, `scripted-spawn` require it.
5. **Perf headroom**: `perf_crosscheck.headroom_to_perf_cap > 0`. If <= 0, level-designer must re-pace (per `05-enemies.md`: "**the cap is a design feature, not a constraint to fight**").
6. **Beat coverage**: every `t` in `spawns` should fall into a beat window from `00-pacing-model.md`. Warn-only, not error.

## Cross-references

- Pacing curve this schema instantiates: `../../00-pacing-model.md`.
- Per-biome layout: `layout.md`.
- Boss spec: `../../02-bosses/<boss>/mechanics.md`.
- Enemy taxonomy: `../../../02-gdd/05-enemies.md`.
- Perf contract: `../../../CLAUDE.md`.
