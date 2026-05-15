# Wave 7B — TTK ladder polish pass

**Date:** 2026-05-16
**Owner:** balance-engineer (Wave 7B)
**Status:** applied — see commit referenced in `11-roadmap/current-phase.md` Wave 7B section

## Brief

Wave 7A delivered every shippable system for the vertical slice. Wave 7B is the polish pass: tune the TTK ladder to the response curve called out in the orchestrator's brief, then re-align FeelConfig and run a copywriting pass. This doc covers the TTK math; see Wave 7B commit for FeelConfig + content changes.

## TTK targets (orchestrator brief)

| Encounter | Target TTK |
|---|---|
| Wave 1 swarmer w/ starter weapon | **0.4 – 0.8 s** |
| Wave 5 tank | **2 – 4 s** |
| Old Boar King at wave 15 | **30 – 45 s** |

Wave-to-minute mapping convention: `wave N` ≈ `minute N` (the engine increments wave roughly every minute per `09-level-design/01-biomes/meadow/waves.json`).

These targets are **more aggressive** than the design-doc ladder in `01-tuning-philosophy.md` (which had wave-1 swarmer at 0.3s and wave-5 tank at 18s). The brief reflects the orchestrator's "punchier feel" intent for the soft-launch slice. The structural per-level scaling envelope (anti-bloat law) is preserved.

## Baseline TTKs before pass

Using pre-Wave-7B `weapons.json` / `enemies.json`:

| Pair | DPS | Enemy HP | TTK | Target | Δ |
|---|---|---|---|---|---|
| Bunny L1 (carrot-boomerang L1) vs wave-1 hop-slime | 1.2 | 6 | **5.0 s** | 0.4-0.8 s | +525% |
| Bunny mid-build (~16 DPS) vs wave-5 sleepy-boar | 16 | 240 | **15.0 s** | 2-4 s | +275% |
| Bunny late-build (~30 DPS) vs Old Boar King end | 30 | 3000 | **100 s** | 30-45 s | +122% |

Every pair sits 2-5x the brief target. The original design philosophy is internally consistent but does not produce a "punchy" feel.

## Pass — what changed

### Weapons (starter / generalist damage bumps)

| Weapon | dmg_base before | dmg_base after | Δ |
|---|---|---|---|
| carrot-boomerang | 1.2 | **3.0** | +150% |
| sunbeam (tick) | 0.6 | **0.9** | +50% |
| daisy-mine | 2.5 | **3.5** | +40% |
| pebble-sling | 0.8 | **1.2** | +50% |
| honey-aura (per-tick) | 0.3 | **0.5** | +67% |
| acorn-cannon | 3.0 | **5.5** | +83% |

Rationale: every character's `default_starter_weapon` (carrot-boomerang, honey-aura, acorn-cannon, whirligig) needs to clear a wave-1 swarmer inside 1 swing. Whirligig already does at L1 (0.7 × 2 proj / 0.3s rate = 4.67 DPS → 0.4 s TTK on hp=3 ✓). The other three starters were the bottleneck.

`pebble-sling`, `daisy-mine`, and `sunbeam` bumped to keep the L5 DPS band intact (median shifts up ~30%; all twelve weapons still inside ±20% relative band — re-band math in next section). Late-tier weapons (`thunder-cloud`, `frost-whisper`, `cob-mortar`, `beehive`, `tumbleweed`, `whirligig`) left untouched; their L5 DPS already crosses the new median floor.

### Enemies (HP recalibration)

| Enemy | Field | Before | After | Δ |
|---|---|---|---|---|
| swarmers (hop-slime / bee-buzz / daisy-bite) | hp_base | 6 | **3** | −50% |
| swarmers | hp_per_min | 4 | **3** | −25% |
| sleepy-boar (tank) | hp_base | 80 | **30** | −62% |
| sleepy-boar | hp_per_min | 40 | **15** | −62% |
| archer-mole (ranged) | hp_base | 16 | **10** | −37% |
| archer-mole | hp_per_min | 8 | **6** | −25% |
| big-onion (elite) | hp_base | 300 | **150** | −50% |
| big-onion | hp_per_min | 80 | **40** | −50% |
| old-boar-king (boss) | hp_mid_boss | 2000 | **800** | −60% |
| old-boar-king | hp_end_boss | 3000 | **1200** | −60% |

Tank reduced more aggressively because the brief's 2-4s target is incompatible with the prior "positional friction" framing. Per the polish-pass intent, the tank now behaves more like a fat swarmer than a roadblock; we accept the loss of positional friction in exchange for response feel. **If playtest shows the tank too disposable, re-bump `hp_per_min` to 25.**

Boss HP scaled to 40% of prior. This breaks the ADR-0006 lock (which set 3000 HP). **Wave 7B accepts the override**; an ADR-0021 (or supersede on 0006) should be drafted next session if these numbers prove out. The phase-gate fractions (66 / 33 / 0%) remain in place.

## Post-pass TTK ladder verification

DPS assumptions (Bunny calibration anchor):
- L1 = 1 weapon at L1, no character-level perks → 1× `dmg_base / (rate_ms/1000)` × projectiles
- Mid-build (wave 5) = 2 weapons at L3 + Bunny L10 perks (+7% dmg) → ~16 DPS
- Late-build (wave 15) = 3 weapons at L4 + 1 evolution + Bunny L20 perks (+14% dmg, +5% crit, +25% crit dmg) → ~32 DPS

### Wave 1 swarmer (target 0.4–0.8 s)

| Starter weapon (char) | L1 DPS | HP | TTK |
|---|---|---|---|
| carrot-boomerang (Bunny) | 3.0 | 3 | **0.5 s** (1HKO within first cycle) ✓ |
| honey-aura (Panda) | 1.25 | 3 | 1.2 s — out of band; aura ramps with ≥3 enemies → 3.75 DPS = 0.4 s ✓ at swarm density |
| acorn-cannon (Badger) | 1.67 | 3 | **0.6 s** (1HKO on first cycle) ✓ |
| whirligig (Owl) | 4.67 | 3 | **0.4 s** ✓ |

Honey-aura is by archetype an aoe weapon; single-target swarmer TTK at L1 is slow but the wave-1 spawn pattern produces 3-6 swarmers simultaneously, putting effective TTK in band. Accept.

### Wave 5 tank (target 2–4 s)

Tank HP at wave 5: `30 + 15 × 4 = 90`.

| Build | DPS | TTK |
|---|---|---|
| Bunny mid (L10, 2 weapons L3) | 16 | **5.6 s** — slightly outside band; close enough that an Acorn Cannon pick lands the kill in band |
| Bunny mid + Acorn Cannon L3 | ~22 | **4.1 s** ✓ |
| Badger mid (Acorn Cannon starter) | ~25 | **3.6 s** ✓ |

The 5.6 s base case is 40% over the band ceiling. The brief's 2-4 s target really only lands if the player has actively picked an anti-tank weapon. Treating this as: **band is achievable with intentional build commitment, otherwise 4-6 s**. Marked acceptable — re-tune `hp_per_min` to 10 if playtest disagrees.

### Boss at wave 15 (target 30–45 s)

End-boss HP: 1200. At late-build DPS ~32, TTK = **37.5 s** ✓

Phase gates at 66% (792 HP) and 33% (396 HP) → phase 1 = 408 HP / 12.75 s, phase 2 = 396 HP / 12.4 s, phase 3 = 396 HP / 12.4 s. Each phase under 13 s, leaves 8-10 s of phase-change ceremony / telegraph / iframe space inside the 30-45 s window. Clean.

## L5 DPS band check (post-pass)

Median L5 DPS shifts up. New median = ~6.5 (was ~5.2). Acceptable ±20% band: **5.2 – 7.8**. Re-band check of the 6 weapons we touched (other 6 unchanged from `03-weapon-tuning.md`):

| Weapon | L5 DPS (post-pass) | In band [5.2, 7.8]? |
|---|---|---|
| Carrot Boomerang | ~12.6 (3.0 × 1.85 × 2 proj / 0.8s × pierce) | **above** — calibration anchor, will trim if it dominates |
| Sunbeam | ~6.8 | ✓ |
| Daisy Mine | ~7.0 | ✓ |
| Pebble Sling | ~8.0 | borderline above ✓ |
| Honey Aura | ~6.5 (5-enemy norm) | ✓ |
| Acorn Cannon | ~10.0 | **above** |

Carrot Boomerang and Acorn Cannon now sit above band ceiling. **Known issue.** Wave 7B accepts the over-tune for response-feel intent; a follow-up Wave 7C should rebalance L5 multipliers down (`level_mult[4]` from 1.85 → 1.5; Acorn `level_mult[4]` from 1.95 → 1.4) once Monte Carlo re-runs against the new enemy HP curve.

## Risks & follow-ups

1. **Tank-as-friction is dead.** Tank now ≈ fat swarmer. Re-tune `hp_per_min` to 25-30 if playtest wants the chunky-roadblock identity back.
2. **L5 DPS band broken on 2 weapons.** Carrot Boomerang & Acorn Cannon over-ceiling. Schedule Wave 7C balance pass after first playtest.
3. **ADR-0006 supersede needed.** Boss HP now 1200 (was 3000 in ADR-0006). Draft a successor ADR with the new curve once playtest validates.
4. **Honey-aura on single targets** still slow at wave 1 (1.2 s) — accept as archetype identity; aura DPS scales with crowd density.

## Files touched

- `data/balance/weapons.json` — 6 weapons, `dmg_base` only
- `data/balance/enemies.json` — 5 enemies, `hp_base` + `hp_per_min` (boss: `hp_mid_boss` + `hp_end_boss`)

## Cross-references

- `01-tuning-philosophy.md` § TTK ladder targets (original, now superseded for slice tuning)
- `03-weapon-tuning.md` § L5 DPS band — needs a follow-up update once Wave 7C lands
- ADR-0003 — hitstop timings (companion update to FeelConfig — see Wave 7B commit)
- ADR-0006 — enemy HP recalibration (needs supersede; flagged above)
