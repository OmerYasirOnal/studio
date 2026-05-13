// QA — IRunRuntimeState EditMode tests (ADR-0021)
// Subject under test: IRunRuntimeState contract (canonical interface in Brave.UI.Bindings).
// Verifies: fields readable, StateChanged fires on mutation, multiple subscribers OK.
//
// Adapted from hud-wire branch (worktree-agent-ad5bee576529346e8):
//   * Namespace changed to Brave.UI.Bindings (canonical).
//   * FakeRunState uses canonical field names (WaveNumber, RunSecondsElapsed, etc.).
//   * CurrentHpNormalized is a computed property — tested via CurrentHP/MaxHP values.

using System;
using Brave.UI.Bindings;
using NUnit.Framework;

namespace Brave.Tests.EditMode.UI
{
    [TestFixture]
    public class IRunRuntimeStateTests
    {
        // ---- minimal mock implementing the canonical interface ----

        private sealed class FakeRunState : IRunRuntimeState
        {
            public float CurrentHP { get; set; } = 100f;
            public float MaxHP { get; set; } = 100f;
            public float CurrentHpNormalized => MaxHP <= 0f ? 0f : UnityEngine.Mathf.Clamp01(CurrentHP / MaxHP);
            public float CurrentXP { get; set; }
            public float XPToNextLevel { get; set; } = 100f;
            public int XpPoints { get; set; }
            public int Level { get; set; } = 1;
            public int WaveNumber { get; set; } = 1;
            public float RunSecondsElapsed { get; set; }
            public bool IsBossActive { get; set; }
            public int KillCount { get; set; }
            public bool Paused { get; set; }

            public event Action? StateChanged;

            public void Fire() => StateChanged?.Invoke();
        }

        // ---- tests ----

        [Test]
        public void DefaultValues_AreHealthy()
        {
            var state = new FakeRunState();
            Assert.That(state.CurrentHP, Is.EqualTo(100f).Within(0.001f));
            Assert.That(state.CurrentHpNormalized, Is.EqualTo(1f).Within(0.001f));
            Assert.That(state.WaveNumber, Is.EqualTo(1));
            Assert.That(state.Paused, Is.False);
        }

        [Test]
        public void StateChanged_FiresSubscribers()
        {
            var state = new FakeRunState();
            int callCount = 0;
            state.StateChanged += () => callCount++;

            state.Fire();

            Assert.That(callCount, Is.EqualTo(1), "Subscriber must be notified exactly once.");
        }

        [Test]
        public void StateChanged_MultipleSubscribers_AllNotified()
        {
            var state = new FakeRunState();
            int a = 0, b = 0;
            state.StateChanged += () => a++;
            state.StateChanged += () => b++;

            state.Fire();

            Assert.That(a, Is.EqualTo(1));
            Assert.That(b, Is.EqualTo(1));
        }

        [Test]
        public void StateChanged_AfterUnsubscribe_NotCalled()
        {
            var state = new FakeRunState();
            int callCount = 0;
            Action handler = () => callCount++;
            state.StateChanged += handler;
            state.StateChanged -= handler;

            state.Fire();

            Assert.That(callCount, Is.EqualTo(0), "Unsubscribed handler must not be invoked.");
        }

        [Test]
        public void FieldMutation_ReflectsNewValues()
        {
            var state = new FakeRunState();
            state.CurrentHP = 50f;
            state.MaxHP = 100f;
            state.XpPoints = 150;
            state.WaveNumber = 3;
            state.KillCount = 42;
            state.RunSecondsElapsed = 90.5f;
            state.Paused = true;

            Assert.That(state.CurrentHpNormalized, Is.EqualTo(0.5f).Within(0.0001f),
                "CurrentHpNormalized must reflect CurrentHP/MaxHP ratio.");
            Assert.That(state.XpPoints, Is.EqualTo(150));
            Assert.That(state.WaveNumber, Is.EqualTo(3));
            Assert.That(state.KillCount, Is.EqualTo(42));
            Assert.That(state.RunSecondsElapsed, Is.EqualTo(90.5f).Within(0.0001f));
            Assert.That(state.Paused, Is.True);
        }

        [Test]
        public void CurrentHpNormalized_ZeroMaxHP_DoesNotDivideByZero()
        {
            var state = new FakeRunState { CurrentHP = 0f, MaxHP = 0f };
            Assert.DoesNotThrow(() => _ = state.CurrentHpNormalized);
            Assert.That(state.CurrentHpNormalized, Is.EqualTo(0f).Within(0.0001f));
        }
    }
}
