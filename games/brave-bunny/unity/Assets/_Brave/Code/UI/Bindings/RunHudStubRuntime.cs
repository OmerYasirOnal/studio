// Brave Bunny — UI / Bindings / RunHudStubRuntime
//
// Pure-C# placeholder implementation of <see cref="IRunRuntimeState"/>. Lets
// the Run-HUD render plausible values in the Unity editor (and in EditMode
// tests) before gameplay-engineer ships the real RunController binding.
//
// Mutable properties so tests + scene-preview can dial in scenarios without
// constructing the full gameplay stack.
//
// ADR-0021: StateChanged event is a no-op on this stub — the HUD falls back
// to per-frame Update() polling when no live state is bound.
//
// Cross-refs:
//   * Wave-5 ui-engineer dispatch — this is the editor-preview stub.
//   * docs/02-gdd/07-progression.md — XP curve baseline (100 XP @ L1).

#nullable enable

using System;

namespace Brave.UI.Bindings
{
    /// <summary>
    /// Mutable stub <see cref="IRunRuntimeState"/> for editor-preview and
    /// EditMode tests. Production code uses gameplay-engineer's RunController.
    /// </summary>
    public sealed class RunHudStubRuntime : IRunRuntimeState
    {
        // ---- defaults (mirror the placeholder values in the wireframe) ----
        public const float DefaultMaxHP = 100f;
        public const float DefaultCurrentHP = 50f;
        public const float DefaultXPToNextLevel = 100f;
        public const float DefaultCurrentXP = 38f;
        public const int DefaultLevel = 1;
        public const int DefaultWaveNumber = 1;
        public const float DefaultRunSecondsElapsed = 0f;
        public const bool DefaultIsBossActive = false;

        public float CurrentHP { get; set; } = DefaultCurrentHP;
        public float MaxHP { get; set; } = DefaultMaxHP;

        /// <summary>Computed from CurrentHP/MaxHP; allocation-free.</summary>
        public float CurrentHpNormalized =>
            MaxHP <= 0f ? 0f : UnityEngine.Mathf.Clamp01(CurrentHP / MaxHP);

        public float CurrentXP { get; set; } = DefaultCurrentXP;
        public float XPToNextLevel { get; set; } = DefaultXPToNextLevel;

        /// <summary>Raw cumulative XP (stub only: rounded CurrentXP cast to int).</summary>
        public int XpPoints => (int)CurrentXP;

        public int Level { get; set; } = DefaultLevel;
        public int WaveNumber { get; set; } = DefaultWaveNumber;
        public float RunSecondsElapsed { get; set; } = DefaultRunSecondsElapsed;
        public bool IsBossActive { get; set; } = DefaultIsBossActive;

        /// <summary>Total enemies killed (always 0 on the stub).</summary>
        public int KillCount { get; set; } = 0;

        /// <summary>Paused state (always false on the stub).</summary>
        public bool Paused { get; set; } = false;

        /// <summary>
        /// No-op event on the stub — the HUD uses per-frame Update() polling
        /// when <see cref="RunHudStubRuntime"/> is the active state.
        /// </summary>
        public event Action? StateChanged;

        /// <summary>
        /// Manually fire StateChanged for unit tests that need to exercise
        /// the event path against the stub.
        /// </summary>
        public void FireStateChanged() => StateChanged?.Invoke();
    }
}
