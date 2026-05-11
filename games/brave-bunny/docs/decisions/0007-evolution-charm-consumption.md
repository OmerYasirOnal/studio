# ADR 0007 — Evolution charm consumption

**Date:** 2026-05-12
**Status:** accepted
**Owner:** orchestrator (synthesizing balance-engineer wave-4 flag)

## Context

`docs/02-gdd/04-weapons.md` lists 6 weapon evolution recipes. Each recipe requires a level-5 weapon + a level-5 passive charm. Balance-engineer (wave 4) flagged that **Magnet Charm**, **Crit Charm**, and **Damage Charm** are each required by 2 separate recipes, but a player can only equip 6 weapons + 6 passives total in a single run.

Question: when an evolution triggers, is the charm **consumed** (freeing a passive slot) or **retained** (still occupies the slot)?

## Decision

**Charms are consumed by evolution.**

When a player evolves a weapon, the required charm is removed from their passive inventory, freeing a passive slot. The evolved weapon takes the weapon slot. The player can then pick up another charm if a level-up offers it.

This gives:

- Build crafting depth (pillar 2 served — picking the evolution is a real cost/benefit decision)
- Run pacing — players make more meaningful upgrade picks as runs progress
- Clear UX — the post-evolution toast says "Magnet Charm consumed → Carrot Boomerang evolved to Harvest Cyclone"

## Consequences

- `data/balance/weapons.json` schema: each evolution recipe has `"consumes_charm": true` field (always true at launch; reserved for future "permanent charm" cosmetics)
- gameplay-engineer: evolution event removes the charm from the active passive inventory
- ui-engineer: post-evolution toast string keyed via `narrative/05-localization-keys.md` — to be added: `{EVOLUTION_CONSUME_TOAST}`
- balance-engineer: charm pickup rate during late-game level-ups must support the choose-and-consume loop — verify in next Monte Carlo pass
- A player who picked Damage Charm for one evolution can still get another Damage Charm later (rolling stock); the charm itself is universally available, just slotted

## Alternatives considered

- **Charms retained (not consumed)** — rejected. Removes choice tension; once you have the 3 universal charms (magnet/crit/damage) you auto-qualify for almost all evolutions and the depth dissolves into a stat-stacking exercise.
- **Charm becomes a "fused" passive that grants a tiny universal bonus** — interesting but adds rule complexity; defer to post-launch experimentation.
- **Make each evolution require a unique charm** — rejected. Would force expanding the charm roster from 6 to 12+ items, blowing scope.

## References

- `docs/02-gdd/04-weapons.md`
- `docs/10-balance/03-weapon-tuning.md`
- `data/balance/weapons.json`
- `data/balance/passives.json`
