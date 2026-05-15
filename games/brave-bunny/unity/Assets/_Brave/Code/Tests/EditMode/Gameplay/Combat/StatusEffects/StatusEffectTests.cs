// QA — StatusEffect + StatusEffectApplier EditMode tests (Wave 10).
// Subject under test:
//   * Brave.Gameplay.Combat.StatusEffects.StatusEffect (and 5 concrete subclasses)
//   * Brave.Gameplay.Combat.StatusEffects.StatusEffectApplier
// What we verify:
//   * Apply fires OnApply once and is reflected in the per-enemy state.
//   * Tick advances duration and fires OnTick / ApplyTickDamage for DoT.
//   * Expire fires OnExpire when duration drains to zero (state restored).
//   * Stack — multiple different effects coexist on the same enemy.
//   * Refresh — same effect re-applied keeps a single instance with extended duration.
// Pattern: real Enemy MonoBehaviour on a throwaway GameObject; configure with a small
//          EnemyDefinition so IsAlive returns true; never go through HitDetector or
//          broadphase (out of scope).

using Brave.Gameplay.Combat.StatusEffects;
using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay.Combat
{
    [TestFixture]
    public class StatusEffectTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const float BaseHp = 100f;
        private const float Epsilon = 0.0001f;
        private const int SlowDurationMs = 1000;
        private const float SlowMagnitude = 0.4f;          // 40% slow → 60% speed
        private const float SlowMultiplierExpected = 0.6f;
        private const int BurnDurationMs = 600;
        private const int BurnTickIntervalMs = 200;
        private const float BurnDamagePerTick = 5f;
        private const int FreezeDurationMs = 500;
        private const int StunDurationMs = 400;
        private const int PoisonDurationMs = 600;
        private const int PoisonTickIntervalMs = 200;
        private const float PoisonDamagePerTick = 3f;
        private const float DtSecondsTwoHundredMs = 0.2f;  // = 200 ms
        private const float DtSecondsThreeHundredMs = 0.3f;
        private const float DtSecondsHalfSec = 0.5f;
        private const float DtSecondsOneSec = 1.0f;

        private GameObject _enemyGo = null!;
        private Enemy _enemy = null!;
        private EnemyDefinition _def = null!;
        private StatusEffectApplier _applier = null!;

        [SetUp]
        public void SetUp()
        {
            _enemyGo = new GameObject("Test_StatusEffectEnemy");
            _enemy = _enemyGo.AddComponent<Enemy>();
            _def = ScriptableObject.CreateInstance<EnemyDefinition>();
            _def.slug = "test-status-target";
            _def.baseHP = BaseHp;
            _enemy.Configure(_def, BaseHp, behavior: null!, owner: null!);

            _applier = new StatusEffectApplier();
            StatusEffectApplier.ResetAllStateForTests();
        }

        [TearDown]
        public void TearDown()
        {
            _applier.Clear();
            StatusEffectApplier.ResetAllStateForTests();
            if (_enemyGo != null) Object.DestroyImmediate(_enemyGo);
            if (_def != null) Object.DestroyImmediate(_def);
        }

        // ---- APPLY ----

        [Test]
        public void Apply_Slow_ReducesSpeedMultiplier()
        {
            _applier.Apply(_enemy, new SlowEffect(SlowDurationMs, SlowMagnitude));
            var state = StatusEffectApplier.GetOrCreateState(_enemy);
            Assert.That(state.SpeedMultiplier, Is.EqualTo(SlowMultiplierExpected).Within(Epsilon),
                "Slow magnitude=0.4 should leave a 0.6 speed multiplier.");
            Assert.That(_applier.ActiveCount(_enemy), Is.EqualTo(1),
                "Applying one effect should register exactly one active entry.");
            Assert.That(_applier.HasEffect(_enemy, "status.slow"), Is.True);
        }

        [Test]
        public void Apply_Stun_BlocksAttacks_DoesNotSlow()
        {
            _applier.Apply(_enemy, new StunEffect(StunDurationMs));
            var state = StatusEffectApplier.GetOrCreateState(_enemy);
            Assert.That(state.CanAttack, Is.False, "Stun must block attacks.");
            Assert.That(state.SpeedMultiplier, Is.EqualTo(1f).Within(Epsilon),
                "Stun must NOT touch the speed multiplier.");
        }

        [Test]
        public void Apply_Freeze_ZeroesSpeedAndMarksFrozen()
        {
            _applier.Apply(_enemy, new FreezeEffect(FreezeDurationMs));
            var state = StatusEffectApplier.GetOrCreateState(_enemy);
            Assert.That(state.SpeedMultiplier, Is.EqualTo(0f).Within(Epsilon));
            Assert.That(state.IsFrozen, Is.True);
        }

        // ---- TICK / DoT ----

        [Test]
        public void Tick_Burn_AppliesDamagePerTickAndDrainsHp()
        {
            _applier.Apply(_enemy, new BurnEffect(BurnDurationMs, BurnDamagePerTick, BurnTickIntervalMs));
            float hpBefore = _enemy.Hp;
            // Advance one tick interval → exactly one DoT pulse should land.
            _applier.Tick(DtSecondsTwoHundredMs);
            Assert.That(_enemy.Hp, Is.EqualTo(hpBefore - BurnDamagePerTick).Within(Epsilon),
                "Burn at 200ms interval should land 1 pulse after 200ms.");
            // Another 200ms = second pulse.
            _applier.Tick(DtSecondsTwoHundredMs);
            Assert.That(_enemy.Hp, Is.EqualTo(hpBefore - 2 * BurnDamagePerTick).Within(Epsilon),
                "Burn should land a second pulse after another 200ms.");
        }

        [Test]
        public void Tick_Poison_AppliesArmorPiercingDamage()
        {
            _applier.Apply(_enemy, new PoisonEffect(PoisonDurationMs, PoisonDamagePerTick, PoisonTickIntervalMs));
            float hpBefore = _enemy.Hp;
            _applier.Tick(DtSecondsTwoHundredMs);
            Assert.That(_enemy.Hp, Is.EqualTo(hpBefore - PoisonDamagePerTick).Within(Epsilon),
                "Poison should land one raw pulse after one interval.");
            var state = StatusEffectApplier.GetOrCreateState(_enemy);
            Assert.That(state.IsPoisoned, Is.True);
        }

        // ---- EXPIRE ----

        [Test]
        public void Tick_PastDuration_ExpiresSlow_RestoresSpeed()
        {
            _applier.Apply(_enemy, new SlowEffect(SlowDurationMs, SlowMagnitude));
            // Advance past the duration in one shot.
            _applier.Tick(DtSecondsOneSec + 0.1f);
            var state = StatusEffectApplier.GetOrCreateState(_enemy);
            Assert.That(state.SpeedMultiplier, Is.EqualTo(1f).Within(Epsilon),
                "After Slow expires, speed multiplier must be restored to 1.");
            Assert.That(_applier.HasEffect(_enemy, "status.slow"), Is.False);
            Assert.That(_applier.ActiveCount(_enemy), Is.EqualTo(0));
        }

        [Test]
        public void Tick_PastDuration_ExpiresStun_RestoresAttack()
        {
            _applier.Apply(_enemy, new StunEffect(StunDurationMs));
            _applier.Tick(DtSecondsHalfSec); // 500ms > 400ms duration
            var state = StatusEffectApplier.GetOrCreateState(_enemy);
            Assert.That(state.CanAttack, Is.True, "Stun must clear CanAttack flag on expire.");
        }

        [Test]
        public void Tick_PastDuration_ExpiresFreeze_RestoresMultiplier()
        {
            _applier.Apply(_enemy, new FreezeEffect(FreezeDurationMs));
            _applier.Tick(DtSecondsOneSec); // > 500ms
            var state = StatusEffectApplier.GetOrCreateState(_enemy);
            Assert.That(state.SpeedMultiplier, Is.EqualTo(1f).Within(Epsilon));
            Assert.That(state.IsFrozen, Is.False);
        }

        // ---- STACK ----

        [Test]
        public void Stack_SlowAndBurn_Coexist()
        {
            _applier.Apply(_enemy, new SlowEffect(SlowDurationMs, SlowMagnitude));
            _applier.Apply(_enemy, new BurnEffect(BurnDurationMs, BurnDamagePerTick, BurnTickIntervalMs));
            Assert.That(_applier.ActiveCount(_enemy), Is.EqualTo(2),
                "Different status types must stack — both should be active.");
            var state = StatusEffectApplier.GetOrCreateState(_enemy);
            Assert.That(state.SpeedMultiplier, Is.EqualTo(SlowMultiplierExpected).Within(Epsilon),
                "Slow's speed multiplier holds when Burn is also active.");
            Assert.That(state.IsBurning, Is.True);
        }

        [Test]
        public void Stack_ThreeDifferentEffects_AllActive()
        {
            _applier.Apply(_enemy, new SlowEffect(SlowDurationMs, SlowMagnitude));
            _applier.Apply(_enemy, new BurnEffect(BurnDurationMs, BurnDamagePerTick, BurnTickIntervalMs));
            _applier.Apply(_enemy, new StunEffect(StunDurationMs));
            Assert.That(_applier.ActiveCount(_enemy), Is.EqualTo(3),
                "Slow + Burn + Stun must all coexist.");
            var state = StatusEffectApplier.GetOrCreateState(_enemy);
            Assert.That(state.CanAttack, Is.False, "Stun flag must hold.");
            Assert.That(state.IsBurning, Is.True, "Burn flag must hold.");
            Assert.That(state.SpeedMultiplier, Is.EqualTo(SlowMultiplierExpected).Within(Epsilon));
        }

        // ---- REFRESH ----

        [Test]
        public void Refresh_SameTypeReapplied_DoesNotDuplicate_ExtendsDuration()
        {
            // Apply a slow, drain half its duration, then re-apply with a fresh duration.
            _applier.Apply(_enemy, new SlowEffect(SlowDurationMs, SlowMagnitude));
            _applier.Tick(DtSecondsHalfSec); // 500ms in → 500ms remaining
            Assert.That(_applier.ActiveCount(_enemy), Is.EqualTo(1));

            _applier.Apply(_enemy, new SlowEffect(SlowDurationMs, SlowMagnitude));
            Assert.That(_applier.ActiveCount(_enemy), Is.EqualTo(1),
                "Re-applying the same effect type must NOT duplicate the list entry.");

            var slow = _applier.FindEffect(_enemy, "status.slow");
            Assert.That(slow, Is.Not.Null);
            Assert.That(slow!.remainingMs, Is.EqualTo(SlowDurationMs),
                "Refresh must set remainingMs to the new (longer) duration.");
        }

        [Test]
        public void Refresh_LongerMagnitudeWins()
        {
            // Apply a weak slow first, then a stronger one — magnitude should jump up.
            _applier.Apply(_enemy, new SlowEffect(SlowDurationMs, magnitude: 0.2f));
            float weakMultiplier;
            {
                var s = StatusEffectApplier.GetOrCreateState(_enemy);
                weakMultiplier = s.SpeedMultiplier;
                Assert.That(weakMultiplier, Is.EqualTo(0.8f).Within(Epsilon),
                    "Pre-condition: weak slow gives 0.8 multiplier.");
            }

            _applier.Apply(_enemy, new SlowEffect(SlowDurationMs, magnitude: 0.5f));
            var slow = _applier.FindEffect(_enemy, "status.slow");
            Assert.That(slow!.magnitude, Is.EqualTo(0.5f).Within(Epsilon),
                "Refresh policy: stronger magnitude replaces weaker.");
        }

        [Test]
        public void Refresh_ShorterDuration_DoesNotShortenExisting()
        {
            // Apply a long slow, then a shorter one — remaining duration must NOT shrink.
            _applier.Apply(_enemy, new SlowEffect(SlowDurationMs, SlowMagnitude));
            int beforeRefresh;
            {
                var s = _applier.FindEffect(_enemy, "status.slow");
                beforeRefresh = s!.remainingMs;
            }
            _applier.Apply(_enemy, new SlowEffect(durationMs: 100, magnitude: SlowMagnitude));
            var slow = _applier.FindEffect(_enemy, "status.slow");
            Assert.That(slow!.remainingMs, Is.GreaterThanOrEqualTo(beforeRefresh),
                "Refresh must never shorten an existing effect.");
        }
    }
}
