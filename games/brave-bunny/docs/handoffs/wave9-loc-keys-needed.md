# Wave 9 — Loc keys needed (LiveOps / Battle pass)

Owner: localizer (Phase-6 translation pass).
Consumed by: `Code/UI/Controllers/BattlePassController.cs` + `UI/Documents/BattlePass.uxml`.

Append to `_Brave/Localization/{lang}.json` for every supported language. The
controller falls back to the raw key when no entry exists (LocalizationProvider
echo behaviour), so missing translations stay screamingly visible to QA.

## Required keys

| Key                          | English source        | Notes |
|------------------------------|-----------------------|-------|
| `battlepass.title`           | Battle Pass           | Header title. |
| `battlepass.tier_1` … `battlepass.tier_30` | Tier 1 … Tier 30 | Per-tier label. 30 entries. |
| `battlepass.claim`           | Claim                 | Active CTA on cells. |
| `battlepass.claimed`         | Claimed               | Cell state when already taken. |
| `battlepass.locked`          | Locked                | Cell state when tier not reached or premium-gated. |
| `battlepass.row_free`        | Free                  | Free row label. |
| `battlepass.row_premium`     | Premium               | Premium row label. |
| `battlepass.activate_premium`| Activate              | Premium-pass activation CTA. |
| `battlepass.premium_active`  | Premium pass: active  | Premium-strip status text. |
| `battlepass.premium_locked`  | Premium pass: locked  | Premium-strip status text. |

## Generation hint

Tier keys can be bulk-emitted with a one-liner against the master string table
once the translator opens the file:

```jq
range(1;31) | "battlepass.tier_\(.)": "Tier \(.)"
```

## Status

- [ ] EN entries committed
- [ ] TR / ES / PT-BR pass-1 translations
- [ ] QA review of fallback (missing key) behaviour on the tier rail
