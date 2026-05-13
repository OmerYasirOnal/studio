// Tech-spec 08: Run state is owned by GameStateManager; RunController is the per-state
// orchestrator for the in-Run subsystems (wave runner, combat, level-up, hero).
//
// ADR-0021: RunController now implements IRunRuntimeState so RunHudController can bind
// directly to it via BindState(). Mutators (SetHp, AddXp, SetWave, RecordKill,
// Pause/Resume) raise StateChanged after each mutation. The HUD subscribes to this
// event and redraws the full view — see RunHudController.BindState().

#nullable enable

using System;
using Brave.Gameplay.Definitions;
using Brave.Gameplay.Events;
using Brave.Gameplay.Spawning;
using Brave.UI.Bindings;
using UnityEngine;

namespace Brave.Gameplay.Run
{

/// <summary>
/// Top-level orchestrator for a single run. Owns the run-clock, wave runner, and
/// level-up controller. Lifecycle: Start → Pause/Resume → End. Saves the run-end report
/// to the systems-engineer's ISaveService at RunEnd state-entry (tech-spec 03 save triggers).
///
/// Implements <see cref="IRunRuntimeState"/> so the HUD can subscribe to live data
/// without knowing about MonoBehaviour internals (ADR-0021).
/// </summary>
public sealed class RunController : MonoBehaviour, IRunRuntimeState
{
    [SerializeField] private CharacterDefinition? _activeCharacter;
    [SerializeField] private BiomeDefinition? _activeBiome;
    [SerializeField] private DeathChannel? _deathChannel;

    private RunTimer? _timer;
    private WaveRunner? _waveRunner;
    private RunState _runState = RunState.Intro;

    // ---- IRunRuntimeState backing fields ----
    private float _currentHp;
    private float _maxHp = 100f;
    private float _currentXp;
    private float _xpToNextLevel = 100f;
    private int _xpPoints;
    private int _level = 1;
    private int _waveNumber = 1;
    private int _killCount;

    // ---- public accessors (non-IRunRuntimeState) ----
    public RunState State => _runState;
    public float RunSeconds => _timer?.Seconds ?? 0f;
    public CharacterDefinition? Character => _activeCharacter;
    public BiomeDefinition? Biome => _activeBiome;

    // ---- IRunRuntimeState implementation ----

    /// <inheritdoc/>
    public float CurrentHP => _currentHp;

    /// <inheritdoc/>
    public float MaxHP => _maxHp;

    /// <inheritdoc/>
    public float CurrentHpNormalized =>
        _maxHp <= 0f ? 0f : Mathf.Clamp01(_currentHp / _maxHp);

    /// <inheritdoc/>
    public float CurrentXP => _currentXp;

    /// <inheritdoc/>
    public float XPToNextLevel => _xpToNextLevel;

    /// <inheritdoc/>
    public int XpPoints => _xpPoints;

    /// <inheritdoc/>
    public int Level => _level;

    /// <inheritdoc/>
    public int WaveNumber => _waveNumber;

    /// <inheritdoc/>
    public float RunSecondsElapsed => _timer?.Seconds ?? 0f;

    /// <inheritdoc/>
    public bool IsBossActive => _runState == RunState.BossActive;

    /// <inheritdoc/>
    public int KillCount => _killCount;

    /// <inheritdoc/>
    public bool Paused => _runState == RunState.Paused;

    /// <inheritdoc/>
    public event Action? StateChanged;

    private void RaiseStateChanged() => StateChanged?.Invoke();

    // ---- IRunRuntimeState mutators ----

    /// <summary>Set current HP. Clamps to [0, MaxHP]. Raises <see cref="StateChanged"/>.</summary>
    public void SetHp(float hp)
    {
        _currentHp = Mathf.Clamp(hp, 0f, _maxHp);
        RaiseStateChanged();
    }

    /// <summary>Set max HP and optionally clamp current HP. Raises <see cref="StateChanged"/>.</summary>
    public void SetMaxHp(float maxHp)
    {
        _maxHp = Mathf.Max(1f, maxHp);
        _currentHp = Mathf.Min(_currentHp, _maxHp);
        RaiseStateChanged();
    }

    /// <summary>Add XP to current level bucket and raw total. Raises <see cref="StateChanged"/>.</summary>
    public void AddXp(float xp)
    {
        _xpPoints += Mathf.RoundToInt(xp);
        _currentXp += xp;
        RaiseStateChanged();
    }

    /// <summary>Set XP-to-next-level threshold. Raises <see cref="StateChanged"/>.</summary>
    public void SetXpToNextLevel(float threshold)
    {
        _xpToNextLevel = Mathf.Max(1f, threshold);
        RaiseStateChanged();
    }

    /// <summary>Reset current-level XP bucket and increment level. Raises <see cref="StateChanged"/>.</summary>
    public void LevelUp()
    {
        _currentXp = 0f;
        _level++;
        RaiseStateChanged();
    }

    /// <summary>Set the current wave number. Raises <see cref="StateChanged"/>.</summary>
    public void SetWave(int wave)
    {
        _waveNumber = Mathf.Max(1, wave);
        RaiseStateChanged();
    }

    /// <summary>Increment kill counter by one. Raises <see cref="StateChanged"/>.</summary>
    public void RecordKill()
    {
        _killCount++;
        RaiseStateChanged();
    }

    // ---- lifecycle ----

    public void Initialise(CharacterDefinition character, BiomeDefinition biome, WaveRunner waveRunner, RunTimer timer)
    {
        _activeCharacter = character;
        _activeBiome = biome;
        _waveRunner = waveRunner;
        _timer = timer;
        _timer.RunEnded += OnTimerEnded;
        // Seed HP from character definition if available.
        _maxHp = 100f;
        _currentHp = _maxHp;
        RaiseStateChanged();
    }

    public void BeginIntro() => _runState = RunState.Intro;

    public void Resume()
    {
        _runState = RunState.Running;
        _timer?.Start();
        RaiseStateChanged();
    }

    public void Pause()
    {
        _runState = RunState.Paused;
        _timer?.Pause();
        RaiseStateChanged();
    }

    public void ResumeFromPause()
    {
        _runState = RunState.Running;
        _timer?.Resume();
        RaiseStateChanged();
    }

    public void End(RunResult result)
    {
        _runState = RunState.Ending;
        _timer?.Stop();
        // TODO(Phase 5): build RunEndReport and raise DeathChannel.
        _deathChannel?.Raise(new DeathEvent(
            characterSlugHash: _activeCharacter != null ? _activeCharacter.slug.GetHashCode() : 0,
            runSeconds: RunSeconds,
            enemiesKilled: _killCount,
            cause: result switch
            {
                RunResult.Death   => DeathCause.Killed,
                RunResult.Quit    => DeathCause.Quit,
                RunResult.Victory => DeathCause.Victory,
                _ => DeathCause.Killed,
            }));
        RaiseStateChanged();
    }

    private void Update()
    {
        if (_runState != RunState.Running && _runState != RunState.BossActive) return;
        float dt = Time.deltaTime;
        float now = _timer!.Tick(dt);
        // TODO(Phase 5): real playerPos source; currently zero placeholder.
        _waveRunner?.Tick(now, Vector2.zero);
    }

    private void OnTimerEnded() => End(RunResult.Victory);
}

}
