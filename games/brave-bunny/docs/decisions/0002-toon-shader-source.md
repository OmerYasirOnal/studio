# ADR 0002 — Toon shader source

**Date:** 2026-05-12
**Status:** accepted
**Owner:** orchestrator (synthesizing art-director wave-2 flag)

## Context

The art bible (`docs/07-art-bible/00-style-overview.md` and `03-character-style.md`) commits to a custom toon-ramp shader with: 1-tone shadow ramp, hero outline pass, per-biome LUT, `_HeroSaturationBoost` shader property (heroes get +20% saturation vs surroundings). Tech-architect's perf budget caps lighting+post combined at 5 ms on iPhone 12 baseline.

Three sources are viable:

- **A. Custom Shader Graph URP toon** — written in-house, full control, MIT-equivalent (our code).
- **B. Free MIT toon shader** — adopt a community library like Unity-Chan Toon Shader 2 or JTRP if MIT-compatible.
- **C. Unity Asset Store paid toon shader** — explicitly forbidden by `core/docs/asset-policy.md`.

## Decision

**Option A — Custom Shader Graph URP toon, in-house.**

Rationale:

1. The art bible's requirements (hero saturation boost coupled to outline pass + per-biome LUT) are sufficiently specific that any community shader would need heavy modification.
2. Modifying a community shader couples us to its upstream lifecycle and licensing terms — exactly the dependency surface the framework's "zero external paid API" rule tries to avoid.
3. URP Shader Graph is mature in Unity 6 LTS; the build cost is moderate (estimated 2-3 days for a senior gameplay-engineer, less with sample shader-graphs from Unity's templates).
4. MIT-licensed community shaders we shipped would still need attribution in credits — fine, but the in-house cost is small enough that we eat it for purity.

## Consequences

- `unity/Assets/Shaders/Toon/` will house the shader graph + supporting shader files.
- Tech-architect's tech spec section 06 (Rendering) must include a shader complexity table cross-referenced with this ADR.
- Gameplay-engineer owns shader work in Phase 5 (Prototype). Until then, prototypes can use URP's stock Lit shader.
- Per-biome LUT files become an asset-curator deliverable (CC0 LUTs can be generated from palette JSONs).
- If iPhone SE 3 baseline shows the shader >5 ms, fallback path is to drop the outline pass on swarm enemies (already noted in `02-lighting.md`).

## Alternatives considered

- **B. Free MIT toon shader** — rejected for the dependency reason. Reconsider if Phase 5 shows the in-house cost overruns by >100%.
- **C. Paid Unity Asset Store shader** — rejected categorically by `core/docs/asset-policy.md`.

## References

- `docs/07-art-bible/00-style-overview.md`
- `docs/07-art-bible/03-character-style.md`
- `docs/07-art-bible/05-vfx-style.md`
- `core/docs/asset-policy.md`
