# Wave 10 — Loc Keys Needed (handoff to loc-agent)

**From:** Wave 10 parallel agents (combo, crit, achievements, …)
**To:** loc-agent
**Date:** 2026-05-16
**Files to update:** `unity/Assets/_Brave/Localization/en.json`, `unity/Assets/_Brave/Localization/tr.json` (+ id/ph parity)

## A — Combo / kill-streak badge (gameplay agent)

Source: `_Brave/UI/Documents/ComboBadge.uxml` + `Code/UI/Controllers/ComboBadgeController.cs`.

| Key             | English (placeholder)         | Notes                                            |
|-----------------|-------------------------------|--------------------------------------------------|
| `combo.tier_1`  | Combo!                        | Tier-1 (silver) — appears at streak >= 3.        |
| `combo.tier_2`  | Big combo!                    | Tier-2 (gold) — appears at streak >= 5.          |
| `combo.tier_3`  | RAINBOW STREAK!               | Tier-3 (rainbow) — appears at streak >= 10.      |
| `combo.multikill` | Combo                       | Generic label under the streak number.           |

### Notes for translator

- Cartoon-friendly. Banned: "kill streak" framing — keep "combo" / "streak" only. "Rascals" is the established euphemism in en.json (see `weapons.thunder-cloud.description`).
- Tier-3 string should feel rare and excited; the badge plays a rainbow gradient (`combo-tier-3` USS class).
- Diminutives + exclamation work well in `tr.json` (e.g. tier-1 → `Kombo!`, tier-3 → `GÖKKUŞAĞI KOMBOSU!`).

## Consumer files

- `unity/Assets/_Brave/UI/Documents/ComboBadge.uxml` (uses `combo.multikill` via loc-key attribute)
- `unity/Assets/_Brave/Code/UI/Controllers/ComboBadgeController.cs` (runtime fetches tier strings via LocalizationProvider when wired)
- `unity/Assets/_Brave/Code/Gameplay/Combat/ComboService.cs` (fires `ComboChangedEvent` with `tier ∈ {0,1,2,3}`)

## Status

- [ ] EN entries committed
- [ ] TR / ID / PH translations
- [ ] QA review: badge stays hidden below tier-1 threshold

Append additional Wave-10 loc keys (crit pop-ups, achievement toasts, etc.) below as those agents land.
