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
