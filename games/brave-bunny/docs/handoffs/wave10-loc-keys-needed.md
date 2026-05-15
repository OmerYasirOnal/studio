# Wave 10 — Loc Keys Needed (handoff to loc-agent)

**From:** Wave 10 character-abilities agent
**To:** loc-agent
**Date:** 2026-05-16
**Files to update:** `unity/Assets/_Brave/Localization/en.json`, `unity/Assets/_Brave/Localization/tr.json`

## Per-character passive abilities (16 keys — 8 names + 8 descriptions)

Wave 10 added 8 `CharacterAbility` subclasses under
`unity/Assets/_Brave/Code/Gameplay/Characters/`, one per launch character.
Each needs a localised name + description.

### English suggested copy (draft — loc-agent owns final wording)

```json
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
