# Wave 9 — Weapon Roster Expansion (6 new base weapons)

**Date:** 2026-05-16
**Owner:** balance-engineer (Wave 9)
**Status:** applied — see Wave 9 commit referenced in `11-roadmap/current-phase.md`
**Sister docs:** `wave7-ttk-pass.md` (TTK targets + DPS band origin), `03-weapon-tuning.md` (per-weapon ladders for the original 12), GDD `02-gdd/04-weapons.md` (design intent for the original 12).

## Brief

The shipped roster is 12 base weapons + 8 evolutions. Wave 9 adds **6 new base weapons** to the roster (12 → 18), using only the 7 existing archetype subclasses (`Projectile`, `Mine`, `Cloud`, `SplashProjectile`, `Aura`, `Summon`, `Beam`). All new weapons import automatically via the ADR-0020 archetype dispatch — no `BalanceJsonImporter.cs` changes.

Evolutions for these six are scoped to a follow-up Wave (separate agent / `evolutions[]` block).

## The six new weapons

| Slug | Display | Archetype | JSON `archetype` | Key disambiguator | Targeting |
|---|---|---|---|---|---|
| `storm-cloud` | Storm Cloud | Cloud | `area` | `cloud_lifetime_ms` | `random-in-range` |
| `sapling-summon` | Sapling Summon | Summon | `summon` | `lifetime_ms` | `drop-in-range` |
| `maple-boomerang` | Maple Boomerang | Projectile | `projectile` | — | `nearest` |
| `sunflower-beam` | Sunflower Beam | Beam | `utility-beam` | — | `nearest-sweep-lock` |
| `cherry-bomb` | Cherry Bomb | SplashProjectile | `area` | `splash_units_base` | `random-screen-position` |
| `wasp-swarm` | Wasp Swarm | Summon | `summon` | — (no lifetime ⇒ persistent orbital) | `orbit-then-dive` |

### Design notes

- **Storm Cloud** — sibling of `thunder-cloud` (4-zap base vs Thunder Cloud's 3-zap), tighter range, electrical/storm flavor. Battle-pass S2-T8 unlock.
- **Sapling Summon** — `summon` with `lifetime_ms`: drops a stationary sprout that ticks for the duration, then despawns. Achievement unlock.
- **Maple Boomerang** — inherent pierce-2 at L2 (the brief's "2-target piercer"); L5 doubles up with +1 projectile. Biome-bound to Autumn Grove.
- **Sunflower Beam** — narrower-ramp beam vs Sunbeam (lower base DPS, longer range 6.5 units, same evolution path is intentionally absent — utility-only).
- **Cherry Bomb** — arc-toss → small splash AOE on land. Slightly faster + smaller splash than `cob-mortar`. Achievement unlock.
- **Wasp Swarm** — three mini-orbital projectiles routed via `summon` with no `lifetime_ms` (persistent like `beehive`). L2 +1 wasp, L5 +range_units for wider orbit.

## TTK ladder verification

### DPS targets (per `wave7-ttk-pass.md` § L5 DPS band check)

- **L5 DPS band:** 5.2 – 7.8 (median ~6.5, ±20%)
- **L1 vs Wave-1 swarmer (HP 3):** ideal 0.4–0.8 s; archetype-tolerated up to ~2 s for slow/utility weapons
- **L5 weapon contribution vs Wave-5 tank (HP 90):** mid-build 2-4 s total — single weapons land 11–17 s on tank in isolation, which is by design (matches `cob-mortar` ≈ 12 s)

### Per-weapon DPS (post-tune, archetype-aware single-target unless noted)

| Weapon | L1 DPS | L5 DPS | L5 in band? | TTK swarmer (3 HP, L1) | TTK L5 tank (90 HP, single weapon) |
|---|---|---|---|---|---|
| storm-cloud | 2.00¹ | 6.90¹ | **IN** | 1.50 s² | 13.0 s |
| sapling-summon | 2.00³ | 5.29³ | **IN** | 1.50 s² | 17.0 s |
| maple-boomerang | 1.82 | 6.39 | **IN** | 1.65 s² | 14.1 s |
| sunflower-beam | 4.00 | 7.67 | **IN** | 0.75 s ✓ | 11.7 s |
| cherry-bomb | 1.50 | 5.31 | **IN** | 2.00 s² | 17.0 s |
| wasp-swarm | 3.60 | 6.35 | **IN** | 0.83 s ✓ | 14.2 s |

¹ Cloud DPS = `dmg × zaps × proj / rate_s` aggregated per spawn cycle.
² Out-of-band TTK on single swarmer at L1 — accepted per archetype identity (cloud / drop-in / arc-toss / splash all delay first hit by design; AOE compensates with multi-enemy throughput). Same pattern as `daisy-mine` (1.71 s) and `cob-mortar` (1.88 s) in the shipped roster.
³ Sapling steady-state DPS during sprout's `lifetime_ms`; ignores spawn-gap downtime which is ~12.5% (1000 ms spawn cooldown vs 8000 ms lifetime ≈ 88.9% uptime).

### TTK at archetype crowd density

Aura/cloud/summon/splash archetypes scale with enemy density. The single-target table above is a worst-case; effective TTK at the **wave-1 swarm density (3-6 enemies simultaneously)** drops the TTK ratios into band for the three crowd-favored weapons:

| Weapon | Single-target L1 | At 3-enemy density |
|---|---|---|
| storm-cloud | 1.50 s/kill | ~0.5 s/kill (zap distributes across 3 enemies inside cloud) |
| cherry-bomb | 2.00 s/kill | ~1.0 s/kill (splash hits ~2 enemies on average) |
| wasp-swarm | 0.83 s/kill | 0.83 s/kill (already in band; 3 wasps target nearest, no density bonus) |

### TTK vs end boss (HP 1200, target 30–45 s late-build)

Late-build (3 weapons L4 + 1 evolution + Bunny L20 perks) hits ~32 DPS per `wave7-ttk-pass.md`. Adding any of the 6 new weapons at L5 contributes 5.3–7.7 DPS, slotting a third-or-fourth weapon into the build, keeping the 30–45 s window intact (32 + 6 ≈ 38 DPS → ~31.6 s).

## Outliers / known issues

1. **`sapling-summon` and `cherry-bomb` sit at the band floor (5.29 / 5.31).** Both slow archetypes by design; accept. If playtest reads "too weak," bump `dmg_base` by 0.1 in a Wave 9 hotfix.
2. **`sunflower-beam` L5 sits at 7.67 — ~98% of band ceiling.** Within tolerance, but if Wave 10 adds more beam-class weapons we should re-trim its `level_mult[4]` 1.25 → 1.20.
3. **Wave-1 swarmer TTK on the four area/splash archetype weapons exceeds the 0.4–0.8 s ideal band.** Accepted under same exception that covers `daisy-mine`, `cob-mortar`, `thunder-cloud`, `tumbleweed` in the shipped roster — these are **build weapons**, not starter weapons, and their `unlock` gates them behind battle-pass / achievement progression so the wave-1 swarmer is rarely faced with one of them as the only weapon.
4. **No evolutions defined.** All six entries have `"evolution": null`. Wave 10 (separate agent) is responsible for the evolved variants.

## L5 DPS band re-check (full 18-weapon roster)

Sub-band weapons in the shipped roster (`frost-whisper` ~1.0, `tumbleweed` ~1.5, `honey-aura` ~2.8 single-target) remain unchanged — they were already designed as crowd / debuff / utility weapons with intentional archetype identity below the band floor. Wave 9 introduces no new sub-band entries; all 6 land **IN [5.2, 7.8]** by single-target calc.

Above-band weapons in the shipped roster (`carrot-boomerang` ~14, `whirligig` ~14.6, `sunbeam` ~9.4) are L5 ceiling weapons by character intent (Bunny / Owl signature paths). Wave 9 introduces no new above-band entries.

## Files touched

- `data/balance/weapons.json` — appended 6 entries to `weapons[]` array (12 → 18)
- `docs/10-balance/wave9-weapons.md` — this doc (new)
- `docs/handoffs/wave9-loc-keys-needed.md` — loc keys for translation agent (new)

## Cross-references

- `wave7-ttk-pass.md` — defines the 5.2-7.8 L5 DPS band
- `03-weapon-tuning.md` — per-weapon DPS tables for the original 12 (needs a Wave 9 supplement once these six get evolution recipes in Wave 10)
- `02-gdd/04-weapons.md` § Weapon archetypes — design intent; doc currently lists 12 weapons and will need an addendum in a GDD update Wave
- ADR-0020 — archetype-config sidecar SO dispatch (the importer mechanism that makes these 6 weapons auto-load without code changes)
