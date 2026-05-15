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

## — Profile —

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
- [ ] Profile screen QA on iPhone SE 3 (numeric column overflow check)

## — Achievements —

# Wave 10 — Loc Keys Needed (handoff to loc-agent)

**From:** Wave 10 achievement-engineer
**To:** loc-agent
**Date:** 2026-05-16
**Files to update:** `unity/Assets/_Brave/Localization/en.json`, `unity/Assets/_Brave/Localization/tr.json`

## A — Achievement names + descriptions (40 keys)

Wave 10 added the 20 launch achievement entries to `Data/Definitions/Achievements/AchievementCatalog.asset`. Catalog ids drive the loc key suffix per the conventional pattern `achievement.<id>.name` + `achievement.<id>.description`.

### English suggested copy (draft — loc-agent owns final wording)

```json
"achievement.first-boss-kill.name": "First Strike",
"achievement.first-boss-kill.description": "Defeat your first boss.",

"achievement.slayer.name": "Slayer",
"achievement.slayer.description": "Defeat 1,000 enemies in total.",

"achievement.survivor.name": "Survivor",
"achievement.survivor.description": "Reach wave 50 in a single run.",

"achievement.untouchable.name": "Untouchable",
"achievement.untouchable.description": "Win a run without taking damage.",

"achievement.evolutionist.name": "Evolutionist",
"achievement.evolutionist.description": "Trigger your first weapon evolution.",

"achievement.completionist.name": "Completionist",
"achievement.completionist.description": "Claim all 7 daily rewards in a cycle.",

"achievement.streak-master.name": "Streak Master",
"achievement.streak-master.description": "Reach a 20-hit kill combo.",

"achievement.crit-lord.name": "Crit Lord",
"achievement.crit-lord.description": "Land 300 critical hits in total.",

"achievement.treasure-hunter.name": "Treasure Hunter",
"achievement.treasure-hunter.description": "Collect 10,000 gold over your lifetime.",

"achievement.star-collector.name": "Star Collector",
"achievement.star-collector.description": "Collect 100 stars over your lifetime.",

"achievement.variety.name": "Variety",
"achievement.variety.description": "Use six different weapons.",

"achievement.iron-player.name": "Iron Player",
"achievement.iron-player.description": "Play for one hour total.",

"achievement.marathon.name": "Marathon",
"achievement.marathon.description": "Survive a single run longer than 8 minutes.",

"achievement.speed-run.name": "Speed Run",
"achievement.speed-run.description": "Clear wave 30 in under 5 minutes.",

"achievement.premium-buyer.name": "Premium Buyer",
"achievement.premium-buyer.description": "Purchase the battle pass.",

"achievement.generous.name": "Generous",
"achievement.generous.description": "Spend 500 stars on character unlocks.",

"achievement.loyal.name": "Loyal",
"achievement.loyal.description": "Log in for 7 days in a row.",

"achievement.quest-master.name": "Quest Master",
"achievement.quest-master.description": "Claim 30 daily quests in total.",

"achievement.world-tour.name": "World Tour",
"achievement.world-tour.description": "Clear all 3 biomes at least once.",

"achievement.bossbane.name": "Bossbane",
"achievement.bossbane.description": "Defeat any boss 10 times."
```

## B — Achievement panel + toast chrome (5 keys)

```json
"achievement.panel.title": "Achievements",
"achievement.panel.claim": "Claim",
"achievement.panel.claimed": "Claimed",
"achievement.panel.locked": "Locked",
"achievement.toast.unlocked": "Achievement Unlocked!"
```

## Notes
- Catalog ids are stable slugs persisted in `SaveData.Achievements`. Do not rename without a save migration.
- 20 achievements × 2 keys (.name + .description) = 40 keys, plus 5 chrome keys = 45 total.
- Toast and panel both consult these keys at runtime through `Loc.T(...)`.
