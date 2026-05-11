namespace Brave.Gameplay.Run;

/// <summary>
/// Sub-state of the global <c>Run</c> state in tech-spec 08. The state-machine doc treats
/// the Run state as a single node; this enum is the gameplay-side sub-state used by
/// <see cref="RunController"/> to gate logic (intro countdown, normal play, boss active, ending).
/// </summary>
public enum RunState
{
    Intro,        // countdown 3-2-1-go
    Running,
    Paused,
    BossActive,
    Ending,
}
