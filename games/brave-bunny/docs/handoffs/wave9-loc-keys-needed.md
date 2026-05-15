# Wave 9 — Localization keys needed

Append-only ledger of new `loc-key` strings introduced by Wave 9 work.
The localization engineer (or ui-engineer's loc sub-role) consumes this file
to extend `_Brave/Localization/{lang}.json` tables.

## Daily Rewards (Wave 9 — daily-login calendar)

Source: `_Brave/UI/Documents/DailyRewards.uxml` + `Code/UI/Controllers/DailyRewardsController.cs`.

| Key            | English (fallback)        | Notes                                  |
|----------------|---------------------------|----------------------------------------|
| `daily.title`  | Daily rewards             | Modal title                            |
| `daily.day_1`  | Day 1                     | Reward cell label                      |
| `daily.day_2`  | Day 2                     | Reward cell label                      |
| `daily.day_3`  | Day 3                     | Reward cell label                      |
| `daily.day_4`  | Day 4                     | Reward cell label                      |
| `daily.day_5`  | Day 5                     | Reward cell label                      |
| `daily.day_6`  | Day 6                     | Reward cell label                      |
| `daily.day_7`  | Day 7                     | Milestone cell label                   |
| `daily.claim`  | Claim today's gift        | CTA button                             |
| `daily.claimed`| Come back tomorrow.       | Post-claim hint label                  |
