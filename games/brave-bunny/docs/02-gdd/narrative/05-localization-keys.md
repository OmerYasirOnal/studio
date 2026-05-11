# Localization Keys — Brave Bunny

> Master key list for every visible string at launch. Owner: narrative-designer. Consumers: ui-engineer (lints raw English out of `.uxml` / `.cs`), localizer (TR-first, EN-second).
>
> Conventions: keys are `{SCREAMING_SNAKE}`. Variables inside copy are `{UPPER_SNAKE}`. TR locale uses informal "sen." Reading level ≤ 8th grade. No banned vocab (see tone-bible §2). No emoji. No string concatenation across translatable fragments.
>
> All keys below are the **EN source** and a **TR seed** translation. Where copy contains a runtime variable (`{GOLD}`, `{HERO}`, `{BIOME}`, `{TIME}`, `{PRICE}`, `{DAY}`), the variable is preserved verbatim in both locales.

---

## UI button strings

- {BTN_PLAY} → "Play"
- {BTN_PLAY_TR} → "Oyna"
- {BTN_BACK} → "Back"
- {BTN_BACK_TR} → "Geri"
- {BTN_CONFIRM} → "Yes, do it."
- {BTN_CONFIRM_TR} → "Tamam, yapalım."
- {BTN_CANCEL} → "Not yet."
- {BTN_CANCEL_TR} → "Şimdilik kalsın."
- {BTN_START_RUN} → "Off we go."
- {BTN_START_RUN_TR} → "Haydi başlayalım."
- {BTN_QUIT_RUN} → "Head home for now."
- {BTN_QUIT_RUN_TR} → "Şimdilik eve dönelim."
- {BTN_RETRY} → "One more hop."
- {BTN_RETRY_TR} → "Bir zıplama daha."
- {BTN_CONTINUE} → "Carry on."
- {BTN_CONTINUE_TR} → "Devam edelim."
- {BTN_PICK_HERO} → "Pick your hero."
- {BTN_PICK_HERO_TR} → "Kahramanını seç."
- {BTN_PICK_BIOME} → "Pick a region."
- {BTN_PICK_BIOME_TR} → "Bir bölge seç."
- {BTN_UPGRADE_TAKE} → "Take it."
- {BTN_UPGRADE_TAKE_TR} → "Aldım."
- {BTN_UPGRADE_SKIP} → "Skip this gift."
- {BTN_UPGRADE_SKIP_TR} → "Bu hediyeyi geç."
- {BTN_REROLL} → "Try again."
- {BTN_REROLL_TR} → "Tekrar deneyelim."
- {BTN_OPEN_SETTINGS} → "Settings"
- {BTN_OPEN_SETTINGS_TR} → "Ayarlar"
- {BTN_OPEN_SHOP} → "Burrow Shop"
- {BTN_OPEN_SHOP_TR} → "Yuva Dükkânı"
- {BTN_OPEN_KITCHEN} → "Kitchen"
- {BTN_OPEN_KITCHEN_TR} → "Mutfak"
- {BTN_CLAIM_DAILY} → "Claim today's gift."
- {BTN_CLAIM_DAILY_TR} → "Bugünkü hediyeyi al."
- {BTN_SHARE_RUN} → "Show a friend."
- {BTN_SHARE_RUN_TR} → "Bir arkadaşa göster."
- {BTN_PAUSE} → "Take a breath."
- {BTN_PAUSE_TR} → "Bir nefes al."
- {BTN_RESUME} → "Back at it."
- {BTN_RESUME_TR} → "Devam."
- {BTN_DISMISS_TIP} → "Got it, thanks."
- {BTN_DISMISS_TIP_TR} → "Anladım, sağ ol."
- {BTN_DISMISS_PATCH_NOTES} → "Sounds good."
- {BTN_DISMISS_PATCH_NOTES_TR} → "Tamamdır."

## In-game flavor

- {LEVEL_UP_FLAVOR_GENERIC} → "Choose your gift."
- {LEVEL_UP_FLAVOR_GENERIC_TR} → "Hediyeni seç."
- {LEVEL_UP_FLAVOR_PLUCKY} → "You feel pluckier. Choose your gift."
- {LEVEL_UP_FLAVOR_PLUCKY_TR} → "Biraz daha cesur hissediyorsun. Hediyeni seç."
- {LEVEL_UP_EVOLVE} → "Two gifts want to become one. Pick the pair."
- {LEVEL_UP_EVOLVE_TR} → "İki hediye birleşmek istiyor. Çifti seç."
- {RUN_END_WIN_GENERIC} → "Whew. Worth a carrot."
- {RUN_END_WIN_GENERIC_TR} → "Oh be. Bir havuca değdi."
- {RUN_END_LOSE_GENERIC} → "Tuckered out — but you banked {GOLD} carrots."
- {RUN_END_LOSE_GENERIC_TR} → "Yorulduk — ama {GOLD} havuç kasaya girdi."
- {HERO_REVIVE_PROMPT} → "Bunny got knocked silly. Want a quick nap and one more try?"
- {HERO_REVIVE_PROMPT_TR} → "Tavşan biraz sersemledi. Kısa bir şekerleme ve bir hak daha?"
- {SHARE_CARD} → "{HERO} cleared the {BIOME} in {TIME}. Beat that?"
- {SHARE_CARD_TR} → "{HERO}, {BIOME} bölgesini {TIME} sürede temizledi. Geçebilir misin?"
- {DAILY_STREAK_HOOK} → "Three days running. Sturdy little adventurer."
- {DAILY_STREAK_HOOK_TR} → "Üç gün üst üste. Sağlam küçük maceracı."
- {WAVE_INCOMING} → "Heads up — more rascals on the way."
- {WAVE_INCOMING_TR} → "Dikkat — sahaya yine yaramazlar geliyor."
- {ENEMY_DEFEATED_GENERIC} → "Rascal sent packing."
- {ENEMY_DEFEATED_GENERIC_TR} → "Yaramaz yola koyuldu."
- {PICKUP_CARROT} → "+{COUNT} carrot."
- {PICKUP_CARROT_TR} → "+{COUNT} havuç."
- {PICKUP_RIBBON} → "A bright ribbon. Pretty."
- {PICKUP_RIBBON_TR} → "Parlak bir kurdele. Hoş."
- {BIOME_INTRO_MEADOW} → "Sunny patch. Bunny pops out, ears up, ready to hop."
- {BIOME_INTRO_MEADOW_TR} → "Güneşli bir köşe. Tavşan dışarı zıpladı, kulaklar dik, hazır."
- {BIOME_INTRO_BEACH} → "Warm sand, salt in the air, and a crab who looks very busy."
- {BIOME_INTRO_BEACH_TR} → "Sıcak kum, tuzlu hava ve epey meşgul görünen bir yengeç."
- {BIOME_INTRO_FOREST} → "Cool shade, leaves whispering. Mind your step under the canopy."
- {BIOME_INTRO_FOREST_TR} → "Serin gölge, yapraklar fısıldıyor. Ağaçların altında adımına dikkat et."
- {BIOME_INTRO_CAVERN} → "Glow-mushrooms light the way. Someone's been down here already."
- {BIOME_INTRO_CAVERN_TR} → "Işıltılı mantarlar yolu aydınlatıyor. Birileri buraya önceden inmiş."
- {BIOME_INTRO_SNOW} → "Soft drifts, quiet hills. Footprints lead off toward the pines."
- {BIOME_INTRO_SNOW_TR} → "Yumuşak kar yığınları, sessiz tepeler. Ayak izleri çamların arkasına doğru gidiyor."
- {BOSS_INTRO_BOAR} → "Old Boar's awake. Mind your tail."
- {BOSS_INTRO_BOAR_TR} → "Koca Yaban uyandı. Kuyruğuna dikkat."
- {BOSS_INTRO_CRAB} → "Crab Captain's on shore patrol. Watch the pincer."
- {BOSS_INTRO_CRAB_TR} → "Yengeç Kaptan kıyıda nöbette. Kıskaca dikkat."
- {BOSS_INTRO_OAK} → "Mama Oak's roots are up. Step lightly."
- {BOSS_INTRO_OAK_TR} → "Koca Meşe'nin kökleri uyandı. Hafif bas."
- {BOSS_INTRO_MOLE} → "Cave Mole's in the floor. Listen for the rumble."
- {BOSS_INTRO_MOLE_TR} → "Sinsi Köstebek yerin altında. Sesini dinle."
- {BOSS_INTRO_YETI} → "Big Snow-yeti's grumpy. Keep your paws warm."
- {BOSS_INTRO_YETI_TR} → "Koca Kar-yetisi huysuz. Patilerin sıcak kalsın."

## Settings / accessibility

- {SETTINGS_AUDIO} → "Audio"
- {SETTINGS_AUDIO_TR} → "Ses"
- {SETTINGS_MUSIC_VOLUME} → "Music volume"
- {SETTINGS_MUSIC_VOLUME_TR} → "Müzik sesi"
- {SETTINGS_SFX_VOLUME} → "Sound effects"
- {SETTINGS_SFX_VOLUME_TR} → "Efekt sesi"
- {SETTINGS_HAPTICS} → "Vibration"
- {SETTINGS_HAPTICS_TR} → "Titreşim"
- {SETTINGS_LANGUAGE} → "Language"
- {SETTINGS_LANGUAGE_TR} → "Dil"
- {SETTINGS_REDUCED_MOTION} → "Calmer screen movement"
- {SETTINGS_REDUCED_MOTION_TR} → "Daha sakin ekran hareketi"
- {SETTINGS_LARGE_TEXT} → "Larger text"
- {SETTINGS_LARGE_TEXT_TR} → "Daha büyük yazı"
- {SETTINGS_HIGH_CONTRAST} → "Higher contrast"
- {SETTINGS_HIGH_CONTRAST_TR} → "Daha yüksek kontrast"
- {SETTINGS_LEFTY_LAYOUT} → "Left-hand layout"
- {SETTINGS_LEFTY_LAYOUT_TR} → "Sol el yerleşimi"
- {SETTINGS_COLORBLIND_MODE} → "Color-friendly mode"
- {SETTINGS_COLORBLIND_MODE_TR} → "Renk dostu mod"
- {SETTINGS_AUTO_AIM} → "Aim helper"
- {SETTINGS_AUTO_AIM_TR} → "Nişan yardımı"
- {SETTINGS_RESET_TUTORIAL} → "Show me the tour again."
- {SETTINGS_RESET_TUTORIAL_TR} → "Turu bana bir daha göster."
- {SETTINGS_PRIVACY} → "Privacy"
- {SETTINGS_PRIVACY_TR} → "Gizlilik"
- {SETTINGS_CREDITS} → "Made by"
- {SETTINGS_CREDITS_TR} → "Yapanlar"
- {SETTINGS_VERSION_LABEL} → "Version {VERSION}"
- {SETTINGS_VERSION_LABEL_TR} → "Sürüm {VERSION}"

## Monetization (carefully written, tone-bible-correct)

- {IAP_BATTLE_PASS_PROMPT} → "Want to add the Pass for {PRICE}?"
- {IAP_BATTLE_PASS_PROMPT_TR} → "Pass'i {PRICE} karşılığında alalım mı?"
- {IAP_BATTLE_PASS_BENEFIT} → "Extra weekly carrots and a cosmetic ribbon."
- {IAP_BATTLE_PASS_BENEFIT_TR} → "Haftalık ekstra havuç ve süs kurdelesi."
- {IAP_GIFT_BANNER} → "A friendly sponsor sent you a gift."
- {IAP_GIFT_BANNER_TR} → "Dost bir sponsor sana hediye gönderdi."
- {IAP_COSMETIC_HAT_PROMPT} → "A little hat for Bunny — {PRICE}?"
- {IAP_COSMETIC_HAT_PROMPT_TR} → "Tavşana ufak bir şapka — {PRICE}?"
- {AD_REVIVE_PROMPT} → "Watch a quick ad to hop back in?"
- {AD_REVIVE_PROMPT_TR} → "Kısa bir reklam izleyip yeniden zıplayalım mı?"
- {AD_DOUBLE_REWARD_PROMPT} → "Watch a quick ad to double your carrots?"
- {AD_DOUBLE_REWARD_PROMPT_TR} → "Kısa bir reklam izleyip havuçları ikiye katlayalım mı?"
- {AD_OPT_OUT_HINT} → "You can turn ads off in Settings."
- {AD_OPT_OUT_HINT_TR} → "Reklamları Ayarlar'dan kapatabilirsin."
- {STORE_PURCHASE_THANKS} → "Thanks for the snack money."
- {STORE_PURCHASE_THANKS_TR} → "Atıştırmalık parası için teşekkürler."
- {STORE_PURCHASE_FAIL} → "Something hiccupped. Please try again in a moment."
- {STORE_PURCHASE_FAIL_TR} → "Bir aksilik oldu. Birazdan tekrar dener misin."

## Tone-bible-banned alternatives reminder

> These strings DO NOT exist in the build. Listed so future writers (event copy, voiceover, marketing) do not accidentally re-introduce them. The "replaced by" column is the canonical equivalent.

- "You died." — replaced by **{RUN_END_LOSE_GENERIC}**: "Tuckered out — but you banked {GOLD} carrots."
- "Enemy killed." — replaced by **{ENEMY_DEFEATED_GENERIC}**: "Rascal sent packing."
- "Game Over." — replaced by **{BTN_RETRY}** + **{RUN_END_LOSE_GENERIC}** combo; we never use the phrase.
- "Destroy your enemies." — replaced by intro flavor on each biome card; "destroy" is permanently banned.
- "Slay the boss." — replaced by per-boss intro cards (`04-boss-intros.md`); "slay" is permanently banned.
- "Dark cave / Cursed forest." — replaced by **{BIOME_INTRO_CAVERN}** / **{BIOME_INTRO_FOREST}**; "dark" / "cursed" are permanently banned.
- "Epic loot drop." — replaced by **{LEVEL_UP_FLAVOR_PLUCKY}**; "epic" is permanently banned.
- "Don't be a noob — buy the Pass!" — replaced by **{IAP_BATTLE_PASS_PROMPT}** (never punch down at the player; never imply not paying = noob).
- "Pay to win." — the brand promise is no-pay-to-win; this phrase never appears, not even as a denial.
- "RIP." — replaced by **{HERO_REVIVE_PROMPT}** ("Bunny got knocked silly.").

---

## Cross-references

- Voice register, banned vocab list, sample copy: `00-tone-bible.md`.
- Biome-intro hook lines (canonical source for `{BIOME_INTRO_*}`): `03-biome-flavor.md`.
- Boss intro / taunt / phase / defeat lines: `04-boss-intros.md`.
- Hero-specific idle / attack / level-up / win lines: `02-character-bios/*.md`.
- Variable contract: `{GOLD}` integer carrots; `{HERO}` localized hero display name; `{BIOME}` localized biome display name; `{TIME}` mm:ss; `{COUNT}` integer pickup count; `{PRICE}` storefront-localized price string (Apple/Google formatted); `{VERSION}` semver string; `{DAY}` integer day index.
- ui-engineer enforces `Loc("key")` lint at commit time per tone-bible §6.
