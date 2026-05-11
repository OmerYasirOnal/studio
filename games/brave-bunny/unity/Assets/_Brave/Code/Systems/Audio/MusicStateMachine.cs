// Brave Bunny — Systems / Audio
// Tech spec: docs/06-tech-spec/07-audio.md (snapshot crossfade per state transition)
//            docs/06-tech-spec/08-state-machine.md (Boot/MainMenu/Run/RunEnd states)

#nullable enable

using Brave.Systems.Context;

namespace Brave.Systems.Audio;

public interface IMusicStateMachine : IService
{
    void EnterHome();
    void EnterLobby();
    void EnterRun(string biomeSlug);
    void EnterBoss();
    void EnterRunEnd(bool win);
}

/// <summary>
/// Owns the high-level music snapshot transitions. Durations sourced from
/// 07-audio.md table:
///   Splash → Home:        400 ms
///   Home/Lobby → Run:     800 ms (Snapshot_Run_<biome>)
///   Run → Boss:           600 ms
///   Boss → Run-end:       400 ms
///   Run-end → Home:       800 ms
///   Default SO → SO:      400 ms cubic crossfade
/// </summary>
public sealed class MusicStateMachine : IMusicStateMachine
{
    private const string SnapHome = "Snapshot_Home";
    private const string SnapLobby = "Snapshot_Lobby";
    private const string SnapBoss = "Snapshot_Run_Boss";
    private const string SnapRunEndWin = "Snapshot_Run_End_Win";
    private const string SnapRunEndLose = "Snapshot_Run_End_Lose";

    private readonly IAudioMixerDriver _driver;

    public MusicStateMachine(IAudioMixerDriver driver) { _driver = driver; }

    public void EnterHome() => _driver.SnapshotTransition(SnapHome, 0.4f);
    public void EnterLobby() => _driver.SnapshotTransition(SnapLobby, 0.4f);
    public void EnterRun(string biomeSlug) => _driver.SnapshotTransition($"Snapshot_Run_{biomeSlug}", 0.8f);
    public void EnterBoss() => _driver.SnapshotTransition(SnapBoss, 0.6f);
    public void EnterRunEnd(bool win) => _driver.SnapshotTransition(win ? SnapRunEndWin : SnapRunEndLose, 0.4f);
}
