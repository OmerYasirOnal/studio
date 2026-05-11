// Tech-spec 09: payload struct definitions for the 5 approved channels.
// All payloads are readonly struct (pass-by-value, zero GC).
namespace Brave.Gameplay.Events;

public readonly struct DeathEvent
{
    public readonly int characterSlugHash;
    public readonly float runSeconds;
    public readonly int enemiesKilled;
    public readonly DeathCause cause;

    public DeathEvent(int characterSlugHash, float runSeconds, int enemiesKilled, DeathCause cause)
    {
        this.characterSlugHash = characterSlugHash;
        this.runSeconds = runSeconds;
        this.enemiesKilled = enemiesKilled;
        this.cause = cause;
    }
}

public enum DeathCause { Killed, Quit, TimedOut, Victory }

public readonly struct LevelUpEvent
{
    public readonly int newLevel;
    public readonly int xpRemainder;
    public LevelUpEvent(int newLevel, int xpRemainder)
    {
        this.newLevel = newLevel;
        this.xpRemainder = xpRemainder;
    }
}

public readonly struct BossPhaseEvent
{
    public readonly int newPhase;          // 1, 2, 3
    public readonly int bossSlugHash;
    public BossPhaseEvent(int newPhase, int bossSlugHash)
    {
        this.newPhase = newPhase;
        this.bossSlugHash = bossSlugHash;
    }
}

public readonly struct EnemyKilledEvent
{
    public readonly int enemySlugHash;
    public readonly UnityEngine.Vector3 position;
    public readonly bool wasElite;
    public readonly float runSeconds;
    public EnemyKilledEvent(int enemySlugHash, UnityEngine.Vector3 position, bool wasElite, float runSeconds)
    {
        this.enemySlugHash = enemySlugHash;
        this.position = position;
        this.wasElite = wasElite;
        this.runSeconds = runSeconds;
    }
}

public enum PickupKind { XpGemSmall, XpGemMedium, XpGemLarge, GoldCoin, Heart, SoulShard }

public readonly struct PickupEvent
{
    public readonly PickupKind kind;
    public readonly int amount;
    public readonly UnityEngine.Vector3 position;
    public PickupEvent(PickupKind kind, int amount, UnityEngine.Vector3 position)
    {
        this.kind = kind;
        this.amount = amount;
        this.position = position;
    }
}
