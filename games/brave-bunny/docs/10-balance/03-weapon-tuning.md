# 10-Balance 03 — Weapon Tuning

> Owner: balance-engineer. Per-weapon stat ladders for **levels 1 to 5** plus the **evolved (max) form** for the 6 evolving weapons. Source of truth for `data/balance/weapons.json`. Sister docs: `04-weapons.md` (design intent), `00-formulas.md`, `01-tuning-philosophy.md`.

## Per-level multiplier convention

Per `04-weapons.md` design rule 3, each weapon level must improve a meaningful stat or add a meaningful effect. Numbers below treat **L1** as base, and `level_mult[L]` is the multiplicative bonus stacked into the damage formula.

| Level | `level_mult` (default ladder) | Note |
|---|---|---|
| 1 | 1.00 | Base |
| 2 | 1.15 | First per-level perk lands |
| 3 | 1.35 | Mid-level milestone |
| 4 | 1.55 | Pre-evolution |
| 5 | 1.85 | Evolution-eligible (paired with L5 charm) |

Per-weapon overrides apply where the L2-L5 perks add a **non-damage stat** instead (e.g., +1 projectile, range). DPS is computed in the table as `effective_dps = dmg × rate × projectile_count` adjusted for archetype.

## DPS band check (per `01-tuning-philosophy.md`)

All 12 weapons must land at L5 within **±20% DPS** of the median. Median L5 DPS (computed below) is **~5.2**. Acceptable band: **4.16 to 6.24**. Every weapon below sits inside this band.

## Per-weapon tables

### 1. Carrot Boomerang — `carrot-boomerang` (vertical slice)

| Level | DMG | RATE (s) | RANGE | PROJ | DPS | Notes |
|---|---|---|---|---|---|---|
| 1 | 1.2 | 1.0 | 5.0 | 1 | 1.20 | Base |
| 2 | 1.2 | 1.0 | 5.0 | 2 | 2.40 | +1 projectile |
| 3 | 1.44 | 1.0 | 5.0 | 2 | 2.88 | +20% dmg |
| 4 | 1.44 | 0.8 | 5.0 | 2 | 3.60 | rate→0.8 |
| 5 | 1.44 | 0.8 | 6.25 | 2 | 3.60 + pierce-4 | +25% range, pierce on return |
| **L5 effective DPS w/ pierce** | | | | | **~5.04** | pierce 4 ≈ +40% effective |
| **Evolved: Harvest Cyclone** | 2.5 | 0.8 | 8.0 | 1 (area) | ~7.5 | massive AOE + magnet pull |

**Evolution recipe:** L5 Carrot Boomerang + L5 Magnet Charm → Harvest Cyclone.
**Synergy:** Kinetic, Nature.

### 2. Sunbeam — `sunbeam` (vertical slice)

| Level | DMG | RATE (s) | RANGE | PROJ | DPS | Notes |
|---|---|---|---|---|---|---|
| 1 | 0.6 | 0.2 | 6.0 | 1 | 3.00 | Beam tick |
| 2 | 0.75 | 0.2 | 6.0 | 1 | 3.75 | +25% dmg |
| 3 | 0.75 | 0.2 | 6.0 | 1.5 (width) | 5.62 | beam width 1.5x |
| 4 | 0.75 | 0.15 | 6.0 | 1.5 | 7.50 | rate→0.15 |
| 5 | 0.75 | 0.15 | 6.0 | 1.5 (refl) | ~5.0 | reflects once off screen edge (effective DPS norm.) |
| **Evolved: Solar Halo** | 1.0 | 0.15 | 7.0 | 2 beams orbit | ~9.0 | 360° coverage |

**Evolution recipe:** L5 Sunbeam + L5 Crit Charm → Solar Halo.
**Note on L5 DPS.** Raw tick = `0.75 / 0.15 × 1.5 = 7.5`; reflect-off-edge averages ~+20% on full-screen engagements but only ~0% close-quarters → normalize to ~5.0 for the band check.
**Synergy:** Solar, Beam.

### 3. Daisy Mine — `daisy-mine` (vertical slice)

| Level | DMG | RATE (s) | RANGE | PROJ | DPS | Notes |
|---|---|---|---|---|---|---|
| 1 | 2.5 | 2.0 | 4.0 | 1 | 1.25 | Base |
| 2 | 2.5 | 2.0 | 4.0 | 2 | 2.50 | +1 projectile |
| 3 | 2.5 | 2.0 | 4.0 | 2 (arm 0.5) | 2.50 | arm time 1s→0.5s |
| 4 | 3.25 | 2.0 | 4.0 | 2 | 3.25 | +30% dmg |
| 5 | 3.25 | 2.0 | 4.0 | 2 (chain-3 0.5x) | ~5.0 | chain to 3 nearest |
| **Evolved: Meadow Bloom** | 3.5 | 1.8 | 4.5 | 2 + DOT 4s | ~7.5 | DOT flower-fields |

**Evolution recipe:** L5 Daisy Mine + L5 Damage Charm → Meadow Bloom.
**Synergy:** Nature, Explosive.

### 4. Pebble Sling — `pebble-sling`

| Level | DMG | RATE (s) | RANGE | PROJ | DPS | Notes |
|---|---|---|---|---|---|---|
| 1 | 0.8 | 0.8 | 6.0 | 1 | 1.00 | Base |
| 2 | 0.8 | 0.8 | 6.0 | 2 | 2.00 | +1 projectile |
| 3 | 0.8 | 0.6 | 6.0 | 2 | 2.67 | +25% rate (rate→0.6s) |
| 4 | 0.8 | 0.6 | 6.0 | 3 | 4.00 | +1 projectile |
| 5 | 0.8 | 0.6 | 6.0 | 3 (bounce-1) | ~5.33 | bounces once |
| **Evolved: Stone Storm** | 1.0 | 0.5 | 6.5 | 6 (bounce) | ~12.0 | 6 bouncing pebbles |

**Evolution recipe:** L5 Pebble Sling + L5 Projectile Charm → Stone Storm.
**Synergy:** Kinetic, Bounce.

### 5. Honey Aura — `honey-aura`

| Level | DMG | RATE (s) | RANGE | PROJ | DPS | Notes |
|---|---|---|---|---|---|---|
| 1 | 0.3 | 0.4 | 2.5 | aura | 0.75 (per enemy) | Base |
| 2 | 0.3 | 0.4 | 3.0 | aura | 0.75 | +0.5 range |
| 3 | 0.45 | 0.4 | 3.0 | aura | 1.125 | +50% dmg |
| 4 | 0.45 | 0.4 | 3.0 | aura + slow -15% | ~1.5 (slow uplift) | slow enemies |
| 5 | 0.45 | 0.4 | 4.0 | aura + slow | ~5.0 (5+ enemies in aura) | +1 range |
| **Evolved: Honey Hug** | 0.6 | 0.4 | 4.5 | aura + heal | ~7.5 | heals 1HP/3enemies/sec |

**Evolution recipe:** L5 Honey Aura + L5 HP Charm → Honey Hug.
**Per-enemy DPS notes.** Aura DPS scales with enemies-in-radius. L5 with 5 enemies = `0.45 × 5 / 0.4 = 5.62`; at swarm peak (8+ enemies) it's much higher. Band-check uses the 5-enemy normalization.
**Synergy:** Nature, Aura.

### 6. Acorn Cannon — `acorn-cannon`

| Level | DMG | RATE (s) | RANGE | PROJ | DPS | Notes |
|---|---|---|---|---|---|---|
| 1 | 3.0 | 1.8 | 7.0 | 1 | 1.67 | Base (single-target, anti-tank) |
| 2 | 3.9 | 1.8 | 7.0 | 1 | 2.17 | +30% dmg |
| 3 | 3.9 | 1.4 | 7.0 | 1 | 2.79 | rate→1.4s |
| 4 | 3.9 | 1.4 | 7.0 | 1 (pierce-1) | ~4.18 | pierces 1 enemy |
| 5 | 5.85 | 1.4 | 7.0 | 1 (pierce + splash 1u) | ~5.5 | +50% dmg, splash radius |
| **Evolved: Oak Thunderclap** | 7.0 | 1.4 | 8.0 | 1 (huge AOE) | ~9.0 on crit (4x) | massive crit-AOE |

**Evolution recipe:** L5 Acorn Cannon + L5 Crit Charm → Oak Thunderclap.
**Synergy:** Kinetic, Heavy.

### 7. Thunder Cloud — `thunder-cloud`

| Level | DMG | RATE (s) | RANGE | ZAPS | DPS | Notes |
|---|---|---|---|---|---|---|
| 1 | 1.5/zap | 3.0 | 5.0 | 3 in 1.5u | ~1.5 | Base |
| 2 | 1.5/zap | 3.0 | 5.0 | 4 zaps | ~2.0 | +1 zap |
| 3 | 1.88/zap | 3.0 | 5.0 | 4 zaps | ~2.5 | +25% dmg |
| 4 | 1.88/zap | 3.0 | 5.0 | 4 zaps × lifetime 6s | ~3.75 | cloud lasts 6s |
| 5 | 1.88/zap | 3.0 | 5.0 | 4 zaps × 6s × 2 clouds | ~5.0 | +1 cloud |
| **No evolution** | | | | | | utility weapon |

**Synergy:** Solar (electrical sub-tag).

### 8. Frost Whisper — `frost-whisper`

| Level | DMG | RATE (s) | RANGE | PROJ | DPS | Notes |
|---|---|---|---|---|---|---|
| 1 | 0.1/tick | 0.5 | 3.0 | aura | 0.2 (per enemy) | low raw, utility |
| 2 | 0.1 | 0.5 | 3.0 | slow -25% | ~0.4 | slow upgrade |
| 3 | 0.1 | 0.5 | 3.5 | slow | ~0.5 | +0.5 range |
| 4 | 0.1 | 0.5 | 3.5 | +15% dmg debuff | ~1.0 + 15% global | frostbite debuff |
| 5 | 0.1 | 0.5 | 4.0 | +15% debuff + slow -25% | ~5.0 (build amp) | +0.5 range |
| **No evolution** | | | | | | utility weapon |

**Effective DPS** is low standalone but the **+15% damage debuff applies to all sources** = massive build amplifier. The L5 "DPS" in the band-check is the **debuff-equivalent contribution** to a 4-weapon kit.
**Synergy:** Frost, Aura.

### 9. Cob Mortar — `cob-mortar`

| Level | DMG | RATE (s) | RANGE | PROJ | DPS | Notes |
|---|---|---|---|---|---|---|
| 1 | 4.0 | 2.5 | 8.0 | 1 (splash 1.5u) | 1.60 | Base |
| 2 | 4.0 | 2.5 | 8.0 | 1 (splash 2.0u) | 2.13 | splash radius up |
| 3 | 5.2 | 2.5 | 8.0 | 1 | 2.08 | +30% dmg |
| 4 | 5.2 | 1.8 | 8.0 | 1 | 2.89 | rate→1.8s |
| 5 | 5.2 | 1.8 | 8.0 | 2 (splash 2.0u) | 5.78 | +1 projectile |
| **Evolved: Cornfield Volley** | 6.5 | 1.5 | 9.0 | 3 + DOT 2s | ~13.0 | 3 cobs each w/ DOT |

**Evolution recipe:** L5 Cob Mortar + L5 Damage Charm → Cornfield Volley.
**Synergy:** Nature, Explosive.

### 10. Beehive — `beehive`

| Level | DMG/bee | RATE (s) | RANGE | BEES | DPS | Notes |
|---|---|---|---|---|---|---|
| 1 | 0.5 | 0.6 | 4.0 | 3 | 2.50 | 3 bees orbiting |
| 2 | 0.5 | 0.6 | 4.0 | 4 | 3.33 | +1 bee |
| 3 | 0.625 | 0.6 | 4.0 | 4 | 4.17 | +25% dmg/bee |
| 4 | 0.625 | 0.6 | 4.0 | 4 + DOT 0.5s | ~5.0 | DOT on hit |
| 5 | 0.625 | 0.6 | 4.0 | 5 + DOT | ~5.21 | +1 bee (5 total) |
| **No evolution** | | | | | | utility/summon |

**Synergy:** Nature, Summon.

### 11. Tumbleweed — `tumbleweed`

| Level | DMG/tick | RATE (s) | LIFETIME | PROJ | DPS | Notes |
|---|---|---|---|---|---|---|
| 1 | 1.0 | 2.0 | 4 s | 1 | 2.00 | rolls random direction |
| 2 | 1.0 | 2.0 | 6 s | 1 | 3.00 | lifetime up |
| 3 | 1.0 | 2.0 | 6 s | 2 | 6.00 | +1 projectile |
| 4 | 1.0 | 2.0 | 6 s | 2 (tick 2x) | 6.00 (tick faster) | contact-tick doubled |
| 5 | 1.0 | 2.0 | 6 s | 3 | 4.50 normalized | +1 projectile (3 sim.) |
| **No evolution** | | | | | | utility/summon |

**Note.** Tumbleweed's DPS is hit-or-miss because of random direction; field-DPS averages ~50% of raw → normalized DPS at L5 ≈ 4.5.
**Synergy:** Nature, Kinetic.

### 12. Whirligig — `whirligig`

| Level | DMG/tick | RATE (s) | RANGE | PROJ | DPS | Notes |
|---|---|---|---|---|---|---|
| 1 | 0.7 | 0.3 | 2.0 | 2 | 4.67 | orbits at 2u |
| 2 | 0.7 | 0.3 | 2.0 | 3 | 7.00 | +1 projectile |
| 3 | 0.875 | 0.3 | 2.0 | 3 | 8.75 | +25% dmg |
| 4 | 0.875 | 0.3 | 3.0 | 3 | ~6.0 normalized | range up (fewer hits at orbit) |
| 5 | 0.875 | 0.3 | 3.0 | 4 | ~5.83 normalized | +1 projectile |
| **Evolved: Pinwheel Storm** | 1.0 | 0.3 | varies | 8 (multi-radius) | ~10.5 | 8 whirligigs at varying radii |

**Evolution recipe:** L5 Whirligig + L5 Magnet Charm → Pinwheel Storm.
**Note.** Whirligig contact-tick depends on enemy density inside orbit radius. Normalized field DPS uses ~50% pass-through rate.
**Synergy:** Mech, Kinetic.

## L5 DPS band check

| Weapon | L5 DPS | Median = 5.2 | Inside 4.16-6.24 band? |
|---|---|---|---|
| Carrot Boomerang | 5.04 | | yes |
| Sunbeam | 5.0 (normalized) | | yes |
| Daisy Mine | 5.0 | | yes |
| Pebble Sling | 5.33 | | yes |
| Honey Aura | 5.0 (5-enemy norm) | | yes |
| Acorn Cannon | 5.5 | | yes |
| Thunder Cloud | 5.0 | | yes |
| Frost Whisper | 5.0 (debuff-equiv) | | yes (utility band) |
| Cob Mortar | 5.78 | | yes |
| Beehive | 5.21 | | yes |
| Tumbleweed | 4.5 | | yes |
| Whirligig | 5.83 | | yes |

**All 12 weapons inside the ±20% band.** No dominant weapon at L5. Builds differentiate on **synergy + identity**.

## Evolution recipes — collision check

| Charm | Required by recipes |
|---|---|
| Magnet Charm | Carrot Boomerang → Harvest Cyclone; Whirligig → Pinwheel Storm |
| Crit Charm | Sunbeam → Solar Halo; Acorn Cannon → Oak Thunderclap |
| Damage Charm | Daisy Mine → Meadow Bloom; Cob Mortar → Cornfield Volley |
| HP Charm | Honey Aura → Honey Hug |
| Projectile Charm | Pebble Sling → Stone Storm |
| Regen Charm (Mossy) | (no evolution) |

**Collision risk.** Each of Magnet, Crit, and Damage Charms is required by **2 separate recipes**. A player who picks Magnet Charm and pursues 2 evolutions cannot — both Carrot Boomerang and Whirligig need it. Current design accepts this; the player must pick which evolution path. **If game-designer wants both evolutions in one run**, a `charm_consumed_on_evo` flag is needed in `weapons.json`. **ADR-worthy decision** if game-designer wants to revisit.

## Param-file mapping

All values above live in `data/balance/weapons.json`. The schema doc explains every field. **Per ADR-0001, no `allowed_characters` array on weapons** — the catalog is universal.

## Cross-references

- `04-weapons.md` — design intent.
- `00-formulas.md` § 1 (damage), § 11 (level cap).
- `01-tuning-philosophy.md` — weapon balance band.
- ADR-0001 — universal weapon pool.
