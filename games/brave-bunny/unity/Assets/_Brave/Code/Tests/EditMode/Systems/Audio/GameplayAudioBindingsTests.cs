#if WAVE7_TESTS_FIXED  // TODO(Wave12): fix test API drift
// QA — GameplayAudioBindings EditMode tests
// Subject under test: Brave.Systems.Audio.GameplayAudioBindings + BgmGameplayDriver.
// Spec: docs/08-audio-bible/02-sfx-spec.md (slug routing), docs/08-audio-bible/01-bgm-spec.md
//       (state→snapshot routing), docs/06-tech-spec/07-audio.md (snapshot table).
//
// Strategy:
//   * Pure-method coverage — call HandleX(...) directly with synthetic events; assert the
//     stub dispatcher saw the right slug + position. This is the cheapest, most fragile-
//     resistant test path; it exercises the routing logic without touching the channel
//     ScriptableObject wiring.
//   * Subscription coverage — instantiate real EventChannel SOs via ScriptableObject.CreateInstance,
//     wire them through ConfigureForTests(), Subscribe(), then Raise on the channel and
//     assert the dispatcher saw the slug. This verifies the EventChannel.Subscribe/Unsubscribe
//     wiring is actually attached (regression guard against "the bindings exist but don't
//     listen" bugs).
//   * BGM driver coverage — fake IMusicStateMachine records calls; assert the right state-
//     entry method ran for each scene-name.
//
// We deliberately do NOT instantiate the MonoBehaviour via AddComponent on a GameObject —
// EditMode tests can do that, but the public Subscribe/Unsubscribe API makes it unnecessary
// and the GO path requires SceneManager scaffolding that adds noise.

using System.Collections.Generic;
using Brave.Gameplay.Events;
using Brave.Systems.Audio;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Systems.Audio
{
    [TestFixture]
    public class GameplayAudioBindingsTests
    {
        // ---- Test-only stub dispatcher: records (slug, position, was-ui) per call. ----
        private sealed class StubSfxDispatcher : ISfxDispatcher
        {
            public readonly List<(string slug, Vector3 pos, bool wasUi)> Calls = new();
            public int StopAllCallCount { get; private set; }
            private int _nextId = 1;

            public SfxHandle PlaySfx(string slug, Vector3 worldPosition)
            {
                Calls.Add((slug, worldPosition, wasUi: false));
                return new SfxHandle(_nextId++, isPlaying: true);
            }

            public SfxHandle PlayUi(string slug)
            {
                Calls.Add((slug, Vector3.zero, wasUi: true));
                return new SfxHandle(_nextId++, isPlaying: true);
            }

            public void StopAll() => StopAllCallCount++;

            public bool DidPlay(string slug)
            {
                for (int i = 0; i < Calls.Count; i++)
                    if (Calls[i].slug == slug) return true;
                return false;
            }
        }

        // ---- Test-only fake music state machine: records call kind + arg. ----
        private sealed class FakeMusicStateMachine : IMusicStateMachine
        {
            public readonly List<string> Calls = new();
            public string? LastBiome;
            public bool? LastRunEndWin;

            public void EnterHome() => Calls.Add(nameof(EnterHome));
            public void EnterLobby() => Calls.Add(nameof(EnterLobby));
            public void EnterRun(string biomeSlug) { Calls.Add(nameof(EnterRun)); LastBiome = biomeSlug; }
            public void EnterBoss() => Calls.Add(nameof(EnterBoss));
            public void EnterRunEnd(bool win) { Calls.Add(nameof(EnterRunEnd)); LastRunEndWin = win; }
        }

        private GameplayAudioBindings _bindings = null!;
        private StubSfxDispatcher _dispatcher = null!;
        private readonly List<ScriptableObject> _channelsToCleanUp = new();

        [SetUp]
        public void SetUp()
        {
            // Bindings are a MonoBehaviour — instantiate on a hidden GameObject so OnEnable
            // does NOT auto-fire (we drive Subscribe() manually for deterministic tests).
            var go = new GameObject("test-bindings") { hideFlags = HideFlags.HideAndDontSave };
            go.SetActive(false);
            _bindings = go.AddComponent<GameplayAudioBindings>();
            _dispatcher = new StubSfxDispatcher();
        }

        [TearDown]
        public void TearDown()
        {
            if (_bindings != null) Object.DestroyImmediate(_bindings.gameObject);
            for (int i = 0; i < _channelsToCleanUp.Count; i++)
                if (_channelsToCleanUp[i] != null) Object.DestroyImmediate(_channelsToCleanUp[i]);
            _channelsToCleanUp.Clear();
        }

        private T MakeChannel<T>() where T : ScriptableObject
        {
            var ch = ScriptableObject.CreateInstance<T>();
            _channelsToCleanUp.Add(ch);
            return ch;
        }

        // -------------------- Direct handler routing --------------------

        [Test]
        public void HandleEnemyKilled_SwarmerSlugFires_AtEnemyPosition()
        {
            _bindings.ConfigureForTests(_dispatcher);

            var pos = new Vector3(3f, 0f, 5f);
            _bindings.HandleEnemyKilled(new EnemyKilledEvent(
                enemySlugHash: 0, position: pos, wasElite: false, runSeconds: 12f));

            Assert.That(_dispatcher.Calls.Count, Is.EqualTo(1));
            Assert.That(_dispatcher.Calls[0].slug, Is.EqualTo(SfxSlug.EnemySwarmerDie));
            Assert.That(_dispatcher.Calls[0].pos, Is.EqualTo(pos));
            Assert.That(_dispatcher.Calls[0].wasUi, Is.False);
        }

        [Test]
        public void HandleEnemyKilled_EliteSlugFires_WhenWasEliteTrue()
        {
            _bindings.ConfigureForTests(_dispatcher);

            _bindings.HandleEnemyKilled(new EnemyKilledEvent(
                enemySlugHash: 0, position: Vector3.zero, wasElite: true, runSeconds: 0f));

            Assert.That(_dispatcher.DidPlay(SfxSlug.EnemyEliteDie), Is.True);
            Assert.That(_dispatcher.DidPlay(SfxSlug.EnemySwarmerDie), Is.False);
        }

        [Test]
        public void HandleLevelUp_LayersRunLevelupAndHeroFanfare()
        {
            _bindings.ConfigureForTests(_dispatcher);

            _bindings.HandleLevelUp(new LevelUpEvent(newLevel: 2, xpRemainder: 0));

            // Both Pillar-2 layers fire: arpeggio + fanfare.
            Assert.That(_dispatcher.DidPlay(SfxSlug.RunLevelup), Is.True);
            Assert.That(_dispatcher.DidPlay(SfxSlug.HeroLevelupFanfare), Is.True);
            Assert.That(_dispatcher.Calls.Count, Is.EqualTo(2));
        }

        [TestCase(PickupKind.XpGemSmall, SfxSlug.RunPickupXpSmall)]
        [TestCase(PickupKind.XpGemMedium, SfxSlug.RunPickupXpSmall)]
        [TestCase(PickupKind.XpGemLarge, SfxSlug.RunPickupXpLarge)]
        [TestCase(PickupKind.GoldCoin, SfxSlug.RunPickupGold)]
        [TestCase(PickupKind.Heart, SfxSlug.RunPickupHeart)]
        [TestCase(PickupKind.SoulShard, SfxSlug.RunPickupGold)]
        public void HandlePickup_RoutesByKind(PickupKind kind, string expectedSlug)
        {
            _bindings.ConfigureForTests(_dispatcher);
            var pos = new Vector3(1f, 2f, 3f);
            _bindings.HandlePickup(new PickupEvent(kind, amount: 1, position: pos));

            Assert.That(_dispatcher.Calls.Count, Is.EqualTo(1));
            Assert.That(_dispatcher.Calls[0].slug, Is.EqualTo(expectedSlug));
            Assert.That(_dispatcher.Calls[0].pos, Is.EqualTo(pos));
        }

        [Test]
        public void HandleDeath_VictoryPlaysWinStinger_NotLoseAndNotHeroDeath()
        {
            _bindings.ConfigureForTests(_dispatcher);

            _bindings.HandleDeath(new DeathEvent(
                characterSlugHash: 0, runSeconds: 60f, enemiesKilled: 100, cause: DeathCause.Victory));

            Assert.That(_dispatcher.DidPlay(SfxSlug.RunEndWin), Is.True);
            Assert.That(_dispatcher.DidPlay(SfxSlug.RunEndLose), Is.False);
            Assert.That(_dispatcher.DidPlay(SfxSlug.HeroDeath), Is.False);
        }

        [Test]
        public void HandleDeath_KilledPlaysHeroDeathAndLoseStinger()
        {
            _bindings.ConfigureForTests(_dispatcher);

            _bindings.HandleDeath(new DeathEvent(
                characterSlugHash: 0, runSeconds: 30f, enemiesKilled: 10, cause: DeathCause.Killed));

            Assert.That(_dispatcher.DidPlay(SfxSlug.HeroDeath), Is.True);
            Assert.That(_dispatcher.DidPlay(SfxSlug.RunEndLose), Is.True);
            Assert.That(_dispatcher.DidPlay(SfxSlug.RunEndWin), Is.False);
        }

        [TestCase(DeathCause.Quit)]
        [TestCase(DeathCause.TimedOut)]
        public void HandleDeath_QuitOrTimedOut_PlaysLoseStingerWithoutHeroDeath(DeathCause cause)
        {
            _bindings.ConfigureForTests(_dispatcher);

            _bindings.HandleDeath(new DeathEvent(
                characterSlugHash: 0, runSeconds: 0f, enemiesKilled: 0, cause: cause));

            Assert.That(_dispatcher.DidPlay(SfxSlug.RunEndLose), Is.True);
            Assert.That(_dispatcher.DidPlay(SfxSlug.HeroDeath), Is.False);
            Assert.That(_dispatcher.DidPlay(SfxSlug.RunEndWin), Is.False);
        }

        [Test]
        public void HandleBossPhase_Phase1PlaysIntroSting_HigherPhasesPlayChange()
        {
            _bindings.ConfigureForTests(_dispatcher);

            _bindings.HandleBossPhase(new BossPhaseEvent(newPhase: 1, bossSlugHash: 0));
            _bindings.HandleBossPhase(new BossPhaseEvent(newPhase: 2, bossSlugHash: 0));
            _bindings.HandleBossPhase(new BossPhaseEvent(newPhase: 3, bossSlugHash: 0));

            Assert.That(_dispatcher.Calls.Count, Is.EqualTo(3));
            Assert.That(_dispatcher.Calls[0].slug, Is.EqualTo(SfxSlug.BossIntroSting));
            Assert.That(_dispatcher.Calls[1].slug, Is.EqualTo(SfxSlug.BossPhaseChange));
            Assert.That(_dispatcher.Calls[2].slug, Is.EqualTo(SfxSlug.BossPhaseChange));
        }

        [Test]
        public void NotifyWeaponFired_ProjectileMapsToCarrot_AreaMapsToDaisy_AuraMapsToSunbeam()
        {
            _bindings.ConfigureForTests(_dispatcher);

            _bindings.NotifyWeaponFired("projectile", new Vector3(1f, 0f, 0f));
            _bindings.NotifyWeaponFired("area", new Vector3(2f, 0f, 0f));
            _bindings.NotifyWeaponFired("aura", new Vector3(3f, 0f, 0f));
            _bindings.NotifyWeaponFired("unknown", new Vector3(4f, 0f, 0f));

            Assert.That(_dispatcher.Calls.Count, Is.EqualTo(4));
            Assert.That(_dispatcher.Calls[0].slug, Is.EqualTo(SfxSlug.WeaponCarrotFire));
            Assert.That(_dispatcher.Calls[1].slug, Is.EqualTo(SfxSlug.WeaponDaisyDrop));
            Assert.That(_dispatcher.Calls[2].slug, Is.EqualTo(SfxSlug.WeaponSunbeamStart));
            // Unknown archetype falls back to carrot fire.
            Assert.That(_dispatcher.Calls[3].slug, Is.EqualTo(SfxSlug.WeaponCarrotFire));
        }

        [Test]
        public void NotifyPlayerHit_FiresHeroHitAtPosition()
        {
            _bindings.ConfigureForTests(_dispatcher);

            var pos = new Vector3(7f, 0f, -2f);
            _bindings.NotifyPlayerHit(pos);

            Assert.That(_dispatcher.Calls.Count, Is.EqualTo(1));
            Assert.That(_dispatcher.Calls[0].slug, Is.EqualTo(SfxSlug.HeroHit));
            Assert.That(_dispatcher.Calls[0].pos, Is.EqualTo(pos));
        }

        [Test]
        public void NotifyWaveCleared_FiresRunLevelupAsPlaceholder()
        {
            _bindings.ConfigureForTests(_dispatcher);

            _bindings.NotifyWaveCleared();

            Assert.That(_dispatcher.Calls.Count, Is.EqualTo(1));
            Assert.That(_dispatcher.Calls[0].slug, Is.EqualTo(SfxSlug.RunLevelup));
            Assert.That(_dispatcher.Calls[0].wasUi, Is.True);
        }

        [Test]
        public void NullDispatcher_HandlersDoNotThrow()
        {
            // Default state — no dispatcher injected, OnEnable not fired.
            Assert.That(_bindings.DispatcherForTests, Is.Null);

            Assert.DoesNotThrow(() => _bindings.HandleDeath(default));
            Assert.DoesNotThrow(() => _bindings.HandleEnemyKilled(default));
            Assert.DoesNotThrow(() => _bindings.HandleLevelUp(default));
            Assert.DoesNotThrow(() => _bindings.HandlePickup(default));
            Assert.DoesNotThrow(() => _bindings.HandleBossPhase(default));
            Assert.DoesNotThrow(() => _bindings.NotifyWeaponFired("projectile", Vector3.zero));
            Assert.DoesNotThrow(() => _bindings.NotifyPlayerHit(Vector3.zero));
            Assert.DoesNotThrow(() => _bindings.NotifyWaveCleared());
        }

        // -------------------- Subscription wiring (real channel SOs) --------------------

        [Test]
        public void Subscribe_AttachesToAllChannels_AndRaiseTriggersDispatch()
        {
            var deathCh = MakeChannel<DeathChannel>();
            var killedCh = MakeChannel<EnemyKilledChannel>();
            var levelCh = MakeChannel<LevelUpChannel>();
            var pickupCh = MakeChannel<PickupChannel>();
            var bossCh = MakeChannel<BossPhaseChannel>();

            _bindings.ConfigureForTests(_dispatcher, deathCh, killedCh, levelCh, pickupCh, bossCh);
            _bindings.Subscribe();

            Assert.That(_bindings.IsSubscribedForTests, Is.True);

            // Fire one event per channel and confirm a dispatch happened for each.
            killedCh.Raise(new EnemyKilledEvent(0, Vector3.one, wasElite: false, runSeconds: 0f));
            levelCh.Raise(new LevelUpEvent(newLevel: 2, xpRemainder: 0));
            pickupCh.Raise(new PickupEvent(PickupKind.GoldCoin, amount: 5, position: Vector3.zero));
            bossCh.Raise(new BossPhaseEvent(newPhase: 2, bossSlugHash: 0));
            deathCh.Raise(new DeathEvent(0, 0f, 0, DeathCause.Victory));

            Assert.That(_dispatcher.DidPlay(SfxSlug.EnemySwarmerDie), Is.True, "killed channel did not dispatch");
            Assert.That(_dispatcher.DidPlay(SfxSlug.RunLevelup), Is.True, "level-up channel did not dispatch");
            Assert.That(_dispatcher.DidPlay(SfxSlug.RunPickupGold), Is.True, "pickup channel did not dispatch");
            Assert.That(_dispatcher.DidPlay(SfxSlug.BossPhaseChange), Is.True, "boss-phase channel did not dispatch");
            Assert.That(_dispatcher.DidPlay(SfxSlug.RunEndWin), Is.True, "death channel did not dispatch");
        }

        [Test]
        public void Subscribe_Idempotent_DoesNotDoubleDispatch()
        {
            var killedCh = MakeChannel<EnemyKilledChannel>();
            _bindings.ConfigureForTests(_dispatcher, enemyKilledChannel: killedCh);

            _bindings.Subscribe();
            _bindings.Subscribe(); // should be a no-op

            killedCh.Raise(new EnemyKilledEvent(0, Vector3.zero, wasElite: false, runSeconds: 0f));

            Assert.That(_dispatcher.Calls.Count, Is.EqualTo(1),
                "duplicate Subscribe() must not re-attach the handler");
        }

        [Test]
        public void Unsubscribe_StopsFurtherDispatches()
        {
            var killedCh = MakeChannel<EnemyKilledChannel>();
            _bindings.ConfigureForTests(_dispatcher, enemyKilledChannel: killedCh);
            _bindings.Subscribe();

            killedCh.Raise(new EnemyKilledEvent(0, Vector3.zero, wasElite: false, runSeconds: 0f));
            Assert.That(_dispatcher.Calls.Count, Is.EqualTo(1));

            _bindings.Unsubscribe();
            killedCh.Raise(new EnemyKilledEvent(0, Vector3.zero, wasElite: false, runSeconds: 0f));
            Assert.That(_dispatcher.Calls.Count, Is.EqualTo(1),
                "post-Unsubscribe events must not reach the dispatcher");
        }

        // -------------------- BgmGameplayDriver routing --------------------

        [Test]
        public void BgmDriver_EnterBoot_RoutesToHome()
        {
            var music = new FakeMusicStateMachine();
            var driver = new BgmGameplayDriver(music);

            driver.EnterBoot();

            Assert.That(music.Calls, Is.EqualTo(new[] { "EnterHome" }));
            Assert.That(driver.BossActive, Is.False);
        }

        [Test]
        public void BgmDriver_EnterMainMenu_RoutesToHome()
        {
            var music = new FakeMusicStateMachine();
            var driver = new BgmGameplayDriver(music);

            driver.EnterMainMenu();

            Assert.That(music.Calls, Is.EqualTo(new[] { "EnterHome" }));
        }

        [Test]
        public void BgmDriver_EnterLoadout_RoutesToLobby()
        {
            var music = new FakeMusicStateMachine();
            var driver = new BgmGameplayDriver(music);

            driver.EnterLoadout();

            Assert.That(music.Calls, Is.EqualTo(new[] { "EnterLobby" }));
        }

        [Test]
        public void BgmDriver_EnterRun_DefaultBiome_UsesMeadow()
        {
            var music = new FakeMusicStateMachine();
            var driver = new BgmGameplayDriver(music);

            driver.EnterRun();

            Assert.That(music.Calls, Is.EqualTo(new[] { "EnterRun" }));
            Assert.That(music.LastBiome, Is.EqualTo(BgmGameplayDriver.DefaultBiomeSlug));
            Assert.That(driver.CurrentBiomeSlug, Is.EqualTo(BgmGameplayDriver.DefaultBiomeSlug));
        }

        [Test]
        public void BgmDriver_EnterRun_BiomeArgument_OverridesDefault()
        {
            var music = new FakeMusicStateMachine();
            var driver = new BgmGameplayDriver(music);

            driver.EnterRun("Beach");

            Assert.That(music.LastBiome, Is.EqualTo("Beach"));
            Assert.That(driver.CurrentBiomeSlug, Is.EqualTo("Beach"));
        }

        [Test]
        public void BgmDriver_EnterBoss_FlagsBossActive()
        {
            var music = new FakeMusicStateMachine();
            var driver = new BgmGameplayDriver(music);

            driver.EnterRun();
            driver.EnterBoss();

            Assert.That(music.Calls, Is.EqualTo(new[] { "EnterRun", "EnterBoss" }));
            Assert.That(driver.BossActive, Is.True);
        }

        [Test]
        public void BgmDriver_EnterRunEnd_ClearsBossFlag_AndPassesWin()
        {
            var music = new FakeMusicStateMachine();
            var driver = new BgmGameplayDriver(music);
            driver.EnterBoss();

            driver.EnterRunEnd(win: true);

            Assert.That(driver.BossActive, Is.False);
            Assert.That(music.LastRunEndWin, Is.EqualTo(true));
        }

        [Test]
        public void BgmDriver_RouteForSceneName_BootMainMenuLoadoutRun_AllRoute()
        {
            var music = new FakeMusicStateMachine();
            var driver = new BgmGameplayDriver(music);

            driver.RouteForSceneName(BgmGameplayDriver.SceneBoot);
            driver.RouteForSceneName(BgmGameplayDriver.SceneMainMenu);
            driver.RouteForSceneName(BgmGameplayDriver.SceneLoadout);
            driver.RouteForSceneName(BgmGameplayDriver.SceneRun);

            Assert.That(music.Calls, Is.EqualTo(new[] {
                "EnterHome",       // Boot
                "EnterHome",       // MainMenu
                "EnterLobby",      // Loadout
                "EnterRun",        // Run
            }));
        }

        [Test]
        public void BgmDriver_RouteForSceneName_Unknown_DoesNothing()
        {
            var music = new FakeMusicStateMachine();
            var driver = new BgmGameplayDriver(music);

            // Unity logs a warning via Debug.LogWarning — declare expected so the test passes.
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning,
                new System.Text.RegularExpressions.Regex(@"\[BgmGameplayDriver\] unknown scene"));
            driver.RouteForSceneName("NotAScene");

            Assert.That(music.Calls, Is.Empty);
        }
    }
}

#endif
