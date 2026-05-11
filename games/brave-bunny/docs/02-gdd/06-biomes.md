# GDD 06 — Biomes

> The 5 launch biomes for Brave Bunny. The vertical slice ships **Meadow only** (per `00-overview.md` scope; the existing scaffold also calls this "Carrot Fields" — Meadow is the **canonical surface name** post-tone-bible, Carrot Fields is the in-fiction zone within Meadow). Sister docs: `00-overview.md` (scope + pillars), `02-meta-loop.md` (biome unlock ladder source of truth), `05-enemies.md` (per-biome rascal variant bank), `07-bosses.md` (boss per biome), `07-art-bible/04-environment-style.md` (tile palettes — to be authored by art-director), `narrative/03-biome-flavor.md` (mood paragraphs — to be authored by narrative-designer).

## Design philosophy

A biome must do three things in 7-10 minutes:

1. **Read instantly** — a stranger seeing a 1-second clip should name the biome (Meadow vs Beach vs Forest vs Cavern vs Snow). The 32-pixel silhouette test for characters has a biome corollary: **the 240-pixel tile-strip test** — a stranger can name the biome from a 240-pixel horizontal strip of ground tiles + 1 hero prop.
2. **Pace its hazards** — Meadow has none (it is the calibration baseline). Each subsequent biome adds exactly one new hazard type so the player learns one thing per unlock.
3. **Share enemy roles, not enemy art** — per `05-enemies.md`, the 5 roles (swarmer / tank / ranged / elite / boss) are constant; only the skin and palette change. This is the CC0 recolor pipeline thesis.

All biome unlocks are **gameplay-gated, never paywalled** (per `02-meta-loop.md`). The biome list ships in the build; only the unlock flag flips.

## Tone-bible naming reconciliation

The tone bible (`narrative/00-tone-bible.md`) names enemies as "rascals" (Meadow), "scamps" (Forest), "troublemakers" (Bramble), "wee beasties" (Marsh), "snow-pests" (Snowfield). The biome list below uses the **gameplay-readable surface names** (Meadow / Beach / Forest / Cavern / Snow) with the tone-bible terms mapped per-biome in the "Enemy term" row. Narrative-designer to extend the tone bible to cover Beach and Cavern in `narrative/03-biome-flavor.md`.

## Biome roster

### Cross-biome at-a-glance

| Order | Biome | Tone-bible enemy term | Hazards introduced | Boss (→ `07-bosses.md`) | Unlock condition |
|---|---|---|---|---|---|
| 1 | Meadow | rascals | None (calibration baseline) | Old Boar King | Free (starter, vertical-slice) |
| 2 | Beach | wee beasties (mapped from Marsh — coastal flavor) | Sand traps (1.5× slowdown patches) | Crab Captain | Complete Meadow 3 times |
| 3 | Forest | scamps | Root snares (0.4 s root) + low-vision underbrush | Mama Oak | Reach any character Lv 8 |
| 4 | Cavern | troublemakers (mapped from Bramble — claustrophobic flavor) | Stalactite drop zones + low-light reveal radius | Sneaky Cave Mole | Defeat 2 biome bosses |
| 5 | Snow | snow-pests | Ice slides (post-movement drift) + cold-tick (light DOT in open patches) | Big Snow-yeti | Defeat 3 biome bosses |

Hazard escalation is monotonic — each biome strictly adds a new hazard class; nothing is taken away. This is the **teach-one-thing rule**.

---

## Meadow

**Theme:** Verdant, sunny, beginner biome. Open green field with low wooden fences, a lone tree, a wooden well, scattered mushroom clusters. The carrots are missing — the bunny goes looking.

**Mood:** Warm noon sun, soft breeze, a tune you'd hum to a child. Reference `narrative/03-biome-flavor.md` (to be authored) — voice: "the kitchen-table picnic before the adventure starts." No darkness, no menace; the rascals look more annoyed than angry.

**Hazards:** None. Meadow is the **calibration biome**; balance-engineer treats Meadow TTK as the 1.0 anchor for all other biomes. New players never die to a hazard here — only to enemies.

**Tile palette:** Reference `07-art-bible/04-environment-style.md` (to be authored). Base hex anchors for art-director: grass `#7CC95F`, fence wood `#A37344`, mushroom red `#D6453F`, well stone `#9DA4AD`, sky `#9BD6F2`. Palette = saturated, warm, low contrast between adjacent tiles.

**Enemy variant set (per `05-enemies.md`):**

| Role | Rascal name | Notes |
|---|---|---|
| Swarmer | hop-slime (green slime, springy) | calibration baseline swarmer |
| Swarmer | bee-buzz (small bumble) | first projectile-adjacent swarmer cue |
| Swarmer | daisy-bite (walking flower with teeth) | flora flavor |
| Swarmer | ladybug (rolling) | reused per existing `05-enemies.md` Carrot Fields entry |
| Tank | sleepy boar (bandaged tusks) | per `05-enemies.md` Carrot Fields tank entry |
| Tank | sleepy ox | per existing taxonomy |
| Ranged | archer-mole (toy slingshot) | per existing taxonomy |
| Elite | Big Onion (rolling, leafy hair) | per existing taxonomy |

**Hero prop (level-design landmark — picked by level-designer per arena layout):** lone tree / wooden well / mushroom cluster. One prop per arena; serves as a positional anchor for the mid-boss and the end-boss arena framings.

**Boss:** Old Boar King (see `07-bosses.md`). Sleepy, grumpy, charges in straight lines. Vertical-slice boss.

**Unlock condition:** Free (starter, vertical-slice biome). The first thing the player ever sees.

**Average run target time:** 7-10 minutes (genre baseline per `00-overview.md`).

**Difficulty notes:** This is the **easiest biome in the game.** Balance-engineer tunes Meadow so a first-time player with no upgrades survives to ~minute 5 on their first attempt. The hard test: a stranger plays Meadow once, dies, and **wants to try again** without prompting. If they uninstall after one Meadow death, balance is wrong.

---

## Beach

**Theme:** Golden hour, warm sand, gentle waves at the edges. Palm trees, a thatched hut, a coconut pile. The carrots are washed up on the shore — the bunny goes hopping after them.

**Mood:** Late-afternoon picnic. Sand between paws, a little too warm, a little too bright. Reference `narrative/03-biome-flavor.md` (to be extended) — voice: "the holiday postcard." Wee beasties skitter sideways like real crabs do; the register is goofy-coastal, never threatening.

**Hazards:**
- **Sand-trap patches** — 1.0-unit-radius circles, visible as darker sand. Movespeed × 0.65 while standing inside. Telegraph: a faint sparkle ring on the sand 0.4 s before the patch becomes active.
- Sand traps appear from minute 2 onward, 1-2 visible at any time. Never block the player completely; always a path around.

**Tile palette:** Reference `07-art-bible/04-environment-style.md` (to be authored). Base hex anchors: sand `#F2D89B`, wet sand `#D6B97A`, palm green `#6BA660`, hut thatch `#C49A5B`, ocean `#5BB6D6`, sky-gold `#FFD27A`. Warm shift vs Meadow's cool-noon palette.

**Enemy variant set:**

| Role | Rascal name | Notes |
|---|---|---|
| Swarmer | crab (sidles sideways) | new swarmer motion archetype |
| Swarmer | gull (low-flying) | first airborne-feeling swarmer |
| Swarmer | sand-puff (tiny dust-devil) | minion type summoned by Crab Captain boss |
| Swarmer | mosquito (reused from Honey Swamp art) | recolor reuse |
| Tank | bog-boar recolor (sun-bleached) | recolor of Meadow tank |
| Ranged | throw-frog (toy beanbag) | per `05-enemies.md` Honey Swamp ranged entry, re-used |
| Elite | Giant Bumble recolor → Big Hermit Crab | recolor + accessory swap |

**Hero prop:** palm / hut / coconut pile. The coconut pile is destructible (cosmetic-only — drops a single Carrot pickup).

**Boss:** Crab Captain (see `07-bosses.md`). Sweeps with one giant pincer, summons sand-puffs.

**Unlock condition:** Complete Meadow 3 times (any character). Gameplay-gated, never paywalled.

**Average run target time:** 7-10 minutes.

**Difficulty notes:** ~10% harder than Meadow per balance-engineer's TTK ladder. The single new hazard (sand traps) teaches the player to **read the ground**, which is the prerequisite skill for Cavern's stalactite zones and Snow's ice slides. Beach swarmers move sideways and at varied speeds — the player who relied on straight-line dodge in Meadow has to widen their footwork.

---

## Forest

**Theme:** Dappled light through dense canopy. Ancient oaks, log bridges, mushroom rings, fallen branches. The carrots have rolled into the underbrush — the bunny goes hunting.

**Mood:** Cool shade, the rustle of leaves, the feeling of being slightly watched but not stalked. Reference `narrative/03-biome-flavor.md` — voice: "the back-garden adventure where you're sure something's nesting in the bush." Scamps are mischievous, not malevolent.

**Hazards:**
- **Root snares** — 0.8-unit-radius circles, visible as a knot of brown vine. On contact, root the player for **0.4 s** (cannot move; can still auto-attack). 5 s cooldown per snare patch. Telegraph: vines ripple visibly 0.5 s before activation.
- **Low-vision underbrush** — patches of tall fern reduce the player's visual reveal radius from 8.0 units to 5.5 units while standing inside. Cosmetic-only mechanically, but cues the player to step out of cover before drafting.
- Hazards appear from minute 1 onward. Forest is the **first biome where the player can be hit by a hazard during normal play.**

**Tile palette:** Reference `07-art-bible/04-environment-style.md`. Base hex anchors: canopy-shade green `#3F6B3A`, sun-dapple green `#8FBE5A`, oak bark `#6B4F2A`, mushroom cap orange `#D87B36`, root brown `#4A3A24`, sky-through-canopy `#A8C9F0`. Higher contrast than Meadow; dappled-shadow tiles are core.

**Enemy variant set:**

| Role | Rascal name | Notes |
|---|---|---|
| Swarmer | nut-bug (acorn-shelled) | flora-flavor swarmer |
| Swarmer | shroom-bounce (mushroom-cap on legs) | bouncy movement variant |
| Swarmer | vine-trip (creeping vine head) | summoned by Mama Oak boss |
| Swarmer | worm (reused from Carrot Fields) | recolor reuse |
| Tank | sleepy-bear-cub | new tank skin |
| Tank | oak-walker (mossy boar recolor) | recolor |
| Ranged | acorn-slinger (squirrel with slingshot) | recolor of archer-mole |
| Elite | Treant-Sprout (small walking tree) | new elite skin |

**Hero prop:** ancient oak / log bridge / mushroom ring. The mushroom ring is a small AOE buff for the player when stood in (cosmetic-only at launch; +5% pickup magnet inside, 0% outside — the lightest possible biome-interactive mechanic to playtest before deeper interactions).

**Boss:** Mama Oak (see `07-bosses.md`). Roots the player in place; rains acorns.

**Unlock condition:** Reach any character Lv 8.

**Average run target time:** 7-10 minutes.

**Difficulty notes:** ~25% harder than Meadow. Root snares are the first **active punish** hazard — they don't just slow, they take a control input away. Balance-engineer to verify that no single snare can trigger a wipe on a Lv-8 character without compounding enemy pressure.

---

## Cavern

**Theme:** Dim, torchlit underground. Stalactites overhead, glow-mushrooms underfoot, gem outcrops embedded in the walls. The carrots have rolled into a sinkhole — the bunny goes spelunking.

**Mood:** Hushed, cool, faintly echoey. Reference `narrative/03-biome-flavor.md` — voice: "the basement-after-dinner expedition with a flashlight." Troublemakers skitter; the cavern is not haunted (banned per tone bible), it is just **a place where small things live**.

**Hazards:**
- **Stalactite drop zones** — 1.2-unit-radius circles, telegraphed by a falling-dust VFX **0.8 s** before impact. On hit: 25 hp damage + 0.3 s stagger. Drop zones appear from minute 2; never more than 2 active at once.
- **Reduced reveal radius** — Cavern's baseline reveal radius is **6.5 units** (vs 8.0 in other biomes). Torch-prop tiles within 4.0 units of the player extend reveal to 8.0 locally (cosmetic-only at launch; conveys the dim-cavern mood without making the player navigate by smell).
- Stalactite zones are the first **hazard with a damage value**, not just a movement penalty. This is the biome where the player learns to read the ceiling.

**Tile palette:** Reference `07-art-bible/04-environment-style.md`. Base hex anchors: cavern floor stone `#4A4550`, torchlight orange `#F5A04A`, glow-mushroom cyan `#5FE0D6`, gem-outcrop purple `#8E5BC9`, stalactite grey `#7A7480`, ambient dark `#2A2632`. Cavern is the **darkest palette** in the game — but no black, no skulls, no spider-web visuals per tone bible.

**Enemy variant set:**

| Role | Rascal name | Notes |
|---|---|---|
| Swarmer | bat-mini (small, fluttery) | new swarmer motion (hovering, not strict ground) |
| Swarmer | glow-bug (luminescent, glows on hit) | telegraphs its own hits via glow |
| Swarmer | rock-tumble (rolling pebble-creature) | reuse of Carrot Fields ladybug recolor |
| Swarmer | cave-slime (deep-blue recolor) | recolor |
| Tank | stone-ox (heavy, slow) | new tank skin or recolor of Sleepy Ox |
| Ranged | crystal-slinger (gem-headed mole) | recolor of archer-mole |
| Elite | Stalagmite-Walker (legs sprouting from a rock) | new elite skin |

**Hero prop:** stalactite / glow-mushroom / gem outcrop. Glow-mushrooms are interactive (cosmetic-only: pulse-glow when bunny is within 2 units, no mechanical effect). Gem outcrops drop 1 extra Carrot pickup when broken by player melee (cosmetic farming surface).

**Boss:** Sneaky Cave Mole (see `07-bosses.md`). Burrows, teleports, strikes from below.

**Unlock condition:** Defeat 2 biome bosses (any pair — Old Boar King + Crab Captain qualifies, as does Crab Captain + Mama Oak, etc.).

**Average run target time:** 7-10 minutes.

**Difficulty notes:** ~40% harder than Meadow. Stalactite damage + low reveal radius compound into the first biome where **positioning is a real consideration mid-fight**. The mole boss compounds this with burrow-teleport mind-games. Balance-engineer to verify that a freshly-unlocked Cavern run with no prior Cavern experience is winnable — but not easily — on the second attempt.

---

## Snow

**Theme:** Overcast bright, soft snowfall. Pine trees, ice formations, a snug igloo, frosted shrubs. The carrots have been frozen into snowballs — the bunny goes thawing.

**Mood:** The hush of a winter morning. Reference `narrative/03-biome-flavor.md` — voice: "the snow-day walk in your favorite coat." Snow-pests look chilled and grumpy, not feral. The danger is the **cold**, not the bite.

**Hazards:**
- **Ice slides** — large patches of glossy ice tile, visible as a different ground texture. Player movement on ice has **0.4 s of drift** after the joystick releases (vs 0.0 s on grass/sand/stone). Cosmetic for new players (annoying); a real consideration when chained with a stalactite zone or enemy density.
- **Cold-tick** — in **open patches** between igloo and pine prop coverage, a light DOT applies: **2 hp/second**. Stepping within 3.0 units of an igloo, pine, or campfire prop suppresses the tick for 4 seconds after exit. Encourages prop-aware pathing.
- **Snowdrift** — a thicker variant of sand-trap (Beach hazard): movespeed × 0.55 inside, telegraph is a slow-falling-snow VFX 0.5 s before the patch settles.
- Snow combines all prior hazard *classes* (slowdown, drift, damage-over-time, prop-suppression). It is the **graduation biome**.

**Tile palette:** Reference `07-art-bible/04-environment-style.md`. Base hex anchors: snow white `#F4F8FA`, shadow-blue `#B5C6D8`, pine green `#3F6B5C`, ice cyan `#9FE0E8`, igloo block `#D6DCE0`, overcast-sky `#C5CDD2`. Brightest overall lightness in the game — overcast-bright, no harsh sun, no nighttime.

**Enemy variant set:**

| Role | Rascal name | Notes |
|---|---|---|
| Swarmer | ice-puff (snowy dust-devil cousin of Beach sand-puff) | recolor |
| Swarmer | snow-rat (small, scampering) | new swarmer skin |
| Swarmer | frost-bee (Honey Swamp bee recolor) | recolor |
| Swarmer | penguin-chick (reused per `05-enemies.md` Frost Burrow entry) | reuse |
| Tank | yak (heavy, slow, fluffy) | per existing `05-enemies.md` Frost Burrow tank entry |
| Tank | walrus | per existing taxonomy |
| Ranged | snowball-mole | per existing taxonomy |
| Elite | Snow Yeti-Cub | per existing taxonomy |

**Hero prop:** pine / ice formation / igloo. Igloo is the **first sheltering prop** in the game — bunny gains 4 s of cold-tick suppression on exit. Pines provide a smaller suppression aura (2 s). Ice formations are decorative-only (no shelter; visual reward, not mechanical).

**Boss:** Big Snow-yeti (see `07-bosses.md`). Ice-stomp shockwaves + cold-aura that doubles cold-tick around the boss.

**Unlock condition:** Defeat 3 biome bosses (any trio).

**Average run target time:** 7-10 minutes.

**Difficulty notes:** ~60% harder than Meadow. Snow is the **launch endgame biome.** A player who can clear Snow consistently on a Lv-15+ character is ready for the post-launch rune system and the planned dungeon-rush event. Balance-engineer treats Snow as the upper bound for the launch TTK ladder; anything harder waits for post-launch content.

---

## Cross-references

- Biome **unlock conditions** source of truth: `02-meta-loop.md` (this doc mirrors with one calibration adjustment — Beach's "complete Meadow 3 times" is new and overrides the meta-loop's "Honey Swamp → reach Lv 5" entry; meta-loop to be reconciled in the next pass).
- Per-biome **enemy variant bank** source of truth: `05-enemies.md` per-biome variant table.
- Per-biome **boss spec** source of truth: `07-bosses.md`.
- Per-biome **palette** source of truth: `07-art-bible/04-environment-style.md` (art-director to author).
- Per-biome **mood paragraph** source of truth: `narrative/03-biome-flavor.md` (narrative-designer to author; tone bible voice).
- **Hazard tuning numbers** (slow %, root duration, DOT rate, ice drift, stalactite damage) go to `data/balance/biomes.json` (balance-engineer owns).
- **Tile palette + hero prop modeling**: Quaternius CC0 kit-bash + Blender recolor pipeline per `core/docs/asset-policy.md`.
