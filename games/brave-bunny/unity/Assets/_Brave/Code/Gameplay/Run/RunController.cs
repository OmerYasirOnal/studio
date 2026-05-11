// Tech-spec 08: Run state is owned by GameStateManager; RunController is the per-state
// orchestrator for the in-Run subsystems (wave runner, combat, level-up, hero).
using Brave.Gameplay.Definitions;
using Brave.Gameplay.Events;
using Brave.Gameplay.Spawning;
using UnityEngine;

namespace Brave.Gameplay.Run;

/// <summary>
/// Top-level orchestrator for a single run. Owns the run-clock, wave runner, and
/// level-up controller. Lifecycle: Start → Pause/Resume → End. Saves the run-end report
/// to the systems-engineer's ISaveService at RunEnd state-entry (tech-spec 03 save triggers).
/// </summary>
public sealed class RunController : MonoBehaviour
{
    [SerializeField] private CharacterDefinition _activeCharacter;
    [SerializeField] private BiomeDefinition _activeBiome;
    [SerializeField] private DeathChannel _deathChannel;

    private RunTimer _timer;
    private WaveRunner _waveRunner;
    private RunState _state = RunState.Intro;

    public RunState State => _state;
    public float RunSeconds => _timer?.Seconds ?? 0f;
    public CharacterDefinition Character => _activeCharacter;
    public BiomeDefinition Biome => _activeBiome;

    public void Initialise(CharacterDefinition character, BiomeDefinition biome, WaveRunner waveRunner, RunTimer timer)
    {
        _activeCharacter = character;
        _activeBiome = biome;
        _waveRunner = waveRunner;
        _timer = timer;
        _timer.RunEnded += OnTimerEnded;
    }

    public void BeginIntro() => _state = RunState.Intro;

    public void Resume()
    {
        _state = RunState.Running;
        _timer?.Start();
    }

    public void Pause()
    {
        _state = RunState.Paused;
        _timer?.Pause();
    }

    public void ResumeFromPause()
    {
        _state = RunState.Running;
        _timer?.Resume();
    }

    public void End(RunResult result)
    {
        _state = RunState.Ending;
        _timer?.Stop();
        // TODO(Phase 5): build RunEndReport and raise DeathChannel.
        _deathChannel?.Raise(new DeathEvent(
            characterSlugHash: _activeCharacter != null ? _activeCharacter.slug.GetHashCode() : 0,
            runSeconds: RunSeconds,
            enemiesKilled: 0,
            cause: result switch
            {
                RunResult.Death   => DeathCause.Killed,
                RunResult.Quit    => DeathCause.Quit,
                RunResult.Victory => DeathCause.Victory,
                _ => DeathCause.Killed,
            }));
    }

    private void Update()
    {
        if (_state != RunState.Running && _state != RunState.BossActive) return;
        float dt = Time.deltaTime;
        float now = _timer.Tick(dt);
        // TODO(Phase 5): real playerPos source; currently zero placeholder.
        _waveRunner?.Tick(now, Vector2.zero);
    }

    private void OnTimerEnded() => End(RunResult.Victory);
}
