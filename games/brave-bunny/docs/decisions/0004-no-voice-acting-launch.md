# ADR 0004 — No voice acting at launch

**Date:** 2026-05-12
**Status:** accepted
**Owner:** orchestrator (synthesizing art-director wave-3 flag)

## Context

`docs/08-audio-bible/00-audio-overview.md` proposed no voice acting at launch. Two forces drive this:

1. **Cost discipline** — the framework forbids paid AI TTS (ElevenLabs, OpenAI etc.) and we have no budget for live voice talent or studio time. Hiring even one TR + one EN voice would blow the indie cost target and create localization burdens for PH/ID.
2. **Schedule** — 8-week vertical-slice window can't absorb VO recording / mixing / per-character session pickups.

The audio bible reserved a `Voice` mixer bus for future use.

## Decision

**No voice acting at launch.** Narrative is carried entirely by text (matching the tone bible) and ambient SFX. The `Voice` mixer bus stays declared but unused.

Post-launch (v1.1+) reconsideration triggers:

- Sustained D30 ≥ 12% in soft launch markets (unlocks budget for VO)
- Partnership with a free/CC-BY voice talent collective
- Player feedback in soft launch explicitly cites VO absence as a gap

## Consequences

- ui-engineer: no audio cue tied to a character "voice"; rely on UI SFX from `02-sfx-spec.md`
- narrative-designer: keeps lines short enough to read at-a-glance (≤ 18 word sentences, already enforced)
- art-director: no VO budget line in the audio mixer; voice bus exists but is muted
- localization: TR / EN at launch; PH (English) and ID (English) parallel. VO would multiply translation cost — sidestepped.
- App Store rating: 7+ unaffected (text-only ratings are simpler)

## Alternatives considered

- **Hire live voice talent (per language)** — rejected. Cost + schedule.
- **AI TTS (ElevenLabs etc.)** — categorically forbidden by `core/docs/asset-policy.md`.
- **Use CC0 character-grunt SFX as "voice"** — partially adopted. Hit/jump grunts ARE in `02-sfx-spec.md` under `enemy_*` and `weapon_*` SFX. No human speech though.
- **Player-recorded VO contest** — interesting; revisit post-launch as a community event.

## References

- `docs/08-audio-bible/00-audio-overview.md`
- `docs/08-audio-bible/04-mixer-routing.md`
- `core/docs/asset-policy.md`
