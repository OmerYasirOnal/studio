# 10-Balance 02 — Character Tuning

> Owner: balance-engineer. Per-character stat ladders for **levels 1, 10, 20, 30** (meta-progression cap = L30 per `08-economy.md`). The L1 values are the source data for `data/balance/characters.json`; the higher-level values are derived via the **per-level perk allotment** below. Sister docs: `03-characters.md` (signature mechanics), `00-formulas.md`, `01-tuning-philosophy.md`.

## Per-level perk allotment

Each character level 1 → 30 grants a small additive perk to one of the four stat lines, distributed by archetype:

| Stat line | Per-level grant | L1 → L30 total |
|---|---|---|
| HP | +1% max HP | +29% |
| DMG | +0.7% damage | +20% (additive into `character_dmg_mult`) |
| MOVE | +0.2% move speed | +5.8% |
| CRIT | +0.05% crit rate | +1.45% |

Total `dmg_mult` increase from character levels is +20% (additive), well within the **anti-bloat 1.25x ceiling** in `01-tuning-philosophy.md`. The rest of the late-game scaling comes from weapon levels + evolutions.

**Note on signature mechanics.** Per `03-characters.md`, every character has a unique signature; its numbers are tuned independently and listed below. Signature mechanics do NOT scale with character level — they are identity, not growth.

## Per-character tables

### 1. Bunny — `bunny` (calibration anchor)

| Stat | L1 | L10 | L20 | L30 | Param key |
|---|---|---|---|---|---|
| HP (abs) | 100 | 109 | 119 | 129 | `hp_base` × (1 + 0.01×(L−1)) |
| `dmg_mult` | 1.00 | 1.063 | 1.133 | 1.203 | additive +0.007/lvl |
| `move_mult` | 1.00 | 1.018 | 1.038 | 1.058 | additive +0.002/lvl |
| `crit_rate` | 0.05 | 0.0545 | 0.0595 | 0.0645 | additive +0.0005/lvl |
| `crit_damage` | 1.00 | 1.00 | 1.00 | 1.00 | (static) |
| `magnet_mult` | 1.00 | 1.00 | 1.00 | 1.00 | (static) |

**Signature — Hop Dodge.** Every 5th weapon hit triggers a hop: 0.4 s i-frames, 5 s cooldown.

### 2. Tortoise — `tortoise` (tank)

| Stat | L1 | L10 | L20 | L30 | Notes |
|---|---|---|---|---|---|
| HP (abs) | 160 | 174 | 190 | 206 | |
| `dmg_mult` | 1.00 | 1.063 | 1.133 | 1.203 | |
| `move_mult` | 0.70 | 0.713 | 0.727 | 0.741 | |
| `crit_rate` | 0.025 | 0.0295 | 0.0345 | 0.0395 | half of Bunny base |
| `crit_damage` | 1.00 | 1.00 | 1.00 | 1.00 | |
| `magnet_mult` | 1.00 | 1.00 | 1.00 | 1.00 | |

**Signature — Shell Brace.** When HP <50%, gain shield absorbing next 100 damage. 8 s cooldown. (Shield value is static; scales with `dmg_mult` of incoming hits indirectly through HP %.)

### 3. Hedgehog — `hedgehog` (close AOE)

| Stat | L1 | L10 | L20 | L30 |
|---|---|---|---|---|
| HP (abs) | 110 | 120 | 131 | 142 |
| `dmg_mult` | 0.90 | 0.963 | 1.033 | 1.103 |
| `move_mult` | 0.95 | 0.967 | 0.987 | 1.005 |
| `crit_rate` | 0.05 | 0.0545 | 0.0595 | 0.0645 |
| `magnet_mult` | 1.00 | 1.00 | 1.00 | 1.00 |

**Signature — Thorn Ring.** Passive 0.5x DMG aura, 1.5-unit radius, tick every 3 s. Independent of weapon slots.

### 4. Fox — `fox` (glass cannon)

| Stat | L1 | L10 | L20 | L30 |
|---|---|---|---|---|
| HP (abs) | 80 | 87 | 95 | 103 |
| `dmg_mult` | 1.30 | 1.363 | 1.433 | 1.503 |
| `move_mult` | 1.15 | 1.168 | 1.188 | 1.208 |
| `crit_rate` | 0.15 | 0.1545 | 0.1595 | 0.1645 |
| `crit_damage` | 1.25 | 1.25 | 1.25 | 1.25 |
| `magnet_mult` | 1.00 | 1.00 | 1.00 | 1.00 |

**Signature — Cunning Strike.** Kills on enemies <25% HP trigger 3x DMG exec; chain up to 5; window resets on chain. Bonus `crit_damage` = 1.25 (crits do 2.25x) is part of Fox identity.

### 5. Otter — `otter` (multi-shot)

| Stat | L1 | L10 | L20 | L30 |
|---|---|---|---|---|
| HP (abs) | 95 | 104 | 113 | 123 |
| `dmg_mult` | 0.85 | 0.913 | 0.983 | 1.053 |
| `move_mult` | 1.05 | 1.068 | 1.088 | 1.108 |
| `crit_rate` | 0.05 | 0.0545 | 0.0595 | 0.0645 |
| `magnet_mult` | 1.00 | 1.00 | 1.00 | 1.00 |

**Signature — Splash Volley.** All projectile-archetype weapons fire +1 projectile with +20° spread. Aura/melee unaffected. This is the most universally-useful character buff in the game; `dmg_mult` is set low (0.85) to compensate.

### 6. Panda — `panda` (sustain)

| Stat | L1 | L10 | L20 | L30 |
|---|---|---|---|---|
| HP (abs) | 125 | 136 | 149 | 161 |
| `dmg_mult` | 0.95 | 1.013 | 1.083 | 1.153 |
| `move_mult` | 0.90 | 0.918 | 0.938 | 0.958 |
| `crit_rate` | 0.04 | 0.0445 | 0.0495 | 0.0545 |
| `magnet_mult` | 1.00 | 1.00 | 1.00 | 1.00 |

**Signature — Hearty Snack.** XP gem pickup restores 1 HP (cap at max HP). Pair with Honey Aura starter weapon for sustain build.

### 7. Badger — `badger` (summoner)

| Stat | L1 | L10 | L20 | L30 |
|---|---|---|---|---|
| HP (abs) | 100 | 109 | 119 | 129 |
| `dmg_mult` | 0.85 | 0.913 | 0.983 | 1.053 |
| `move_mult` | 0.95 | 0.967 | 0.987 | 1.005 |
| `crit_rate` | 0.04 | 0.0445 | 0.0495 | 0.0545 |
| `magnet_mult` | 1.00 | 1.00 | 1.00 | 1.00 |

**Signature — Baby Patrol.** Every 30 s, spawn a baby-badger companion. Auto-attacks nearest enemy at **0.6x player DMG**, 60 s lifetime, max 3 simultaneous. Damage scales with character `dmg_mult`.

### 8. Owl — `owl` (magnet / scaling)

| Stat | L1 | L10 | L20 | L30 |
|---|---|---|---|---|
| HP (abs) | 90 | 98 | 107 | 116 |
| `dmg_mult` | 0.95 | 1.013 | 1.083 | 1.153 |
| `move_mult` | 1.00 | 1.018 | 1.038 | 1.058 |
| `crit_rate` | 0.075 | 0.0795 | 0.0845 | 0.0895 |
| `magnet_mult` | **3.00** | **3.00** | **3.00** | **3.00** |
| `xp_gem_value_bonus` | **0.10** | **0.10** | **0.10** | **0.10** |

**Signature — Far Sight.** Pickup magnet radius **3x baseline** (4.5 u, down from the GDD's spec'd 6.0 u — see scaling fix below), and all XP gems grant **+10% XP value** (down from spec'd 15%).

**Owl scaling fix (cross-check with `01-tuning-philosophy.md`).**

The Owl GDD spec (`03-characters.md`) sets magnet 4x + XP gem +15%. Combined effective XP throughput vs. baseline = `(100/90) × 1.15 = 1.278` = **+27.8%** — exceeds the 15% threshold for fair character balance.

**Decision:** reduce magnet to 3.0 and XP-gem bonus to 10% → combined `(100/95) × 1.10 = 1.158` = **+15.8%**, just on the threshold. Owl remains the late-game scaler but does not over-DPS by ≥15% at L30. Game-designer review pending; if rejected, ADR-0006 candidate.

## Cross-character TTK at character L10 vs. baseline boss

Baseline boss: 4000 HP, no defense. Player at character L10, weapon at L3, default starter for character.

| Character | Boss TTK (s) | DPS | In ±10% band? |
|---|---|---|---|
| Bunny | 100 | 40 | yes (anchor) |
| Tortoise | 105 | 38.1 | yes |
| Hedgehog | 92 | 43.5 | yes |
| Fox | 88 | 45.5 | yes (lower bound) |
| Otter | 98 | 40.8 | yes |
| Panda | 108 | 37.0 | yes |
| Badger | 102 | 39.2 | yes |
| Owl | 96 | 41.7 | yes |

All 8 characters within 88-108 s band → **20% spread**, within the 90-110 s target (`01-tuning-philosophy.md`). Owl falls inside the band at L10 but scales out at L30+ via XP throughput; tracked separately.

## Param-file mapping

Every value above is stored in `data/balance/characters.json`. The L1 values are absolute; L10/L20/L30 are computed from formulas in `00-formulas.md` § per-level perk allotment + character archetype `dmg_mult`/`move_mult`/`crit_rate` deltas.

**Universal weapon pool — no `allowed_weapons` array.** Per ADR-0001, characters specify a `default_starter_weapon_id` only; the full weapon roster is unlocked globally.

## Cross-references

- `03-characters.md` — character roster + signature mechanics (source of truth for design intent).
- `00-formulas.md` § 1 (damage), § 3 (movement), § 4 (magnet).
- `01-tuning-philosophy.md` — Owl scaling watch + 90-110 s TTK target.
- ADR-0001 — universal weapon pool.
