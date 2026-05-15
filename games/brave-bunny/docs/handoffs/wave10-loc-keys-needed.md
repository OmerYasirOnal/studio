# Wave 10 — Loc Keys Needed (handoff to loc-agent)

**From:** Wave 10 profile-screen agent
**To:** loc-agent
**Date:** 2026-05-16
**Files to update:** `unity/Assets/_Brave/Localization/en.json`, `unity/Assets/_Brave/Localization/tr.json`, `id.json`, `ph.json`

## A — Player Profile screen (~17 keys)

Wave 10 adds a Profile screen (`Assets/_Brave/UI/Documents/Profile.uxml`) with
three tabs: Stats / Characters / Achievements (the third is a deep-link to
AchievementsPanel — see that agent's handoff for its own keys).

Source files that reference these keys:
- `Assets/_Brave/UI/Documents/Profile.uxml`
- `Assets/_Brave/Code/UI/Controllers/ProfileController.cs` (constants on `ProfileScreenLogic`)
- `Assets/_Brave/UI/Documents/Home.uxml` (Profile entry button)

| Key                              | English source                                            | Notes |
|----------------------------------|-----------------------------------------------------------|-------|
| `profile.title`                  | Profile                                                   | Header title |
| `profile.open_button`            | Profile                                                   | Home-screen entry button |
| `profile.tab_stats`              | Stats                                                     | Tab label |
| `profile.tab_characters`         | Characters                                                | Tab label |
| `profile.tab_achievements`       | Achievements                                              | Tab label |
| `profile.stats_heading`          | Lifetime stats                                            | Card heading |
| `profile.stat_kills`             | Total kills                                               | Stat row label |
| `profile.stat_runs`              | Total runs                                                | Stat row label |
| `profile.stat_best_wave`         | Best wave reached                                         | Stat row label |
| `profile.stat_best_time`         | Best run time                                             | Stat row label |
| `profile.stat_bosses`            | Bosses defeated                                           | Stat row label |
| `profile.stat_evolutions`        | Evolutions triggered                                      | Stat row label |
| `profile.stat_playtime`          | Total playtime                                            | Stat row label |
| `profile.characters_empty`       | No characters yet.                                        | Empty-state hint on Characters tab |
| `profile.achievements_heading`   | Achievements                                              | Card heading on Achievements tab |
| `profile.achievements_hint`      | Open the achievements panel for the full list.            | Hint copy |
| `profile.open_achievements`      | Open achievements                                         | CTA → AchievementsPanel screen |
| `profile.character_locked`       | Locked                                                    | Fallback progress label when no unlock-hint key resolves |
| `profile.character_runs`         | Runs {count}                                              | Per-character progress segment; `{count}` token like quest keys |
| `profile.character_bosses`       | Bosses {count}                                            | Per-character progress segment |
| `profile.character_best_wave`    | Best wave {count}                                         | Per-character progress segment |

## Notes for translator

- Numeric values (e.g. `1234`, `5:05`, `2h 03m`) are rendered by code in invariant
  culture — they are not translated. Only the row **labels** need translation.
- `{count}` is a literal placeholder substituted at runtime by `ProfileScreenLogic.Substitute`.
- Keep stat-row labels under ~20 chars so the right-aligned numeric column stays
  on-screen at iPhone SE 3 widths.
- The fallback `profile.character_locked` is only used when a character has
  **no** `characters.<slug>.unlock_hint` key in the table — defensive against
  drift from `docs/02-gdd/03-characters.md`.

## Cross-refs

- `Assets/_Brave/UI/Documents/Profile.uxml`
- `Assets/_Brave/Code/UI/Controllers/ProfileController.cs`
- `Assets/_Brave/Code/Systems/Stats/LifetimeStatsService.cs`
- `docs/handoffs/wave9-loc-keys-needed.md` (pattern reference)

## Status

- [ ] EN entries committed
- [ ] TR / ID / PH translations
- [ ] Profile screen QA on iPhone SE 3 (numeric column overflow check)
