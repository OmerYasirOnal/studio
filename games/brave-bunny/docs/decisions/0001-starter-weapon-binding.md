# ADR 0001 — Starter-weapon character binding

**Date:** 2026-05-12
**Status:** accepted
**Owner:** orchestrator (synthesizing game-designer wave-2 flag)

## Context

The 8-character roster in `docs/02-gdd/03-characters.md` and the 12-weapon table in `docs/02-gdd/04-weapons.md` create an implicit coupling: each character has a thematic "starter weapon" (e.g., Bunny → Carrot Boomerang, Badger → Acorn Cannon). Two design options exist:

- **A. Character-bound starters** — each character can only equip their thematic starter. Strong identity, restricts player expression.
- **B. Universal pool with character-bound *defaults*** — character has a default starter, but the player can swap to any unlocked weapon after acquisition. Weaker identity, stronger expression.

Game-designer flagged this in their wave-2 hand-off ("ADR-worthy decision bumped") because it changes the loadout-system data model and tech-architect's save schema.

## Decision

**Option B — Universal pool with character-bound defaults.**

Rationale:

1. Restricting weapons by character cuts the build-crafting depth pillar #2. Players want to discover synergies, not be told.
2. The character-identity job is carried by the **signature mechanic** and **stat baseline**, not by weapon lock-in.
3. Live-ops flexibility — future weapons can ship without re-tagging every character.
4. UX simpler — one loadout screen, not per-character.

Starter weapon is the *default-equipped* slot for a character; once a weapon is unlocked, it's universally available across all owned characters.

## Consequences

- Loadout system stores an `equipped_weapon_id` per character profile, but the weapon catalog is global.
- ScriptableObject schema (`CharacterDefinition.cs`) keeps `defaultStarterWeapon` but no `allowedWeapons` allow-list.
- UI: loadout screen has one weapon-picker that shows all unlocked weapons; selecting one previews it on the active character.
- Balance: weapon balance must hold across all character × weapon pairings, not just thematic ones. Balance-engineer must extend TTK ladders accordingly.
- Tutorial: first run still equips Bunny → Carrot Boomerang to anchor identity at FTUE.

## Alternatives considered

- **A. Character-bound starters** — rejected. Cuts depth pillar; competitor analysis (Survivor.io, VS) shows universal weapons drive the genre's word-of-mouth.
- **C. Tiered binding (basic universal, signature locked)** — rejected. Adds rule complexity without payoff; the binding rules become a UX explanation tax.

## References

- `docs/02-gdd/03-characters.md`
- `docs/02-gdd/04-weapons.md`
- `docs/handoffs/game-designer-*.md` (wave-2 hand-off, "ADR-worthy decision bumped" section)
