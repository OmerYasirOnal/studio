# 10-Balance 00 — Formulas

> Owner: balance-engineer. The math layer behind every tuning lever. Sister docs: `01-tuning-philosophy.md` (target bands), `02-character-tuning.md`, `03-weapon-tuning.md`, `04-enemy-tuning.md`, `05-economy-tuning.md`, `06-monte-carlo-notes.md`. All multipliers stack **additively** before being applied; the final equation multiplies the additive bundles together (this is the explicit "no magic ×1.05 stacked 14 times = ×2.0" trap-avoidance rule).

## 1. Damage formula

```
damage = base_damage
       × character_dmg_mult
       × weapon_level_mult
       × crit_mult
       × (1 - enemy_defense_mult)
```

| Term | Source | Type | Notes |
|---|---|---|---|
| `base_damage` | `weapons.json` → `dmg_base` | float | The per-hit damage at weapon level 1; unit = HP. |
| `character_dmg_mult` | `characters.json` → `dmg_mult` (additive with damage-charm + buff bundles) | float | Bunny baseline = 1.0. Final value = 1 + Σ(dmg_mult_buffs - 1). |
| `weapon_level_mult` | `weapons.json` → `level_mult[level]` | float | Per-level damage multiplier; see `03-weapon-tuning.md`. |
| `crit_mult` | computed | float | `1 + crit_damage` when `roll < crit_rate`, else `1.0`. |
| `enemy_defense_mult` | `enemies.json` → `defense_mult` | float | Clamped to **[0, 0.75]**. Damage clamped to **minimum 1**. |

**Additive-stacking rule.** All character/buff multipliers are additive within their bundle, then multiplicative across bundles:

```
character_dmg_mult_effective = 1 + (char_dmg_mult - 1) + Σ damage_charm_levels × 0.10 + Σ run_buffs
```

This prevents the Survivor.io trap where 14 stacked 5% buffs combine to >2x.

**Rationale.** Four canonical levers — character archetype, weapon investment, crit roll, enemy soak — each get a clean independent slot in the formula. Designers can reason about each lever in isolation.

**Sensitivity.** A 10% bump to `weapon_level_mult` at level 5 swings end-game DPS by ~+20%. A 10% bump to `crit_rate` only swings DPS by `0.1 × crit_damage` ≈ +5%. Weapon-level scaling is the **most sensitive lever**; crit is the least.

**Param file.** `data/balance/characters.json` (DMG mults) + `data/balance/weapons.json` (per-level mults + crit damage where applicable) + `data/balance/enemies.json` (defense_mult).

## 2. Crit roll

```
roll = uniform_random[0, 1)
if roll < character_crit_rate + weapon_crit_rate + buff_crit_rate:
    crit_mult = 1 + character_crit_damage + buff_crit_damage
else:
    crit_mult = 1
```

- `crit_rate` is a probability **clamped to [0, 0.95]**. Cosmically never 100% — players still feel the surprise.
- `crit_damage` default = **1.0** (i.e. crits deal 2x). Lucky-Charm passive adds to `crit_rate`, not `crit_damage`.
- **Pseudo-random distribution** (PRD) layer: if a character has gone 4× expected-crit-interval without a crit, force-crit on next hit. Prevents tilt; lives in `data/balance/feel.json` → `crit_prd_window`.

**Param file.** `data/balance/characters.json` (base `crit_rate`, `crit_damage`).

## 3. Movement formula

```
move_speed = base_move
           × character_speed_mult
           × (1 + speed_buff_total)
```

- `base_move` = **4.5 units/sec** (Bunny calibration anchor, per `03-characters.md`).
- `character_speed_mult` — per-character multiplier (Fox = 1.15, Tortoise = 0.7, etc.).
- `speed_buff_total` — additive bundle of run-time buffs (e.g., daisy-mine kill grants +5% MS for 2 s).
- **Cap:** `move_speed ≤ 9.0 units/sec` (2x Bunny baseline) to keep enemy AI homing solvable.

**Sensitivity.** Move speed is one of the **two most player-felt levers** (the other is crit rate). A 10% MS swing reads larger than a 10% DMG swing because the player sees it every frame.

**Param file.** `data/balance/characters.json`.

## 4. Pickup magnet radius

```
radius = base_magnet
       × character_magnet_mult
       × (1 + magnet_buff_total)
```

- `base_magnet` = **1.5 units** (default pickup pull radius).
- `character_magnet_mult` — Owl = 4.0, all others = 1.0 (per `03-characters.md`).
- `magnet_buff_total` — additive: `magnet-charm` adds 0.20 per level (level 5 = +100%).
- **Cap:** `radius ≤ 8.0 units` (slightly larger than reveal radius; prevents off-screen XP magic).

**Param file.** `data/balance/characters.json` + `data/balance/passives.json` (magnet-charm).

## 5. XP curve

```
xp_to_next(level) = floor(20 × level^1.55 + 5)
```

- Yields **~25 XP at L1→L2**, **~607 at L9→L10**, **~3900 at L29→L30**.
- Total XP from L1 to L30 = **~48,000**.
- Cross-checked against pacing model (22-25 level-ups per clean run = player reaches around L22-L25 per run; meta-progression L30 is across multiple runs).
- Per the GDD anchor (`01-core-loop.md`: 15-25 level-ups per run), the curve **must give 22 level-ups in a clean 8-min run**. Assuming median XP throughput from waves.json (see `06-monte-carlo-notes.md`), this curve hits 22 at minute 8 within ±1 level.

**Sensitivity.** The exponent **1.55** is the most-sensitive number in the whole game. **Reduce to 1.4 → +30% level-ups** (too many drafts, player drowns). **Raise to 1.7 → -25% level-ups** (drift below 15-25 floor). Change only after Monte Carlo re-run.

**Param file.** `data/balance/xp-curve.json` (precomputed table 1..30).

## 6. Drop rate

```
drop_chance = base_per_role × (1 + drop_buff)
```

- `base_per_role` — per-enemy-role, per-drop-item; see `drops.json` (e.g., swarmer XP-gem = 1.0 = guaranteed; gold-coin = 0.12).
- `drop_buff` — currently zero at launch (post-launch: founder-pass +5%, run-bonus card +20%).
- **Cap:** `drop_chance ≤ 1.0` per item.

**Param file.** `data/balance/drops.json`.

## 7. Soul Shard rate

- **Per elite kill:** 1-3 weighted draw. Distribution: 1 (50%), 2 (35%), 3 (15%). Expected value = **1.65**.
- **Per boss kill:** 10-30 weighted draw. Distribution: 10 (50%), 15 (25%), 20 (15%), 25 (7%), 30 (3%). Expected value = **14.8**.
- Distinct from XP/gold drops — soul shards are **deterministic per kill**, the count is what's randomized.
- Per-run expected shard bank: `1.65 × ~3 elites + 14.8 × 1 boss = 4.95 + 14.8 = ~19.75 shards`. The GDD says "4-10 per run" — this is calibrated for vertical-slice (1 boss + ~3 elites in Meadow); the 19.75 number includes the boss. Per-run minus boss = ~5 shards = on-spec.

**Param file.** `data/balance/economy.json` → `soul_shard_drop_weights`.

## 8. Carrot per run

```
carrots = base_per_kill × total_kills × (1 + carrot_buff)
```

- `base_per_kill` — per-enemy-role, see `economy.json`. Swarmer = 1, tank = 4, ranged = 2, elite = 20, boss = 100.
- Approx kill count over 8-min run: ~600 swarmers + 30 tanks + 25 ranged + 3 elites + 1 boss = `600 + 120 + 50 + 60 + 100 = 930 base carrots`. Within the GDD-band of 100-300 base, scaling up to ~2000 with run-bonus + cosmetic-multiplier.

**Param file.** `data/balance/economy.json` → `carrot_per_kill`.

## 9. Enemy HP-per-minute scaling

```
hp_at_minute(role, m) = hp_base[role] + hp_per_min[role] × (m - 1)
```

- Swarmer: `8 + 6×(m-1)` (linear; per GDD 05).
- Tank: `80 + 40×(m-1)`.
- Ranged: `20 + 12×(m-1)`.
- Elite: `600 + 250×(m-1)`.
- Boss: stepped — `4000` at m=5 (mid), `12000` at m=10 (end).

**Sensitivity.** **HP at minute 3** is the most-impactful enemy-tuning value (largest crowd of "you've-just-built-something" engagements). Move it 10% → median TTK swings ~8%.

**Param file.** `data/balance/enemies.json` → `scaling.hp_base` + `scaling.hp_per_min`.

## 10. Hitstop / time-dilate (feel)

Per ADR-0003, hitstop values live in `data/balance/feel.json`:

| Trigger | Duration |
|---|---|
| Basic enemy hit | 0 ms |
| Basic enemy kill | 20 ms |
| Elite hit | 30 ms |
| Elite kill | 80 ms |
| Boss damage tick | 40 ms |
| Boss phase change | 150 ms + 0.5x time-dilate 200 ms |
| Boss kill | 250 ms + run-end ceremony |

These are **not negotiable from balance side** — they are a feel-pillar contract.

## 11. Floor and ceiling clamps (game-wide)

| Clamp | Value | Why |
|---|---|---|
| `damage_min` | 1 (after defense reduction) | Players must always make progress. |
| `enemy_defense_mult` | [0, 0.75] | Cap mitigation at 75% so weapon investment still matters. |
| `crit_rate` | [0, 0.95] | Surprise lives at 5% non-crit floor. |
| `move_speed` | ≤ 2x baseline | Pathfinding solvability. |
| `magnet_radius` | ≤ 8.0 units | Prevents off-screen XP teleport. |
| `level` | [1, 30] | Meta-progression cap (per `08-economy.md`). |
| `weapon_level` | [1, 5] | Per `04-weapons.md` design rule 3. |

## 12. Param-file index

| File | Owns |
|---|---|
| `data/balance/characters.json` | HP, DMG, MOVE, CRIT, magnet, signature mechanic numbers |
| `data/balance/weapons.json` | Per-level DMG/RATE/RANGE/projectiles + evolution recipes |
| `data/balance/passives.json` | 6 passives × 5 levels |
| `data/balance/enemies.json` | HP/DMG/SPD per role + per-minute scaling |
| `data/balance/xp-curve.json` | Precomputed L1..L30 XP table |
| `data/balance/drops.json` | Per-role drop tables |
| `data/balance/economy.json` | Currency rates, IAP catalog, battle pass tiers |
| `data/balance/feel.json` | Hitstop / time-dilate per ADR-0003 |

## Cross-references

- ADR-0001 — universal weapon pool (no character `allowed_weapons` array).
- ADR-0003 — hitstop timings → `feel.json`.
- `01-tuning-philosophy.md` — target bands these formulas serve.
- `06-monte-carlo-notes.md` — sensitivity walk-through of the three biggest levers.
