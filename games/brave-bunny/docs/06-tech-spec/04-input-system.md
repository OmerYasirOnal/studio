# Tech Spec 04 — Input System

> Owner: tech-architect. The input model for Brave Bunny: virtual joystick contract, action maps, multi-touch handling, and the 1-frame latency target from US-13. Sister docs: GDD `01-core-loop.md` (auto-attack contract — no fire button), GDD `11-feel-pillars.md` Pillar 5 (UI responsiveness), `05-performance-budget.md` (input + UI budgets).

## Package

**Unity Input System** package (`com.unity.inputsystem`, free, bundled with Unity 6 LTS). Legacy `Input.GetAxis` API is **disabled** in Player Settings (`Active Input Handling = Input System Package (New)`).

## Action maps

Two action maps, switched contextually by the boot composition root:

| Map | Active during | Actions |
|---|---|---|
| `Player` | Run scene gameplay | `Move` (Value, Vector2), `Pause` (Button) |
| `UI` | All menus + level-up draft + run-end | `Navigate`, `Submit`, `Cancel`, `Point`, `Click` |

The Input System's auto-switching when a `UIDocument` panel is focused is **disabled**; we switch maps explicitly so the joystick never receives input during a draft pause.

## Virtual joystick contract

The joystick is the only locomotion input. **Dynamic placement.**

### Placement

- **Dynamic** — on `pointer-down` anywhere in the **bottom-left quadrant** (defined as `x < screenWidth * 0.5` AND `y < screenHeight * 0.6`), the joystick spawns centered on the pointer's down position.
- Why dynamic over static: works for all thumb sizes, doesn't fight the player's grip, and matches the Habby-family convention players already know.

### Visuals

- **Base ring:** semi-transparent circle, biome-tinted (per art bible). Diameter = `2 * stickRadius` (see below).
- **Thumb knob:** opaque inner circle tracking the finger.
- **Fade-in:** 80 ms ease-out on spawn; **fade-out:** 200 ms on pointer-up.

### Geometry

Stick radius is **proportional to the shorter screen edge** so it scales with device:

| Device | Shorter edge | Stick radius | Inner dead zone |
|---|---|---|---|
| iPhone 12 (390 × 844 pt) | 390 pt | **70 pt** | 8% × 70 = ~6 pt |
| iPhone SE 3 (375 × 667 pt) | 375 pt | **56 pt** | 8% × 56 = ~4.5 pt |
| iPad mini 6 | 744 pt | 84 pt (capped) | ~7 pt |

The formula: `stickRadius = clamp(0.18 * shorterEdge, 50, 84)` pt. Locked in `InputSettings` SO; design changes go through balance review.

### Movement output

- **Output type:** `Vector2` in the range `[-1, 1]` per axis (normalized).
- **Dead zone:** 8% of stick radius (no movement output below this).
- **Outer clamp:** finger drag past stick radius clamps to the ring edge; output magnitude stays `1.0`.
- **No analog ramp:** the magnitude inside the deadband is zero; outside the deadband it goes from a small positive value to 1.0 linearly. No quadratic curve at launch (balance-engineer may add one post-launch via a curve asset).

### Tap-to-move (accessibility, post-launch)

- A settings toggle: `settings.tapToMove`. When enabled, a tap anywhere in the play area moves the hero toward that point until either a new tap or the hero arrives. Joystick disabled in this mode.
- Out of scope for launch; the field exists in the save schema (`03-save-system.md`) so the toggle ships pre-wired.

## Multi-touch model

The Input System surfaces all simultaneous touches; we deliberately ignore extras at launch:

- **Touch 0 (first finger down):** drives the joystick.
- **Touch 1+:** ignored at launch — reserved for future tap-skill characters (e.g., a "tap to dash" mechanic). Hooks exist in the action map (`Skill0`, `Skill1` actions) bound to no input.
- The pause button (top-right HUD) lives on the **UI map**, not the Player map, so it routes through `Click` rather than competing for Touch 0.
- A finger that started outside the joystick spawn quadrant **does not** spawn a joystick — prevents accidental drag from a tap on the HUD's right side.

## Latency target

US-13 (acceptance: pointer-down → first hero-velocity update ≤ 16.6 ms at p99) drives the pipeline shape.

### Frame timeline

```
T=0     pointer-down lands in OS (UITouch event)
T+1-2ms iOS dispatches to UnityEngine via main-thread runloop callback
T+2-3ms Unity Input System polls the event during PlayerLoop.EarlyUpdate
T+3-5ms Brave.UI joystick reads the bound action's value
T+5-7ms Brave.Gameplay PlayerController reads the joystick output via DI
T+7-9ms PlayerController.Tick applies velocity to Rigidbody (or NavMesh)
T+10ms  Render submission begins
T+16.6ms Frame presents
```

Target: pointer-down to velocity update **within the same frame**, presented at the next vsync. We avoid:

- **Coroutines** for input — adds 1-frame latency.
- **InvokeRepeating / WaitForSeconds** in the input path.
- **OnEnable input rebinding** in Run scene — bindings are bound once at boot.

### Measurement

The qa-engineer hooks a debug `InputLatencyProbe` that timestamps pointer-down and the first frame the `PlayerController` writes velocity > 0. Asserted in PlayMode tests at the 99th percentile across a 200-input sample.

## Pause input

- A `Pause` button in the **top-right safe area** (per US-2 acceptance — wireframe `13-hud-joystick.html`), ≥ 88 pt tap target.
- Routes through the UI map's `Click`; on activation, switches input map from `Player` to `UI` and pushes the pause modal panel.
- Tap acknowledgment within 1 frame per Pillar 5; uses the same `ButtonResponder` USS class as all UI buttons.

## Haptics

- iOS: `UIImpactFeedbackGenerator.Light` on pointer-down to give the joystick a "click" feel.
- Android: `Vibrator.vibrate(20ms)` equivalent.
- Per-event haptic firings beyond joystick spawn (e.g., on kill, on pickup) are owned by the audio/feedback system, not the input system.
- Toggle: `settings.hapticsEnabled` (defaults true).

## Editor controls

For desktop iteration:

| Map | Action | Editor binding |
|---|---|---|
| Player | Move | WASD / Arrow keys |
| Player | Pause | Escape |
| UI | Submit | Enter |
| UI | Cancel | Escape |
| UI | Point | Mouse position |
| UI | Click | Left mouse |

Editor input is **not** built into shipping IPAs (Input System strips desktop bindings via define `UNITY_IOS || UNITY_ANDROID`).

## Cross-references

- US-13 — joystick responsiveness ≤ 1 frame.
- GDD `01-core-loop.md` — auto-attack contract (no fire button).
- GDD `11-feel-pillars.md` Pillar 5 — tap acknowledgment within 1 frame.
- `05-performance-budget.md` — input + UI budget allocation.
- `00-engine-and-version.md` — Unity Input System version pinned to Unity 6 LTS bundle.
