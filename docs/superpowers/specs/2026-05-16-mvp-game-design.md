# MVP Game — Brave Bunny playable build

> **Status:** Approved (high-level) — awaits spec review and implementation plan.
> **Date:** 2026-05-16
> **Author:** Claude (with Ömer Yasir Önal)
> **Builds on:** `docs/superpowers/specs/2026-05-16-engine-pivot-design.md` (engine choice + folder layout + perf rules)
> **Ships as:** TestFlight Build #7 (continuing from Build #6 = rotating cube)

---

## 1. Context

Build #6 shipped to TestFlight but contains only an empty R3F scene with a rotating cube. The engine + iOS pipeline is proven; now we need an actual playable game on top. Sprint A delivered the runtime shell (Vite + R3F + Capacitor); this MVP delivers the first version of the game that someone can pick up and play.

**Target:** A 5-minute Survivor.io-flavored run with one playable hero, two simultaneous auto-attack weapons, three enemy archetypes, XP-driven leveling with a 3-of-N upgrade draft, and seven UI screens (Boot, Lobby, HUD, Draft, EndRun, Profile, Settings).

**Not in scope:** Boss fight, meta progression beyond a single save, in-app purchases, multiple biomes, multiple characters, full 200-enemy stress test (relaxed to 60), monetization SDKs (AdMob/IAP).

---

## 2. Goals & non-goals

### Goals

1. **Playable in one tap.** Open the app → Lobby → Play → in a real 5-min run within 5 seconds of launch.
2. **Real assets.** Hero and enemies are Quaternius CC0 animals, not capsules. Carrot Fields biome decoration from Kenney Nature Kit.
3. **Full UI shell.** 7 screens implemented with consistent visual language (rounded 16px corners, hot-coral accent on hero/CTA, soft purple ground).
4. **Lossy but feels.** SFX on hits, level-ups, pickups; one looping BGM track.
5. **Saves persist.** `metaStore` writes via `@capacitor/preferences`; total runs, best kill count, total gold banked survive across launches.
6. **iOS-first ship.** Build #7 on TestFlight with all of the above. Aesthetic relaxations for MVP are explicit (no boss, no full perf target).

### Non-goals

1. **No VAT pipeline yet.** Stock R3F `SkinnedMesh` for enemies. VAT bake lands in a later plan once enemy count crosses 60 on iPhone 12.
2. **No physics solver.** Manual AABB grid for hero ↔ enemy ↔ projectile collision; rapier deferred.
3. **No multiplayer, no leaderboard, no analytics.**
4. **No App Store submission.** TestFlight internal testing only (matches existing setup since Build #5).
5. **No new monetization plumbing.** Settings has [Reset save]; no Restore Purchases yet.

---

## 3. Architecture overview

```
┌───────────────────────────────────────────────────────────────┐
│                     iOS App (WKWebView)                       │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │                React (HTML overlay)                     │  │
│  │   Boot │ Lobby │ HUD │ Draft │ EndRun │ Profile │ Set   │  │
│  │                            ↕                            │  │
│  │                      zustand stores                     │  │
│  │             runStore │ metaStore │ settingsStore        │  │
│  └─────────────────────────────────────────────────────────┘  │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │                R3F <Canvas>  (3D world)                 │  │
│  │   ┌─ miniplex world ─────────────────────────────────┐  │  │
│  │   │  systems: input → movement → spawn → AI →        │  │  │
│  │   │  combat → collision → pickup → lifecycle         │  │  │
│  │   └──────────────────────────────────────────────────┘  │  │
│  │   render: Hero · EnemySwarm · ProjectileSwarm ·         │  │
│  │           VFXSwarm · Biome · Camera                     │  │
│  └─────────────────────────────────────────────────────────┘  │
│  Web Audio API (pre-decoded AudioBuffer pool)                 │
│  Capacitor: Preferences (save), no AdMob/IAP yet              │
└───────────────────────────────────────────────────────────────┘
```

**Screen state machine** (lives in `runStore.phase`):
```
boot → lobby → run ⇄ draft (paused) → run → endrun → lobby
                                          ↑
                                hero dies or 5-min timer
```

`profile` and `settings` are modal overlays accessible from lobby. Pressing the system back gesture during `run` triggers a pause modal (deferred to polish — for MVP, no pause).

---

## 4. Screen specifications

| Screen | Render | Mounts on |
|---|---|---|
| **Boot** | App logo + spinner | `phase === 'boot'`; 1.5s splash then `phase = 'lobby'` |
| **Lobby** | Title, [▶ PLAY] big button center, [Profile] [Settings] icon row bottom | `phase === 'lobby'` |
| **HUD** | top-left HP bar, top-center timer + kill counter, bottom XP bar with level number, left thumb joystick | `phase === 'run'` |
| **DraftModal** | 3 cards horizontally, each shows upgrade icon + name + description, tap to pick → resume | `phase === 'draft'` |
| **EndRunSummary** | Big "Run complete" header, stats grid (kills, time survived, XP earned, gold banked), [▶ RESTART] [⌂ LOBBY] | `phase === 'endrun'` |
| **Profile** | Career stats: total runs, best kills, total gold banked, total time | overlay, dismiss on tap outside |
| **Settings** | BGM volume slider, SFX volume slider, [Reset save] danger button | overlay, dismiss on tap outside |

**Visual language:**
- Background: `#1a0d2e` (soft purple) for shell, `#9be37c` (saturated grass) for in-run ground
- Hero/CTA accent: `#ff6f3c` (hot coral)
- Pickups: emissive yellow-green
- Rounded corners: 16px on all containers, 24px on big CTAs
- Font: system-ui, weight 700 for headlines, 500 for body

---

## 5. Gameplay mechanics

### 5.1 Hero (Bunny)

- Mesh: Quaternius Universal Animation Library 2 humanoid rig + Bunny skin
- HP: 100, regen 0/s for MVP
- Movement: 4u/s (Unity units = three.js units 1:1 for our scale)
- Camera: top-down 3/4 perspective; FOV 35°, pitch 55° down, distance 18u, smoothed 0.15s follow
- Animations: `Idle`, `Run`, `Hit`, `Death` (UAL2 ships these)

### 5.2 Weapons (always-on auto-attack, 2 concurrent)

| Weapon | Type | Range | Damage | Tick | Notes |
|---|---|---|---|---|---|
| **Carrot Spear** | Melee cone | 2u front | 15 | 0.6s | 60° cone; hits all enemies in arc |
| **Pebble Sling** | Auto-target projectile | 8u | 10 | 0.4s | targets nearest enemy; pooled `InstancedMesh` quad billboards |

Both fire from Day 1. Draft upgrades can buff or evolve them.

### 5.3 Enemies (3 archetypes)

| Archetype | Mesh | HP | Speed | Damage on touch | Spawn weight (first 30s) |
|---|---|---|---|---|---|
| **Slime** | Quaternius animated slug/slime | 10 | 2u/s | 5 | 0.8 |
| **Wolf** | Quaternius animated wolf | 25 | 4u/s | 10 | 0.15 |
| **Mushroom** | Quaternius animated mushroom | 60 | 1u/s | 15 | 0.05 |

AI: simple seek-toward-hero (no path finding); spawn at random points outside camera frustum + 2u padding; despawn if >40u from hero.

### 5.4 XP + Leveling

- On enemy death: spawn XP gem at corpse position
- Pickup: magnet radius 2u (auto-pulled when hero enters)
- XP per gem: matches enemy XP value (Slime 2, Wolf 5, Mushroom 12)
- Level-up threshold: `xp_for_level(n) = 10 + n*5` cumulative
- Max level for MVP: 20

### 5.5 Draft (level-up modal)

On level up: pause game (`phase = 'draft'`), show 3-of-N from the pool:

| Upgrade | Effect | Max stacks |
|---|---|---|
| Spear Damage+ | +5 damage | 5 |
| Sling Damage+ | +3 damage | 5 |
| HP+ | +20 max HP | 5 |
| Move Speed+ | +0.5u/s | 3 |
| Magnet+ | +1u magnet radius | 3 |
| Attack Rate+ | -0.05s tick for both weapons | 4 |

Player picks 1 of 3 random offers; modal closes; `phase = 'run'`. If max-stacked on all 3 offered, re-roll once; if still no progress, pick first offer automatically (edge case).

### 5.6 Death + restart

- Hero HP ≤ 0 → trigger `Death` anim (0.8s) → `phase = 'endrun'`
- EndRun summary computes: kills, time survived, XP earned, gold banked (`gold = floor(kills * 1.5 + (time_seconds / 10))`)
- Banked gold added to `metaStore.totalGold`; run stats added to `metaStore` (total runs, etc.)
- [RESTART] → reset run state, `phase = 'run'`; [LOBBY] → `phase = 'lobby'`

### 5.7 Win condition

- 5-min timer survival → `phase = 'endrun'` with bonus banked gold (`time_bonus = 50`)
- Same EndRun summary path

---

## 6. Data model

### 6.1 Entity components (miniplex)

```ts
type Entity = {
  // Spatial
  position?: Vec3;
  velocity?: Vec3;
  rotation?: number; // y-axis only, radians

  // Render
  meshRef?: Ref<Object3D>;
  archetype?: 'hero' | 'slime' | 'wolf' | 'mushroom' | 'projectile' | 'pickup' | 'vfx';
  modelKey?: string; // for instanced render lookup

  // Combat
  hp?: number;
  maxHp?: number;
  damage?: number;
  team?: 'hero' | 'enemy';

  // Behavior
  movement?: 'seek-hero' | 'projectile' | 'pickup-magnet' | 'none';
  speed?: number;
  ttl?: number; // for projectiles + vfx, seconds

  // Combat tick
  weapons?: WeaponInstance[];

  // Pickup
  xpValue?: number;
};

type WeaponInstance = {
  kind: 'spear' | 'sling';
  damage: number;
  tickInterval: number;
  cooldown: number; // seconds remaining
  level: number;
};
```

### 6.2 Balance JSON (re-uses existing `docs/10-balance/` data; `tools/assets/balance-sync.mjs` copies to `app/src/data/`)

- `weapons.json` — Carrot Spear, Pebble Sling stats per level
- `enemies.json` — Slime, Wolf, Mushroom stats
- `waves.json` — spawn rate ramp over 5 min (existing)
- `characters.json` — Bunny stats + palette

### 6.3 Save schema

```ts
type Save = {
  version: 1;
  totalRuns: number;
  bestKills: number;
  longestRun: number; // seconds
  totalGold: number;
  totalXpEarned: number;
  settings: {
    bgmVolume: number; // 0-1
    sfxVolume: number; // 0-1
  };
};
```

Stored under `@capacitor/preferences` key `brave-bunny.save.v1`. Schema versioned for migrations.

---

## 7. Asset pipeline (MVP — relaxed)

### 7.1 Sources

- **Hero**: Quaternius UAL2 humanoid rig + Bunny skin → `app/assets/glb/heroes.glb`
- **Enemies**: Quaternius Ultimate Animated Animal Pack → `Wolf.glb`, plus Quaternius Animated Animals smaller pack for Slime + Mushroom (if Mushroom isn't in animals, use a static low-poly mushroom + simple wobble animation in code)
- **Biome**: Kenney Nature Kit (trees, rocks, ground tile) → `app/assets/glb/biome.glb`
- **SFX**: Kenney UI Audio + Casino Audio packs (CC0)
- **BGM**: Kenney Music Pack or OpenGameArt CC0 mellow loop

### 7.2 Compression

Run once on each source `.glb`:
```bash
npx @gltf-transform/cli meshopt input.glb output.glb
npx @gltf-transform/cli prune output.glb output.glb
```

Target total `.glb` size: < 4 MB.

### 7.3 Loading

R3F `useGLTF` with DRACO/Meshopt loaders preconfigured. Hero `SkinnedMesh` cloned via `SkeletonUtils.clone()` for per-hero variants (MVP has just Bunny; structure ready for vertical slice).

### 7.4 Recolor (Bunny)

Simple `material.color.setHex()` per material clone. Palette texture pipeline deferred until vertical slice has 8 hero variants.

---

## 8. UI implementation

### 8.1 Components

Owned by ui-engineer per `core/.claude/agents/ui-engineer.md`:

```
app/src/ui/
  Boot.tsx
  Lobby.tsx
  HUD.tsx
  DraftModal.tsx
  EndRunSummary.tsx
  Profile.tsx
  Settings.tsx
  Joystick.tsx           # virtual touch joystick component
  Bar.tsx                # reusable HP/XP bar
  CTAButton.tsx          # reusable big-CTA button
  styles.css             # design system
```

### 8.2 State stores

```
app/src/state/
  runStore.ts            # phase, time, kills, level, xp, hero entity ref
  metaStore.ts           # save load/persist via Capacitor Preferences
  settingsStore.ts       # bgm/sfx volume
```

Selectors via `useStore(state => state.field)` — NO React Context.

### 8.3 Joystick

Touch + mouse virtual joystick (left half of screen). Pointer down sets origin, drag relative to origin yields a `Vec2` (normalized to unit circle), which `runStore.input` exposes. Movement system reads `runStore.input` each frame.

---

## 9. Audio

### 9.1 Bus

```
app/src/audio/
  AudioBus.ts            # singleton: ctx, masterGain, sfxGain, bgmGain
  loaderPool.ts          # pre-decode buffers from /public/audio/*.ogg
  index.ts               # API: play('hit'), play('levelup'), bgm.start()
```

### 9.2 SFX list

| Event | File | Notes |
|---|---|---|
| `hit` | hit.ogg | weapon hits enemy |
| `enemyHit` | enemy-hit.ogg | enemy hits hero |
| `gem` | gem.ogg | XP pickup |
| `levelup` | levelup.ogg | level reached |
| `draftPick` | click.ogg | draft choice |
| `death` | death.ogg | hero death |
| `click` | click.ogg | UI tap |
| `evolve` | evolve.ogg | weapon evolution (deferred but file ready) |

### 9.3 BGM

Single 30-90s looping track; volume controlled by `settingsStore.bgmVolume * masterGain`.

---

## 10. Perf budget for MVP

| Budget | Target | Stretch |
|---|---|---|
| Active enemies on screen | 60 | 100 |
| Projectiles | 30 | 50 |
| VFX puffs | 20 | 30 |
| Draw calls | ≤ 60 | ≤ 80 (spec ceiling) |
| Tris on-screen | ≤ 150k | ≤ 250k (spec ceiling) |
| Frame rate on iPhone 12 | ≥ 55 fps sustained | ≥ 60 fps |
| First contentful paint inside WKWebView | ≤ 2.0 s | ≤ 1.5 s |
| Total `.glb` assets | ≤ 4 MB | ≤ 8 MB |
| JS bundle gzipped | ≤ 350 KB | (current Sprint A: 288 KB) |

**MVP relaxes the spec's 200-enemy target down to 60.** Reaching 200 requires VAT pipeline (next plan).

---

## 11. Risks + fallbacks

| Risk | Mitigation |
|---|---|
| Quaternius models don't share humanoid rig across hero + enemies | They don't — UAL2 is humanoid-only; enemies use per-species rig. Hero uses UAL2 (Bunny on humanoid), enemies use per-species. |
| Stock SkinnedMesh × 60 enemies tanks fps on iPhone 12 | Per Sprint D plan, fall back to: (a) reduce visible enemy count to 40 with stronger cull, (b) use a single LOD-1 mesh per archetype |
| Joystick input on iOS isn't responsive enough | Use raw `pointermove` events bypassing React's synthetic events; verify <50ms input latency |
| Audio latency in WKWebView ruins game-feel | Pre-decode all SFX buffers at boot; use `AudioContext.suspend/resume` carefully; if still bad, fall back to HTMLAudioElement preloaded |
| Draft modal interrupts feel | 250ms slide-in animation; tap-to-pick instant; no extra confirm step |

---

## 12. Stream decomposition (preview of plan)

Parallel-dispatchable streams (full plan will detail each):

| Stream | Owner agent | Wave |
|---|---|---|
| **S1: ECS world + pooling** | systems-engineer | 1 |
| **S2: Asset pipeline (download + compress)** | asset-curator | 1 |
| **S3: Audio bus + Kenney SFX load** | systems-engineer | 1 |
| **S4: Hero + camera + joystick** | gameplay-engineer | 2 (needs S1) |
| **S5: UI screens + state stores** | ui-engineer | 2 (needs S1) |
| **S6: Combat + weapons + projectile pool** | gameplay-engineer | 3 (needs S1, S4) |
| **S7: Enemies + AI + spawning** | gameplay-engineer | 3 (needs S1, S2) |
| **S8: XP/Leveling/Draft** | gameplay-engineer | 4 (needs S6, S7) |
| **S9: Save + Profile + Settings wiring** | systems-engineer | 4 (needs S5) |
| **S10: Integration + iPhone 12 perf smoke + Build #7 ship** | build-engineer | 5 (needs all) |

10 streams, 5 waves. Wave 1 starts in parallel (S1, S2, S3 independent). Wave 5 is single-threaded ship.

CI policy during MVP execution: bb-* workflows already path-guarded so they only fire on changes; if any specific workflow becomes a bottleneck, disable temporarily by renaming `.yml` → `.yml.disabled` (per user permission). Build #7 ship does not depend on bb-* CI being green; depends on the `bb-ios-build` workflow which now actually runs jobs.

---

## 13. Approval log

- 2026-05-16 — MVP scope (Medium MVP — Quaternius hero + 3 enemies + 2 weapons + 7 screens + audio + save) approved by Ömer Y.
- 2026-05-16 — High-level architecture, screen flow, mechanics, perf relaxation (60 enemy target for MVP) approved by Ömer Y.
- _Pending_ — Spec doc review by Ömer Y. before invoking `writing-plans` for the MVP implementation plan.
