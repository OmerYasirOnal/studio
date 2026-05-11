#nullable enable
// Tech-spec 09 § Approved channels. One concrete SO subclass per approved channel.
// Each readonly-struct payload is defined here for proximity; payloads are pass-by-value
// (zero GC). Per ADR-0013, ScriptableObject channels win on designer visibility.

using System;

using UnityEngine;

namespace Brave.Gameplay.Events
{
    // ----- Payloads (readonly struct; zero-allocation pass-by-value) -----

    public enum DeathCause { Killed = 0, Quit = 1, TimedOut = 2, Victory = 3 }

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
        public readonly int newPhase;            // 1, 2, 3
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
        public readonly Vector3 position;
        public readonly bool wasElite;
        public readonly float runSeconds;

        public EnemyKilledEvent(int enemySlugHash, Vector3 position, bool wasElite, float runSeconds)
        {
            this.enemySlugHash = enemySlugHash;
            this.position = position;
            this.wasElite = wasElite;
            this.runSeconds = runSeconds;
        }
    }

    public enum PickupKind
    {
        XpGemSmall  = 0,
        XpGemMedium = 1,
        XpGemLarge  = 2,
        GoldCoin    = 3,
        Heart       = 4,
        SoulShard   = 5,
        Chest       = 6,
        Magnet      = 7,
    }

    public readonly struct PickupEvent
    {
        public readonly PickupKind kind;
        public readonly int amount;
        public readonly Vector3 position;

        public PickupEvent(PickupKind kind, int amount, Vector3 position)
        {
            this.kind = kind;
            this.amount = amount;
            this.position = position;
        }
    }

    // ----- Concrete SO channels — assets live in Assets/_Brave/Data/Definitions/EventChannels/ -----

    [CreateAssetMenu(menuName = "Brave/Events/Death", fileName = "DeathChannel", order = 0)]
    public sealed class DeathChannel : EventChannel<DeathEvent> { }

    [CreateAssetMenu(menuName = "Brave/Events/LevelUp", fileName = "LevelUpChannel", order = 1)]
    public sealed class LevelUpChannel : EventChannel<LevelUpEvent> { }

    [CreateAssetMenu(menuName = "Brave/Events/BossPhase", fileName = "BossPhaseChannel", order = 2)]
    public sealed class BossPhaseChannel : EventChannel<BossPhaseEvent> { }

    [CreateAssetMenu(menuName = "Brave/Events/EnemyKilled", fileName = "EnemyKilledChannel", order = 3)]
    public sealed class EnemyKilledChannel : EventChannel<EnemyKilledEvent> { }

    [CreateAssetMenu(menuName = "Brave/Events/Pickup", fileName = "PickupChannel", order = 4)]
    public sealed class PickupChannel : EventChannel<PickupEvent> { }
}
