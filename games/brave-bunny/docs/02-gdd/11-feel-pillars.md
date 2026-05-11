# GDD 11 — Feel Pillars

> The frame-level and number-level definitions of what "good" feels like in Brave Bunny. Every pillar has a **violation flag** — a concrete, testable condition that makes the build fail QA. These are the rules the gameplay-engineer, ui-engineer, and art-director cross-reference when polishing.
>
> Pillars are sorted by visibility per second of play (kills > pickups > UI > deaths).

## Pillar 1 — Every kill must shake the room

- **Definition:** No enemy ever dies silently. Every kill produces a visible + audible response within 1 frame of the killing blow.
- **Frame / number spec:**
  - Screenshake amplitude: **2 px** on trash kill, **4 px** on elite, **8 px** on boss.
  - Recovery: **80 ms** ease-out for trash; **160 ms** for elite; **240 ms** for boss.
  - VFX: corpse-puff particle burst (8–12 particles) on every kill, biome-tinted.
  - SFX: 1-of-3 round-robin enemy-death stinger at **−9 dB** for trash, **−6 dB** for elite, **−3 dB** for boss.
- **Violation flag:** an enemy disappears from screen with no shake, no particle, and no sound within the same frame as its HP reaching 0. Auto-fails QA.

## Pillar 2 — Every level-up is a celebration

- **Definition:** Level-up is the dopamine hook of the loop. It must feel like a *moment*, not a popup.
- **Frame / number spec:**
  - Time-dilate to **0.4x for 200 ms** before full pause (the "stretch" perception trick).
  - Gold particle burst from player, **30 particles**, 1.0 unit radius, 400 ms lifetime.
  - Audio fanfare: 4-note arpeggio at **−3 dB**, 600 ms total.
  - UI card slam-in: cards translate from **+300 px Y** to rest position with an overshoot bezier `(0.34, 1.56, 0.64, 1.0)` over **280 ms**, staggered **40 ms** between cards.
  - Card backdrop dim: black overlay 0 → 60% opacity over 160 ms.
- **Violation flag:** cards appear via fade or instant pop, or audio cue is missing, or time-dilate is skipped. Auto-fails QA.

## Pillar 3 — Pickup is satisfying

- **Definition:** Every pickup is a micro-reward; the player's brain must register "I got something" within 120 ms.
- **Frame / number spec:**
  - XP gem magnetizes from **1.5 unit** radius (heart from 2.5 unit, gold from full screen on enemy death).
  - On pickup contact: scale tween **1.0 → 1.4 → 1.0** over **120 ms** (ease-out then ease-in).
  - Soft chime at **−3 dB**, sample-pitched by gem tier (small +200 cents, large −200 cents).
  - Particle: 4-particle micro-burst at pickup point, **180 ms** lifetime.
  - XP bar fills with a **80 ms** lerp, not instant; a fast-but-readable rise.
- **Violation flag:** pickup snaps without scale-pulse or audio, or XP bar jumps instantly. Auto-fails QA.

## Pillar 4 — Auto-attack has impact

- **Definition:** Auto-attacks must read as deliberate hits, not bullets passing through cardboard.
- **Frame / number spec:**
  - Hit-flash on enemy: white tint at **0.7 alpha** for **50 ms**, ease-out to 0.
  - Directional knockback on hit: **3 px** along projectile vector, restored over **80 ms** (visual only, not pathfinding).
  - Hitstop on elite kill: **60 ms** full game-time freeze (player + projectiles + enemies all halt).
  - Hitstop on boss damage tick > 5% HP: **40 ms**.
  - No hitstop on trash (would feel laggy at high density).
  - Projectile impact VFX: 6-particle radial puff, biome-tinted, **120 ms** lifetime.
- **Violation flag:** an elite dies with no hitstop, or any enemy receives damage with no hit-flash. Auto-fails QA.

## Pillar 5 — UI taps are responsive

- **Definition:** Every UI control must acknowledge touch within 1 frame and complete its confirmation animation in ≤ 120 ms. Players never wonder "did it register?".
- **Frame / number spec:**
  - Button down-state (scale to **0.95**, tint −10% luminance) visible within **1 frame (16.6 ms at 60 fps)** of pointer-down.
  - Press-confirmation animation: scale back to 1.0 + soft "tick" SFX at **−12 dB** within **120 ms** of pointer-up.
  - Tap target minimum: **88 pt** (iOS HIG floor, also clears iPhone SE 3 thumb-zone tests).
  - Disabled / locked buttons get a distinct shake (3 px horizontal, 2 oscillations, **180 ms**) on tap — never silent rejection.
  - No UI control may use animation > 200 ms on a single tap response.
- **Violation flag:** any tap that produces no visual response within 1 frame, or any locked button that silently does nothing. Auto-fails QA.

## Pillar 6 — Death is dignified

- **Definition:** Death is the end of a run, not a punishment screen. The player must feel they earned something even when they lose. This pillar implements `00-overview.md` pillar 3 ("Dignity-by-design") at the frame level.
- **Frame / number spec:**
  - On HP = 0: time-dilate to **0.3x** for **300 ms**, then pause.
  - Gold particle burst from hero corpse, **60 particles**, 2.0 unit radius, 600 ms lifetime.
  - Camera dollies in **5%** over 300 ms.
  - Audio: a soft "wind-down" stinger at **−6 dB**, 800 ms.
  - Run-end tally screen slides in from bottom over **400 ms** with a cubic ease-out.
  - Tally lines bank in series: gold, soul-shards, pass-XP — each with a **250 ms** slam-and-count animation at **−9 dB** tick-tick SFX.
  - First **800 ms** of the tally screen is non-skippable (the dopamine must land).
  - Revive offer appears after tally, never before — the player sees their reward *first*.
- **Violation flag:** death cuts directly to a "Game Over" screen, or the tally is skippable in < 800 ms, or the revive offer interrupts the celebration. Auto-fails QA.

## Pillar 7 — The screen is always alive

- **Definition:** Between draft events, the screen still breathes. Idle states do not exist; ambient animation, parallax, and crowd density carry the moment-to-moment.
- **Frame / number spec:**
  - Hero idle animation: 2-frame bob at **0.5 Hz**, ±2 px Y.
  - Biome parallax: 2 layers, far at **0.3x** scroll, mid at **0.7x** scroll relative to camera.
  - Ambient grass / props: 4-direction wind-sway loop, ±3°, **2 s** cycle, randomized phase per instance.
  - Minimum on-screen enemy count by minute 1: **8**; by minute 5: **80**; per `CLAUDE.md` perf contract, hard cap 200.
- **Violation flag:** the screen has fewer than 8 enemies AND no scripted scene-event for > 3 s. Auto-fails QA.

## Pillar 8 — Audio mix never crushes the moment

- **Definition:** The mix has headroom so that fanfares (level-up, boss death, run-end) cut through. SFX density is high but never muddy.
- **Frame / number spec:**
  - Master mix ceiling: **−6 dB** (leaves 6 dB headroom for fanfares).
  - Concurrent SFX cap: **12 voices** (over-cap, lowest-priority voice culls).
  - Ducking: music ducks **−4 dB** for **200 ms** on level-up fanfare and boss-kill stinger.
  - Footsteps and pickup chimes route through a separate bus at **−9 dB** ceiling so they never overwhelm combat SFX.
- **Violation flag:** level-up fanfare is buried (< 3 dB above combat SFX RMS at moment of trigger). Flagged for audio-bible re-mix.

## Cross-references

- These pillars are the **acceptance criteria** for any vertical-slice playable build. QA's checklist in `docs/qa/` references each violation flag by pillar number.
- The numeric specs (px, ms, dB) belong to the GDD; their *implementation* lives in tuning ScriptableObjects per `CLAUDE.md` principle 6 ("no magic numbers in code").
- Companion docs: `01-core-loop.md` (what happens), `00-overview.md` (why we made it).
