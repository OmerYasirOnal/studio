# ASC Metadata Schema — Brave Bunny: Survivors

> Authoritative description of the JSON files in this directory. Each locale
> file (`en-US.json`, `tr-TR.json`, …) is one row in the App Store Connect
> metadata table; the schema documented here mirrors the fields fastlane
> `deliver` expects under `fastlane/metadata/<locale>/<field>.txt`.
>
> Cross-refs:
> - Tone bible: `docs/02-gdd/narrative/00-tone-bible.md` (voice rules)
> - ADR-0016: `docs/decisions/0016-app-store-display-name.md` (`name` is locked)
> - Fastlane: `tools/ci/fastlane/Fastfile` (constants reused: APP_IDENTIFIER, etc.)
> - Upload runbook: `tools/ci/scripts/asc-upload-metadata.sh`
> - Apple reference: <https://developer.apple.com/help/app-store-connect/reference/app-information>

## 1. File naming convention

`<bcp-47-locale>.json` — one file per ASC-supported locale. Locale codes
follow Apple's ASC list (e.g. `en-US`, `tr-TR`, not `en` / `tr`).

Wave 11 ships:
- `en-US.json` (primary, ADR-0016)
- `tr-TR.json` (soft-launch market per `GAME.md :: soft_launch_markets`)

Deferred (per cut-list #3): `tl-PH`, `id-ID`. English fallback is acceptable
until narrative-designer ships their copy.

## 2. Required top-level fields

| Field             | Type   | ASC limit | Maps to                                | Notes |
|-------------------|--------|-----------|----------------------------------------|-------|
| `name`            | string | 30 chars  | App Information → Name                 | Display name on the store front. **Locked to `Brave Bunny: Survivors` per ADR-0016.** |
| `subtitle`        | string | 30 chars  | App Information → Subtitle             | Short tagline under the name. Reads on the search result row. |
| `promotional_text`| string | 170 chars | App Information → Promotional Text     | Optional but recommended. Editable WITHOUT a binary update. Use for short campaigns. |
| `description`     | string | 4000 chars| App Information → Description          | Long-form. Plain text only; ASC strips HTML. Newlines respected; we use them for FEATURES / WHY PLAY / SUPPORT sections. |
| `keywords`        | string | 100 chars | App Information → Keywords             | **Comma-separated, no spaces around commas** (ASC docs are explicit). Hidden from users; powers search. |
| `release_notes`   | string | 4000 chars| Version → What's New                   | The "What's New in This Version" string. Required for every version submission. |
| `support_url`     | URL    | 255 chars | App Information → Support URL          | Required by ASC. Must be reachable HTTPS. |
| `marketing_url`   | URL    | 255 chars | App Information → Marketing URL        | Optional but expected. Falls back to the App Store page if empty. |
| `privacy_url`     | URL    | 255 chars | App Information → Privacy Policy URL   | Required for any app collecting user data; required for any app using AdMob. |

## 3. Optional fields (tolerated by upload script)

| Field             | Type   | Notes |
|-------------------|--------|-------|
| `apple_tv_privacy_policy` | string | tvOS only — not used for Brave Bunny. |
| `copyright`       | string | Defaults to "© 2026 Ömer Yasir Önal" via Fastfile constants if absent. |
| `primary_category`| string | Set once at submission time via the App Store Connect dashboard, not via fastlane deliver. |

## 4. Underscored bookkeeping fields

Fields starting with `_` are **not** uploaded — they are documentation that
travels with the file:

| Field               | Purpose |
|---------------------|---------|
| `_comment`          | Free-text purpose statement. |
| `_schema_version`   | Bumped when this file changes. Consumers should read `_schema_version` for compatibility checks. |
| `_locale`           | BCP-47 code — must match the filename basename. |
| `_authored_by`      | Comma-separated role list. |
| `_authored_at`      | ISO-8601 date. |
| `_adr_refs`         | ADR numbers this row depends on. |
| `_field_lengths`    | Hand-maintained length report — useful in code review. Authoritative count is whatever `wc -c` says. |
| `_todo_validation`  | Action items the next dispatch should run before submission. |

## 5. Character-limit enforcement

`tools/ci/scripts/asc-upload-metadata.sh` enforces limits **before** uploading.
The script fails fast (exit 1) if any field exceeds its limit, so we never
push a payload ASC will reject for length. Limits enforced in the script
today are: `name ≤ 30`, `subtitle ≤ 30`, `keywords ≤ 100`, `description ≤ 4000`.

`promotional_text` (170) and `release_notes` (4000) are documented here but
not gated in v1; add gates if either string ever approaches its limit.

## 6. Tone-bible compliance

All EN strings must pass the `narrative/00-tone-bible.md §2` vocabulary check:
no `kill / slay / die / epic / lol`, no emoji in copy, sentences ≤ 18 words.

Turkish strings follow the same register — kitchen-table cheer, fond
narrator, no shouting. The TR file should be reviewed by a native speaker
before each version submission (narrative-designer dispatch).

## 7. Upload workflow

```text
edit en-US.json / tr-TR.json
       │
       ▼
asc-upload-metadata.sh --dry-run    (validates JSON + length limits)
       │
       ▼
asc-upload-metadata.sh              (materialises fastlane metadata/ tree
                                     in a temp dir, invokes fastlane deliver
                                     with --skip_binary_upload --skip_screenshots)
       │
       ▼
App Store Connect dashboard         (review the queued changes; submit when ready)
```

## 8. Adding a new locale

1. Copy `en-US.json` to `<new-locale>.json` (e.g. `id-ID.json`).
2. Translate every non-`_` string. Keep newlines and section headers.
3. Update `_locale`, `_authored_by`, `_authored_at`.
4. Add `<new-locale>` to the `LOCALES` array in `asc-upload-metadata.sh`.
5. Run `asc-upload-metadata.sh --dry-run` to confirm limits pass.
6. Open a PR; tag narrative-designer for review.

## 9. Schema validation TODO

Apple does not publish a JSON Schema for ASC metadata. Validation today is:

- Field presence / length checks in `asc-upload-metadata.sh`.
- Manual sanity-read by a human reviewer.

Once we add a CI lane that calls `fastlane deliver --validate-only` against
a sandbox app record we will have machine-checked compliance. Until then,
the `_todo_validation` field in each locale file is the breadcrumb that
keeps this on the radar.
