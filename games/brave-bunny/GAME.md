---
# GAME.md — the canonical config file for this game.
# Edit immediately after `new-game.sh` runs. Every agent reads this to know
# what they're building.

name: brave-bunny
display_name: Brave Bunny
genre: action-roguelite
template: action-roguelite
scaffolded: 2026-05-11
framework_version_used: "0.1.0"

# Inspirations (3-5 specific shipped titles)
inspiration:
  - survivor.io
  - vampire-survivors
  - archero

# Target platforms (priority order)
platforms:
  - ios
  - android
priority_platform: ios

# Engine
engine: unity-6-lts
engine_pipeline: urp
language: c#
ui_framework: ui-toolkit

# Performance target — baseline device for tech-architect's perf budget
target_devices:
  - iphone-12          # 60 fps baseline
  - iphone-se-3        # safe-area + small-screen tests

# Visual identity
art_style: low-poly-cartoon-saturated
camera: top-down-3-4-perspective

# Release window
target_release: 2026-Q3
soft_launch_markets: [tr, ph, id]

# Bundle id (build-engineer reads this)
bundle_id_pattern: com.yasironal.brave-bunny

# Live-ops cadence post-launch (game-designer fills 12-content-roadmap.md from this)
live_ops:
  first_event_weeks_after_launch: 2
  balance_pass_cadence_weeks: 2

# Monetization design hints (NOT IMPLEMENTATION — that's tech spec)
monetization:
  iap: true
  rewarded_ads: true
  battle_pass: true
  no_pay_to_win: true
---

# Brave Bunny

## Concept

A top-down 3/4-perspective action-roguelite where animal heroes face down ever-thickening swarms in saturated low-poly biomes. 7-10 minute runs, three random upgrades per wave, build crafting, persistent meta-progression. Habby-family feel (Survivor.io, Archero) with a Cat Quest / Crossy Road visual register.

## Why this game

The Habby/auto-battler space is hot but visually homogenous (pixelated, grim, or generic anime). Brave Bunny's CC0-mascot-friendly low-poly cartoon look opens a TikTok-shareable, family-safe slot that's currently underfilled in TR/PH/ID soft-launch markets — and it lets one developer ship in eight weeks because every asset is CC0-recolored, not custom-modeled.

## North-star metric

**D1 retention ≥ 40%** in soft-launch markets (TR / PH / ID) at vertical-slice gate. Secondary: median run length 7-10 minutes.

## Cut list

If behind schedule, cut in this order:

1. **Meta-progression beyond character unlocks** — ship with only character-level upgrades, defer rune system to v1.1
2. **Boss roster beyond 1** — ship vertical slice with one boss across all biomes; vary cosmetically
3. **Localization beyond TR/EN** — drop PH/ID localization, soft-launch with English in those markets
