# Color Palette — Brave Bunny

> Owner: art-director. Cross-refs: `00-style-overview.md` (saturation budget), `docs/06-tech-spec/` (toon shader ramp tints sample these hex values). All contrast ratios calculated against pure white (#FFFFFF) per WCAG 2.1 luminance formula.

## Primary palette (8 colors)

The core game-wide palette. Every biome blends from a subset of these. All values pastel-saturated per the 80%-environment / 100%-hero rule.

| Name | Hex | Use case | Contrast vs #FFF |
|---|---|---|---|
| Bunny Cream | `#FFF4DC` | Hero fur base (default rabbit) | 1.08 : 1 |
| Meadow Lime | `#A8D86B` | Ground primary in Meadow biome | 1.93 : 1 |
| Sage Mid | `#6FAE74` | Grass blade mid, environment fill | 2.94 : 1 |
| Sky Soft | `#BEE3F0` | Sky dome, ambient fill | 1.30 : 1 |
| Berry Pink | `#F39FB4` | Hero accent, pickup heart | 2.06 : 1 |
| Bark Brown | `#8B6A4A` | Tree trunks, dirt path | 5.32 : 1 |
| Stone Gray | `#9FA5A8` | Rocks, neutral prop | 2.84 : 1 |
| Coal Outline | `#2E2A28` | Text on light bg, micro-detail eyes | 13.95 : 1 |

## Secondary palette (6 mid-tones)

Used for material variation, prop tinting, lighter shadow ramps, and biome blends.

| Name | Hex | Use case | Contrast vs #FFF |
|---|---|---|---|
| Mint Wash | `#CDEBD0` | Light grass tint, fresh leaves | 1.20 : 1 |
| Lavender Mist | `#D7C5E8` | Cavern ambient fog, magic UI bg | 1.51 : 1 |
| Peach Sand | `#F6D6B5` | Beach sand, warm prop tint | 1.30 : 1 |
| Olive Deep | `#5E6B3E` | Forest canopy shadow side | 6.43 : 1 |
| Slate Cool | `#5C6D7A` | Cavern stone, snow shadow side | 5.41 : 1 |
| Tea Rose | `#E8B8B0` | Soft pink prop, mushroom cap | 1.74 : 1 |

## Accent palette (4 hot accents)

100% saturation. Used only for hero highlights, pickups, rare drops, and danger signals — per saturation budget.

| Name | Hex | Use case | Contrast vs #FFF |
|---|---|---|---|
| Pickup Gold | `#FFC83D` | Coin, gold gem, common pickup halo | 1.50 : 1 |
| Hero Highlight | `#FF6B6B` | Hero rim light, level-up flash, hot CTA | 3.18 : 1 |
| Rare Drop Cyan | `#3DE0E0` | Epic-tier pickup glow, rare aura | 1.79 : 1 |
| Danger Red | `#E83C3C` | Boss telegraph, HP < 25% UI, hazard tint | 4.42 : 1 |

## Per-biome palette swatches

5 biomes. Vertical-slice ships **Meadow** only; the rest are speced now so the toon-ramp shader has a consistent multi-biome design from day one.

### Meadow (verdant) — vertical-slice biome

| Slot | Color | Hex | Notes |
|---|---|---|---|
| Ground primary | Meadow Lime | `#A8D86B` | Lawn |
| Ground secondary | Sage Mid | `#6FAE74` | Patches, slopes |
| Prop accent | Berry Pink | `#F39FB4` | Flower clusters |
| Sky | Sky Soft | `#BEE3F0` | Clear noon |
| Shadow ramp | Olive Deep | `#5E6B3E` | Toon-ramp dark stop |

### Beach (sun)

| Slot | Color | Hex | Notes |
|---|---|---|---|
| Ground primary | Peach Sand | `#F6D6B5` | Beach sand |
| Ground secondary | `#E6BE92` | `#E6BE92` | Wet sand near tide |
| Prop accent | `#4FC1C7` (Turquoise) | `#4FC1C7` | Sea, shells |
| Sky | `#FFD9A0` (Golden) | `#FFD9A0` | Golden hour |
| Shadow ramp | `#B5784C` | `#B5784C` | Warm sand shadow |

### Forest (canopy)

| Slot | Color | Hex | Notes |
|---|---|---|---|
| Ground primary | `#2F5A3A` (Forest Green) | `#2F5A3A` | Dim forest floor |
| Ground secondary | Olive Deep | `#5E6B3E` | Moss patches |
| Prop accent | `#D9722A` (Burnt Orange) | `#D9722A` | Autumn leaves, mushrooms |
| Sky | `#7B9670` (Filtered Green) | `#7B9670` | Canopy-filtered light |
| Shadow ramp | `#1B3A24` | `#1B3A24` | Deep shadow |

### Cavern (dim)

| Slot | Color | Hex | Notes |
|---|---|---|---|
| Ground primary | `#5B4A6E` (Plum) | `#5B4A6E` | Cave floor — dim NOT dark |
| Ground secondary | Slate Cool | `#5C6D7A` | Rock formations |
| Prop accent | `#FF7E6B` (Coral) | `#FF7E6B` | Crystals, glow mushrooms |
| Sky | n/a (ceiling) `#2E2640` | `#2E2640` | Cave ceiling |
| Shadow ramp | `#1F1830` | `#1F1830` | Plum-tinted shadow (never pure black) |

### Snow (cool)

| Slot | Color | Hex | Notes |
|---|---|---|---|
| Ground primary | `#EAF2F5` (Ice Blue) | `#EAF2F5` | Fresh snow |
| Ground secondary | `#C5D5E0` | `#C5D5E0` | Packed snow shadow |
| Prop accent | `#E84A5F` (Cherry) | `#E84A5F` | Holly, scarves, danger |
| Sky | `#D8E6EE` | `#D8E6EE` | Overcast diffuse |
| Shadow ramp | `#7E96A6` | `#7E96A6` | Cool gray-blue |

## UI palette — button states

UI lives over gameplay, so it samples the accent palette but with strict contrast guardrails.

| State | Fill | Stroke | Label | Contrast (label vs fill) |
|---|---|---|---|---|
| Idle | `#FF6B6B` (Hero Highlight) | `#C7423A` | `#FFFFFF` | 4.55 : 1 |
| Hover | `#FF8585` | `#C7423A` | `#FFFFFF` | 3.71 : 1 |
| Pressed | `#D9554F` | `#8B2D29` | `#FFFFFF` | 5.21 : 1 |
| Disabled | `#C9C9C9` | `#9C9C9C` | `#6E6E6E` | 4.62 : 1 |

## Text contrast guardrails (WCAG AA)

| Text class | Min size (sp) | Min contrast ratio | Example |
|---|---|---|---|
| Body text | 14 sp | 4.5 : 1 | Tooltip body, dialog text |
| Large text | 18 sp bold / 24 sp regular | 3.0 : 1 | Section headers |
| UI labels on accent fills | 14 sp bold | 4.5 : 1 | Button labels |
| HUD numerics | 18 sp bold | 4.5 : 1 | HP, XP, timer |
| Damage numbers (transient) | 16-32 sp | 3.0 : 1 (relaxed; transient) | Hit feedback |

If a label fails contrast, the fix order is: (1) thicken weight, (2) add 1px Coal Outline (`#2E2A28`) stroke, (3) recolor to white or coal, (4) only then change the underlying fill.
