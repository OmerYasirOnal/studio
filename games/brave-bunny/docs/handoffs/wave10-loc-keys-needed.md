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

## — character abilities —

**From:** Wave 10 character-abilities agent
## Per-character passive abilities (16 keys — 8 names + 8 descriptions)

Wave 10 added 8 `CharacterAbility` subclasses under
`unity/Assets/_Brave/Code/Gameplay/Characters/`, one per launch character.
Each needs a localised name + description.
"character_ability.hop.name": "Hop",
"character_ability.hop.description": "Bouncy feet — +10% move speed, always on.",

"character_ability.shell.name": "Shell",
"character_ability.shell.description": "Thick armour — +50% HP, but -20% move speed.",

"character_ability.quills.name": "Quills",
"character_ability.quills.description": "Spiky coat reflects 5% of incoming damage back at attackers.",

"character_ability.cunning.name": "Cunning",
"character_ability.cunning.description": "Critical strikes deal an extra +100% damage.",

"character_ability.slick.name": "Slick",
"character_ability.slick.description": "Projectiles fly 15% faster.",

"character_ability.restore.name": "Restore",
"character_ability.restore.description": "Regenerate 1 HP per second when no enemy has been killed for 3 seconds.",

"character_ability.tenacity.name": "Tenacity",
"character_ability.tenacity.description": "Deal +25% damage while at or below 30% HP.",

"character_ability.foresight.name": "Foresight",
"character_ability.foresight.description": "Gain +1 extra reroll on every level-up choice.",
```

### Notes
- Key pattern: `character_ability.<id>.{name,description}` — id is the bare
  ability slug (no "ability." prefix), matching `characters.json:ability_id`.
- Display surface: character-select panel sub-card under the existing signature
  mechanic blurb (UI agent owns the wireframe in `docs/05-wireframes/`).
- Wave 10 introduces 8 abilities; if a 9th launch character is added later, a
  new key pair must follow the same pattern.

## — Run QoL —

# Wave 10 — Loc Keys Needed (handoff to loc-agent)

**From:** Wave 10 QoL agent (focus-pause + quit-confirm + FPS toggle)
## QuitConfirmDialog modal (4 keys)

Wave 10 added a quit-confirm dialog interposed between the pause-modal Quit
button and the actual scene exit. Loc keys are referenced via `loc-key=` on
`QuitConfirmDialog.uxml`.
"quit_confirm.title": "Quit your run?",
"quit_confirm.message": "Your run will end and progress for this run will be lost.",
"quit_confirm.confirm": "Quit run",
"quit_confirm.cancel": "Keep playing"
```

### Turkish suggested copy (draft)

```json
"quit_confirm.title": "Koşunu bırak?",
"quit_confirm.message": "Koşun sona erecek ve bu koştaki ilerlemen kaybolacak.",
"quit_confirm.confirm": "Koşuyu bırak",
"quit_confirm.cancel": "Oynamaya devam et"
```

## Notes

- Tone: warm, slightly playful — matches the existing Brave Bunny voice ("rascals" / cartoon flavour).
- The Confirm button is destructive — keep the cancel option visually safer.
- No new keys for the FPS counter (numeric only) or the auto-pause-on-focus (silent — surfaces the existing pause modal which already has its own loc-keys).
