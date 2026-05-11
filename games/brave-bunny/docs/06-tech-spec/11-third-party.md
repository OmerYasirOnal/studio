# Tech Spec 11 — Third-party Packages

> Owner: tech-architect. The approved third-party Unity packages for Brave Bunny, their licenses, and the canonical `unity/Packages/manifest.json` shipped with the project. **All packages must be free + permissively licensed** (MIT / Apache 2.0 / CC0 / Unity Companion License). Per repo-root `CLAUDE.md`: no paid third-party services, no AI runtime middleware, no Asset Store paid plugins. Cross-refs: `00-engine-and-version.md` (Unity 6 LTS + IL2CPP), `02-data-model.md` (Newtonsoft Json for save serialization), `03-save-system.md` (Newtonsoft Json), `06-rendering.md` (URP + Shader Graph), `07-audio.md` (Audio Mixer is Unity built-in — no third party), `10-build-and-ci.md` (Fastlane for build).

## Approved package list

| Package | Source | License | Version (target) | Purpose |
|---|---|---|---|---|
| **Universal Render Pipeline** | Unity (free, bundled) | Unity Companion | bundled with Unity 6 LTS | URP rendering substrate per `06-rendering.md` |
| **Shader Graph** | Unity (free, bundled) | Unity Companion | bundled | Authoring custom toon shader per ADR-0002 |
| **VFX Graph** | Unity (free) | Unity Companion | bundled | Particle authoring per `07-art-bible/05-vfx-style.md` |
| **Input System** | Unity (free) | Unity Companion | 1.8.x | Action maps + virtual joystick per `04-input-system.md` |
| **Animation Rigging** | Unity (free) | Unity Companion | 1.3.x | IK + procedural rig blends |
| **TextMeshPro** | Unity (free, bundled) | Unity Companion | bundled | All in-game text (UI Toolkit + 3D world labels) |
| **Newtonsoft Json** (`com.unity.nuget.newtonsoft-json`) | Unity (free; wraps the MIT library) | MIT | 3.2.x | Save serialization per `03-save-system.md` |
| **UniTask** (`com.cysharp.unitask`) | Cysharp (GitHub) | MIT | 2.5.x | Coroutine-free async — replaces coroutines in hot loops |
| **Burst** | Unity (free) | Unity Companion | 1.8.x | C# → native compile for collision + spawn jobs |
| **Mathematics** | Unity (free) | Unity Companion | 1.3.x | SIMD math types for jobs (`float2`, `float3`) |
| **Collections** | Unity (free) | Unity Companion | 2.5.x | `NativeArray<T>`, `NativeList<T>` for jobs |
| **Jobs** (`com.unity.jobs`) | Unity (free) | Unity Companion | bundled | Job System for spatial-hash broadphase per `05-performance-budget.md` |
| **Addressables** | Unity (free) | Unity Companion | 2.2.x | **v1.1+ optional** — AssetBundles deferred per `10-build-and-ci.md` |
| **Unity IAP** (`com.unity.purchasing`) | Unity (free) | Unity Companion | 5.0.x | IAP — required for App Store delivery; framework allow-list exception per repo-root `CLAUDE.md` |
| **Google Mobile Ads (AdMob)** | Google (free SDK) | Google EULA (free for use) | 11.x | Rewarded-ad revive + battle-pass perk; framework allow-list exception per repo-root `CLAUDE.md` |
| **glTFast** (`com.unity.cloud.gltfast`) | Unity (free) | Apache 2.0 | 6.x | glTF 2.0 import for Quaternius CC0 sources per `00-engine-and-version.md` |
| **Unity Localization** (`com.unity.localization`) | Unity (free) | Unity Companion | 1.5.x | Localization tables (EN at launch; TR / PH / ID for soft-launch markets) |
| **Test Framework** (`com.unity.test-framework`) | Unity (free) | Unity Companion | bundled | NUnit + PlayMode harness for `Brave.Tests.*` asmdefs |

### Explicitly **NOT** approved without an ADR

| Package | Reason banned |
|---|---|
| **DoTween Pro** | Paid Asset Store — use DoTween Free (MIT) if absolutely needed, but UniTask covers our async surface (preferred) |
| **Odin Inspector** | Paid Asset Store — use Unity's built-in serialization + custom editors instead |
| **Behavior Designer** | Paid Asset Store — boss AI uses hand-rolled state machines per `08-state-machine.md` pattern |
| **PlayMaker** | Paid Asset Store — design rejects visual-scripting for gameplay |
| **A* Pathfinding Project (Pro)** | Paid tier — Unity NavMesh covers our locomotion needs |
| **Any paid AI runtime middleware** (Convai, Inworld, etc.) | Repo-root `CLAUDE.md` forbids paid APIs |
| **ML-Agents** | Free, but adds substantial binary weight; revisit only if a runtime ML feature ships |
| **Cinemachine** | Free + Unity Companion-licensed, but **not adopted at launch** — survivor camera is fixed top-down, hand-coded camera follow. Reconsider for Phase 6 polish if camera shake / boss framing complexity grows |

Cinemachine is the one borderline call — it's free, but we don't have the camera complexity to justify it. If a Phase 5 reviewer wants it, file an ADR.

## Why each non-Unity package earned its slot

- **Newtonsoft Json:** `JsonUtility` (Unity built-in) cannot handle polymorphism or `null` defaults gracefully; our save schema in `03-save-system.md` needs JObject mutation in migrations. MIT-licensed; free; ships via Unity's Package Manager.
- **UniTask:** Coroutines allocate; UniTask doesn't. Our 0-allocation hot-loop contract from `05-performance-budget.md` makes coroutines unusable for state transitions, audio ducking, and animation timing. UniTask is the de facto standard for Unity async/await with zero alloc.
- **glTFast:** Quaternius CC0 packs ship as glTF 2.0; Unity's built-in importer is FBX-only. glTFast is Unity's own Apache 2.0 package — no external dependency added.
- **Google Mobile Ads:** AdMob is the only no-paid-tier rewarded-ads SDK that is App Store-permitted at our pre-revenue tier. Repo-root `CLAUDE.md` carves out IAP + AdMob explicitly as "revenue side, not generation side."

## `unity/Packages/manifest.json`

The canonical manifest committed to git:

```json
{
  "dependencies": {
    "com.unity.render-pipelines.universal": "17.0.3",
    "com.unity.shadergraph": "17.0.3",
    "com.unity.visualeffectgraph": "17.0.3",
    "com.unity.inputsystem": "1.8.2",
    "com.unity.animation.rigging": "1.3.0",
    "com.unity.textmeshpro": "3.2.0-pre.10",
    "com.unity.nuget.newtonsoft-json": "3.2.1",
    "com.cysharp.unitask": "2.5.10",
    "com.unity.burst": "1.8.18",
    "com.unity.mathematics": "1.3.2",
    "com.unity.collections": "2.5.1",
    "com.unity.jobs": "0.70.0-preview.7",
    "com.unity.addressables": "2.2.2",
    "com.unity.purchasing": "5.0.0",
    "com.google.ads.mobile": "10.4.0",
    "com.unity.cloud.gltfast": "6.10.1",
    "com.unity.localization": "1.5.3",
    "com.unity.test-framework": "1.4.5",
    "com.unity.ugui": "2.0.0",
    "com.unity.modules.audio": "1.0.0",
    "com.unity.modules.physics": "1.0.0",
    "com.unity.modules.physics2d": "1.0.0",
    "com.unity.modules.ui": "1.0.0",
    "com.unity.modules.uielements": "1.0.0"
  },
  "scopedRegistries": [
    {
      "name": "OpenUPM (UniTask)",
      "url": "https://package.openupm.com",
      "scopes": ["com.cysharp.unitask"]
    }
  ]
}
```

**Notes on the manifest:**

- UniTask is fetched from OpenUPM (community registry); the registry serves verbatim mirrors of the GitHub source under MIT license. No paid tier.
- Google Mobile Ads is fetched from Google's UPM registry (added by build-engineer at first beta build); listed here for completeness even though its registry entry is added at first use.
- Version pins are exact (no `^` / `~`); upgrades go through ADR.

## License audit

Every package above is **free + permissive**. The license matrix:

- **MIT:** Newtonsoft Json, UniTask, DoTween (if added later).
- **Apache 2.0:** glTFast.
- **Unity Companion License:** all Unity-authored packages — free for our revenue tier per `00-engine-and-version.md`.
- **Google EULA (free):** AdMob — free for revenue use; revenue share is Google's standard ad-network model, not a fee to Google.

Per repo-root `CLAUDE.md` principle 8 ("CC0 / OFL / MIT / CC-BY only"), every package conforms. CI guard: a `tools/ci/license-audit.sh` (build-engineer to wire) parses `manifest.json`, verifies each package's license against the allow-list, and fails the build on a mismatch.

## When to add a new package

1. **Search Unity-built-in first** — if Unity ships it, use it.
2. **Search MIT / Apache 2.0 GitHub second** — Cysharp, neuecc, alelievr have authored most of the free Unity ecosystem.
3. **ADR required** before adding a new package — document the use case, the license, and the rejected alternatives.
4. **Lock the version** in `manifest.json` exactly; never use `latest` or floating ranges.

## Cross-references

- Repo-root `CLAUDE.md` — principles 8 (asset policy) + zero-paid-API rule.
- `core/docs/asset-policy.md` — CC0 / MIT only baseline.
- `00-engine-and-version.md` — Unity 6 LTS, IL2CPP, ASTC 4×4 (drives Burst + glTFast inclusion).
- `02-data-model.md` — Newtonsoft Json consumer for SO generation.
- `03-save-system.md` — Newtonsoft Json consumer for save serialization.
- `05-performance-budget.md` — UniTask + Burst + Jobs + Collections drive the zero-alloc hot-loop contract.
- `06-rendering.md` — URP + Shader Graph + VFX Graph.
- `10-build-and-ci.md` — Fastlane (Ruby gem, separate from Unity packages, not in `manifest.json`).
