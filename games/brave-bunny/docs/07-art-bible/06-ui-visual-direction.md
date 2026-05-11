# UI Visual Direction — Brave Bunny

> Owner: art-director. Cross-refs: `01-color-palette.md` (UI button states + WCAG contrast guardrails), `00-style-overview.md` (saturation budget — UI accents are 100% S), `docs/02-gdd/11-feel-pillars.md` (Pillar 5 UI responsiveness: ≤120 ms confirmation, ≥88 pt tap target), `games/brave-bunny/CLAUDE.md` (perf contract — UI shares the 80-DC budget; target ≤8 DC for UI). All fonts are **SIL OFL via Google Fonts** — zero paid fonts. UI runs on **Unity UI Toolkit (USS)** so styling is data-driven.

## Style thesis

UI is a **soft cartoon greeting card**: rounded corners, generous padding, friendly shadows, never harsh. The UI extends the saturation-budget rule from `00-style-overview.md` — accent fills are 100% S, body chrome is 60-70% S, backdrops are near-neutral. Text and CTAs always win the contrast war.

## Button system

### Shape + corner radius

| Property | Value |
|---|---|
| Corner radius | **24 px** for primary CTAs and large cards; **16 px** for medium buttons; **12 px** for chips/tags |
| Min height (primary CTA) | 56 px |
| Min width (primary CTA) | 144 px |
| Padding (horizontal) | 24 px |
| Padding (vertical) | 12 px |
| Tap target (final hit-rect) | **≥ 44 pt** per Apple HIG; **≥ 88 pt** for primary in-run controls per Pillar 5 |

### Button states (sample the UI palette in `01-color-palette.md`)

| State | Fill | Stroke | Label | Transform |
|---|---|---|---|---|
| Idle | `#FF6B6B` (Hero Highlight) | `#C7423A` 1 px | `#FFFFFF` Fredoka 16 sp bold | scale 1.00 |
| Hover | `#FF8585` | `#C7423A` 1 px | `#FFFFFF` | scale 1.02 over 100 ms |
| Pressed | `#D9554F` | `#8B2D29` 1 px | `#FFFFFF` | scale 0.95 within 16 ms (Pillar 5) |
| Disabled | `#C9C9C9` | `#9C9C9C` 1 px | `#6E6E6E` | no transform; 60% opacity |
| Locked (tap rejected) | `#C9C9C9` | `#9C9C9C` 1 px | `#6E6E6E` + padlock icon | 3 px horizontal shake, 2 oscillations, 180 ms (Pillar 5) |

Confirmation animation on pointer-up: scale back to 1.00 + soft "tick" SFX at -12 dB within **120 ms** (Pillar 5).

## Typography hierarchy

All four fonts are **SIL OFL** via Google Fonts. No paid fonts ship. Fallback stack: `system-ui`.

| Role | Font family | Weight | Size | Line height | Use |
|---|---|---|---|---|---|
| H1 — Screen title | **Fredoka** | 600 (Semi-Bold) | 24 sp | 32 sp | "Loadout", "Battle Pass", "Achievements" |
| H2 — Section header | **Fredoka** | 500 (Medium) | 20 sp | 28 sp | "Equipped weapons", "Daily rewards" |
| Body | **Nunito** | 400 (Regular) | 14 sp | 20 sp | Description text, tooltips, modal copy |
| Body emphasis | **Nunito** | 700 (Bold) | 14 sp | 20 sp | Inline emphasis, key terms |
| Numerics (HUD + stats) | **Baloo 2** | 700 (Bold) | 18 sp | 22 sp | HP, XP counter, gold, timer, damage numbers |
| Numerics (large display) | **Baloo 2** | 700 (Bold) | 32 sp | 36 sp | Run-end tally lines, level-up "Lv 5" badge |
| Button label | **Fredoka** | 500 (Medium) | 16 sp | 20 sp | All button CTAs |
| Micro-label / tag | **Nunito** | 700 (Bold) | 11 sp | 14 sp | Rarity tags, "NEW" badges |

All text colors default to **Coal Outline `#2E2A28`** on light backgrounds and **`#FFFFFF`** on accent fills. WCAG AA contrast guarded per `01-color-palette.md` §Text contrast guardrails.

## Iconography (summary — full spec in `07-iconography.md`)

| Property | Value |
|---|---|
| Base canvas | **24 × 24 px** (with 2 px internal padding → 20 × 20 px visual) |
| Line weight | 2 px solid stroke |
| Style | Monoline with single palette accent fill |
| "New" notification | 8 px **accent dot** (Pickup Gold `#FFC83D`) top-right of icon |
| Format | SVG primary, PNG @ 1x/2x/3x fallback for Unity UI Toolkit |

## Card style

Used for: weapon/passive draft cards (level-up modal), character select tiles, achievement rows, store SKUs.

| Property | Value |
|---|---|
| Corner radius | 16 px |
| Stroke | 1 px `#E0DAD3` (warm neutral) |
| Drop shadow | 1 px offset Y, 2 px blur, `#2E2A28` at 12% opacity |
| Padding | 16 px |
| Hover state | Stroke shifts to **biome accent** (per `01-color-palette.md` — e.g. `#A8D86B` in Meadow) + shadow Y offset → 2 px |
| Pressed | Scale to 0.97 over 80 ms |
| Rarity ring (when used) | Single-stroke ring color from rarity palette (`07-iconography.md`) |

## Modal style

Used for: level-up draft, settings, store confirms, IAP confirms, character unlock.

| Property | Value |
|---|---|
| Corner radius | **16 px** |
| Backdrop dim | Black overlay 0 → **60% opacity** over 160 ms (Pillar 2 spec) |
| Modal max-width | 480 pt (centered, 24 pt margin from safe area on smaller devices) |
| Padding | 24 pt |
| Close button | Top-right, 32 × 32 pt tap target, "X" icon Coal Outline `#2E2A28` |
| Open animation | Translate Y `+300 px → 0` with cubic-bezier `(0.34, 1.56, 0.64, 1.0)` over 280 ms (Pillar 2) |
| Close animation | Translate Y `0 → +200 px` + fade to 0 over 200 ms ease-in |
| Esc/back gesture | Equivalent to close button; only allowed when no destructive action pending |

## Toast style

Used for: "Daily streak claimed", "New unlock available", offline error.

| Property | Value |
|---|---|
| Position | Bottom of safe area, 16 pt above home indicator |
| Width | min(360 pt, screen-width − 32 pt) |
| Corner radius | 16 px |
| Background | `#2E2A28` at 92% opacity |
| Label | Nunito 14 sp `#FFFFFF` |
| Icon (optional) | 24 × 24, left side |
| Slide-in | 200 ms ease-out from `+80 px` Y |
| Dwell | **2.5 s** |
| Slide-out | 200 ms ease-in to `+80 px` Y |
| Stacking | Max 1 toast on screen; new toast replaces (fade-cross 120 ms) |

## Safe-area discipline

Every screen accounts for both extremes:

| Device | Top safe area | Bottom safe area | Notes |
|---|---|---|---|
| iPhone 12 / 13 / 14 (notch) | 47 pt | 34 pt | Avoid hit-rects in the 24-pt-wide notch column |
| iPhone SE 3 (Touch-ID home button) | 20 pt | 0 pt | No home indicator clearance needed |
| iPad (rare — secondary target) | 24 pt | 20 pt | Layout reflows; not a launch target |

Rules:
- HUD numerics never enter the top 47 pt strip.
- Primary CTAs never enter the bottom 34 pt strip.
- Full-bleed art (boss-intro frame, run-end tally backdrop) extends edge-to-edge but **all interactive elements clamp to safe area**.

## Tap target rule (restated from Pillar 5)

Minimum tap target = **44 pt** (Apple HIG floor). Primary in-run controls and the level-up cards = **88 pt** to clear iPhone SE 3 thumb-zone tests. The visual button graphic may be smaller; the hit-rect is the gate.

## Empty states

Every empty list/screen gets:

1. **Friendly art** — single character illustration at 96 × 96 px (Bunny shrug pose by default, biome-specific where relevant).
2. **Single line of copy** — tone-bible-correct (warm, optimistic, never apologetic). Examples: "No daily streaks yet — start one today!" / "No friends added — invite a hopper!" / "Nothing claimed — your stash is empty for now."
3. **Single CTA button** — drives the player to the action that fills the empty state.

No "Oops!", no "Error!", no exclamation marks in empty-state copy except for the CTA cheer.

## DC budget reservation

UI canvas budget: **≤ 8 draw calls** at peak (HUD + active modal + 1 toast). UI Toolkit batches well — most screens land at 3-5 DC. Hand-off to ui-engineer to validate per-screen in profiler.
