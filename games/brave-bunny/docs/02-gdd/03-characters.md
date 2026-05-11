# GDD 03 — Characters

> The full 8-character roster for Brave Bunny launch. The vertical slice ships **1** (Bunny). All eight character meshes are recolored / kitbashed from the **Quaternius Animated Animals** CC0 pack (per `00-overview.md` + `core/docs/asset-policy.md`). Sister docs: `00-overview.md` (scope), `02-meta-loop.md` (unlock cost source), `04-weapons.md` (signature weapon hooks), `narrative/` (voice traits).

## Design philosophy

Each character must pass three tests before shipping:

1. **32-pixel silhouette test** — a stranger can name the animal at 32×32 pixels (art-director enforces in `07-art-bible/`).
2. **One-line signature** — the character has exactly one passive that a player can describe after one run.
3. **Playstyle diversity** — the 8 characters cover 8 distinct playstyle slots; no two characters share a "main feel" (see roster diversity statement at bottom).

## Stat baseline definitions

All stats are **multipliers vs Bunny's 1.0 baseline**. Bunny is the calibration anchor — balance-engineer treats `1.0 = Bunny` across the TTK ladder.

| Stat | Bunny baseline (raw) | Unit |
|---|---|---|
| HP | 100 | hit points |
| MOVE | 4.5 | units/second |
| DMG | 1.0 | damage multiplier on owned weapons |
| CRIT% | 5 | percent base crit chance |

Per `CLAUDE.md` principle 6, the raw numbers live in `data/balance/characters.json`. This doc owns the **multipliers and design intent**, not the source-of-truth numbers.

## Roster — full 8 characters

### 1. Bunny — `bunny` (vertical-slice hero)

| Field | Value |
|---|---|
| Slug | `bunny` |
| Display name | Bunny |
| Animal | Rabbit (Quaternius Animated Animals: `Rabbit.glb`) |
| Silhouette | Tall ears + round body — instantly readable as "bunny" at 32 px |
| Signature mechanic | **Hop Dodge** — every 5th weapon hit triggers a hop that auto-dodges the next incoming attack (i-frames for 0.4 s, 5 s cooldown) |
| Stats baseline | HP 1.0 / MOVE 1.0 / DMG 1.0 / CRIT% 1.0 |
| Unlock condition | Free (starter) |
| Voice trait | Earnest, optimistic, slightly bashful — the "I'll try my best!" archetype |
| Playstyle | Balanced dodge — the calibration baseline; safe in all biomes |

### 2. Tortoise — `tortoise`

| Field | Value |
|---|---|
| Slug | `tortoise` |
| Display name | Tortoise |
| Animal | Tortoise (Quaternius: `Tortoise.glb` — recolor: forest-green shell) |
| Silhouette | Low + wide dome — domed shell reads as "turtle" even at low res |
| Signature mechanic | **Shell Brace** — when HP drops below 50%, gain a temporary shell shield absorbing the next 100 damage; 8 s cooldown |
| Stats baseline | HP 1.6 / MOVE 0.7 / DMG 1.0 / CRIT% 0.5 |
| Unlock condition | 200 Stars (cheapest unlock) OR "Survive 100 elite kills" achievement |
| Voice trait | Stoic, slow-spoken, surprisingly wise — the "patient elder" archetype |
| Playstyle | Tank — soak damage, scale through positioning over reflex |

### 3. Hedgehog — `hedgehog`

| Field | Value |
|---|---|
| Slug | `hedgehog` |
| Display name | Hedgehog |
| Animal | Hedgehog (Quaternius: `Hedgehog.glb`) |
| Silhouette | Spiky orb on stubby legs — spines are the read |
| Silhouette description | Compact round body, 12 distinct spine tufts in radial pattern |
| Signature mechanic | **Thorn Ring** — passive ring of spike damage emits every 3 s, 1.5-unit radius, 0.5x DMG per tick |
| Stats baseline | HP 1.1 / MOVE 0.95 / DMG 0.9 / CRIT% 1.0 |
| Unlock condition | 400 Stars OR Battle Pass Season 1 Tier 15 |
| Voice trait | Grumpy, dry-witted, secretly soft — the "leave me alone" archetype |
| Playstyle | Close-range AOE — works the space immediately around the player |

### 4. Fox — `fox`

| Field | Value |
|---|---|
| Slug | `fox` |
| Display name | Fox |
| Animal | Fox (Quaternius: `Fox.glb` — recolor: warm orange + cream belly) |
| Silhouette | Slim body + bushy tail — tail is the read |
| Signature mechanic | **Cunning Strike** — kills on enemies below 25% HP trigger an exec dealing 3x DMG (also resets exec window on chain-kills, max 5 chain) |
| Stats baseline | HP 0.8 / MOVE 1.15 / DMG 1.3 / CRIT% 3.0 |
| Unlock condition | 600 Stars OR "Reach wave 30" achievement |
| Voice trait | Sly, quick-witted, theatrical — the "watch this" archetype |
| Playstyle | Glass cannon — high reward, low margin for error |

### 5. Otter — `otter`

| Field | Value |
|---|---|
| Slug | `otter` |
| Display name | Otter |
| Animal | Otter (Quaternius: `Otter.glb` — recolor: river-brown) |
| Silhouette | Long body + paddle tail + roundish head — playful posture |
| Signature mechanic | **Splash Volley** — all projectile weapons fire +1 projectile with a +20° spread (aura/melee unaffected) |
| Stats baseline | HP 0.95 / MOVE 1.05 / DMG 0.85 / CRIT% 1.0 |
| Unlock condition | 800 Stars OR Limited event "Splash Festival" (free, week 6 post-launch) |
| Voice trait | Bubbly, mischievous, loves to share — the "best friend" archetype |
| Playstyle | Multi-shot DPS — best paired with projectile weapons + projectile passive |

### 6. Panda — `panda`

| Field | Value |
|---|---|
| Slug | `panda` |
| Display name | Panda |
| Animal | Panda (Quaternius: `Panda.glb`) |
| Silhouette | Round + black-white patches — patch contrast does the work |
| Signature mechanic | **Hearty Snack** — every XP gem picked up restores 1 HP (caps at maxHP) |
| Stats baseline | HP 1.25 / MOVE 0.9 / DMG 0.95 / CRIT% 0.8 |
| Unlock condition | 1000 Stars OR Battle Pass Season 1 Tier 30 (free track capstone) |
| Voice trait | Gentle, food-obsessed, oddly profound — the "wise gourmet" archetype |
| Playstyle | Sustain — high uptime via constant pickup healing; rewards aggressive farming |

### 7. Badger — `badger`

| Field | Value |
|---|---|
| Slug | `badger` |
| Display name | Badger |
| Animal | Badger (Quaternius: `Badger.glb` — recolor: cream face stripe) |
| Silhouette | Low + striped face — the face stripe is the silhouette tell |
| Signature mechanic | **Baby Patrol** — every 30 s, spawn a baby-badger companion (auto-attacks nearest, 0.6x DMG, 60 s lifetime, max 3 simultaneously) |
| Stats baseline | HP 1.0 / MOVE 0.95 / DMG 0.85 / CRIT% 0.8 |
| Unlock condition | 1200 Stars OR "Defeat the Wolf boss 25 times" achievement |
| Voice trait | Gruff parent, protective, secretly proud — the "den mother" archetype |
| Playstyle | Summoner — board-control via persistent minions; rewards survival over peak DPS |

### 8. Owl — `owl`

| Field | Value |
|---|---|
| Slug | `owl` |
| Display name | Owl |
| Animal | Owl (Quaternius: `Owl.glb` — recolor: tawny brown + cream belly) |
| Silhouette | Big head + small body + flared wings when idle — head:body ratio is the read |
| Signature mechanic | **Far Sight** — pickup magnet radius is 4x baseline (6.0 units), and all XP gems grant +15% XP value |
| Stats baseline | HP 0.9 / MOVE 1.0 / DMG 0.95 / CRIT% 1.5 |
| Unlock condition | 1500 Stars OR "Reach wave 50" achievement (hardest in game) |
| Voice trait | Aloof scholar, cryptic, kind once warmed up — the "mysterious mentor" archetype |
| Playstyle | Magnet / scaling — out-levels other characters via XP throughput; weakest early, strongest late |

## Silhouette test (32 px)

Art-director will validate each character against the 32×32 silhouette test before vertical-slice gate:

| Character | Primary silhouette cue | Secondary cue |
|---|---|---|
| Bunny | Tall vertical ears | Round body |
| Tortoise | Wide dome shell | Low-to-ground stance |
| Hedgehog | Radial spine tufts | Spherical body |
| Fox | Bushy tail (1.5x body length) | Slim profile |
| Otter | Long body + paddle tail | Belly-up idle pose option |
| Panda | High-contrast 2-tone patches | Round head + ears |
| Badger | Cream face-stripe over dark fur | Low + long body |
| Owl | Oversized head (40% of silhouette) | Flared wings when idle |

If any character fails the test, recolor first, kitbash second, scrap last.

## Unlock-order sequencing

Pulled from `02-meta-loop.md` for cross-reference:

| Order | Character | Cost | Earn-time (F2P avg) |
|---|---|---|---|
| 1 | Bunny | Free | Day 0 |
| 2 | Tortoise | 200 Stars | Day 7 |
| 3 | Hedgehog | 400 Stars | Day 21 |
| 4 | Fox | 600 Stars | Day 40 |
| 5 | Otter | 800 Stars | Day 60 |
| 6 | Panda | 1000 Stars | Day 90 |
| 7 | Badger | 1200 Stars | Day 130 |
| 8 | Owl | 1500 Stars | Day 180 |

Earn-time assumes ~8 Stars/day F2P (per `02-meta-loop.md`). Spenders cut these times by 3–10x depending on tier.

## Roster diversity statement

The 8 characters intentionally tile the playstyle space. Each character occupies a distinct slot — no two characters reward the same build pattern. This is the explicit answer to Capybara Go!'s "all characters feel similar" risk flagged in `01-research/03-positioning.md`.

| Slot | Character | Why this character owns this slot |
|---|---|---|
| **Tank** | Tortoise | Only character with HP > 1.5x baseline + active mitigation |
| **DPS (raw)** | Fox | Highest DMG multiplier + crit + exec chain |
| **DPS (sustained)** | Hedgehog | Passive AOE rewards sustained close-range positioning |
| **Utility** | Otter | +1 projectile is the most universally applicable buff in the game |
| **Summoner** | Badger | Only character with persistent autonomous companions |
| **Sustain** | Panda | Only character with mid-run regen tied to pickups |
| **Magnet / Scaling** | Owl | Only character that scales XP gain — out-levels the run |
| **Dodge / Baseline** | Bunny | Only character with active dodge i-frames; serves as calibration anchor |

No two characters share a slot. If a future character proposal duplicates a slot, the slot must be re-defined first (ADR-worthy).

## Cross-references

- Unlock costs **source of truth** is `02-meta-loop.md` (this doc mirrors).
- Stat raw values **source of truth** is `data/balance/characters.json` (balance-engineer owns).
- Signature mechanics **interact with** weapons in `04-weapons.md` — e.g., Otter +1 projectile applies to all archetype=projectile weapons; Hedgehog Thorn Ring is independent of weapon slots.
- Voice traits feed into `narrative/` character bios; one-line voice tag is the brief, full bio lives in narrative-designer's folder.
- Mesh source verified CC0 via `core/docs/asset-policy.md` — Quaternius Animated Animals pack.
