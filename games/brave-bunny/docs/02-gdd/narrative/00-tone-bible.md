# Tone Bible — Brave Bunny

> Canonical voice reference for every visible string in the game: UI buttons, modals, boss intros, level-up flavor, run-end summaries, share cards, push copy. Owner: narrative-designer. Consumers: ui-engineer, localizer, marketing.

## 1. Tonal anchor

Brave Bunny reads like **Cat Quest's dry wink, narrated with the understated calm of Slay-the-Spire, wrapped in the wordless charm of Crossy Road**. The bunny is brave and a little silly. The narrator is fond, never smug. We never raise our voice. We never look down on the player. The world is warm, the stakes are small, the carrots are real. Copy should feel like a friendly older sibling cheering you on at the kitchen table — not an esports caster, not a meme account.

## 2. Vocabulary do / don't

### DO — words that fit the register

| Theme | Use |
|---|---|
| Hero traits | brave, plucky, swift, sturdy, bright, gentle, cheery, sunny, fierce-but-fair |
| Combat verbs | bonk, bop, scatter, shoo, send packing, see off, tumble, biff |
| Failure | fainted, tuckered out, needs a nap, took a tumble, ran out of pep |
| Praise | well played, sturdy effort, nicely done, that's the spirit, plucky |
| Reward | carrot, basket, ribbon, gift, present, treat |

### DON'T — words that break the register

| Banned | Why |
|---|---|
| kill, slay, slaughter | Violent register; fails 7+ rating tone |
| blood, gore, corpse | Visual gore implication |
| death, die, dying, dead | Replace with "fainted" / "tuckered out" / "needs a nap" |
| dark, evil, demon, hell | Gothic register; wrong genre tone |
| destroy, annihilate, obliterate | Esports-shouty |
| epic, sick, lit, GOAT, based | Will age out; wrong meme generation |
| bro, dude, fam, bestie | Wrong register, doesn't translate |

### Enemy naming by biome

Use the **softest specific term** that still reads as adversary. Vary by biome to avoid repetition:

| Biome | Enemy term |
|---|---|
| Meadow | rascals |
| Forest | scamps |
| Bramble | troublemakers |
| Marsh | wee beasties |
| Snowfield | snow-pests |

## 3. Reading level

- **Target**: U.S. 8th grade. Flesch-Kincaid grade ≤ 8.5.
- **Sentence length**: ≤ 18 words. Most sentences should sit at 8–12.
- **Syllables**: prefer one-syllable verbs (bop, nap, win) over multi-syllable (defeat, recuperate).
- **No idioms that don't translate**. "Bite the dust," "kick the bucket," "piece of cake" — banned. TR and EN readers need the same mental picture.
- **No contractions in error states**; full words read clearer when translated.

## 4. Humor register

- **Dry wink, not loud joke.** One small smile per string, not a punchline.
- **Never punch down.** No string mocks the player, their skill, their wallet, or their region.
- **Fourth wall is for special occasions.** A nod every 30+ strings, not every other line. ("Worth a carrot," not "lol u died again.")
- **No pop-culture references.** No movies, no celebrities, no current memes. The game must read the same in 2026 and 2030.
- **No emoji in copy.** Visual icons are the art team's job; emoji age and re-render unevenly across OS versions.

## 5. Sample copy strings

All keys are written as `{KEY}` placeholders for the ui-engineer / localizer. Variables inside copy use `{UPPER_SNAKE}`.

```
{BTN_CONFIRM_START_RUN}:     "Off we go."
{BTN_CONFIRM_UPGRADE}:       "Take it."
{BTN_CONFIRM_QUIT_RUN}:      "Head home for now."

{MODAL_DISMISS_TIP}:         "Got it, thanks."
{MODAL_DISMISS_PATCH_NOTES}: "Sounds good."

{BOSS_INTRO_BOAR}:           "Old Boar's awake. Mind your tail."
{BOSS_INTRO_BADGER}:         "The Badger's grumpy today. Be nimble."
{BOSS_INTRO_OWL}:            "Big Owl's watching. Stay light on your paws."

{LEVEL_UP_PICK}:             "You feel pluckier. Choose your gift."
{LEVEL_UP_EVOLVE}:           "Two gifts want to become one. Pick the pair."

{RUN_END_WIN}:               "Whew. Worth a carrot."
{RUN_END_LOSE}:              "Tuckered out — but you banked {GOLD} carrots."

{SHARE_CARD}:                "{HERO} cleared the {BIOME} in {TIME}. Beat that?"

{HERO_REVIVE}:               "Bunny got knocked silly. Want a quick nap and one more try?"
{DAILY_STREAK_HOOK}:         "Three days running. Sturdy little adventurer."
{IAP_GIFT_BANNER}:           "A friendly sponsor sent you a gift."
```

## 6. Localization keys policy

- **All visible strings keyed** via `Loc("key")`. The ui-engineer enforces this at lint time; no raw English in `.uxml` or `.cs` UI files.
- **Order of locales**: TR first (priority market), EN second (global + PH at launch).
- **PH ships in English at launch** per positioning decision; Tagalog is post-launch.
- **No string concatenation** across translatable fragments. Whole sentences, with `{VARS}` inside.
- **TR register**: informal-but-friendly second person ("sen"), softening particles welcome ("biraz," "şöyle bir"). Avoid the formal "siz."

### Initial TR translations for the sample set

```
{BTN_CONFIRM_START_RUN}:     "Haydi başlayalım."
{BTN_CONFIRM_UPGRADE}:       "Aldım."
{BTN_CONFIRM_QUIT_RUN}:      "Şimdilik eve dönelim."

{MODAL_DISMISS_TIP}:         "Anladım, sağ ol."
{MODAL_DISMISS_PATCH_NOTES}: "Tamamdır."

{BOSS_INTRO_BOAR}:           "Koca Yaban uyandı. Kuyruğuna dikkat."
{BOSS_INTRO_BADGER}:         "Porsuk bugün huysuz. Çevik ol."
{BOSS_INTRO_OWL}:            "Koca Baykuş izliyor. Patilerin hafif olsun."

{LEVEL_UP_PICK}:             "Biraz daha cesur hissediyorsun. Hediyeni seç."
{LEVEL_UP_EVOLVE}:           "İki hediye birleşmek istiyor. Çifti seç."

{RUN_END_WIN}:               "Oh be. Bir havuca değdi."
{RUN_END_LOSE}:              "Yorulduk — ama {GOLD} havuç kasaya girdi."

{SHARE_CARD}:                "{HERO}, {BIOME} bölgesini {TIME} sürede temizledi. Geçebilir misin?"

{HERO_REVIVE}:               "Tavşan biraz sersemledi. Kısa bir şekerleme ve bir hak daha?"
{DAILY_STREAK_HOOK}:         "Üç gün üst üste. Sağlam küçük maceracı."
{IAP_GIFT_BANNER}:           "Dost bir sponsor sana hediye gönderdi."
```

## 7. Voice anti-patterns

Strings that would fail review:

- **"DESTROY YOUR ENEMIES!"** — shouty, banned verb, esports register.
- **"You have been slain."** — banned vocab; death framing.
- **"Lvl up bro 🔥💯"** — wrong register, emoji, ages out, untranslatable.
- **"Git gud."** — punches down at the player.
- **"Mom got you a present!"** — sounds gross and unsafe; use `{IAP_GIFT_BANNER}` above.
- **"Crush the demon horde in the dark forest of doom."** — five banned words in one sentence.
- **"Skill issue."** — punching down + meme that will age out.

---

**Owner**: narrative-designer · **Reviewers**: game-designer, ui-engineer, localizer · **Status**: v1 draft for vertical-slice gate
