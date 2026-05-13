// QA — RunHudController / IRunRuntimeState binding EditMode tests (ADR-0021)
// Subject under test: BindState wiring contract without a full UXML document.
// Strategy: RunHudController is a MonoBehaviour that requires UIDocument; we cannot
// instantiate it in pure EditMode without a scene. We therefore test the binding
// contract through:
//   (a) IRunRuntimeState mock — verifies event subscription plumbing.
//   (b) A thin pure-C# BindingBroker helper that mirrors the BindState logic so it
//       can be exercised without MonoBehaviour lifecycle overhead.
// Full end-to-end element propagation (SetHp → hp-bar-fill style) is covered by
// PlayMode/Smoke/RunStartTests.cs once the Run.unity scene is wired.
//
// Adapted from hud-wire branch (worktree-agent-ad5bee576529346e8):
//   * Interface namespace: Brave.UI.Bindings (canonical, not Brave.Gameplay.Run).
//   * FakeState uses canonical field names.

using System;
using Brave.UI.Bindings;
using NUnit.Framework;

namespace Brave.Tests.EditMode.UI
{
    /// <summary>
    /// Mirrors the bind/unbind logic of RunHudController.BindState so we can test
    /// the subscription contract in EditMode without instantiating a MonoBehaviour.
    /// </summary>
    internal sealed class BindingBroker
    {
        private IRunRuntimeState? _state;
        public int RefreshCount { get; private set; }

        public void Bind(IRunRuntimeState state)
        {
            if (_state != null) _state.StateChanged -= OnChanged;
            _state = state;
            _state.StateChanged += OnChanged;
            // Immediate initial refresh.
            OnChanged();
        }

        public void Unbind()
        {
            if (_state == null) return;
            _state.StateChanged -= OnChanged;
            _state = null;
        }

        private void OnChanged() => RefreshCount++;
    }

    // ---- fake state implementing the canonical interface ----

    internal sealed class FakeBindingState : IRunRuntimeState
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

    [TestFixture]
    public class RunHudBindingTests
    {
        [Test]
        public void Bind_TriggerImmediateRefresh()
        {
            var broker = new BindingBroker();
            var state = new FakeBindingState();

            broker.Bind(state);

            Assert.That(broker.RefreshCount, Is.EqualTo(1),
                "BindState must trigger an immediate sync so the HUD is correct before the first StateChanged fires.");
        }

        [Test]
        public void Bind_StateChanged_PropagatesRefresh()
        {
            var broker = new BindingBroker();
            var state = new FakeBindingState();
            broker.Bind(state); // +1 immediate

            state.Fire();  // +1
            state.Fire();  // +1

            Assert.That(broker.RefreshCount, Is.EqualTo(3));
        }

        [Test]
        public void Unbind_StopsRefreshes()
        {
            var broker = new BindingBroker();
            var state = new FakeBindingState();
            broker.Bind(state);
            broker.Unbind();

            int countBeforeFire = broker.RefreshCount;
            state.Fire();

            Assert.That(broker.RefreshCount, Is.EqualTo(countBeforeFire),
                "After Unbind, state changes must not trigger refreshes.");
        }

        [Test]
        public void Rebind_UnsubscribesOldState()
        {
            var broker = new BindingBroker();
            var stateA = new FakeBindingState();
            var stateB = new FakeBindingState();

            broker.Bind(stateA); // +1
            broker.Bind(stateB); // +1 (rebind immediate)

            int countAfterBothBinds = broker.RefreshCount; // 2
            stateA.Fire(); // must NOT trigger refresh because A is unbound

            Assert.That(broker.RefreshCount, Is.EqualTo(countAfterBothBinds),
                "Old state must be unsubscribed when a new state is bound.");
        }

        [Test]
        public void Rebind_NewState_TriggersPropagation()
        {
            var broker = new BindingBroker();
            var stateA = new FakeBindingState();
            var stateB = new FakeBindingState();

            broker.Bind(stateA);
            broker.Bind(stateB);
            int baseline = broker.RefreshCount;

            stateB.Fire();

            Assert.That(broker.RefreshCount, Is.EqualTo(baseline + 1),
                "New state fires must reach the broker.");
        }
    }
}
