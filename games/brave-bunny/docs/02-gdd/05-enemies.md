# GDD 05 — Enemies

> The full enemy taxonomy for Brave Bunny: 5 roles, 5 biomes, scaling, spawn density, and the per-minute pressure curve. Vertical slice ships **15 enemy archetypes** across the 4 wave-pattern templates (per `00-overview.md` scope). Sister docs: `01-core-loop.md` (HP/DMG scaling lifeblood), `11-feel-pillars.md` (kill-shake spec), `09-level-design/` (waves.json source of truth).

## Taxonomy — 5 roles

Every enemy in the game belongs to one of these roles. The role defines its **stats baseline, AI behavior, and player-pressure profile**. Visual skin can re-use a body mesh across biomes (palette + accessory swap per `00-overview.md` CC0 pipeline thesis).

| Role | Player pressure | Density at peak | HP profile | Spawn pattern |
|---|---|---|---|---|
| **Swarmer** | Volume / overwhelm | 60–80% of on-screen count | Low | Mass spawns in rings + arcs |
| **Tank** | Positional friction | 5–10% | High | Slow march from edges |
| **Ranged** | Hit-and-cover demand | 10–15% | Medium | Spawns at safe distance, kites |
| **Elite** | Mini-boss moment | 1–2 per minute peak | Very high | Scripted spawn, telegraphed |
| **Boss** | Setpiece | 1 per run mid + 1 end | Massive | Arena event |

### Visual variant bank

Per `00-overview.md`: **4 swarmer variants × 5 biomes**, **3 tank variants**, **3 ranged variants**, **1 elite per biome (5 total)**, **5 bosses (1 per biome)**. All meshes from Quaternius Animated Animals + recolor; behavior is shared by role, art is shared by biome.

Total mesh-skin count at launch: 20 swarmer + 15 tank + 15 ranged + 5 elite + 5 boss = **60 enemy skins**. Vertical slice uses 4 swarmer + 3 tank + 3 ranged + 1 elite + 1 boss skins = **12 skins** + 3 wave-template variants = **15 archetypes** as scoped.

## Stat baselines

All values **per minute** of run time. HP and DMG scale per minute via the formula in `data/balance/scaling.json` (balance-engineer owns). The numbers below are minute-1 baselines.

### Swarmer

| Field | Value |
|---|---|
| Role | Swarmer |
| HP baseline | 8 hp at min 1; +6 hp/min linear (so 50 hp at min 8) |
| DMG baseline | Contact: 5 hp/touch. No ranged. |
| Movement | 1.1x player movespeed; pure homing (no avoidance, no kiting) |
| Telegraph | None (they walk straight at you — readability is collective density, not individual) |
| Drop table | XP gem small (100%), gold coin (12%) → see `data/balance/drops.json` |
| Cartoon visual cue | Bug-eyed slime, bouncy fly, friendly-looking ladybug — round shapes, expressive eyes, no fangs |

### Tank

| Field | Value |
|---|---|
| Role | Tank |
| HP baseline | 80 hp at min 1; +40 hp/min (so 360 hp at min 8) |
| DMG baseline | Contact: 18 hp/touch. No ranged. |
| Movement | 0.6x player movespeed; homing with **charge** every 4 s (1.5x speed burst for 1 s) |
| Telegraph | Pre-charge wind-up: 12-frame body-tense animation + grunt SFX; total **0.4 s telegraph window** |
| Drop table | XP gem medium (100%), gold coin (60%), heart (8%) |
| Cartoon visual cue | Lumbering boar with bandaged tusks, sleepy ox, plodding walker — bulky but smiley; no horns sharper than rounded buttons |

### Ranged

| Field | Value |
|---|---|
| Role | Ranged |
| HP baseline | 20 hp at min 1; +12 hp/min |
| DMG baseline | Ranged projectile: 12 hp/hit. Contact: 4 hp. |
| Movement | 0.9x player movespeed; **kite behavior** — backs away if player <3.0 units, fires if 3–6 units |
| Telegraph | Pre-fire wind-up: arm-raise + SFX cue, **0.5 s telegraph window**, projectile travel 0.6 s at 4 units/sec |
| Drop table | XP gem medium (100%), gold coin (35%) |
| Cartoon visual cue | Archer-mole with a slingshot, throw-frog with a beanbag, slingshot-pig with a paper-airplane dart — toy-like, never lethal-looking |

### Elite

| Field | Value |
|---|---|
| Role | Elite |
| HP baseline | 600 hp at min 1; +250 hp/min (so 2350 hp at min 8) |
| DMG baseline | Contact: 25 hp/touch. Ranged (if applicable): 18 hp/hit. |
| Movement | 0.7x player movespeed; biome-flavored behavior (e.g., Frost Burrow elite charges, Honey Swamp elite belly-slams) |
| Telegraph | Per-attack telegraph: minimum **0.6 s window** with VFX ring marking AOE; per `11-feel-pillars.md` pillar 4, elite kills get 60 ms hitstop |
| Drop table | Guaranteed chest (1), XP gem large (100%), gold coin (100% ×5), 1 Soul Shard |
| Cartoon visual cue | One per biome — Carrot Fields: "Big Onion" (rolling onion with leafy hair); Honey Swamp: "Giant Bumble"; Sky Garden: "Cloud Walker"; Frost Burrow: "Snow Yeti-Cub"; Volcano Hop: "Lava Lobster" |

### Boss

| Field | Value |
|---|---|
| Role | Boss |
| HP baseline | 4000 hp at min 5 (mid-run); 12000 hp at min 10 (end-run); scales with biome tier |
| DMG baseline | Contact: 35 hp; ranged: 25 hp; AOE: 50 hp inside marker |
| Movement | Boss-unique; 3-phase HP gates (66%, 33%) trigger new attack pattern |
| Telegraph | Every attack telegraphed with **0.8 s minimum window**, AOE ring or projectile arc preview |
| Drop table | 3 Soul Shards (mid-boss) or 5 Soul Shards (end-boss), 1 guaranteed character-shard pull at end-boss, 100% chest |
| Cartoon visual cue | The Big Bad Wolf for vertical-slice — Carrot Fields biome. Launch roster: Wolf (Carrot Fields), Bee Queen (Honey Swamp), Hawk King (Sky Garden), Frost Bear (Frost Burrow), Magma Croc (Volcano Hop). All have soft features, large eyes, expressive faces; the Wolf is *more "wolf in pajamas" than Brothers Grimm*. |

## Per-biome variant table

| Biome | Swarmer variants | Tank variants | Ranged variants | Elite | Boss |
|---|---|---|---|---|---|
| Carrot Fields | Slime, Fly, Ladybug, Worm | Boar, Sleepy Ox | Archer-Mole | Big Onion | Wolf |
| Honey Swamp | Mud-Slime, Mosquito, Frog-pup, Jellyfish | Bog-Boar, Mud-Walker | Throw-Frog | Giant Bumble | Bee Queen |
| Sky Garden | Cloud-Wisp, Sparrow, Butterfly, Bumblebee | Sky-Ox, Wind-Walker | Slingshot-Pig | Cloud Walker | Hawk King |
| Frost Burrow | Snow-Slime, Snow-Fly, Snow-Bug, Penguin-chick | Yak, Walrus | Snowball-Mole | Snow Yeti-Cub | Frost Bear |
| Volcano Hop | Fire-Slime, Cinder-Moth, Magma-Bug, Lava-Newt | Magma-Boar, Stone-Ox | Ember-Frog | Lava Lobster | Magma Croc |

(20 swarmer + 10 tank + 5 ranged + 5 elite + 5 boss = 45 named entries — under the 60-skin budget because some variants share meshes across biomes via recolor.)

## Scaling formula references

- **HP scaling** lives in `data/balance/scaling.json`. Per role, linear-per-minute (swarmer +6, tank +40, ranged +12).
- **Spawn-rate scaling** lives in `data/balance/waves.json`. Per minute, the **total intended on-screen count** climbs from ~12 at min 1 to ~150 by min 10 (hard cap 200 from `CLAUDE.md`).
- **Pool affordances**: per `brave-bunny/CLAUDE.md`, pooling is mandatory for every spawnable; tech-architect's ADR-0005 governs the API. Enemies recycle to pool on death, **never `Destroy()`**.

## Enemies-per-minute density chart

The ASCII table below is the **target intent** for the wave-density curve. Cross-checks the perf contract (`CLAUDE.md`: 200 active enemies cap on iPhone 12 at 60 fps).

```
Min |  Swarmers | Tanks | Ranged | Elites | Boss | TOTAL on-screen
----+-----------+-------+--------+--------+------+----------------
  1 |        10 |     0 |      0 |      0 |    0 |     10
  3 |        35 |     2 |      3 |      0 |    0 |     40
  5 |        60 |     4 |      8 |      1 |    1 |     74 (mid-boss event)
  7 |       100 |     7 |     12 |      1 |    0 |    120
 10 |       145 |    10 |     20 |      2 |    1 |    178 (end-boss event)
```

### Density notes

- **Min 1**: low pressure, learning beat. Swarmers only. Player meets the auto-attack contract.
- **Min 3**: tanks and ranged enter; first draft is well underway; player feels build choices.
- **Min 5**: mid-boss event. Total counts spike briefly to ~120 during the boss arena (boss + adds), drops back to ~70 after.
- **Min 7**: peak "normal" density. Player should feel pressured but not lost.
- **Min 10**: end-boss event. Final test. Total counts spike to ~190+ during boss; **must stay ≤ 200 hard cap**.
- **Tail-out (10+)** for runs that extend past the 10-min target: density holds at ~150 with elite cadence ramping (1 elite every 30 s instead of 60 s).

### Perf budget cross-check

| Frame budget | Allocation |
|---|---|
| Enemy entities | ≤ 200 |
| Enemy draw calls | ≤ 40 (5 batched skins per call avg) |
| Enemy tri-count | ≤ 100k (within 250k on-screen tris budget) |
| Enemy collision tests | ≤ 200 player-vs-enemy + ≤ 50 projectile-vs-enemy/frame |

If the wave-pattern template breaches 200 at any tick, balance-engineer + level-designer collaborate on a re-pace — never raise the cap. **The cap is a design feature, not a constraint to fight.**

## Cartoon visual rules — banned-words enforcement

Per `narrative/` banned-words list and `00-overview.md` family-safe pillar:

- **No skulls** in any enemy design (use round button-buttons or smiley masks).
- **No blood** in death VFX (corpse-puff biome-tinted per `11-feel-pillars.md` pillar 1).
- **No demonic** imagery (horns must look like rounded buttons or party hats).
- **No realistic weapons** on enemies (archer-mole's bow is a toy slingshot; the boar's tusks are foam/bandaged).
- **No fangs in close-up** — enemies can have visible teeth but they read as goofy, not threatening.

This rule is checked at **art-director sign-off** per enemy skin and at **QA acceptance** per build.

## Cross-references

- HP/DMG raw numbers: **source of truth** `data/balance/scaling.json` (balance-engineer).
- Wave spawn schedule: **source of truth** `data/balance/waves.json` (level-designer; `brave-bunny/CLAUDE.md`: gameplay-engineer never modifies).
- Drop tables: `data/balance/drops.json` (balance-engineer + game-designer).
- Kill-shake / hitstop spec: `11-feel-pillars.md` pillars 1 + 4.
- Pooling API: tech-architect ADR-0005 (referenced by `brave-bunny/CLAUDE.md`).
- Boss attack patterns and arena layout deferred to `09-level-design/` per role.
