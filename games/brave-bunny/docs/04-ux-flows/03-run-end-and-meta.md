# UX Flow 03 — Run-End and Meta

> The flow from "run-end tally fully banked" through "player spends earned currency in the meta layer." Owner: ux-designer. Consumers: ui-engineer, systems-engineer. Source user stories: US-30 to US-42, plus US-50, US-58. Sister flows: `02-run-loop.md` (entry) and `04-monetization-and-iap.md` (purchase branches).

## KPI guardrails

- **Home strap tap-rate ≥ 35%** on the "Next thing" surface (US-31).
- **Character unlock action ≤ 2 taps** from Home → roster → Unlock (US-32).
- **Battle pass claim tap-rate ≥ 50%** when claimable tier exists (US-35).
- **Play CTA always live** — no energy / cooldown text anywhere (US-36).
- **Daily mission tray loads in ≤ 1 frame** after Home render (US-33).

## Screens referenced

| Screen key | Wireframe target | First appearance |
|---|---|---|
| `screen=home` | `05-wireframes/36-home-play-cta.html` + `31-home-next-thing.html` | flow entry |
| `screen=daily_mission_tray` | `05-wireframes/33-daily-missions.html` | Home idle |
| `screen=mailbox` | `05-wireframes/40-mailbox.html` + `50-gift-inbox.html` | Mailbox tap |
| `screen=character_roster` | `05-wireframes/32-char-roster.html` | Heroes tap |
| `screen=char_level_upgrades` | `05-wireframes/38-char-level-upgrades.html` | Roster → hero tap |
| `screen=loadout_cosmetic` | `05-wireframes/41-loadout-cosmetic.html` | Hero → cosmetics tab |
| `screen=battle_pass` | `05-wireframes/48-battle-pass.html` | BP tap |
| `screen=achievements` | `05-wireframes/34-achievements.html` | Profile → Achievements |
| `screen=profile_stats` | `05-wireframes/42-profile-stats.html` | Profile tap |
| `screen=shop_main` | `05-wireframes/43-shop-main.html` | Shop tap |
| `screen=quit_confirm` | wireframe placeholder | Hardware back / Quit |

## Flow

```mermaid
flowchart TD
    %% ===== ENTRY =====
    A[Run-end Head home tap<br/>from flow 02 node BZ<br/>budget: ≤ 1 s transition] --> B[Home screen render<br/>screen=home<br/>Play pre-focused<br/>NO energy gate<br/>US-36]

    B --> C[Background updates fire<br/>BP increment<br/>daily mission progress<br/>achievement progress<br/>char-XP applied<br/>US-35, US-33, US-34, US-38]

    C --> D[Surface scan for Next-thing strap<br/>priority: streak → BP claim → char shard → daily<br/>US-31]
    D --> E{Pending<br/>top-priority?}
    E -->|unclaimed streak| F[Strap: claim streak<br/>copy: variant of DAILY_STREAK_HOOK<br/>US-31, US-37]
    E -->|BP tier claimable| G[Strap: Battle pass tier X ready<br/>US-31]
    E -->|char shard threshold| H[Strap: New hero unlock available<br/>US-31]
    E -->|daily mission near complete| I[Strap: 1 mission left for today<br/>US-31]
    E -->|nothing| J[Strap: NEXT_NUDGE_PLAY<br/>Off we go. Carrots await.<br/>US-31]

    F --> K[Home idle<br/>player chooses path]
    G --> K
    H --> K
    I --> K
    J --> K

    %% ===== HUD AROUND HOME =====
    K --> L[Persistent affordances visible<br/>Play CTA / Heroes / Battle Pass / Achievements / Shop / Mailbox badge / Settings gear / mute / biome row]

    %% ===== PATH 1: STRAP TAP DEEP-LINK =====
    K --> M{Player taps strap?}
    M -->|streak| N[Daily streak modal<br/>see flow 01 node EE<br/>US-37]
    M -->|BP claim| O[→ Battle pass branch]
    M -->|hero unlock| P[→ Heroes branch]
    M -->|mission| Q[Mission tray expand<br/>screen=daily_mission_tray<br/>3 missions visible<br/>resets in HH:MM<br/>US-33]

    %% ===== PATH 2: HEROES (CHARACTER ROSTER + UPGRADE) =====
    K --> R[Heroes tab tap]
    P --> R
    R --> S[Character roster<br/>screen=character_roster<br/>8 slots / 1 unlocked vertical slice<br/>each shows shard cost X / 200<br/>NO spin NO pull NO probability<br/>US-32]
    S --> T{Slot state}
    T -->|locked threshold not met| U[Tap shows non-blocking tooltip<br/>needs N more shards<br/>no paywall<br/>US-32]
    T -->|locked threshold met| V[Single Unlock button<br/>celebratory anim 1.2 s<br/>hero appears in loadout<br/>US-32]
    T -->|unlocked| W[Hero detail<br/>screen=char_level_upgrades<br/>3 stat trees HP / ATK / Move<br/>current → next deltas mirror US-23<br/>cost gold + char-XP<br/>US-38]
    W --> X{Player upgrades?}
    X -->|yes| Y[Confirm tap deducts currency<br/>new threshold may unlock rare draft entry<br/>e.g. Lv 3 unlocks Carrot Bomb<br/>US-38]
    X -->|cosmetics tab| Z[Loadout cosmetic carousel<br/>screen=loadout_cosmetic<br/>3D model rotates<br/>Preview tag on un-owned<br/>NO auto-buy on tap<br/>US-41]
    Z --> AA{Cosmetic action}
    AA -->|equip owned| AB[Saves immediately<br/>no confirm<br/>US-41]
    AA -->|tap un-owned| AC[→ IAP confirm — flow 04 node E]
    Y --> W
    AB --> W
    V --> R

    %% ===== PATH 3: BATTLE PASS =====
    K --> AD[Battle Pass tab tap]
    O --> AD
    AD --> AE[BP screen<br/>screen=battle_pass<br/>50 tier rows scrollable<br/>free left / premium right<br/>claim-ready pulse on completed tiers<br/>US-48]
    AE --> AF{Tier action}
    AF -->|claim free| AG[Reward popup + bank<br/>no confirm needed<br/>US-48]
    AF -->|claim premium owned| AH[Reward popup + bank<br/>US-48]
    AF -->|tap premium locked| AI[→ Unlock premium pass — flow 04 node M]
    AG --> AE
    AH --> AE

    %% ===== PATH 4: ACHIEVEMENTS =====
    K --> AJ[Profile tab tap]
    AJ --> AK[Profile stats screen<br/>screen=profile_stats<br/>lifetime totals<br/>Hero of the day card<br/>tone-bible vocab rascals sent home<br/>US-42]
    AK --> AL[Achievements sub-screen tap<br/>screen=achievements<br/>≥ 30 lifetime entries<br/>≤ 2 taps from Home<br/>US-34]
    AL --> AM{Achievement state}
    AM -->|claimable| AN[Tap claim<br/>cosmetic shard or gold<br/>never power upgrade<br/>US-34]
    AM -->|in progress| AO[Progress bar visible<br/>no action<br/>US-34]
    AN --> AL

    %% ===== PATH 5: SHOP BROWSE (NO IAP POPUP) =====
    K --> AP[Shop tab tap]
    AP --> AQ[Shop main<br/>screen=shop_main<br/>cosmetic-only<br/>≤ 2 taps from Home<br/>no notif badge unless free item<br/>US-43]
    AQ --> AR{Item tap}
    AR -->|tap any item| AS[Preview screen<br/>item on character or icon for weapon<br/>price + Buy<br/>NO auto-confirm<br/>US-43]
    AS --> AT{Choice}
    AT -->|Buy tap| AU[→ IAP confirm — flow 04 node E]
    AT -->|Back / tap outside| AQ

    %% ===== PATH 6: MAILBOX =====
    K --> AV[Mailbox icon tap<br/>badge shows unread count]
    AV --> AW[Mailbox screen<br/>screen=mailbox<br/>3 types: rewards / events / patch notes<br/>US-40]
    AW --> AX{Message type}
    AX -->|reward with expiry| AY[Claim single-tap<br/>banks currency or cosmetic<br/>US-40]
    AX -->|event notice| AZ[Info read<br/>tap dismiss]
    AX -->|patch notes| BA[Text read<br/>copy: MODAL_DISMISS_PATCH_NOTES]
    AX -->|dev-team gift| BB[Copy: IAP_GIFT_BANNER<br/>A friendly sponsor sent you a gift<br/>NEVER a paid SKU in disguise<br/>US-50]
    AY --> AW
    BB --> AW

    %% ===== PATH 7: REPLAY =====
    K --> BC[Play CTA tap<br/>always live<br/>→ flow 02 run loop<br/>US-36]

    %% ===== PATH 8: QUIT (GRACEFUL) =====
    K --> BD{Hardware back<br/>or Quit gesture?}
    BD -->|Android back from Home| BE[Quit confirm dialog<br/>screen=quit_confirm<br/>copy: BTN_CONFIRM_QUIT or Stay]
    BD -->|iOS swipe up| BF[OS-level background<br/>game writes profile to local<br/>resumes on next foreground]
    BE --> BG{Choice}
    BG -->|Quit| BH[Save player profile to disk<br/>flush analytics queue<br/>app exits<br/>NEVER lose meta currency banked]
    BG -->|Stay| K
    BF --> BH

    %% ===== HANDOFFS =====
    BH -. next launch .-> BI[→ flow 01 cold-start returning path]
    BC --> BJ[→ flow 02 run-loop entry]
    AC --> BK[→ flow 04-monetization]
    AI --> BK
    AU --> BK
```

## Branching: graceful quit (US-08 + US-36 implications)

- Quit from anywhere in Home is a **single tap on a confirm dialog** (no nag, no upsell, no "are you sure you want to leave your unclaimed reward?").
- All earned currencies (banked from any past run) are already on disk — no quit-time data loss possible.
- Background-on-iOS uses OS auto-save; foreground resume returns to Home (`screen=home`) — never to mid-run state (runs are sandboxed per `01-core-loop.md`).

## Anti-pattern enforcement (meta)

- **No energy / stamina bar** anywhere (US-36). Settings screen does not even mention these concepts.
- **No locked biome → paywall modal** ever (US-39). Only a non-blocking requirement tooltip.
- **No gacha / pull / spin UI** on character roster (US-32). Direct shard cost only.
- **No combat-power items** in the shop (US-43). Cosmetic-only.
- **No dev-gift message disguising an IAP offer** (US-50).
- **No achievement claim grants a power upgrade** (US-34). Cosmetic shards or gold only.

## Tone-bible-validated copy in this flow

- `{NEXT_NUDGE_PLAY}: "Off we go. Carrots await."` (US-31)
- `{DAILY_STREAK_HOOK}: "Three days running. Sturdy little adventurer."` (US-37)
- `{MISSION_KILL_50}: "Send 50 rascals packing."` (US-33)
- `{MAIL_GIFT_FROM_DEV}: "A small basket from the team. Thanks for sticking around."` (US-40)
- `{IAP_GIFT_BANNER}: "A friendly sponsor sent you a gift."` (US-50)
- Achievement names use "rascals sent home" not "kills" (US-34, tone bible §2).

## Meta-loop loops back to run-loop

The Home → Play CTA is the only exit point that re-enters `02-run-loop.md`. Every other meta path returns to Home with a single Back tap. Designers verify this in wireframe pass.
