# economy.json — schema

> Source of truth for currency rates, carrot/shard drops, IAP catalog skeleton, battle pass tier rewards, daily missions, and the no-P2W audit flags.

## Top-level fields

| Field | Type | Notes |
|---|---|---|
| `schema_version` | string | "1.0" |
| `doc` | string | Human header. |
| `currencies` | object | The 3 currency definitions. |
| `carrot_per_kill` | object | Carrot drop per enemy role. |
| `soul_shard_drop_weights` | object | Weighted shard drops per kill type. |
| `soul_shard_to_carrot_exchange` | object | Exchange rate + daily cap. |
| `character_upgrade_carrot_cost` | array | L1..L30 cost ladder. |
| `character_unlock_costs` | array | Star costs per character. |
| `iap_catalog` | array | SKU table. |
| `cosmetic_prices` | object | Rarity-band prices. |
| `battle_pass` | object | Tier rewards. |
| `daily_missions` | array | Mission templates. |
| `arppu_target_usd_per_month` | int | 22. |
| `no_pay_to_win_audit` | object | Forbidden surfaces declaration. |

## Key sub-shapes

### `currencies.<id>`

| Field | Type | Notes |
|---|---|---|
| `id` | string | Stable slug. |
| `display_name` | string | Player-facing. |
| `premium` | bool | True for Stars only. |
| `earn_in_run` | bool | True for Carrots + Soul Shards. |

### `iap_catalog[]`

| Field | Type | Range | Notes |
|---|---|---|---|
| `sku` | string (kebab-case) | — | Stable identifier. |
| `price_usd` | float | [0.99, 19.99] | Hard cap $19.99. |
| `stars` | int (opt) | — | Stars granted (one-time SKUs). |
| `stars_per_day` | int (opt) | — | Subscription drip. |
| `duration_days` | int (opt) | — | Subscription / time-limited. |
| `extras` | array of string | — | Free-form bonuses (cosmetics, perma bonuses). |
| `one_time` / `subscription` | bool (opt) | — | Flags. |
| `window` | string (opt) | — | Time window if limited. |

### `battle_pass.tiers[]`

| Field | Type | Notes |
|---|---|---|
| `tier` | int | 1..30. |
| `free` | array of reward objects | Free-track rewards. |
| `premium` | array of reward objects | Premium-track rewards. |

Reward shapes vary: `{"item": "carrots", "value": N}`, `{"item": "stars", "value": N}`, `{"item": "soul-shards", "value": N}`, `{"item": "cosmetic-common"}`, `{"item": "weapon-unlock:<id>"}`, `{"item": "character-unlock:<id>"}`, `{"item": "character-shard-pull"}`, etc.

## Example entry expanded (IAP SKU)

```json
{
  "sku": "monthly-bunny-card",
  "price_usd": 4.99,
  "stars_per_day": 35,
  "duration_days": 30,
  "extras": [],
  "subscription": true
}
```

The monthly subscription: $4.99 for 30 days of 35 Stars/day = 1050 Stars/month total. Effective rate 210 Stars/$, vs single-purchase Starter Sprout at 50 Stars/$. ROI = 4.2× per `05-economy-tuning.md`.

## Example entry expanded (character upgrade)

```json
{"level": 10, "cost": 450, "cumulative": 2325}
```

Upgrading a character from L9 to L10 costs 450 carrots; cumulative cost L1→L10 is 2325 carrots. Total to L30 = 22025 carrots (matches `08-economy.md` ladder, ~31000 with the ramped costs at higher tiers).

## Validation rules

- `iap_catalog[].price_usd` ≤ 19.99 (no exceptions; ADR-worthy if proposed).
- `soul_shard_drop_weights[*].w` weights sum to 1.0 per kill type.
- `character_upgrade_carrot_cost[].cumulative` = cumulative sum of prior `cost` values.
- `no_pay_to_win_audit` flags must all stay `"forbidden"` for stat-buff/gacha/energy/region-exploits. Changing these is a brand-trust violation.
- `battle_pass.bp_xp_per_tier × tiers_total` should be reachable by an active F2P player in `season_length_days` per the math in `05-economy-tuning.md`.

## Cross-references

- `docs/02-gdd/08-economy.md` — design philosophy.
- `docs/02-gdd/02-meta-loop.md` — currency baseline + daily streak.
- `docs/02-gdd/09-monetization-design.md` — IAP catalog detail.
- `docs/10-balance/05-economy-tuning.md` — ARPPU math + subscription ROI.
- `docs/10-balance/00-formulas.md` § 7 (soul shards) + § 8 (carrots).
