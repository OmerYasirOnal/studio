# ADR 0020 — Weapon archetype-config sidecar + `EnemyRole.Boss` enum value

**Date:** 2026-05-12
**Status:** accepted
**Owner:** tech-architect (Wave 5B dispatch — design-only)

## Context

ADR-0019 §"Decision Wave 5B" deferred two MED-severity content gaps
surfaced by the Wave 4 balance-engineer hand-off
(`docs/handoffs/balance-engineer-20260512-193850.md` §Blockers) to a
later tech-architect design dispatch. This ADR is that dispatch.

### Gap 1 — `EnemyRole` enum lacks `Boss`

`data/balance/enemies.json` ships `old-boar-king` with `"role": "boss"`,
but `Brave.Gameplay.Definitions.EnemyRole` only declares
`Swarmer | Tank | Ranged | Elite`. The importer
(`BalanceJsonImporter.ImportEnemies`) logs a warning and silently
defaults the role to `Swarmer` — wrong. Boss spawning is fully blocked.

### Gap 2 — `WeaponDefinition` lacks archetype-specific fields

`WeaponLevelData` carries only `damage`, `fireRate`, `range`,
`projectiles`, `upgradeFlavor`. Six of the 12 launch weapons (plus
Daisy Mine, which is one of the three vertical-slice weapons) need
extra archetype-shaped configuration:

| Weapon | Archetype | Missing field(s) | JSON key |
|---|---|---|---|
| Daisy Mine (vertical slice) | Area-mine | `arm_time_ms` | `arm_time_ms` (top-level + L3 perk) |
| Thunder Cloud | Area-cloud | `cloud_lifetime_ms`, `zaps_per_cloud` | both top-level + L2/L4 perks |
| Honey Aura | Aura | `cloud_lifetime_ms` analogue (tick duration) | implicit — driven by `rate_ms` today; future-proof |
| Acorn Cannon | Splash-projectile | `splash_units_base` | L5 perk `dmg_and_splash` |
| Cob Mortar | Splash-area | `splash_units_base`, `travel_ms` | top-level + L2 perk |
| Frost Whisper | Aura-slow | `slow_pct_base` | top-level + L2/L4 perks |
| Whirligig | Orbit-projectile | `lifetime_ms` semantics | not in JSON yet — orbit weapons need non-projectile lifetime |
| Tumbleweed (returning) | Summon | `lifetime_ms` | top-level + L2 perk |

Most of these fields live **at the weapon top-level in JSON**, not
per-level (L1 ships an `arm_time_ms`, then L3 perks override it). The
existing `WeaponLevelData` has no slot for any of them. The importer
currently drops them on the floor.

### Constraint: ADR-0009 registry pattern, not SerializeReference

ADR-0009 governs polymorphic gameplay-mechanic types
(`SignatureMechanic`, `BossAttackPattern`, `AchievementCondition`) via
a `[BraveRegister("typename")]` reflection registry. SerializeReference
was explicitly rejected for save-compat + editor-stability reasons.
Any polymorphism we add to weapon configuration must respect that
rejection.

### Constraint: 3 vertical-slice weapons must not break

Carrot Boomerang, Sunbeam, and Daisy Mine are the vertical-slice
roster. The current `WeaponLevelData` shape satisfies Carrot Boomerang
and Sunbeam fully; Daisy Mine works **except** for `arm_time_ms`. The
new design must not regress any of these three's tests.

### Constraint: importer must remain tractable

`BalanceJsonImporter` already round-trips 60 SO fields per weapon (5
levels × 12 weapons). A redesign that requires the JSON to change
shape is rejected — the JSON is balance-engineer's source of truth.
The importer maps; the design here must map cleanly **from the
existing JSON**.

## Decision

**Option B — `WeaponArchetypeConfig` sidecar ScriptableObject, one
concrete subclass per archetype, resolved by archetype type-name string
via the existing `[BraveRegister]` registry pattern.**

### Shape

- A new abstract `WeaponArchetypeConfig : ScriptableObject` base lives
  in `Code/Gameplay/Definitions/Archetypes/`.
- One concrete subclass per archetype variant carries only the fields
  that archetype actually needs:
  - `ProjectileArchetypeConfig` — empty for now (Carrot Boomerang,
    Pebble Sling base case); future home for `pierce_default`,
    `bounce_default`.
  - `BeamArchetypeConfig` — empty for now (Sunbeam base case); future
    home for `beam_width_units`, `sweep_lock_seconds`.
  - `MineArchetypeConfig` — `armTimeMs : int`.
  - `CloudArchetypeConfig` — `cloudLifetimeMs : int`,
    `zapsPerCloud : int`.
  - `SplashProjectileArchetypeConfig` — `splashUnitsBase : float`,
    `travelMs : int` (the `travel_ms` Cob Mortar already ships).
  - `AuraArchetypeConfig` — `slowPctBase : float`,
    `tickLifetimeMs : int` (covers Honey Aura tick duration too).
  - `SummonArchetypeConfig` — `lifetimeMs : int` (Tumbleweed,
    Whirligig orbit-lifetime).
- Each subclass carries a `[BraveRegister("weapon.archetype.mine")]`
  attribute (or equivalent string keyed off the JSON `archetype` value
  + a discriminator suffix where the JSON `archetype` is too coarse —
  e.g. `"area"` in JSON covers Daisy Mine *and* Thunder Cloud *and*
  Cob Mortar, so the importer disambiguates via the presence of
  archetype-distinct fields like `arm_time_ms` vs `cloud_lifetime_ms`).
- `WeaponDefinition` gains:
  ```text
  public WeaponArchetypeConfig? archetypeConfig;
  ```
  (single nullable ScriptableObject reference — same shape as the
  existing `WeaponEvolutionRecipe? evolution`).
- Per-level **overrides** of archetype fields (the L3 `arm_time_ms`
  perk on Daisy Mine, the L4 `cloud_lifetime_ms` perk on Thunder Cloud)
  do **not** go in `WeaponLevelData`. They go in a parallel
  per-level structure inside the archetype subclass — e.g.
  `MineArchetypeConfig` carries `armTimeMsPerLevel : int[5]` mirroring
  `WeaponLevelData.damage` semantics. The importer fills both arrays
  in the same loop.

### `EnemyRole.Boss = 4`

Trivial addition to the existing enum:

- `EnemyRole.Boss = 4` appended to the existing list.
- The single switch-table caller — `WaveSpawner.cs:47-50` — needs a
  `Boss` arm (likely `bossCapacity` field, value `1` since per the
  GDD only one boss spawns at a time per biome). Out of scope for
  this ADR; flagged for the gameplay-engineer follow-up.
- `EnemyDefinition.OnValidate` already requires a non-zero
  `telegraphWindowSeconds` for non-Swarmer roles, which `Boss` will
  inherit correctly (boss has 0.8 s telegraph per ADR-0006).

### Why Option B (and not A or C)

- **Option A (fat `WeaponLevelData`)** — junk drawer. 60 unused
  `arm_time_ms`/`splash_units`/`slow_pct_base` slots across the 5
  levels of every weapon that doesn't use them. Inspector becomes
  unreadable. Worse, it conflates *per-level* mechanics (damage,
  fire-rate) with *per-weapon* mechanics (arm-time, cloud-lifetime
  ARE per-weapon, with rare per-level perk overrides).
- **Option C (named struct in `WeaponLevelData`)** — same junk drawer,
  just renamed. Doesn't address the per-weapon vs per-level mismatch.
- **Option B** — exactly the shape ADR-0009 chose for mechanics:
  type-name string + registry, with one concrete class per variant
  carrying only its own fields. Inspector shows only the relevant
  archetype's fields. Importer dispatches on JSON `archetype` +
  disambiguator. Save-compat: the type-name is stable, not a CLR-FQN
  hash. The "registry over SerializeReference" pillar of ADR-0009 is
  honoured.

## Consequences

### Files that change (paths only — no code here, gameplay-engineer's
later dispatch implements)

- `unity/Assets/_Brave/Code/Gameplay/Definitions/EnemyRole.cs` — add
  `Boss = 4`.
- `unity/Assets/_Brave/Code/Gameplay/Definitions/WeaponDefinition.cs`
  — add `public WeaponArchetypeConfig? archetypeConfig;`.
- `unity/Assets/_Brave/Code/Gameplay/Definitions/Archetypes/` (new
  folder) — base abstract `WeaponArchetypeConfig.cs` + seven concrete
  subclasses (`ProjectileArchetypeConfig`, `BeamArchetypeConfig`,
  `MineArchetypeConfig`, `CloudArchetypeConfig`,
  `SplashProjectileArchetypeConfig`, `AuraArchetypeConfig`,
  `SummonArchetypeConfig`) each tagged with `[BraveRegister(...)]`.
- `unity/Assets/_Brave/Code/Gameplay/Spawning/WaveSpawner.cs:47-50`
  — add `EnemyRole.Boss => bossCapacity` arm + new
  `[SerializeField] int bossCapacity = 1;` field. (Switch-table audit
  found this is the only `EnemyRole` switch in production code today.)

### Files the importer (`BalanceJsonImporter`) must update

- `ImportWeapons`: for each weapon, after creating the
  `WeaponDefinition` asset, inspect the JSON top-level keys
  (`arm_time_ms`, `cloud_lifetime_ms` + `zaps_per_cloud`,
  `splash_units_base`, `slow_pct_base`, `lifetime_ms`) to pick the
  archetype-config subclass. Create the matching SO under
  `Assets/_Brave/Data/Balance/Archetypes/Weapon_<id>_archetype.asset`
  and assign it to `WeaponDefinition.archetypeConfig`.
- Per-level perk overrides (`arm_time_ms` L3, `cloud_lifetime_ms` L4,
  `zaps_per_cloud` L2, `splash_units` L2/L5, `slow_pct` L2,
  `lifetime_ms` L2) populate the matching `*PerLevel` array on the
  archetype-config subclass using the same carry-forward semantics
  the existing importer applies to `fireRate` and `range`.
- `ImportEnemies`: `ApplyEnumByJsonString("role", "boss", EnemyRole.Boss)`
  now succeeds for `old-boar-king` (no code change needed — once the
  enum value exists the existing `NormalizeEnum` helper resolves it).

### What gameplay-engineer's later implementation dispatch delivers

1. Define the abstract base + 7 subclasses with the fields listed
   above; tag each with `[BraveRegister("weapon.archetype.<name>")]`.
2. Add `archetypeConfig` to `WeaponDefinition`.
3. Add `EnemyRole.Boss = 4`; update `WaveSpawner` switch arm +
   `bossCapacity` field.
4. Extend `BalanceJsonImporter` per the mapping above.
5. Combat code (existing `Weapon.cs` family) reads
   `WeaponDefinition.archetypeConfig` and casts to the expected
   subclass per archetype. EditMode test asserts the cast succeeds
   for all 12 weapons after import.
6. Add `Tests/EditMode/Gameplay/Definitions/WeaponArchetypeConfigTests.cs`
   asserting:
   - All 12 weapon SOs have a non-null `archetypeConfig` after import.
   - For Daisy Mine: `((MineArchetypeConfig)cfg).armTimeMs == 1000`.
   - For Thunder Cloud: `cloudLifetimeMs == 4000`, `zapsPerCloud == 3`.
   - For Frost Whisper: `slowPctBase == 0.10f`.
   - For Acorn Cannon L5: `splashUnitsBase == 1.0f` carry-forward.
   - For Cob Mortar L2: `splashUnitsBase == 2.0f` perk override.
7. Add `Tests/EditMode/Gameplay/Definitions/EnemyRoleTests.cs` arm for
   `Boss` reading from `Enemy_old-boar-king.asset`.

### What stays the same (the 3 vertical-slice weapons)

- **Carrot Boomerang** — gets a `ProjectileArchetypeConfig` (empty
  base case). All 5 levels unchanged. Existing AutoAttack tests pass.
- **Sunbeam** — gets a `BeamArchetypeConfig` (empty for now). All 5
  levels unchanged.
- **Daisy Mine** — gets a `MineArchetypeConfig` with
  `armTimeMs = 1000` and `armTimeMsPerLevel = {1000, 1000, 500, 500,
  500}` (L3 perk halves it). All 5 levels' damage/rate/range
  unchanged. **The arm-time semantics that were previously
  hardcoded-or-missing now live in data.**

### What the importer needs to do (concise checklist)

- [ ] Detect archetype-config subclass from JSON archetype + key
  presence.
- [ ] Create + persist `Weapon_<id>_archetype.asset` per weapon.
- [ ] Carry-forward per-level overrides into the subclass's per-level
  array.
- [ ] Wire `WeaponDefinition.archetypeConfig` reference.
- [ ] Map `role: "boss"` → `EnemyRole.Boss` (works automatically once
  the enum value exists).

### Test surface that needs to exist after implementation

- `MechanicRegistryTests` (existing) extended to scan
  `WeaponArchetypeConfig` subclasses and assert no duplicate
  type-name strings.
- `WeaponArchetypeConfigTests` (new) per §"implementation dispatch
  delivers".
- `EnemyRoleTests` (new) covers the `Boss` value + `WaveSpawner`
  switch arm.
- Existing `AutoAttackController` + `Weapon` tests for Carrot
  Boomerang must still pass unmodified (`archetypeConfig` is an
  additive field; existing tests don't reference it).

## Resolution criteria (when to close this ADR)

- `EnemyRole.Boss` value exists; `WaveSpawner` switch table includes
  the `Boss` arm; switch-table audit (`grep -rn 'EnemyRole\.'
  Code/Gameplay/`) finds no fall-through default that silently maps
  Boss → Swarmer.
- `WeaponDefinition.archetypeConfig` exists; all 12 weapon SOs
  round-trip from `data/balance/weapons.json` through
  `BalanceJsonImporter` with a non-null `archetypeConfig` of the
  correct subclass.
- `Enemy_old-boar-king.asset` has `role == EnemyRole.Boss` after
  re-import.
- The 3 vertical-slice weapons (Carrot Boomerang, Sunbeam, Daisy
  Mine) still pass all existing AutoAttack + Combat tests with no
  test modifications other than additive `archetypeConfig`
  assertions.
- `MechanicRegistryTests` confirms every `[BraveRegister]` on
  `WeaponArchetypeConfig` subclasses resolves at scan-time.

## Alternatives considered

1. **Option A — fat `WeaponLevelData` with optional nullable fields.**
   Rejected. Becomes a junk-drawer of half-applicable fields; inspector
   is unreadable for weapons that don't use a given field; conflates
   per-weapon archetype constants with per-level stat rows
   (`arm_time_ms` is a per-weapon constant, not a per-level stat).
   Also produces inspector-time confusion that has already bitten
   ADR-0007 (charm-consumption fields that didn't apply to every
   passive).
2. **Option C — flat `WeaponArchetypeData` struct held inside
   `WeaponLevelData`.** Rejected. Still a junk drawer, just renamed.
   Doesn't address the per-weapon vs per-level mismatch. Saves one
   asset file per weapon at the cost of all the inspector clarity
   Option B buys.
3. **`[SerializeReference]` on `WeaponDefinition`.** Rejected at the
   ADR-0009 level — registry-based polymorphism is the project
   pattern; SerializeReference is rejected for save-compat +
   editor-stability reasons that apply equally here.
4. **Per-weapon hardcoded `Weapon` subclass with no data binding.**
   Rejected. Defeats balance-engineer's whole pipeline; would
   require recompiles for every tuning change.
5. **Defer to Phase 6 (no design at all).** Rejected. The 6 weapons
   beyond the vertical slice are Phase-6 ramp-up content; the design
   work needs to land in Phase 5 so Phase 6 can implement against a
   stable contract.

## References

- ADR-0009 (polymorphic mechanics via type-name registry — the
  pattern this ADR extends to static config)
- ADR-0018 (XZ-plane migration; orthogonal but recently-touched
  area)
- ADR-0019 (Wave 4 cleanup debt — the parent ADR authorising this
  design dispatch)
- `unity/Assets/_Brave/Code/Gameplay/Definitions/WeaponDefinition.cs`
- `unity/Assets/_Brave/Code/Gameplay/Definitions/WeaponLevelData.cs`
- `unity/Assets/_Brave/Code/Gameplay/Definitions/EnemyRole.cs`
- `unity/Assets/_Brave/Code/Gameplay/Definitions/EnemyDefinition.cs`
- `unity/Assets/_Brave/Code/Gameplay/Spawning/WaveSpawner.cs` (the
  only `EnemyRole` switch-table caller in production code)
- `unity/Assets/_Brave/Code/Gameplay/Combat/MechanicRegistry.cs` (the
  reference implementation for the registry pattern this ADR reuses)
- `data/balance/weapons.json` (importer source-of-truth)
- `docs/02-gdd/04-weapons.md` (mechanic descriptions per archetype)
- `docs/handoffs/balance-engineer-20260512-193850.md` (importer-side
  mapping table that the new importer extension must respect)
