// Brave Bunny — UI / Bindings / RunHudStubRuntime
//
// Pure-C# placeholder implementation of <see cref="IRunRuntimeState"/>. Lets
// the Run-HUD render plausible values in the Unity editor (and in EditMode
// tests) before gameplay-engineer ships the real RunService binding.
//
// Mutable properties so tests + scene-preview can dial in scenarios without
// constructing the full gameplay stack.
//
// Cross-refs:
//   * Wave-5 ui-engineer dispatch — this is the editor-preview stub.
//   * docs/02-gdd/07-progression.md — XP curve baseline (100 XP @ L1).

#nullable enable

namespace Brave.UI.Bindings
{
    /// <summary>
    /// Mutable stub <see cref="IRunRuntimeState"/> for editor-preview and
    /// EditMode tests. Production code uses gameplay-engineer's RunService.
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
        public float CurrentXP { get; set; } = DefaultCurrentXP;
        public float XPToNextLevel { get; set; } = DefaultXPToNextLevel;
        public int Level { get; set; } = DefaultLevel;
        public int WaveNumber { get; set; } = DefaultWaveNumber;
        public float RunSecondsElapsed { get; set; } = DefaultRunSecondsElapsed;
        public bool IsBossActive { get; set; } = DefaultIsBossActive;
    }
}
