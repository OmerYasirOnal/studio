# Wave 9 — Loc keys needed (handoff to loc-agent)

> Wave 9 added 14 new enemy entries to `data/balance/enemies.json` (Beach + Cavern rosters). Each ships a `display_name` field that the HUD reads via a localization key in the form `enemies.<id>.name`. Per the localization pattern in `Code/UI/Theming/LocalizationProvider.cs`, every player-facing string MUST resolve through a key — so the loc-agent needs to add the keys below to `Assets/Resources/Localization/en.json` (and copy-translate `tr.json`).

## Keys to add

```text
enemies.crab.name
enemies.gull.name
enemies.sand-puff.name
enemies.mosquito.name
enemies.bog-boar.name
enemies.throw-frog.name
enemies.big-hermit-crab.name
enemies.bat-mini.name
enemies.glow-bug.name
enemies.rock-tumble.name
enemies.cave-slime.name
enemies.stone-ox.name
enemies.crystal-slinger.name
enemies.stalagmite-walker.name
```

## Suggested English copy (from `display_name` in `enemies.json`)

| Key | English |
|---|---|
| `enemies.crab.name`              | Crab |
| `enemies.gull.name`              | Gull |
| `enemies.sand-puff.name`         | Sand-Puff |
| `enemies.mosquito.name`          | Mosquito |
| `enemies.bog-boar.name`          | Bog Boar |
| `enemies.throw-frog.name`        | Throw Frog |
| `enemies.big-hermit-crab.name`   | Big Hermit Crab |
| `enemies.bat-mini.name`          | Bat-Mini |
| `enemies.glow-bug.name`          | Glow-Bug |
| `enemies.rock-tumble.name`       | Rock-Tumble |
| `enemies.cave-slime.name`        | Cave-Slime |
| `enemies.stone-ox.name`          | Stone Ox |
| `enemies.crystal-slinger.name`   | Crystal Slinger |
| `enemies.stalagmite-walker.name` | Stalagmite Walker |

## Notes for the translator (`tr.json`)

* Keep the rascal-tone register from `docs/02-gdd/narrative/00-tone-bible.md` — Beach is "wee beasties" register, Cavern is "troublemakers" register.
* `sand-puff`, `bat-mini`, `glow-bug`, `cave-slime` are diminutives; preserve the affectionate feel in Turkish (suffixes like `-cik`/`-ciğim` are appropriate).
* `Big Hermit Crab` should land closer to "Koca Keşiş Yengeç" than literal "Büyük" — see existing `big-onion` → `Koca Soğan` pattern.

## Cross-refs

* `data/balance/enemies.json`            — source of truth for `display_name`
* `Code/UI/Theming/LocalizationProvider.cs` — `Resources.Load<TextAsset>("Localization/<lang>")`
* `docs/02-gdd/06-biomes.md`             — Beach + Cavern enemy variant tables
