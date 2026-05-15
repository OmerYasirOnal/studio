// Tech-spec 08: Run state is owned by GameStateManager; RunController is the per-state
// orchestrator for the in-Run subsystems (wave runner, combat, level-up, hero).
//
// ADR-0021: RunController now implements IRunRuntimeState so RunHudController can bind
// directly to it via BindState(). Mutators (SetHp, AddXp, SetWave, RecordKill,
// Pause/Resume) raise StateChanged after each mutation. The HUD subscribes to this
// event and redraws the full view — see RunHudController.BindState().
//
// Run-end report capture (Phase 5 Wave 6):
//   On run end (player death / boss defeated / quit / timeout), RunController
//   builds a populated RunEndReport from running tallies (kills via
//   EnemyKilledChannel, xp via LevelUp/AddXp, gold via PickupChannel, wave via
//   SetWave) and raises RunEndedChannel. CurrentRunEndReport is exposed on
//   IRunRuntimeState for UI consumers (RunEndTallyController).

#nullable enable

using System;
using System.Collections.Generic;
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
    [SerializeField] private RunEndedChannel? _runEndedChannel;
    [SerializeField] private EnemyKilledChannel? _enemyKilledChannel;
    [SerializeField] private LevelUpChannel? _levelUpChannel;
    [SerializeField] private PickupChannel? _pickupChannel;

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

    // ---- Run-end tally backing fields ----
    private int _elitesKilled;
    private int _bossesKilled;
    private int _goldGained;
    private int _soulShardsEarned;
    private int _passXpEarned;
    private RunEndReport? _currentRunEndReport;
    private readonly List<string> _weaponIdsUsed = new();
    private bool _eventsSubscribed;

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
    public RunEndReport? CurrentRunEndReport => _currentRunEndReport;

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

    /// <summary>
    /// Record an elite-flagged kill. Increments both <see cref="KillCount"/> and the
    /// elite sub-tally used in the run-end report. Raises <see cref="StateChanged"/>.
    /// </summary>
    public void RecordEliteKill()
    {
        _killCount++;
        _elitesKilled++;
        RaiseStateChanged();
    }

    /// <summary>
    /// Record a boss-flagged kill. Increments both <see cref="KillCount"/> and the
    /// boss sub-tally used in the run-end report. Raises <see cref="StateChanged"/>.
    /// </summary>
    public void RecordBossKill()
    {
        _killCount++;
        _bossesKilled++;
        RaiseStateChanged();
    }

    /// <summary>Add gold (carrots) gained this run. Used in <see cref="RunEndReport.goldGained"/>.</summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        _goldGained += amount;
        RaiseStateChanged();
    }

    /// <summary>Add soul shards earned this run. Used in <see cref="RunEndReport.soulShardsEarned"/>.</summary>
    public void AddSoulShards(int amount)
    {
        if (amount <= 0) return;
        _soulShardsEarned += amount;
        RaiseStateChanged();
    }

    /// <summary>Add battle-pass XP earned this run. Used in <see cref="RunEndReport.passXpEarned"/>.</summary>
    public void AddPassXp(int amount)
    {
        if (amount <= 0) return;
        _passXpEarned += amount;
        RaiseStateChanged();
    }

    /// <summary>
    /// Append a weapon slug to the loadout snapshot captured at run-end. De-duplicates
    /// repeated calls so multiple stacks of the same weapon list it only once.
    /// </summary>
    public void RegisterEquippedWeapon(string slug)
    {
        if (string.IsNullOrEmpty(slug)) return;
        if (_weaponIdsUsed.Contains(slug)) return;
        _weaponIdsUsed.Add(slug);
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
        SubscribeToChannels();
        RaiseStateChanged();
    }

    public void BeginIntro() => _runState = RunState.Intro;

    public void Resume()
    {
        _runState = RunState.Running;
        _timer?.Start();
        SubscribeToChannels();
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

    /// <summary>
    /// End the run with the given <see cref="RunResult"/>. Populates
    /// <see cref="CurrentRunEndReport"/> and raises both <c>RunEndedChannel</c> (full
    /// report) and <c>DeathChannel</c> (legacy slim payload). Raises
    /// <see cref="StateChanged"/> once at the very end.
    /// </summary>
    public void End(RunResult result) => EndInternal(RunEndReport.OutcomeFromResult(result),
        cause: null);

    /// <summary>
    /// End the run with a specific <see cref="RunOutcome"/> and an explicit cause string
    /// (one of <see cref="RunEndCause"/>). Preferred for new call-sites that distinguish
    /// boss-defeat from timeout — both map to <see cref="RunResult.Victory"/>/<c>Death</c>
    /// in the legacy payload.
    /// </summary>
    public void End(RunOutcome outcome, string cause) => EndInternal(outcome, cause);

    private void EndInternal(RunOutcome outcome, string? cause)
    {
        _runState = RunState.Ending;
        _timer?.Stop();

        var report = BuildRunEndReport(outcome, cause);
        _currentRunEndReport = report;

        _runEndedChannel?.Raise(new RunEndedEvent(report));

        _deathChannel?.Raise(new DeathEvent(
            characterSlugHash: _activeCharacter != null ? _activeCharacter.slug.GetHashCode() : 0,
            runSeconds: report.runDurationSeconds,
            enemiesKilled: report.totalKills,
            cause: outcome switch
            {
                RunOutcome.Win     => DeathCause.Victory,
                RunOutcome.Lose    => DeathCause.Killed,
                RunOutcome.Timeout => DeathCause.TimedOut,
                RunOutcome.Quit    => DeathCause.Quit,
                _ => DeathCause.Killed,
            }));

        // Wave 7A integration: publish on the cross-asmdef bridge so Systems-side
        // meta services (BgmGameplayDriver run-end snapshot, CharacterUnlockService
        // run-completion + boss-defeat tally) can react without Gameplay→Systems
        // asmdef coupling. Bridge is a static no-op when no listener is bound.
        RunEndIntegrationBridge.Notify(report);

        UnsubscribeFromChannels();
        RaiseStateChanged();
    }

    /// <summary>
    /// Assemble the populated <see cref="RunEndReport"/> from current run tallies.
    /// Public so unit tests can verify the projection without driving the channel
    /// side-effects in <see cref="EndInternal"/>.
    /// </summary>
    public RunEndReport BuildRunEndReport(RunOutcome outcome, string? cause = null)
    {
        var report = new RunEndReport
        {
            outcome            = outcome,
            result             = RunEndReport.ResultFromOutcome(outcome),
            deathCause         = cause ?? RunEndReport.DefaultCauseFor(outcome),
            runDurationSeconds = RunSeconds,
            totalKills         = _killCount,
            elitesKilled       = _elitesKilled,
            bossesKilled       = _bossesKilled,
            wavesCleared       = _waveNumber,
            finalLevel         = _level,
            xpGained           = _xpPoints,
            goldGained         = _goldGained,
            soulShardsEarned   = _soulShardsEarned,
            passXpEarned       = _passXpEarned,
            weaponIdsUsed      = _weaponIdsUsed.ToArray(),
            characterId        = _activeCharacter != null ? _activeCharacter.slug : string.Empty,
        };
        return report;
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

    private void OnDisable() => UnsubscribeFromChannels();

    // ---- Event channel wiring ----
    // Subscribing in Initialise + Resume covers both bootstrap paths (scene-loaded
    // RunController and run-time spawned). Unsubscribe on End/Disable so domain reload
    // doesn't leak listeners.

    private void SubscribeToChannels()
    {
        if (_eventsSubscribed) return;
        _enemyKilledChannel?.Subscribe(OnEnemyKilled);
        _levelUpChannel?.Subscribe(OnLevelUp);
        _pickupChannel?.Subscribe(OnPickup);
        _eventsSubscribed = true;
    }

    private void UnsubscribeFromChannels()
    {
        if (!_eventsSubscribed) return;
        _enemyKilledChannel?.Unsubscribe(OnEnemyKilled);
        _levelUpChannel?.Unsubscribe(OnLevelUp);
        _pickupChannel?.Unsubscribe(OnPickup);
        _eventsSubscribed = false;
    }

    private void OnEnemyKilled(EnemyKilledEvent evt)
    {
        if (evt.wasElite) RecordEliteKill();
        else RecordKill();
    }

    private void OnLevelUp(LevelUpEvent evt)
    {
        _level = evt.newLevel;
        _currentXp = evt.xpRemainder;
        RaiseStateChanged();
    }

    private void OnPickup(PickupEvent evt)
    {
        switch (evt.kind)
        {
            case PickupKind.GoldCoin:
                AddGold(evt.amount);
                break;
            case PickupKind.SoulShard:
                AddSoulShards(evt.amount);
                break;
            case PickupKind.XpGemSmall:
            case PickupKind.XpGemMedium:
            case PickupKind.XpGemLarge:
                AddXp(evt.amount);
                break;
            case PickupKind.Heart:
                SetHp(Mathf.Min(_maxHp, _currentHp + evt.amount));
                break;
        }
    }
}

}
