# Game-level rules — extends `/CLAUDE.md`

> Every game inherits the framework rules in the repo-root `CLAUDE.md`. This file is for **overrides and additions specific to this game**.

## Game

- Slug: `brave-bunny`
- Genre: action-roguelite (Survivor.io-like)
- Template: `action-roguelite`
- Scaffolded: `2026-05-11`

## Genre-specific rules

- **TTK math is the lifeblood**: every weapon tuning change must run through balance-engineer's TTK ladder before merging.
- **Wave timing is non-negotiable**: level-designer's `waves.json` is the source of truth; gameplay-engineer never modifies it.
- **Pooling is mandatory** for every spawnable (enemy, projectile, pickup, VFX). Tech-architect's ADR-0005 governs the pool API.

## Performance contract (cross-checked with tech-spec 05)

- 60 fps on iPhone 12 with 200 active enemies + 50 projectiles + 30 VFX puffs.
- Draw-call cap: 80
- Tris cap on-screen: 250k

## Open questions for orchestrator

<Add ADR drafts here as questions surface. Move them to docs/decisions/ once decided.>
