# Wave 10 — Loc Keys Needed (handoff to loc-agent)

**From:** Wave 10 QoL agent (focus-pause + quit-confirm + FPS toggle)
**To:** loc-agent
**Date:** 2026-05-16
**Files to update:** `unity/Assets/_Brave/Localization/en.json`, `unity/Assets/_Brave/Localization/tr.json`

## QuitConfirmDialog modal (4 keys)

Wave 10 added a quit-confirm dialog interposed between the pause-modal Quit
button and the actual scene exit. Loc keys are referenced via `loc-key=` on
`QuitConfirmDialog.uxml`.

### English suggested copy (draft — loc-agent owns final wording)

```json
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
