# ADR 0005 — Engine choice: Unity 6 LTS URP

**Date:** 2026-05-12
**Status:** accepted
**Owner:** orchestrator (anticipates tech-architect Phase 3 lock — recording the decision now so balance-engineer and art-director can stop hedging)

## Context

`games/brave-bunny/GAME.md` already states `engine: unity-6-lts` and `engine_pipeline: urp`. All Phase 1 research (`docs/01-research/02-competitors/`) confirms Unity is the genre baseline. This ADR records the rationale formally so it can be cited.

## Decision

**Unity 6 LTS, Universal Render Pipeline (URP), C# 9, UI Toolkit, .NET Standard 2.1.**

Specific version pin: latest LTS release of Unity 6 at the time of Phase 5 (Prototype) kickoff. Lock via `unity/ProjectSettings/ProjectVersion.txt` once Unity project exists.

## Consequences

- gameplay-engineer / systems-engineer / ui-engineer all build to Unity 6 LTS C# 9 APIs.
- Free Unity Personal license covers studio at our revenue scale; no Unity subscription needed.
- URP shader graph compatible with art bible's toon-ramp custom shader (ADR-0002).
- UI Toolkit (Unity 6) replaces UGUI — already baked into `GAME.md` `ui_framework: ui-toolkit`.
- Asset pipeline: glTF / FBX import via free packages.
- Build size: target ≤ 200 MB iOS app (asset budget already in `07-art-bible/08-asset-budget.md`).
- Performance: matches `docs/06-tech-spec/05-performance-budget.md` (to be authored by tech-architect): 60 fps on iPhone 12 baseline.

## Alternatives considered

- **Godot 4** — rejected for v0.1. Considered for future framework expansion (see `core/README.md` roadmap v0.3). Genre toolchain less mature on Godot mobile; would extend schedule.
- **Unreal Engine 5** — rejected. iOS app size + cold-start latency unsuitable for casual mobile; engine overkill for low-poly cartoon.
- **GameMaker (Vampire Survivors's choice)** — rejected. Strong 2D but our spec is 3D top-down with toon shader.
- **Bevy / custom Rust** — rejected. Not production-ready for solo dev on this timeline.
- **Habby's proprietary stack** — not available; their public hires confirm it's Unity-based anyway.

## References

- `games/brave-bunny/GAME.md`
- `docs/01-research/01-market.md` (genre engine baseline)
- `docs/02-gdd/00-overview.md`
- `docs/07-art-bible/00-style-overview.md`
- `core/README.md` (framework roadmap)
