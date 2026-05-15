// Brave Bunny — Systems / Audio
// Wires gameplay event channels (Brave.Gameplay.Events.*) to the audio dispatcher
// (ISfxDispatcher) so the game finally makes noise on every kill/hit/levelup/pickup/run-end.
//
// Cross-refs:
//   * docs/08-audio-bible/02-sfx-spec.md     — slug catalog (source of truth for clip mapping).
//   * docs/06-tech-spec/09-event-bus.md      — Tier-3 ScriptableObject channels are the loose-
//                                              coupling boundary between Gameplay and Systems.
//   * Brave.Gameplay.Events.{DeathChannel,EnemyKilledChannel,LevelUpChannel,PickupChannel,BossPhaseChannel}.
//   * Brave.Systems.Audio.SfxSlug           — string slug constants.
//
// Design:
//   * MonoBehaviour with [SerializeField] channel refs. Lives in the Run scene next to the
//     other event listeners (RunHudController + co.). Survives in EditMode tests because the
//     hot path (HandleX → dispatcher.PlaySfx) is exercised directly with a stub dispatcher.
//   * NO new event types — we use only the 5 channels approved by tech-spec 09 § Tier-3.
//   * Weapon-fire SFX is hooked through the public NotifyWeaponFired() method because no
//     weapon-fire channel exists yet (would require a new event type and a new ADR). The
//     auto-attack controller can call this directly once Brave.Gameplay depends on Systems,
//     or via a static bridge — both follow-ups are tracked in the hand-off note.
//   * Wave-cleared has no dedicated channel either; for now the WaveCleared SFX is exposed
//     via a public method; gameplay can call it when the wave runner declares a wave done.
//   * Run-end audio dispatches by DeathCause: Victory → run_end_win, Killed/Quit/TimedOut →
//     run_end_lose. The HeroDeath slug is fired alongside Killed so the death cue lands
//     before the run-end stinger.
//
// All slug references must exist in SfxSlug. If a clip file is missing the dispatcher logs
// a warning once (see SfxDispatcher TODOs) and the gameplay loop continues; bindings never
// throw on a missing clip.

#nullable enable

using System;
using Brave.Gameplay.Events;
using Brave.Systems.Context;
using UnityEngine;

namespace Brave.Systems.Audio
{
    /// <summary>
    /// Subscribes to the five gameplay event channels and dispatches the matching SFX slug
    /// through <see cref="ISfxDispatcher"/>. Implements <see cref="IService"/> so it can be
    /// registered with <see cref="GameContext"/> by <see cref="GameContextBootstrap"/> when
    /// the Boot scene wires audio (channel asset wiring still happens in the Run scene where
    /// the SO assets live).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameplayAudioBindings : MonoBehaviour, IService
    {
        // ---- Serialized channel references (wired from Run scene SO assets) ----
        [Header("Gameplay event channels")]
        [SerializeField] private DeathChannel? _deathChannel;
        [SerializeField] private EnemyKilledChannel? _enemyKilledChannel;
        [SerializeField] private LevelUpChannel? _levelUpChannel;
        [SerializeField] private PickupChannel? _pickupChannel;
        [SerializeField] private BossPhaseChannel? _bossPhaseChannel;

        // ---- Dispatcher reference. Set via SetDispatcher() at Boot OR resolved at OnEnable. ----
        private ISfxDispatcher? _dispatcher;
        private bool _subscribed;

        /// <summary>
        /// Inject the SFX dispatcher. Called by the Boot/Run wiring step before
        /// <see cref="OnEnable"/> subscribes. If unset, <see cref="OnEnable"/> resolves
        /// from <see cref="GameContextBootstrap.Context"/> as a fallback.
        /// </summary>
        public void SetDispatcher(ISfxDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        /// <summary>Test-only constructor-like initialiser. Wires channels + dispatcher without
        /// requiring a serialized inspector. Subscriptions are activated by <see cref="Subscribe"/>.</summary>
        public void ConfigureForTests(
            ISfxDispatcher dispatcher,
            DeathChannel? deathChannel = null,
            EnemyKilledChannel? enemyKilledChannel = null,
            LevelUpChannel? levelUpChannel = null,
            PickupChannel? pickupChannel = null,
            BossPhaseChannel? bossPhaseChannel = null)
        {
            _dispatcher = dispatcher;
            _deathChannel = deathChannel;
            _enemyKilledChannel = enemyKilledChannel;
            _levelUpChannel = levelUpChannel;
            _pickupChannel = pickupChannel;
            _bossPhaseChannel = bossPhaseChannel;
        }

        private void OnEnable() => Subscribe();
        private void OnDisable() => Unsubscribe();

        /// <summary>Idempotent subscription pass. Safe to call from tests directly.</summary>
        public void Subscribe()
        {
            if (_subscribed) return;

            // Late-bind the dispatcher from GameContext if Boot never injected one. This keeps
            // the binding functional in scenes loaded directly from the editor (e.g. opening
            // Run.unity without going through Boot).
            if (_dispatcher == null)
            {
                var ctx = GameContextBootstrap.Context;
                if (ctx != null && ctx.TryGet<ISfxDispatcher>(out var resolved))
                    _dispatcher = resolved;
            }

            if (_deathChannel != null) _deathChannel.Subscribe(HandleDeath);
            if (_enemyKilledChannel != null) _enemyKilledChannel.Subscribe(HandleEnemyKilled);
            if (_levelUpChannel != null) _levelUpChannel.Subscribe(HandleLevelUp);
            if (_pickupChannel != null) _pickupChannel.Subscribe(HandlePickup);
            if (_bossPhaseChannel != null) _bossPhaseChannel.Subscribe(HandleBossPhase);

            _subscribed = true;
        }

        /// <summary>Idempotent unsubscribe. Mirrors <see cref="Subscribe"/>.</summary>
        public void Unsubscribe()
        {
            if (!_subscribed) return;
            if (_deathChannel != null) _deathChannel.Unsubscribe(HandleDeath);
            if (_enemyKilledChannel != null) _enemyKilledChannel.Unsubscribe(HandleEnemyKilled);
            if (_levelUpChannel != null) _levelUpChannel.Unsubscribe(HandleLevelUp);
            if (_pickupChannel != null) _pickupChannel.Unsubscribe(HandlePickup);
            if (_bossPhaseChannel != null) _bossPhaseChannel.Unsubscribe(HandleBossPhase);
            _subscribed = false;
        }

        // ---- Channel handlers ----

        /// <summary>Hero hit / death + run-end win-or-lose stinger.</summary>
        internal void HandleDeath(DeathEvent evt)
        {
            if (_dispatcher == null) return;

            // Hero-death pip lands first, then the run-end stinger 1-frame later (the dispatcher
            // queues both onto its 12-voice pool — no synchronous ordering required here).
            switch (evt.cause)
            {
                case DeathCause.Killed:
                    _dispatcher.PlaySfx(SfxSlug.HeroDeath, Vector3.zero);
                    _dispatcher.PlayUi(SfxSlug.RunEndLose);
                    break;
                case DeathCause.TimedOut:
                case DeathCause.Quit:
                    _dispatcher.PlayUi(SfxSlug.RunEndLose);
                    break;
                case DeathCause.Victory:
                    _dispatcher.PlayUi(SfxSlug.RunEndWin);
                    break;
            }
        }

        /// <summary>Per-enemy death poof. Elite vs swarmer routing per audio-bible Pillar 1.</summary>
        internal void HandleEnemyKilled(EnemyKilledEvent evt)
        {
            if (_dispatcher == null) return;
            string slug = evt.wasElite ? SfxSlug.EnemyEliteDie : SfxSlug.EnemySwarmerDie;
            _dispatcher.PlaySfx(slug, evt.position);
        }

        /// <summary>Level-up fanfare + arpeggio (Pillar 2). Both slugs are layered per audio-bible.</summary>
        internal void HandleLevelUp(LevelUpEvent evt)
        {
            if (_dispatcher == null) return;
            _dispatcher.PlayUi(SfxSlug.RunLevelup);
            _dispatcher.PlayUi(SfxSlug.HeroLevelupFanfare);
        }

        /// <summary>Pickup chimes — xp small/large, gold, heart. Soul-shard maps to gold for v0.1.</summary>
        internal void HandlePickup(PickupEvent evt)
        {
            if (_dispatcher == null) return;
            string slug = SlugForPickup(evt.kind);
            _dispatcher.PlaySfx(slug, evt.position);
        }

        /// <summary>Boss phase change sting (also fires on phase 1 = boss intro).</summary>
        internal void HandleBossPhase(BossPhaseEvent evt)
        {
            if (_dispatcher == null) return;
            string slug = evt.newPhase <= 1 ? SfxSlug.BossIntroSting : SfxSlug.BossPhaseChange;
            _dispatcher.PlayUi(slug);
        }

        /// <summary>Pure slug-routing helper. Exposed for unit tests.</summary>
        internal static string SlugForPickup(PickupKind kind) => kind switch
        {
            PickupKind.XpGemSmall  => SfxSlug.RunPickupXpSmall,
            PickupKind.XpGemMedium => SfxSlug.RunPickupXpSmall,
            PickupKind.XpGemLarge  => SfxSlug.RunPickupXpLarge,
            PickupKind.GoldCoin    => SfxSlug.RunPickupGold,
            PickupKind.Heart       => SfxSlug.RunPickupHeart,
            // Soul-shard is meta currency — reuse gold chime for v0.1 until a meta slug ships.
            PickupKind.SoulShard   => SfxSlug.RunPickupGold,
            _ => SfxSlug.RunPickupGold,
        };

        // ---- Public dispatch hooks for callers without dedicated event channels ----

        /// <summary>
        /// Dispatch a weapon-fire SFX. Called directly by the auto-attack controller (or its
        /// successor) when a weapon discharges. No event channel exists yet because adding
        /// one is out-of-scope for this wave (would require a new approved event type).
        /// Archetypes: "projectile" → carrot fire; "aura" → sunbeam loop; "area" → daisy drop.
        /// Unknown archetypes fall through to <see cref="SfxSlug.WeaponCarrotFire"/>.
        /// TODO(audio-bible follow-up): introduce a WeaponFireChannel once GDD 04 lists the
        /// full archetype taxonomy. Until then this is the seam.
        /// </summary>
        public void NotifyWeaponFired(string archetype, Vector3 worldPosition)
        {
            if (_dispatcher == null) return;
            string slug = archetype switch
            {
                "projectile" => SfxSlug.WeaponCarrotFire,
                "aura"       => SfxSlug.WeaponSunbeamStart,
                "area"       => SfxSlug.WeaponDaisyDrop,
                _            => SfxSlug.WeaponCarrotFire,
            };
            _dispatcher.PlaySfx(slug, worldPosition);
        }

        /// <summary>
        /// Dispatch the wave-complete fanfare. Called by the wave runner when a wave clears.
        /// Mapped to <see cref="SfxSlug.RunLevelup"/> for v0.1 because the audio-bible
        /// "wave_complete" cue is folded into the level-up arpeggio in the vertical slice
        /// (see 02-sfx-spec.md vertical-slice column — no dedicated wave_complete slug).
        /// TODO(audio-bible follow-up): split into its own slug if QA flags the overlap.
        /// </summary>
        public void NotifyWaveCleared()
        {
            if (_dispatcher == null) return;
            _dispatcher.PlayUi(SfxSlug.RunLevelup);
        }

        /// <summary>
        /// Dispatch the player-hurt pip. Called from <c>EnemyContactDamage</c> or any other
        /// system that applies damage to the hero. No DamageDealt event channel exists in
        /// tech-spec 09 § Tier-3, so callers invoke this directly.
        /// </summary>
        public void NotifyPlayerHit(Vector3 worldPosition)
        {
            if (_dispatcher == null) return;
            _dispatcher.PlaySfx(SfxSlug.HeroHit, worldPosition);
        }

        // ---- Diagnostics ----

        /// <summary>Test-only accessor for the active dispatcher (null when un-injected).</summary>
        internal ISfxDispatcher? DispatcherForTests => _dispatcher;

        /// <summary>Test-only accessor for the subscription latch.</summary>
        internal bool IsSubscribedForTests => _subscribed;
    }
}
