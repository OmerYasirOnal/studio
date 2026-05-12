# ADR 0016 — App Store display name: "Brave Bunny: Survivors"

**Date:** 2026-05-12
**Status:** accepted
**Owner:** orchestrator (with user approval on naming choice)

## Context

During the first attempt to create the App Store Connect app entry for
bundle id `com.omeryasir.bravebunny`, Apple's New App form rejected the
display name "Brave Bunny" with:

> The app name you entered is already being used. If you have trademark
> rights to this name and would like it released for your use, submit a
> claim.

This is Apple's standard duplicate-name guard. We do **not** hold a
registered trademark on the "Brave Bunny" mark, so the trademark-claim
path is not viable. A different display name is required.

## Decision

**App Store display name:** `Brave Bunny: Survivors`

The split:

| Surface | Name | Why |
|---|---|---|
| App Store listing (ASC display name) | `Brave Bunny: Survivors` | Avoids trademark conflict; the `: Survivors` subtitle is genre-canonical (Survivor.io, Vampire Survivors, Brotato Survivors mode) and signals the auto-battler-roguelite category to discovery-search users. |
| In-game UI / boot logo / window title | `Brave Bunny` | The brand the GDD, art bible, and narrative bible were built around. No internal rename. |
| Bundle ID | `com.omeryasir.bravebunny` | Unchanged. Apple does not care about bundle id collisions across display-name disputes. |
| SKU | `bravebunny` | Unchanged. SKU is internal to ASC. |
| `Fastfile :: APP_NAME` constant | `Brave Bunny: Survivors` | This is the value passed to `Spaceship::ConnectAPI::App.create`. Must match the ASC display name exactly. |
| Marketing / press / social handles | `Brave Bunny` (no subtitle) | The studio brand stays clean; the `: Survivors` decoration is for Apple's store taxonomy, not user-facing copy. |

## Consequences

- The App Store search result will read **Brave Bunny: Survivors**. The
  iOS Home screen springboard label (subtitle stripped to short name)
  will read **Brave Bunny** — set via `CFBundleDisplayName` in Info.plist.
- `Fastfile.APP_NAME` now diverges from the in-game `APP_NAME` literals
  in `_Brave/Code/UI/**/*.cs`. This is intentional and must not be
  "fixed" by anyone running a search-and-replace. The guardrail is the
  ADR-referenced comment on the `APP_NAME =` line in Fastfile.
- Future store-front renames (e.g. world-language App Store localization)
  inherit the same split: localized store name may decorate, in-game
  brand stays "Brave Bunny" (or the Turkish "Cesur Tavşan" for TR runtime
  per existing TR localization, but the ASC TR display would mirror EN).

## Alternatives considered

1. **Submit a trademark claim** — rejected. No registered TM; the
   process is slow and uncertain.
2. **"BraveBunny" (CamelCase, single token)** — rejected. Apple's
   case-insensitive duplicate check would likely still trigger; it also
   reads as a typo and loses the two-word brand rhythm.
3. **"Brave Bunny: Roguelite"** — rejected as a discoverability
   downgrade. "Survivors" is a 3-5x higher-volume App Store keyword in
   the auto-battler subgenre than "roguelite".
4. **"Brave Bunny: Last Stand"** — rejected. Strong dramatic feel but
   genre-ambiguous — could read as wave defense (Plants vs Zombies family)
   rather than auto-attack roguelite.
5. **Rename the project entirely** (e.g. "Carrot Crusader", "Hop
   Hero") — rejected. The GDD, art bible, narrative bible, 14 character
   bios, and 89 loc keys are all built on the Bunny brand. Sunk-cost
   isn't the right framing — the brand is product, not just label —
   and the store-name workaround is cheap.

## References

- `games/brave-bunny/tools/ci/fastlane/Fastfile` (line 34, `APP_NAME` constant)
- `games/brave-bunny/docs/02-gdd/00-overview.md` (brand pillars, unchanged)
- `games/brave-bunny/unity/Assets/_Brave/Localization/en.json` (in-game strings, unchanged)
- Apple ASC duplicate-name screenshot evidence: orchestrator session 2026-05-12
- Genre-naming reference: Survivor.io, Vampire Survivors, Brotato — all use
  `<noun>: Survivors` or `<noun> Survivors` shaped subtitles successfully.
