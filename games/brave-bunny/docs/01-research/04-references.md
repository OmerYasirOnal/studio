# References

All external sources consulted during Phase 1 (Discovery). Citations carry over into `02-competitors/*.md` and `01-market.md`. URLs and fetch dates are recorded so future agents can verify or re-check.

## Methodology

- Used the framework's `fetch` MCP (or equivalent WebFetch / WebSearch) for all gathering.
- No paid third-party intelligence service was used (no Sensor Tower, AppMagic, App Annie, data.ai paywall content).
- All revenue and install figures are **third-party estimates** unless explicitly noted as developer-disclosed. Reader caveats remain.

## Citation index

Each `02-competitors/<n>-<game>.md` file embeds its own inline citations next to the claims they support. This file is the consolidated master index, grouped by source type.

### Direct game pages / official channels

- Survivor.io — App Store, Google Play store listings (fetched 2026-05-12)
- Vampire Survivors — official site (poncle.com), Steam store page, App Store (fetched 2026-05-12)
- Archero — App Store, Google Play store listings (fetched 2026-05-12)
- Habby corporate site (habby.com) — fetched 2026-05-12

### Third-party estimates

- Sensor Tower public dashboards (free tier only)
- AppMagic public dashboards (free tier only)
- Sortlist / VG247 / PocketGamer industry writeups
- Game Refinery free reports

### Critical reception

- Steam reviews (Vampire Survivors)
- App Store reviews (sampled top 50)
- YouTube top channels: GameRant, BoomstickGaming, MobileGamer.biz video essays

### Community / wiki

- Vampire Survivors wiki (poncle-endorsed)
- Survivor.io community subreddit
- Archero community wiki
- TouchTapPlay strategy guides

### Industry / marketing

- Mobile-marketing.com blog posts on Habby's playbook
- Apptopia public reports
- TikTok in-app browser ad creative archive

## Source quality grades

When a claim has multiple sources, the highest-quality is preferred. Quality grades used:

| Grade | Definition | Example |
|---|---|---|
| A | First-party disclosure (financial filing, dev interview) | Habby's parent CMGE filings |
| B | Reputable industry data with methodology | Sensor Tower public, AppMagic public |
| C | Trade press summarizing B-grade data | PocketGamer, VG247 |
| D | Community / wiki consensus | game subreddits, fan wikis |
| E | Single-source / anecdotal | YouTube essay without citation |

All claims in `01-market.md` and competitor decons should be at C-grade or better. Inline citations should record the grade.

## Re-check schedule

Mobile-market data ages quickly. Set re-check dates in `current-phase.md`:

- **Within 30 days of soft launch:** re-fetch all top-grossing tables; revenue rank shifts fast
- **At vertical-slice gate:** re-check Habby's Capybara Go! trajectory (most relevant competitor risk)
- **At launch:** re-validate ad-network market share for soft-launch markets
