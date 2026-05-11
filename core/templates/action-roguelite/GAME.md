---
# GAME.md — the canonical config file for this game.
# Edit immediately after `new-game.sh` runs. Every agent reads this to know
# what they're building.

name: __GAME_NAME__
display_name: __DISPLAY_NAME__
genre: action-roguelite
template: __TEMPLATE__
scaffolded: __SCAFFOLD_DATE__
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
bundle_id_pattern: com.yasironal.__GAME_NAME__

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

# __DISPLAY_NAME__

> Edit this file immediately after scaffolding. Then run `/phase-status`.

## Concept

<2-3 sentences. What's the elevator pitch?>

## Why this game

<What gap in the market does it fill? Why does the world need it?>

## North-star metric

<Single metric. e.g., D1 retention ≥ 40% in soft-launch markets.>

## Cut list

<3 things we will cut first if behind schedule.>
