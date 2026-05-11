# GDD 07 — Bosses

> The 5 launch bosses for Brave Bunny. The vertical slice ships **1 boss** (Old Boar King for Meadow). Sister docs: `06-biomes.md` (which biome each boss owns), `05-enemies.md` (boss role baseline — HP scaling, telegraph minimums), `narrative/00-tone-bible.md` (intro-line voice + banned-words list), `11-feel-pillars.md` (hitstop on boss damage events).

## Design philosophy

A boss is the **emotional cap** of a biome run. It must do four things:

1. **Be misunderstood, not evil.** Per tone bible, no skulls, no demons, no menace. Old Boar King is sleepy and grumpy because he was woken; Crab Captain is territorial about his beach; Mama Oak is protecting her acorns. The bunny is not a hero slaying monsters — the bunny is a brave little tourist who got in the way.
2. **Telegraph everything.** Per `05-enemies.md` boss baseline: every attack has a **≥ 0.8 s telegraph window** with VFX preview. No instant hits, no off-screen damage.
3. **Have 2-3 phases.** A phase transition is the boss noticing the bunny is serious. Phases must read as **escalation in pattern complexity**, not raw stat inflation.
4. **Leave an opening.** Every attack pattern has a per-attack opening — a window where the bunny can punish without trading. The opening duration is the **wind-down** in the catalog below.

## Boss baseline (per `05-enemies.md`)

| Field | Baseline value |
|---|---|
| HP minute-5 mid-boss | 4000 hp |
| HP minute-10 end-boss | 12000 hp |
| Contact damage | 35 hp |
| Ranged damage | 25 hp |
| AOE damage (inside marker) | 50 hp |
| Telegraph minimum | 0.8 s |
| Hitstop on boss-damaging-player events | 60 ms (per `11-feel-pillars.md`) |
| Drop table | 3 Soul Shards mid / 5 end + 1 guaranteed character-shard pull on end-boss + 100% chest |

Exact per-boss HP numbers and damage tuning live in `data/balance/bosses.json` (balance-engineer owns). This doc owns **patterns, phases, openings, and flavor**.

---

## 1. Old Boar King — Meadow

**Biome:** Meadow (vertical-slice boss — the only boss shipped at vertical-slice gate).

**Concept:** The Old Boar King has been napping under the lone oak for nine seasons. His tusks are bandaged because he chipped them on a hazelnut he was very fond of. He charges in straight lines because he has not yet remembered how to turn. He is not evil — he is sleepy, grumpy, and a little embarrassed that the bunny saw him snoring. He huffs, he stomps, and at 50% HP he discovers he can hop, which surprises him as much as it surprises the bunny.

**Silhouette:** Round-bellied boar with low-to-ground stance, two stubby curled tusks (bandaged), a wide-set sleepy-eyed face. From 32 px: a wide low oval with two small tusk-bumps. Reads as "boar" before "danger."

**Phase count:** 2.

**HP:** Baseline minutes per `05-enemies.md` boss tier — exact numbers in `data/balance/bosses.json`. Approximate: ~4000 hp total split 50/50 across phases.

**Movement pattern:** Phase 1 — slow plodding pacing in a 4-unit radius around the arena center, with telegraphed straight-line charges. Phase 2 — same plodding + a new hop-attack that arcs to the player's last position.

### Attack pattern catalog

#### Phase 1 (100% → 50% HP)

| Attack | Telegraph (frames @ 60 fps) | Wind-down (frames) | Damage range | Opening for player |
|---|---|---|---|---|
| **Plod-slam** (contact slam during pacing) | 24 frames (0.4 s) — body-tense + grunt | 18 frames (0.3 s) — recovery huff | 35 hp contact | 0.3 s wind-down |
| **Tusk-charge** (straight-line dash, 4 units/sec, 1.5 s total) | 48 frames (0.8 s) — head-lower + sand-paw + grunt | 36 frames (0.6 s) — skid + dazed wobble | 50 hp on collision | 0.6 s wind-down + can lure into well/tree for self-stagger |
| **Snore-shockwave** (boss falls asleep momentarily, emits a small 2.0-unit AOE pulse and recovers) | 60 frames (1.0 s) — long yawn + Z's VFX | 60 frames (1.0 s) — wake-up shake | 25 hp inside 2.0 unit AOE | 1.0 s wind-down — **largest opening in fight**, this is the DPS window |

#### Phase 2 (50% → 0% HP)

| Attack | Telegraph (frames) | Wind-down (frames) | Damage range | Opening for player |
|---|---|---|---|---|
| **Plod-slam** | unchanged | unchanged | unchanged | unchanged |
| **Tusk-charge** | shortened to 36 frames (0.6 s) — still > minimum | unchanged | unchanged | shorter telegraph; same opening |
| **Hop-stomp** (NEW — boss leaps in a low arc to player's last position, 1.0 s air-time, lands with 1.5-unit AOE) | 48 frames (0.8 s) — squat-down + leaf-puff | 30 frames (0.5 s) — landing wobble | 50 hp AOE inside 1.5 units | 0.5 s wind-down — punish during landing recovery |
| **Snore-shockwave** | unchanged | unchanged | unchanged | unchanged — still the DPS window |

### Phase transitions

| Phase | Trigger | What changes |
|---|---|---|
| 1 → 2 | HP drops below 50% | Boar discovers he can hop. Plays a brief surprised-snort animation (1.0 s, invincible during, no attack). Hop-stomp enters rotation. Tusk-charge telegraph shortens from 0.8 s → 0.6 s. Snore-shockwave frequency increases ~25%. |

### Arena suggestions

- **Size:** 12×12 unit square, slight grass-bordered framing.
- **Traversal:** Open ground; the lone oak prop sits in one corner as a positional anchor. Player can use the tree as a **line-of-sight breaker for the tusk-charge** — the charge does not retarget mid-attack; if the bunny ducks behind the oak, the boar plows into it and self-staggers for 1.5 s (a designer-rewarded opening for clever play).
- **Hazards:** None (Meadow is hazard-free per `06-biomes.md`). The arena itself is the test.

### Cartoon flavor / tone

The Old Boar King looks like a plush toy that has just been woken up from a long nap. No fangs, no glowing eyes, no aggressive posturing. His tusks have soft yellow bandages. His snore plays a small horn-toot SFX, not a roar. The death animation is a slow sit-down, a final yawn, and a soft drop onto his side with three sleep-Z's floating up. The bunny is meant to feel like the boar will be fine in the morning.

### Intro line

```
{BOSS_INTRO_BOAR}: "Old Boar's awake. Mind your tail."
```

(Already canonicalized in `narrative/00-tone-bible.md` §5; this doc references, does not redefine.)

---

## 2. Crab Captain — Beach

**Biome:** Beach.

**Concept:** The Crab Captain has been guarding this stretch of shoreline since long before the bunny was born. He wears a small bottle-cap as a hat (cosmetic-only; rumored to be a legacy of a long-lost shipwreck). He is not malicious — he is **territorial about his sand**, particularly the patch where his sand-puff children play. When the bunny gets too close, he sweeps a giant pincer to shoo. He summons sand-puffs because he genuinely believes they can help, even though they are tiny and confused.

**Silhouette:** One oversized pincer + one regular pincer; low-slung shell with a bottle-cap atop; six legs in a wide stance. From 32 px: a wide-spread shape with one disproportionate forward arm.

**Phase count:** 3.

**HP:** Per `data/balance/bosses.json` (~5500 hp split 40/35/25 across phases — balance-engineer to finalize).

**Movement pattern:** Side-scuttling (true crab-walk: lateral motion only, not turning to face) within a sand arena. In phase 3, gains a forward dash that breaks the lateral rule.

### Attack pattern catalog

#### Phase 1 (100% → 66% HP)

| Attack | Telegraph (frames) | Wind-down (frames) | Damage range | Opening for player |
|---|---|---|---|---|
| **Pincer-sweep** (the big pincer arcs through a 90° front-cone) | 54 frames (0.9 s) — pincer rears back + shadow on sand | 30 frames (0.5 s) — pincer hangs in dirt | 40 hp inside cone | 0.5 s while pincer is down — flank from the small-pincer side |
| **Skitter-strafe** (rapid lateral move, no attack) | n/a (movement, not attack) | n/a | 0 | reposition opportunity for bunny too |
| **Bubble-spit** (a small bubble lobs in a 0.6 s arc toward player's predicted position) | 36 frames (0.6 s) — captain inhales | 18 frames (0.3 s) — exhale puff | 18 hp on hit, AOE 1.0 unit | 0.3 s wind-down between spits |

#### Phase 2 (66% → 33% HP)

| Attack | Telegraph (frames) | Wind-down (frames) | Damage range | Opening for player |
|---|---|---|---|---|
| **Pincer-sweep** | unchanged | unchanged | unchanged | unchanged |
| **Sand-puff summon** (NEW — captain stomps; 3 sand-puff swarmers spawn at his feet, 1.0 s vulnerability during summon) | 48 frames (0.8 s) — high stomp wind-up | 60 frames (1.0 s) — summon-flop | 0 hp from boss, sand-puffs use swarmer contact | **1.0 s** wind-down — biggest DPS window of fight |
| **Bubble-spit** | upgraded to triple-shot (3 bubbles fanned in a 45° spread) | unchanged | 18 hp per bubble | dodge sideways, opening between volleys |

#### Phase 3 (33% → 0% HP)

| Attack | Telegraph (frames) | Wind-down (frames) | Damage range | Opening for player |
|---|---|---|---|---|
| **Pincer-slam** (upgraded sweep — now slams ground for an AOE pulse) | 60 frames (1.0 s) — pincer overhead | 36 frames (0.6 s) — pincer stuck | 50 hp AOE 2.0 unit | 0.6 s while pincer is stuck — guaranteed hit window |
| **Forward dash** (NEW — breaks lateral rule; charges 6 units forward in 0.8 s) | 48 frames (0.8 s) — wide leg-spread + sand-spray | 24 frames (0.4 s) — wall-skid stagger | 50 hp on collision | 0.4 s post-dash; if dashed into edge prop, +1.0 s self-stagger |
| **Sand-puff summon** | unchanged | unchanged | unchanged | unchanged |

### Phase transitions

| Phase | Trigger | What changes |
|---|---|---|
| 1 → 2 | HP < 66% | Sand-puff summon enters rotation. Bubble-spit upgrades to triple-shot. |
| 2 → 3 | HP < 33% | Pincer-sweep becomes Pincer-slam (AOE). Forward dash enters rotation. Skitter-strafe speed +25%. |

### Arena suggestions

- **Size:** 14×14 unit beach square, ocean on one edge (impassable — visual boundary), palm prop in one corner.
- **Traversal:** Open sand with **2 always-present sand-trap patches** (per Beach hazard rules, `06-biomes.md`). Captain ignores his own sand traps; bunny does not.
- **Hazards:** Sand traps active throughout fight. Coconut pile prop in opposite corner — breakable by bunny melee, drops 1 Carrot pickup.

### Cartoon flavor / tone

Bottle-cap hat. Eyes on stalks that look slightly indignant. The sand-puffs are clearly his kids — they look up at him when summoned. On final HP gate, he sits down with a "harrumph" SFX and the bottle-cap tips comically forward. No claws cut anything; the pincer is rounded and toy-like.

### Intro line

```
{BOSS_INTRO_CRAB}: "The Captain's grumpy about his sand. Side-step him."
```

(To be canonicalized in `narrative/00-tone-bible.md` §5 by narrative-designer.)

---

## 3. Mama Oak — Forest

**Biome:** Forest.

**Concept:** Mama Oak is the oldest tree in the forest and the gentlest soul on the path. She has been growing acorns for two hundred seasons and she does not appreciate strangers near her seedlings. When the bunny wanders into her glade, she politely requests they leave by gently snaring their feet with roots. When the bunny does not leave, she politely requests again with more roots. By phase 3 she is dropping acorns at speed, which is the most aggressive thing she knows how to do.

**Silhouette:** Wide trunk + sprawling canopy + two glowing-knot eyes high in the trunk + two root-arms anchored at the base. Stationary boss — does not move from arena center. From 32 px: a wide tree shape with two small face-knots.

**Phase count:** 3.

**HP:** Per `data/balance/bosses.json` (~6000 hp split 33/33/34 — stationary boss compensates with sustained pressure).

**Movement pattern:** Stationary (cannot reposition). The arena rotates around her — the bunny circles, she swivels her face-knots to track, her roots emerge at distance.

### Attack pattern catalog

#### Phase 1 (100% → 66% HP)

| Attack | Telegraph (frames) | Wind-down (frames) | Damage range | Opening for player |
|---|---|---|---|---|
| **Root-spike** (a single root erupts at player's last position, 0.7 s after telegraph) | 42 frames (0.7 s) — ground-crack VFX | 18 frames (0.3 s) — root retraction | 30 hp inside 1.0 unit | dodge the marker, hit the trunk during retraction |
| **Acorn-toss** (lobs a single acorn in a 0.8 s arc) | 36 frames (0.6 s) — branch wind-back | 24 frames (0.4 s) — branch resets | 22 hp on hit, AOE 0.8 unit | 0.4 s between tosses |
| **Leaf-flurry** (a 1.5-unit cone of leaves blows from her canopy outward, gentle pushback) | 30 frames (0.5 s) — canopy rustle | 30 frames (0.5 s) — settle | 10 hp inside cone + pushback 2 units | 0.5 s settle window — push the bunny away from the trunk; reposition timer |

#### Phase 2 (66% → 33% HP)

| Attack | Telegraph (frames) | Wind-down (frames) | Damage range | Opening for player |
|---|---|---|---|---|
| **Root-snare patch** (NEW — 3 stationary root-trap circles appear, lasting 8 s; on contact, root player for 0.6 s) | 60 frames (1.0 s) — three ground-crack VFX | n/a (patches persist) | 0 instant hp; root sets up follow-up | navigate around patches; **patches do not damage boss either**, can be used as cover from acorn arc |
| **Acorn-toss** | upgraded to double-toss (2 acorns, 0.4 s apart, slight spread) | unchanged | unchanged | dodge laterally |
| **Root-spike** | unchanged | unchanged | unchanged | unchanged |

#### Phase 3 (33% → 0% HP)

| Attack | Telegraph (frames) | Wind-down (frames) | Damage range | Opening for player |
|---|---|---|---|---|
| **Acorn-rain** (NEW — full canopy shakes; 8 acorns drop across arena over 3.0 s, telegraphed individually by shadow markers 0.6 s before each) | 60 frames (1.0 s) — canopy shake + leaf shower | 60 frames (1.0 s) — canopy droop | 22 hp per hit | 1.0 s droop window after rain ends; **DPS window** |
| **Root-snare patch** | upgraded to 5 patches | unchanged | unchanged | tighter pathing required |
| **Root-spike** | upgraded to triple-spike (3 spikes in sequence at predicted positions) | unchanged | unchanged | predict and reposition |
| **Leaf-flurry** | removed in phase 3 — Mama Oak has stopped trying to be polite | n/a | n/a | n/a |

### Phase transitions

| Phase | Trigger | What changes |
|---|---|---|
| 1 → 2 | HP < 66% | Root-snare patches enter rotation. Acorn-toss becomes double-toss. Leaf-flurry continues. |
| 2 → 3 | HP < 33% | Acorn-rain enters rotation (the big set-piece attack). Root-spike becomes triple-spike. Root-snare patches up to 5. Leaf-flurry removed. |

### Arena suggestions

- **Size:** 14×14 unit forest clearing, dappled-light tiles, fallen-log border (cosmetic boundary).
- **Traversal:** Mama Oak in dead center. Bunny must circle continuously to avoid root-spike predictions. Two mushroom-ring props at opposite sides of the arena provide +5% magnet zones (per `06-biomes.md` Forest hero prop note).
- **Hazards:** Forest's root-snare ambient hazard is **suppressed during boss fight** (boss owns the snare mechanic this fight, ambient would double-punish). Low-vision underbrush patches remain at arena edges.

### Cartoon flavor / tone

Mama Oak has a kindly, wrinkled-bark face. The face-knot eyes are warm amber, not glowing-red. Her roots are smooth and visible vines, not gnarled or thorny. Acorns are oversized and round, with little hats. On final HP gate, she lets out a long sigh, her canopy droops, and a single acorn pops out and rolls to the bunny's feet (a literal Carrot pickup). She is fine — she is just tired.

### Intro line

```
{BOSS_INTRO_OAK}: "Mama Oak guards her acorns. Tread softly."
```

(To be canonicalized in `narrative/00-tone-bible.md` §5.)

---

## 4. Sneaky Cave Mole — Cavern

**Biome:** Cavern.

**Concept:** The Sneaky Cave Mole has been digging in this cavern for as long as anyone can remember, which for a mole is about three weeks. He does not like to be seen — being seen is the worst thing that can happen to a mole — so he pops up, jabs with his shovel, and pops back down. He is not menacing; he is **shy and twitchy**, and the bunny scares him as much as he scares the bunny. The fight is two animals taking turns being startled.

**Silhouette:** Round body, oversized shovel-paws, tiny dark glasses (he says they help him see in the light — they do not). From 32 px: a teardrop shape with two visible paws at the front.

**Phase count:** 2 (the simplest pattern of the 5 — to compensate for the burrow/teleport mechanic which is mechanically complex enough on its own).

**HP:** Per `data/balance/bosses.json` (~5500 hp split 60/40 — phase 1 longer because teleport defends his HP).

**Movement pattern:** **Burrow/teleport.** The mole spends ~60% of fight underground. Surfaces in telegraphed dig-mound spots, attacks, burrows again. Cannot be hit while underground.

### Attack pattern catalog

#### Phase 1 (100% → 50% HP)

| Attack | Telegraph (frames) | Wind-down (frames) | Damage range | Opening for player |
|---|---|---|---|---|
| **Dig-strike** (mole pops up at a telegraphed mound, jabs with shovel-paw in a 1.0-unit forward cone, burrows back down 0.6 s later) | 36 frames (0.6 s) — dirt-mound rises at target spot | 36 frames (0.6 s) — exposed atop ground | 35 hp inside cone | **0.6 s exposed window** — primary DPS opportunity; bunny stands ready next to the mound |
| **Dirt-spray** (mole surfaces briefly, spits a 2.0-unit-radius dirt cloud, burrows in 0.4 s) | 30 frames (0.5 s) — mound + cough SFX | 24 frames (0.4 s) — exposed | 18 hp + 0.5 s vision-obscure inside cloud | 0.4 s exposed; vision-obscure adds difficulty mid-fight |
| **Surface-skitter** (mole emerges and scampers in a visible 4-unit line, then re-burrows; not an attack, but exposes the boss) | 24 frames (0.4 s) — mound | 60 frames (1.0 s) — scampering visible | 0 (contact damages bunny at 20 hp if touched) | full 1.0 s window — pursuit opportunity but contact-risky |

#### Phase 2 (50% → 0% HP)

| Attack | Telegraph (frames) | Wind-down (frames) | Damage range | Opening for player |
|---|---|---|---|---|
| **Dig-strike** | unchanged | shortened to 24 frames (0.4 s) exposed | unchanged | tighter window, faster reflex required |
| **Triple-mound** (NEW — three mounds rise simultaneously; one is the real mole, two are decoys, all telegraphed identically) | 48 frames (0.8 s) — three mounds, identical | 36 frames (0.6 s) — real mole strikes from one) | 35 hp from real mound only | guess correctly = full 0.6 s window; guess wrong = take the strike |
| **Dirt-spray** | upgraded to double-spray (two surfacings, 0.8 s apart, at different positions) | unchanged | unchanged | reposition between sprays |
| **Stalactite-shake** (NEW — mole stays underground, shakes ceiling; 3 stalactite drop zones telegraph and fall over 2.0 s) | 48 frames (0.8 s) per stalactite zone | n/a | 25 hp + 0.3 s stagger per zone (per Cavern hazard rules) | dodge zones; **boss is not exposed during this attack** — pure dodge phase |

### Phase transitions

| Phase | Trigger | What changes |
|---|---|---|
| 1 → 2 | HP < 50% | Triple-mound enters rotation (mind-game decoy mechanic). Stalactite-shake enters rotation. Dig-strike exposed window shortens 0.6 → 0.4 s. Surface-skitter removed (replaced by triple-mound). |

### Arena suggestions

- **Size:** 12×12 unit cavern chamber, low-light per Cavern reveal radius (6.5 units baseline, 4 torch-props at corners locally extend to 8.0 units).
- **Traversal:** Open dirt floor, four torch-props in corners (torches do not block movement; bunny pathing around them is encouraged for reveal extension). Two stalactite-drop zones can trigger during phase 2 ambient.
- **Hazards:** Cavern's stalactite ambient hazard is **active during the fight** (boss riffs on it during phase 2's stalactite-shake; the ambient version still triggers between boss patterns). Low-reveal radius active throughout.

### Cartoon flavor / tone

Round chubby mole, tiny dark glasses askew, dirt always falling off his paws. Burrow VFX is a goofy dirt-fountain, not a sinister shadow. He emits a small "eep!" SFX every time he surfaces. On final HP gate, he sits up on his haunches, removes his glasses to clean them, blinks twice in confusion, and burrows down slowly while waving a small white flag. Not menacing — bashful.

### Intro line

```
{BOSS_INTRO_MOLE}: "Something's tunneling. Keep your ears up."
```

(To be canonicalized in `narrative/00-tone-bible.md` §5.)

---

## 5. Big Snow-yeti — Snow

**Biome:** Snow.

**Concept:** The Big Snow-yeti is enormous, fluffy, and **profoundly cold**. He is so cold that the air around him is colder; he is so cold that he does not remember being warm. He does not hate the bunny — he simply does not believe the bunny will survive his neighborhood, and is sad about that in advance. He stomps not in anger but in **shivers**. His cold-aura is involuntary. The fight is the largest, slowest, gentlest tragedy in the game.

**Silhouette:** Tall, broad, fluffy white silhouette with a small dark face high in the chest area; oversized paws; no visible weapons. From 32 px: a wide-shouldered triangle tapering to small feet.

**Phase count:** 3.

**HP:** Per `data/balance/bosses.json` (~7500 hp split 40/30/30 — launch endgame boss, the highest total).

**Movement pattern:** Slow plodding with telegraphed ice-stomps. In phase 2, gains a brief charge. In phase 3, the cold-aura expands and the arena itself becomes hostile.

### Attack pattern catalog

#### Phase 1 (100% → 66% HP)

| Attack | Telegraph (frames) | Wind-down (frames) | Damage range | Opening for player |
|---|---|---|---|---|
| **Ice-stomp** (yeti stomps; a 3.0-unit shockwave radiates from him over 0.6 s; ice patches form where the wave passes, persisting 6 s) | 48 frames (0.8 s) — high lift of paw | 30 frames (0.5 s) — paw-down recovery | 40 hp inside expanding wave; ice patches enforce 0.4 s drift | 0.5 s recovery — DPS during, but mind the new ice patches |
| **Snowball-toss** (yeti lobs a large snowball in a 1.0 s arc at player's predicted position) | 36 frames (0.6 s) — wind-up | 24 frames (0.4 s) — arm reset | 28 hp + 1.0 s slow (movespeed × 0.6) | 0.4 s arm-reset window |
| **Cold-aura passive** (1.5 hp/sec DOT inside 4.0 units of boss, always-on) | n/a (passive) | n/a | 1.5 hp/sec | maintain distance; aura is **double Snow biome cold-tick** |

#### Phase 2 (66% → 33% HP)

| Attack | Telegraph (frames) | Wind-down (frames) | Damage range | Opening for player |
|---|---|---|---|---|
| **Ice-stomp** | upgraded to 4.0-unit shockwave | unchanged | unchanged | tighter dodge window |
| **Snowball-volley** (NEW — yeti lobs 3 snowballs in a fan, 0.5 s apart) | 48 frames (0.8 s) — wind-up | 30 frames (0.5 s) — reset | 28 hp each | dodge laterally between volleys |
| **Frost-charge** (NEW — yeti drops to all fours and charges 5 units forward in 1.0 s; trails ice patches behind him) | 60 frames (1.0 s) — squat-down + frost-puff | 36 frames (0.6 s) — slide-stop | 50 hp on collision | 0.6 s slide-stop — guaranteed punish if positioned right |
| **Cold-aura passive** | expanded to 5.0 unit radius | n/a | 2.0 hp/sec | wider safe-distance requirement |

#### Phase 3 (33% → 0% HP)

| Attack | Telegraph (frames) | Wind-down (frames) | Damage range | Opening for player |
|---|---|---|---|---|
| **Ice-stomp-quake** (upgraded ice-stomp — full 6.0-unit shockwave + 4 secondary ice patches scatter at preset positions) | 60 frames (1.0 s) — both paws lift | 42 frames (0.7 s) — heavy recovery | 50 hp AOE | 0.7 s recovery — biggest DPS window |
| **Snowball-volley** | upgraded to 5-shot fan | unchanged | unchanged | reposition during volley |
| **Frost-charge** | unchanged | unchanged | unchanged | unchanged |
| **Blizzard-howl** (NEW — yeti howls; for 4.0 s the entire arena cold-tick rate doubles to 4.0 hp/sec, suppressed only at igloo prop) | 48 frames (0.8 s) — head-tilt-back + intake breath | 60 frames (1.0 s) — howl sustains | n/a direct, but ambient DOT spike | retreat to igloo during howl; full 1.0 s window after howl ends |

### Phase transitions

| Phase | Trigger | What changes |
|---|---|---|
| 1 → 2 | HP < 66% | Snowball-volley + Frost-charge enter rotation. Ice-stomp wave +1.0 unit. Cold-aura radius +1.0 unit. |
| 2 → 3 | HP < 33% | Ice-stomp becomes ice-stomp-quake. Snowball-volley becomes 5-shot. Blizzard-howl enters rotation. The arena itself is now actively trying to freeze the bunny. |

### Arena suggestions

- **Size:** 16×16 unit snow plain (largest arena in launch roster — needed for ice-stomp-quake and snowball-volley spread).
- **Traversal:** One **igloo prop** in a back corner (Snow biome's cold-tick-suppression shelter per `06-biomes.md`; critical during phase 3 blizzard-howl). Two pine props on opposite corners (smaller suppression auras). One frozen-pond patch in the arena center (cosmetic ice — always 0.4 s drift, even outside boss-laid ice patches).
- **Hazards:** All Snow biome ambient hazards remain **active** (ice slides, cold-tick in open patches, snowdrift). The yeti adds his own ice patches and amplifies cold-tick. This is the only fight where ambient + boss hazards layer in full.

### Cartoon flavor / tone

Fluffy white shaggy fur, small dark Charlie-Brown-style face, oversized soft paws (no claws). His breath is visible (small puffs of pale cyan steam). Stomps make a soft *thump-crunch* SFX, not an earthquake roar. On final HP gate, he sits down in the snow, his shivers slow, and a tiny snowflake VFX lands on his nose — he goes cross-eyed looking at it, then gives a slow contented exhale and curls up. He is going for a nap, not dying. The bunny pats his paw.

### Intro line

```
{BOSS_INTRO_YETI}: "Big Yeti's shivering. Keep moving, stay warm."
```

(To be canonicalized in `narrative/00-tone-bible.md` §5.)

---

## Cross-references

- Boss **HP / damage raw numbers** source of truth: `data/balance/bosses.json` (balance-engineer owns; this doc owns patterns, phases, telegraphs).
- Boss **role baseline** (telegraph minimums, hitstop, drops): `05-enemies.md` §Boss row.
- Boss **biome assignment**: `06-biomes.md`.
- Boss **intro lines + voice register**: `narrative/00-tone-bible.md` (lines added to §5 by narrative-designer).
- Boss **arena VFX + telegraph art** spec: `07-art-bible/05-vfx-style.md` (art-director to author).
- Boss **hitstop / kill-feel polish**: `11-feel-pillars.md` pillar 4.
- Boss **drop tables** (Soul Shards, chest, character-shard pull): `data/balance/drops.json` + `02-meta-loop.md` currency model.
- Boss **mesh sourcing**: Quaternius Animated Animals (boar, crab, mole, yeti) + Blender custom for Mama Oak (no animal-pack tree-creature; Blender kitbash flagged in `13-risks-and-cuts.md`).
