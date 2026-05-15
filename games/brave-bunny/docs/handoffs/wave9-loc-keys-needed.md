# Wave 9 — Localization keys needed

Owner: ui-engineer → localization-engineer
Date: 2026-05-16
Status: open

The Wave 9 Shop UI references the following loc keys. Add entries to
`_Brave/Localization/en.json` (canonical) and the three translated files
(`tr.json`, `id.json`, `ph.json`) before the soft-launch build.

## Keys

| Key | English copy | Notes |
|---|---|---|
| `shop.title` | Shop | Header label on Shop.uxml |
| `shop.tab_currency` | Coins | Currency-pack tab |
| `shop.tab_characters` | Heroes | Character-unlock tab |
| `shop.tab_specials` | Specials | Specials / Daily Deal / Remove Ads tab |
| `shop.tab_battle_pass` | Pass | Battle pass premium tab |
| `shop.buy` | Buy | Default CTA label when no price string available |
| `shop.owned` | Owned | Shown on one-time SKUs already purchased |
| `shop.empty` | Nothing on sale here yet. | Empty-state hint |
| `shop.restore` | Restore purchases | Reachable in 1 tap per US-54 |
| `shop.purchase_success` | Purchase complete. Thank you! | Toast |
| `shop.purchase_failed` | Purchase could not be completed. | Toast |

## Consumer files

- `unity/Assets/_Brave/UI/Documents/Shop.uxml` (loc-key attributes)
- `unity/Assets/_Brave/Code/UI/Controllers/ShopController.cs` (runtime `Loc.T`)

## Notes

- Loc style convention in this repo so far is SCREAMING_CASE (e.g. `BTN_BACK`).
  Wave 9 introduces dotted-lowercase per the task brief; localization-engineer
  should decide whether to normalize or accept the mixed style.
- Toasts are short; aim for ≤ 32 characters per the tone bible §3.
