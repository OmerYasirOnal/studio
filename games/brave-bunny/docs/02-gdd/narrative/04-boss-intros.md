# Boss Intros — Brave Bunny

> Per-boss intro cards, in-fight taunts, phase-change lines, and defeat lines for all 5 launch bosses. Owner: narrative-designer. Consumers: ui-engineer (intro card overlay), gameplay-engineer (taunt/phase-change triggers), localizer (TR seeds).
>
> Strict tone-bible adherence (`00-tone-bible.md`). Big Troubles are not evil — they are misunderstood. Old Boar King is sleepy and grumpy; Mama Oak is protective; Big Snow-yeti is just cold and a bit cranky. Each boss gets exactly 4 strings + a TR seed. All intro cards ≤ 12 words.

## Old Boar King — Meadow

**Intro card (≤12 words):** "Old Boar's awake. Mind your tail."
**One-line taunt (during fight):** "Off my daisies, you wee thing."
**Phase-change line (mid-fight):** "Right. Fine. Now I'm cross."
**Defeat line (when defeated):** "Alright, alright. I'll nap somewhere else."
**Localization seed (TR):**
- Intro: "Koca Yaban uyandı. Kuyruğuna dikkat."
- Defeat: "Tamam tamam. Başka yerde kestiririm."

---

## Crab Captain — Beach

**Intro card (≤12 words):** "Crab Captain's on shore patrol. Watch the pincer."
**One-line taunt (during fight):** "My beach. My carrots. Off you go."
**Phase-change line (mid-fight):** "Right then. Both claws now."
**Defeat line (when defeated):** "Fine. You can have the shore for today."
**Localization seed (TR):**
- Intro: "Yengeç Kaptan kıyıda nöbette. Kıskaca dikkat."
- Defeat: "Peki. Bugünlük kıyı sizin olsun."

---

## Mama Oak — Forest

**Intro card (≤12 words):** "Mama Oak's roots are up. Step lightly."
**One-line taunt (during fight):** "These are my little ones. Mind yourself."
**Phase-change line (mid-fight):** "Now you've gone and woken the branches."
**Defeat line (when defeated):** "Off you hop, then. Don't trample the saplings."
**Localization seed (TR):**
- Intro: "Koca Meşe'nin kökleri uyandı. Hafif bas."
- Defeat: "Hadi zıpla bakalım. Fidanlara basma sakın."

---

## Sneaky Cave Mole — Cavern

**Intro card (≤12 words):** "Cave Mole's in the floor. Listen for the rumble."
**One-line taunt (during fight):** "Down here. No — up here. Made you look."
**Phase-change line (mid-fight):** "Right, no more games. Real digging now."
**Defeat line (when defeated):** "Well played. I'll dig somewhere quieter."
**Localization seed (TR):**
- Intro: "Sinsi Köstebek yerin altında. Sesini dinle."
- Defeat: "İyi oynadın. Daha sessiz bir yer kazarım."

---

## Big Snow-yeti — Snow

**Intro card (≤12 words):** "Big Snow-yeti's grumpy. Keep your paws warm."
**One-line taunt (during fight):** "Off my hill. It's a quiet hill."
**Phase-change line (mid-fight):** "Right. Now the proper stomping."
**Defeat line (when defeated):** "Fair. I needed a sit-down anyway."
**Localization seed (TR):**
- Intro: "Koca Kar-yetisi huysuz. Patilerin sıcak kalsın."
- Defeat: "Haklısın. Zaten biraz oturmam lazımdı."

---

## Cross-references

- Boss roster + mechanics: `../06-biomes.md` (boss-per-biome rows). Detailed boss spec doc `07-bosses.md` is owned by game-designer (see `06-biomes.md` cross-refs).
- Voice anchors: `00-tone-bible.md` — note the `{BOSS_INTRO_BOAR}` example string is duplicated here as the canonical Old Boar King intro card.
- Hero counter-voice lines (Bunny, Tortoise, Fox, etc.) live in `02-character-bios/*.md`.
- All boss string keys (intro / taunt / phase / defeat) are registered in `05-localization-keys.md` under the **In-game flavor** section.
- TR register: informal-friendly second-person ("sen"), softening particles welcome. No formal "siz." Per tone-bible §6.
