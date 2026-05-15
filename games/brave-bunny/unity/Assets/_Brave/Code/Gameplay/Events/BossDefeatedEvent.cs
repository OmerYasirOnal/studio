// Brave Bunny — Gameplay/Events/BossDefeatedEvent + BossDefeatedChannel
//
// Tech-spec 09 § Tier 3: typed ScriptableObject event channel for cross-asmdef
// loose coupling. Fired by BossBehavior when the boss reaches 0 HP. Consumers:
//   * RunController — ends the run with RunOutcome.Win, cause="boss_defeated".
//   * CharacterUnlockService (Systems.Progression) — unlocks the Badger when
//     bossId=="old-boar-king" (meta-progression hook per docs/02-gdd/04-meta.md).
//   * Audio / VFX bindings — boss-down stinger + screen flash.
//
// Payload is a readonly struct (pass-by-value, zero GC) per tech-spec 09.
//
// Spec refs:
//   * docs/09-level-design/02-bosses/old-boar-king/mechanics.md § Win condition
//   * docs/decisions/0020-weapon-archetype-config-and-boss-enum.md (boss enum)

#nullable enable

using UnityEngine;

namespace Brave.Gameplay.Events
{
    /// <summary>
    /// Payload broadcast on <see cref="BossDefeatedChannel"/> at the moment the boss's HP
    /// reaches 0 and before the pool-return. <see cref="bossSlugHash"/> mirrors the
    /// <c>BossDefinition.slug.GetHashCode()</c> + <see cref="bossId"/> carries the raw slug
    /// for subscribers that need to switch on identity (e.g. meta-progression unlocks).
    /// </summary>
    public readonly struct BossDefeatedEvent
    {
        /// <summary>Boss slug (e.g. "old-boar-king"). Empty when unknown.</summary>
        public readonly string bossId;

        /// <summary>Hash of <see cref="bossId"/> for zero-alloc switch dispatch.</summary>
        public readonly int bossSlugHash;

        /// <summary>Run-clock seconds at the moment of defeat.</summary>
        public readonly float runSeconds;

        /// <summary>World position of the boss at defeat (for VFX / drops).</summary>
        public readonly Vector3 position;

        public BossDefeatedEvent(string bossId, int bossSlugHash, float runSeconds, Vector3 position)
        {
            this.bossId = bossId;
            this.bossSlugHash = bossSlugHash;
            this.runSeconds = runSeconds;
            this.position = position;
        }
    }

    /// <summary>SO channel asset — designers wire this into BossBehavior + listeners.</summary>
    [CreateAssetMenu(menuName = "Brave/Events/BossDefeated", fileName = "BossDefeatedChannel", order = 6)]
    public sealed class BossDefeatedChannel : EventChannel<BossDefeatedEvent> { }
}
