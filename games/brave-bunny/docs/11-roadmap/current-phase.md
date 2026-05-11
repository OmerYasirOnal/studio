# Current phase

**Phase:** 3 — Tech Architecture (substantially complete; Phase 4 ready to start)

> Phase 1 (Discovery) and Phase 2 (GDD) are both complete. Phase 3 (Tech Architecture) is 100% on documents and ADRs; the next gate is Phase 5 (Prototype), which requires the user to install tmux + Unity 6 LTS + Unity iOS build module before agents can write engine code.

## Phase 1 — Discovery (COMPLETE)

- [x] 5 competitor deconstructions (Survivor.io, Vampire Survivors, Archero, Brotato, Capybara Go!)
- [x] Market overview (`docs/01-research/01-market.md`)
- [x] Positioning + feature matrix + risk matrix (`docs/01-research/03-positioning.md`)
- [x] References + source quality grades (`docs/01-research/04-references.md`)
- [x] Soft-launch market scan (TR/PH/ID)

## Phase 2 — GDD (COMPLETE)

- [x] 13 GDD sections (00 overview through 13 risks-and-cuts)
- [x] Narrative: tone bible, world premise, 8 character bios with TR seeds, biome flavor, 5 boss intros, 89 localization keys
- [x] UX: 62 user stories across 5 epics
- [x] UX: 5 flow diagrams (Mermaid)
- [x] UX: 15 HTML wireframes + shared `_style.css` (iPhone SE 3 fit verified)
- [x] Art bible: 10 sections (overview, palette, lighting, character-style, environment-style, vfx-style, ui-direction, iconography, asset-budget, source-shortlist)
- [x] Audio bible: 5 sections (overview, BGM 12-track spec, SFX 50-slug spec, source-shortlist, mixer-routing)
- [x] Initial balance JSON sheets (8 files + 8 schemas)
- [x] Balance docs: formulas, tuning philosophy, character/weapon/enemy/economy tuning, Monte Carlo notes
- [x] Level design: pacing model + Meadow biome layout + Meadow waves.json + Old Boar King mechanics + arena

## Phase 3 — Tech Architecture (COMPLETE on documents)

- [x] 11 tech-spec docs (engine, project layout, data model, save system, input, perf budget, rendering, audio, state machine, event bus, build-and-ci, third-party)
- [x] 12 ADRs (0001 through 0012; 0011 and 0012 are proposed, deferred to Phase 5)

## Phase 4 — Asset Pipeline (planning complete; fetches pending user approval)

- [x] Asset INDEX with planned roster mapped to CC0 sources (`assets-raw/INDEX.md`)
- [x] LICENSES.md updated with "Planned acquisitions" section
- [x] Blender pipeline templates: 4 example `build.py` recipes
- [ ] **User action needed:** approve actual CC0 fetches (Quaternius, Kenney, Freesound) using `core/tools/asset-pipeline/<source>-fetch.py`
- [ ] Verify Quaternius Otter presence; if absent, write ADR-0013 (Otter fallback)

## Phase 5 — Prototype (BLOCKED on prerequisites)

Blockers (require user action, not agent action):

- [ ] Install tmux (`brew install tmux`)
- [ ] Install Unity 6 LTS + iOS Build module
- [ ] Confirm Apple Developer account (for eventual TestFlight)

Once unblocked: gameplay-engineer + systems-engineer + ui-engineer + qa-engineer can be spawned from a fresh Claude Code foreground session.

## Progress log

| Date | Event |
|---|---|
| 2026-05-11 | Phase 0 (Framework) complete — studio v0.1.0 bootstrapped |
| 2026-05-11 | Phase 0 (Game) complete — brave-bunny scaffolded |
| 2026-05-12 | Phase 1 first pass (3 of 5 competitor decons + market + positioning) |
| 2026-05-12 | Phase 1 second pass (Brotato + Capybara Go!) |
| 2026-05-12 | Phase 2 wave 1 (GDD anchors, art-bible kick, tone bible) |
| 2026-05-12 | Phase 2 wave 2 (characters, weapons, enemies, meta, art content, narrative bios, 62 user stories) |
| 2026-05-12 | Phase 2 wave 3 (GDD systems, art bible finish + audio bible, 5 UX flows, level design) |
| 2026-05-12 | Phase 2 wave 4 (15 wireframes, balance JSON sheets + Monte Carlo, 6 of 11 tech specs) |
| 2026-05-12 | Phase 3 wave 1 (remaining 5 tech specs, asset planning, Blender pipeline) + ADRs 0006-0010 |
| 2026-05-12 | Phase 3 substantially complete; ready for Phase 5 once user prerequisites installed |

## Open blockers

- User prerequisite: `brew install tmux`
- User prerequisite: Unity 6 LTS + iOS Build module install
- User prerequisite: approve `quaternius-fetch.py` / `kenney-fetch.py` runs for actual CC0 downloads (or do them manually)

## Next agents to spawn (when unblocked)

When tmux + Unity are installed:

```bash
# Phase 4 — asset fetches (run from foreground Claude Code)
./core/scripts/spawn-agent.sh asset-curator "Execute the INDEX.md plan: fetch the 8 Quaternius character meshes + 5 Kenney biome packs + Freesound CC0 SFX pack. Use the existing fetch scripts; verify Otter presence (ADR risk noted)."

# Phase 5 — Unity prototype
./core/scripts/spawn-agent.sh gameplay-engineer "Initialize Unity 6 LTS URP project per tech-spec 01-project-layout. Set up the 6 asmdefs. Scaffold Boot scene + GameContext service locator. First gameplay sprint: joystick + auto-attack + 200-enemy stress scene per tech-spec 05-performance-budget."

./core/scripts/spawn-agent.sh systems-engineer "Implement SaveService per tech-spec 03 and ADR-0008 (Newtonsoft JSON). Round-trip test + corruption test. Backup rotation. Generate save_schema_v1 from the spec."

./core/scripts/spawn-agent.sh ui-engineer "Port the 15 wireframes to UI Toolkit UXML. Set up theme.uss from the wireframes' _style.css custom properties. Start with Home screen + Run HUD (vertical-slice critical-path)."

./core/scripts/spawn-agent.sh qa-engineer "Author EditMode tests scaffold per tech-spec 02 (data model validation) + ADR-0009 (mechanic registry tests). Set up Unity Test Framework integration."

./core/scripts/spawn-agent.sh build-engineer "Author Fastlane Fastfile + ios-build.yml workflow per tech-spec 10. Start with 'preview' lane (local archive, no upload). Document the manual cert steps."
```
