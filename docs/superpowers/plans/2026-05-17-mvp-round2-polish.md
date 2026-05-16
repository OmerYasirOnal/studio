# MVP Round 2 Polish Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development.

**Goal:** Build #11 ships a functional MVP. Round 2 elevates it from "works" to "feels good": visual combat feedback, pause/restart UX, audio polish, boss fight, then ship Build #12.

**Architecture:** 4 development phases (A-D) + 1 ship phase (E), each in its own branch + PR. Each phase produces 1 commit per scope, ends with merge to main. Phase E bumps build number + fastlane upload.

**Tech Stack:** Same as Round 1 (Vite + R3F + miniplex + zustand + Capacitor 7). No new dependencies.

**Source:** Live testing of Build #8 + #10 via Playwright on iPhone 12 viewport surfaced 4 high-impact UX gaps after Round 1. This plan addresses them.

---

## Why Round 2

Round 1 (Phases 1-6) shipped a playable game:
- Build #6: rotating cube placeholder
- Build #7: gameplay loop (Bunny + 3 enemies + 2 weapons + XP/draft + UI + audio + save)
- Build #8: visual fixes (animation wiring + scales + projectile/pickup visibility)
- Build #9: hotfixes (biome plane size + XP curve)
- Build #10/#11: full polish (animation state machine + biome decor + UI animations)

What Round 1 left unsolved:
1. **Combat reads as silent** — kills happen but no big "boom" feedback. Player isn't sure they're hitting.
2. **No way out of a run** mid-game except dying or 5-min timer. Frustrating during testing.
3. **Audio probably doesn't play on Capacitor iOS** until user taps (Web Audio policy). Never verified on device.
4. **No boss = no climax**. Every run is uniform-density slime spam.

Round 2 fixes these in 4 phases, ships Build #12.

---

## Phase A: Visual combat feedback

### Goals
- Floating damage numbers above enemies on hit (yellow on normal, orange on big damage)
- Hit spark VFX (3-frame radial burst at hit position)
- Level-up burst (large gold expanding ring around hero)
- Death poof (smoke puff on enemy death, replacing the existing hit-flash)

### Files
- **New**: `app/src/render/DamageNumbers.tsx` — DOM overlay using R3F `<Html>` from drei (or pure HTML overlay synchronized to camera projection)
- **New**: `app/src/render/HitSparks.tsx` — InstancedMesh of small bright orange octahedrons, short TTL
- **New**: `app/src/render/LevelUpBurst.tsx` — single expanding ring mesh, 0.6s lifetime
- **Modify**: `app/src/systems/weapons.ts` — emit hit event with damage value + position
- **Modify**: `app/src/systems/projectiles.ts` — emit hit event
- **Modify**: `app/src/systems/lifecycle.ts` — emit level-up event + death poof event
- **New**: `app/src/systems/events.ts` — tiny event bus (zustand store for transient effects)
- **Modify**: `app/src/render/Game.tsx` — mount new VFX components

### Approach for damage numbers
Use a small zustand store `useVfxStore` that holds a list of active effects (damage number, hit spark, level-up burst). Each effect has TTL. `useFrame` ticks TTLs, removes expired. R3F renders the active set.

Damage numbers float up + fade over 800ms. World-space → screen-space conversion via camera projection, then absolute-positioned `<div>` overlay.

### Verification
- Smoke test: pick up enemy hits, confirm yellow numbers appear above hit position
- Level up → confirm gold ring expands from hero
- Enemy dies → confirm 1-frame smoke poof at death position

---

## Phase B: Pause UI + quick restart (no page reload)

### Goals
- **Pause button** in HUD (top-right corner small ⏸ icon)
- **Pause modal** with [Resume] [Settings] [Quit Run] options
- **Restart button** on EndRunSummary that actually rebuilds run state (not page reload)
- **Quit to lobby** preserves no run state (clean reset)

### Files
- **Modify**: `app/src/ui/HUD.tsx` — add pause button
- **New**: `app/src/ui/PauseModal.tsx` — pause overlay
- **Modify**: `app/src/state/runStore.ts` — add `'paused'` to Phase type, ensure restart cleans `__runBanked` flag, etc.
- **Modify**: `app/src/systems/runLoop.ts` — gate ticking on `phase === 'run'` only (already does this)
- **Modify**: `app/src/render/Hero.tsx` — react to `phase === 'lobby'` for cleanup, `phase === 'run'` after restart for re-spawn
- **Modify**: `app/src/ui/EndRunSummary.tsx` — replace `(window as any).__runBanked` hack with proper state machine

### Approach for restart
Currently EndRun → RESTART → `setPhase('run')` + `reset()`. But Hero.tsx's mount logic checks `if (existing) return`. Need to ensure all entities are cleared on phase transition through 'lobby' OR pause→quit.

Add explicit `resetRun()` that:
1. Removes all entities from world
2. Re-mounts hero
3. Resets all stat counters in runStore
4. Resets draftStore.taken
5. Resets magnet radius

### Verification
- Click ⏸ during run → modal appears, scene freezes (no enemy movement)
- Resume → scene continues
- Quit Run → returns to Lobby with no state leakage
- Death → EndRun → Restart → fresh run with HP 100, kills 0, Lv 1

---

## Phase C: Audio verification + variety

### Goals
- Verify Web Audio init works on Capacitor iOS (will defer to user device test)
- Add ground footstep SFX (every 0.4s while running)
- Add low-HP heartbeat (loop when HP < 30%)
- Camera shake on hero damage (3-frame nudge)

### Files
- **Modify**: `app/src/audio/AudioBus.ts` — add `loop` SFX support, expose `startLoop/stopLoop`
- **New**: Footstep + heartbeat .ogg files in `app/public/audio/`
- **Modify**: `app/src/render/Hero.tsx` — emit footstep on run anim cycle
- **Modify**: `app/src/systems/lifecycle.ts` — check HP threshold, start/stop heartbeat loop
- **Modify**: `app/src/render/CameraRig.tsx` — add `shakeAmount` ref, decay over time, perturb camera position

### Approach for audio init
Web Audio policy requires user interaction before resume. Current Lobby's PLAY button click triggers `audio.startBgm()` which itself triggers init. On Capacitor iOS this MAY work but may need an explicit `audio.ctx.resume()` after the user gesture.

Add safety check in `init()`:
```ts
if (this.ctx?.state === 'suspended') {
  await this.ctx.resume();
}
```

### Verification
- Footstep audio plays in browser when joystick active
- Heartbeat starts when HP drops below 30, stops when HP recovers
- Camera shakes briefly when hero takes damage (visible in screenshots)

---

## Phase D: Boss fight (Big Bad Wolf mid-run boss)

### Goals
- At 2:30 mark, spawn a boss enemy (uses Wolf model + scale 1.5)
- Boss has 800 HP, 3x normal damage, 1.5x speed
- HUD shows boss HP bar at top center when boss is alive
- Boss death drops 100 XP (5 levels worth of normal kills)
- Music intensifies during boss fight (TBD: just SFX, or BGM swap)
- One boss per run

### Files
- **Modify**: `app/src/data/waves.json` — add boss spawn timing + stats
- **Modify**: `app/src/systems/spawn.ts` — boss spawn logic (single instance, gated on game time)
- **Modify**: `app/src/state/runStore.ts` — add `boss` slot (entity ref or just HP/maxHP fields)
- **New**: `app/src/ui/BossHpBar.tsx` — large HP bar with boss name
- **Modify**: `app/src/render/EnemyEntity.tsx` — bigger scale for boss flag, accept "boss" archetype
- **Modify**: `app/src/ecs/components.ts` — add `isBoss?: boolean` field
- **Modify**: `app/src/ui/HUD.tsx` — mount BossHpBar conditionally

### Approach
Boss is a special enemy entity flagged `isBoss: true`. Spawn system checks `time >= 150 && !bossSpawned` then creates boss with overridden stats. Lifecycle handles boss death — emits big level-up + heals hero (banked bonus). Subsequent waves continue normally; no second boss.

### Verification
- Run for 2:30 → boss appears with red HP bar at top
- Carrot Spear + Pebble Sling damage the boss
- Boss attacks hero with bigger damage hits
- Boss death → big XP burst + HP bar disappears

---

## Phase E: Ship Build #12

### Steps
1. Bump `CURRENT_PROJECT_VERSION = 11 → 12` in pbxproj
2. `fastlane beta_no_match` with `BB_PROFILE_UUID=...`
3. ASC verify state = VALID
4. Update memory file

---

## Risks + Fallbacks

| Risk | Mitigation |
|---|---|
| Damage numbers + InstancedMesh hit-sparks → too many draw calls | Cap each effect pool at 20 active; reuse instances |
| Pause modal breaks runLoop gating | Add 'paused' to Phase type and gate runLoop tick on `phase === 'run'` (already does this) |
| Audio doesn't resume on iOS Capacitor | Add explicit `ctx.resume()` after gesture; if still broken, log + ship anyway, defer to native iOS-device test |
| Boss balance too easy / too hard | Tune HP later; 800 HP is the first guess |
| Boss model = Wolf doesn't visually scream "boss" | Add a red emissive tint to boss material; scale 1.5x; HUD shows "Big Bad Wolf" name |

---

## Done criteria

Round 2 complete when:
- [ ] PRs #A, #B, #C, #D each merged to main
- [ ] Build #12 visible on TestFlight as VALID
- [ ] Tests still pass (17 expected, may grow with new system tests)
- [ ] Lint, format, typecheck all green
- [ ] No regressions in Round 1 features (animations, biome, UI, save)
