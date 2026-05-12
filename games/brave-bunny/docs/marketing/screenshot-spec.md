# Screenshot Specification — Brave Bunny: Survivors

> Owner: art-director (spec author). Capture executor: future asset-curator / gameplay-engineer dispatch. Composition overlay executor: future Canva-MCP / Figma-MCP dispatch. Cross-refs: `07-art-bible/00-style-overview.md` (saturation budget), `07-art-bible/01-color-palette.md` (per-biome hex), `02-gdd/00-overview.md` (5 pillars to feature), `02-gdd/03-characters.md` (Bunny is hero), `02-gdd/04-weapons.md` (vertical-slice 3), `02-gdd/06-biomes.md` (Meadow anchor), `02-gdd/narrative/00-tone-bible.md` (voice), `decisions/0016-app-store-display-name.md` (store name is "Brave Bunny: Survivors").
>
> **App Store display name (locked):** `Brave Bunny: Survivors` — never bare "Brave Bunny" in screenshot copy.
> **Tone bar (locked):** Cat Quest's dry wink, kitchen-table cheer. Banned: kill / slay / die / epic / lol / emoji-in-copy. Sentences ≤ 18 words, most 8–12. (See `narrative/00-tone-bible.md`.)
> **iPad scope:** **NOT a launch target.** `GAME.md :: platforms` = `[ios, android]`; no iPad device listed in `target_devices`. Skip iPad screenshot set; document only iPhone classes below.

## 1. Device matrix (Apple iOS — iPhone only, per scope)

Per Apple App Store Connect 2026 screenshot specifications (canonical link: <https://developer.apple.com/help/app-store-connect/reference/screenshot-specifications>). Apple's policy: **at least 1 screenshot per device class**; practical convention is **5 per class** (one hero + four feature beats). Brave Bunny: Survivors ships 5.

| # | Device class | Devices that drive this class | Required portrait resolution | Notes |
|---|---|---|---|---|
| A | **6.7" / 6.9" iPhone** | iPhone 14 Pro Max, 15 Pro Max, 16 Pro Max, 16 Plus | **1290 × 2796 px** | Primary modern class. Dynamic Island top safe area = top 132 px. |
| B | **6.5" iPhone** | iPhone 11 Pro Max, 12 Pro Max, 13 Pro Max, 14 Plus, 15 Plus | **1284 × 2778 px** | Accepted as substitute for 5.5" class per Apple's 2024+ relaxation; we still ship the 5.5" set explicitly for legacy submission acceptance. Notch top safe area = top 88 px. |
| C | **5.5" iPhone** | iPhone 8 Plus and earlier large phones | **1242 × 2208 px** | Still required at submission time **unless** the binary's `MinimumOSVersion` excludes iOS 12 / iPhone 8 Plus. We do not assume that exclusion at vertical-slice gate. Confirm with build-engineer at submission. No notch — full top edge is composition-usable. |

> Drop note: if at submission time build-engineer confirms `MinimumOSVersion ≥ iOS 17`, Apple now accepts only the 6.7" set as canonical, and class B/C become optional. Document the call in an ADR at that time; do **not** delete the 5.5" comps preemptively.

**Asset format:** PNG, sRGB, 72 dpi, no transparency, no rounded corners (Apple adds them). Filename convention: `bb-{class}-{n}-{locale}.png` (e.g. `bb-A-1-en.png`).

## 2. Five-screenshot narrative (carousel order)

The first screenshot is the **hero** — it is the only frame most browsers see in the App Store search-result carousel. The next four sell **scope** (depth, roster, biomes, progression). All five share visual DNA so the carousel reads as one game, not five.

| # | Theme | Composition | Headline (≤7 words) | Subhead (≤12 words) | Background biome | Featured weapon(s) | Visible enemy count |
|---|---|---|---|---|---|---|---|
| 1 | **Hero — action density** | Bunny dead-center, low 3/4 angle. Mid-air Carrot Boomerang arcing back. Two hit-flash VFX rings (Hero Highlight `#FF6B6B`) on flanking swarmers. Pickup-gold XP gem trail spiraling toward player. Action freeze-frame; one Daisy Mine wobble in foreground left. | "Hop. Swarm. Survive." | "An 8-minute roguelite with a smile on every frame." | Meadow (Carrot Fields — Meadow Lime `#A8D86B`, Sky Soft `#BEE3F0`) | Carrot Boomerang (primary, mid-arc), Daisy Mine (foreground wobble) | 18–24 (dense without obscuring Bunny silhouette) |
| 2 | **Feature: Auto-attack + evolutions** | Bunny mid-step toward upper-right; Sunbeam beam locked on a Sleepy Boar tank at frame edge; small UI floater showing weapon level-up arrow `L4 → L5` and evolution-ready icon glow on Carrot Boomerang slot. Bottom 18% of frame is HUD weapon tray. | "One thumb. Big builds." | "Auto-attack, evolve, and watch a build come alive." | Meadow | Sunbeam (locked beam — visible width), Carrot Boomerang (HUD evo-ready glow) | 10–14 (clear, not cluttered — the build, not the swarm, is the read) |
| 3 | **Feature: Hero roster** | Bunny center-stage in run, **3 silhouetted hero busts** along bottom strip (Tortoise, Fox, Owl — picked for max silhouette contrast). Locked-padlock icon on Owl. Lockup says roster count. Composition keeps the 32-px silhouette test honest — each bust reads even at thumbnail size. | "Eight heroes. Eight feels." | "Tortoise tanks. Fox crits. Owl scales. Pick your style." | Meadow (mid-fight) | Carrot Boomerang (single, ambient — the heroes are the read, not the weapon) | 6–10 (background only — bottom-strip roster is the focal layer) |
| 4 | **Feature: Biome variety** | Center frame: Bunny in **Meadow**; thirds carry biome-postcard inserts of **Beach** (golden-hour sand, palm) and **Forest** (dappled canopy, root snare). Inserts are stylised polaroid-tab cards, ≤ 12% frame area each, top-left and top-right corners (under safe-area). | "Five worlds, one carrot quest." | "Meadow, beach, forest, cavern, snow — each plays different." | Meadow primary, Beach + Forest postcard inserts | Carrot Boomerang (Meadow main shot only) | 8–12 (Meadow main only — insert biomes are empty-of-enemies postcards) |
| 5 | **Feature: Progression — level-up draft** | Bunny dimmed at 60% opacity behind a centered **Level-Up Draft card overlay**: 3 cards (Sunbeam L3, Carrot Boomerang L5 evo-ready, Magnet Charm L2). Hero Highlight glow on the evo-ready card. Soft pulse VFX ring behind the card stack. Headline lives **above** the cards. | "Every level, a tiny puzzle." | "Three offers, six builds, infinite runs." | Meadow (dimmed background) | Sunbeam, Carrot Boomerang (card art), Magnet Charm (card art) | 0 visible (cards own the frame; gameplay is paused/dimmed) |

### Narrative arc rationale

- **#1 stops the scroll.** Action density + Bunny smile + golden carrot = TikTok-thumbnail energy in a single frame.
- **#2 answers "what do you do?"** for the auto-battler-curious browser.
- **#3 answers "is there more than one guy?"** — directly addresses the Capybara-Go! "all characters feel similar" positioning risk from `03-positioning.md`.
- **#4 answers "does it stay fresh?"** — sells the 5-biome scope (also addresses the family-safe / not-grim register).
- **#5 sells the depth pillar** — the draft mechanic is the game's build-crafting fingerprint per pillar #2 of `00-overview.md`.

## 3. Per-device adaptations

The five compositions are **layout-shared**. Only safe-area and crop change per class. Headline + subhead frames re-flow; gameplay subject stays centered.

| Aspect | Class A (6.7") | Class B (6.5") | Class C (5.5") |
|---|---|---|---|
| Aspect ratio | 19.5 : 9 | 19.5 : 9 | 16 : 9 (taller-bar) |
| Top safe-area for headline | 132 px Dynamic Island clearance + 40 px breath = headline starts at y = **172 px** | 88 px notch + 40 px breath = headline starts at y = **128 px** | No notch; headline starts at y = **120 px** (status bar 60 px + breath) |
| Headline type size | 96 pt | 96 pt | 80 pt (preserves wrap behavior on 1242-wide canvas) |
| Subhead type size | 42 pt | 42 pt | 36 pt |
| Bottom safe-area for HUD tray | Home-indicator 68 px + breath 32 px = bottom y = **2696 px** | Home-indicator 68 px + breath 32 px = bottom y = **2678 px** | No home-indicator; bottom y = **2148 px** |
| Camera crop | Standard 19.5:9 from Unity Game-view 1290×2796 | Same as A, downscale only | Re-frame: Unity Game-view 1242×2208 (4:3-ish portrait). Bunny **stays centered**; reduce edge enemy density by ~15% so swarm doesn't clip frame |
| Background biome "breathing room" | Full | Full | Tightest crop — Bunny zoom-in by 8% to preserve silhouette read |

> **iPad note (informational only):** if/when iPad is added to scope post-launch, the same 5 compositions re-shoot at 2048×2732 (12.9") and 1640×2360 (11"). iPad versions show **more biome around the hero** (Bunny ~70% screen height vs ~85% on phone) and the postcard inserts in #4 grow to 18% frame area. **Not in launch scope.**

## 4. Asset capture process (runbook for the future capture dispatch)

**Phase 1 — In-Editor capture.** Open the `Run.unity` scene in Unity 6 LTS Editor. Set Game-view Aspect = `1290×2796 Portrait` (add it as a custom resolution if missing). Load a hand-authored capture save that posts Bunny mid-meadow with ~20 enemies pre-spawned at scripted offsets (capture-only spawn-list under `unity/Assets/_Brave/Editor/CaptureKit/`). Pause via `Time.timeScale = 0`. Park camera at the spec'd 55° pitch / 18 u distance from `07-art-bible/00-style-overview.md :: Camera spec`. Run an editor menu hook `Brave/Capture/Shoot Screenshot N` that calls `ScreenCapture.CaptureScreenshotAsTexture(superSize: 1)` and writes PNG to `assets-raw/marketing/screenshots/raw/bb-A-{n}-en.png`. Repeat for shots #1–#5; for each, the spawn-list, camera pose, weapon-fire freeze frame, and VFX trigger are deterministic so reshoots after balance changes match pixel-for-pixel.

**Phase 2 — Composition + overlay.** Hand the 5 raw PNGs to the Canva-MCP / Figma-MCP dispatch with the headline/subhead copy bank below and the per-device adaptation table. The MCP dispatch outputs three exported PNG sets per locale (one per device class), filename `bb-{class}-{n}-{locale}.png`, sRGB, no alpha, no rounded corners. Final 6.7" / 6.5" / 5.5" English sets land in `assets-raw/marketing/screenshots/final/en/` and feed `tools/ci/fastlane/Deliverfile` for App Store Connect upload.

## 5. Headline copy bank — 5 candidates per screenshot, ranked

> Tone gate per `narrative/00-tone-bible.md`: dry wink, kitchen-table cheer, ≤ 18 words/sentence, banned: kill / slay / die / epic / lol / emoji. All candidates pass.

### Screenshot 1 — Hero / action density

| Rank | Headline | Subhead | Tone read |
|---|---|---|---|
| **1 (pick)** | **Hop. Swarm. Survive.** | An 8-minute roguelite with a smile on every frame. | Three-verb thump, mass appeal, lands "playful" + "high-action" |
| 2 | A brave bunny vs. the meadow. | Auto-attack roguelite. One thumb. Big swings. | Story-led, leans Crossy Road |
| 3 | Eight minutes. One carrot. | The cheerful auto-battler for Survivor.io fans. | Pitch-deck phrase; cool but less playful |
| 4 | Carrots out. Rascals in. | Auto-fire, level up, build a tiny legend. | Tone-bible-perfect ("rascals"), maybe too soft for hero shot |
| 5 | Hop in. | One bunny, twelve weapons, five worlds, no pressure. | Single-beat hook; risky as carousel-first frame |

### Screenshot 2 — Auto-attack + evolutions

| Rank | Headline | Subhead | Tone read |
|---|---|---|---|
| **1 (pick)** | **One thumb. Big builds.** | Auto-attack, evolve, and watch a build come alive. | Tightest pitch; matches Survivor.io's "one finger" UVP |
| 2 | Pick a gift. Evolve a weapon. | Two of these together become something brand new. | Story of the screenshot; longer |
| 3 | Aim? Never met her. | Every weapon auto-fires. You just hop. | Punchy, playful — wink-heavy, on-brand |
| 4 | Twelve weapons want to grow up. | Pair the right two and watch them evolve. | Concept-led; lyrical |
| 5 | Build a build. | Auto-fire weapons that evolve when you pair them right. | Self-referential cute; risks being cryptic |

### Screenshot 3 — Hero roster

| Rank | Headline | Subhead | Tone read |
|---|---|---|---|
| **1 (pick)** | **Eight heroes. Eight feels.** | Tortoise tanks. Fox crits. Owl scales. Pick your style. | Distinct, specific, fast |
| 2 | Pick your animal. | Bunny, fox, owl, panda, and four more friends to unlock. | Soft-warm; very Crossy Road |
| 3 | A roster of pluck. | Eight animals, eight signature passives, one big meadow. | Tone-bible vocab ("pluck"); literary |
| 4 | Choose your hop. | From plucky Bunny to wise Owl — eight ways to play. | Cute pun; risks losing non-EN readers |
| 5 | Not just a bunny. | Unlock eight animal heroes; each plays its own way. | Frames as feature-list; less playful |

### Screenshot 4 — Biome variety

| Rank | Headline | Subhead | Tone read |
|---|---|---|---|
| **1 (pick)** | **Five worlds, one carrot quest.** | Meadow, beach, forest, cavern, snow — each plays different. | Crisp scope statement; "carrot quest" lands the smile |
| 2 | The carrots are everywhere. | Hop through five biomes, each with its own hazards. | Story-frame; on tone |
| 3 | A meadow. A beach. A forest. And more. | Five biomes, each teaches one new thing. | Listy; matches teach-one-thing rule from `06-biomes.md` |
| 4 | Sun, sand, snow, shadow. | Five biomes, five moods, one cheerful little bunny. | Lyrical; risks florid |
| 5 | New ground, new rascals. | Each biome adds a hazard and a fresh set of rascals. | Tone-vocab heavy ("rascals"); needs reader buy-in |

### Screenshot 5 — Progression / level-up draft

| Rank | Headline | Subhead | Tone read |
|---|---|---|---|
| **1 (pick)** | **Every level, a tiny puzzle.** | Three offers, six builds, infinite runs. | Sells the depth pillar; tone-perfect |
| 2 | Three gifts. Choose one. | Every level-up is a small fork in the run. | Direct, mechanics-led |
| 3 | Two gifts want to become one. | Pair the right pair, and a weapon evolves. | Quote of `LEVEL_UP_EVOLVE` from tone bible — on-brand |
| 4 | Pluckier already. | Choose a gift, change your run, keep hopping. | Tone-vocab; very soft |
| 5 | Build crafting, bunny-sized. | Three random offers per level. Six evolution recipes. | Genre-literate; less smile |

## 6. Localisation plan

Apple supports per-locale screenshot uploads. Brave Bunny: Survivors soft-launches in **TR / PH / ID** (per `GAME.md :: soft_launch_markets`). The composition is locale-agnostic — only the headline/subhead text frame swaps. The runtime locale table already covers UI; marketing copy gets new keys.

**Locales required at launch (per ADR-0016 + `01-research/01-market.md`):**

| Locale | Source | Composition | Copy status |
|---|---|---|---|
| `en` | this doc — Part B picks | shared layout | **defined here** |
| `tr` | TR runtime exists (`Localization/tr.json`) | shared layout | **gap — narrative-designer follow-up** |
| `tl-PH` (Filipino) | no runtime loc yet (per cut-list, PH/EN may ship) | shared layout | **gap — narrative-designer follow-up; English fallback is acceptable per cut-list item #3** |
| `id` (Indonesian) | no runtime loc yet (same cut-list) | shared layout | **gap — narrative-designer follow-up; English fallback acceptable** |

**Keys added (English baseline ships now, TR/PH/ID added by narrative-designer):**

```
screenshot_1_headline   "Hop. Swarm. Survive."
screenshot_1_subhead    "An 8-minute roguelite with a smile on every frame."
screenshot_2_headline   "One thumb. Big builds."
screenshot_2_subhead    "Auto-attack, evolve, and watch a build come alive."
screenshot_3_headline   "Eight heroes. Eight feels."
screenshot_3_subhead    "Tortoise tanks. Fox crits. Owl scales. Pick your style."
screenshot_4_headline   "Five worlds, one carrot quest."
screenshot_4_subhead    "Meadow, beach, forest, cavern, snow — each plays different."
screenshot_5_headline   "Every level, a tiny puzzle."
screenshot_5_subhead    "Three offers, six builds, infinite runs."
```

Authoritative key list lives in `unity/Assets/_Brave/Localization/screenshot-keys.json`. **Narrative-designer follow-up dispatch** must add `tr`, `tl-PH`, `id` columns to that file. TR has the highest priority (largest soft-launch market by player count per `01-research/01-market.md`); PH and ID may ship as English fallback under cut-list item #3 if the schedule tightens.

## 7. Canva / Figma overlay template plan

For the next composition dispatch (using Canva MCP or Figma MCP):

Produce **3 overlay templates**, one per device class (A: 1290×2796, B: 1284×2778, C: 1242×2208). Each template is a single artboard with the following named layers, all editable, ordered back-to-front:

1. `BG_SCREENSHOT` — placeholder rectangle the size of the artboard; the raw Unity capture gets pasted here.
2. `HEADLINE` — text frame, positioned per the per-device safe-area table (§3). Type spec: **Fredoka SemiBold** (CC0/OFL — already in `assets-raw/custom/fonts/`), tracking −10, leading 110%, color `#FFFFFF` with 6 px Coal Outline `#2E2A28` stroke and 12 px Y-offset shadow at 35% opacity. Wraps to 2 lines max.
3. `SUBHEAD` — text frame directly under `HEADLINE`, 32 px gap. Type: **Fredoka Regular**, same color/stroke recipe at 70% stroke alpha. Wraps to 2 lines max.
4. `WATERMARK_LOGO` — the bunny app-icon mark from `assets-raw/custom/branding/app-icon-1024.svg`, scaled to 180 px wide for class A/B, 140 px wide for class C, bottom-right corner with 64 px padding from edges, **10% opacity** per task brief.
5. `SAFE_AREA_GUIDE` — non-exported guide layer showing the Dynamic Island / notch / home-indicator zones so the overlay never lands inside a system UI region.

The composition dispatch fills `HEADLINE` and `SUBHEAD` per locale, swaps `BG_SCREENSHOT`, exports PNG. Five frames × three device classes × four locales (once narrative-designer ships TR/PH/ID) = **60 deliverables**. The English-only first pass is **15 deliverables** (5 frames × 3 classes).

## 8. Self-review hooks (for the capture + composition dispatches)

Before submitting to App Store Connect, the future dispatch verifies:

- [ ] All five compositions use `Brave Bunny: Survivors` if/where the product name appears as text in the frame (e.g. logo lockup) — never bare "Brave Bunny" (ADR-0016).
- [ ] Bunny is the most saturated entity on-screen in every frame (saturation budget rule from `00-style-overview.md`).
- [ ] No banned tone-bible vocabulary in any headline or subhead.
- [ ] Headline ≤ 7 words / subhead ≤ 12 words.
- [ ] Headline and subhead live above the gameplay subject in the top third (Apple's empirical best-practice; reads on the carousel tile).
- [ ] No system UI bleeds into the composition (status bar, Dynamic Island, home indicator).
- [ ] PNG output is sRGB, no alpha, no rounded corners, exact dimensions per device class.
- [ ] Filename matches `bb-{class}-{n}-{locale}.png`.
