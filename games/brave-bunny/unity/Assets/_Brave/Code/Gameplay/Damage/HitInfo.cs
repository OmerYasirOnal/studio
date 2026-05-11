#nullable enable
// Tech-spec 09 Tier-2: readonly struct passed by value through the damage event path.
// Zero-GC; lifetime is one stack frame.

using System;

using UnityEngine;

namespace Brave.Gameplay.Damage
{
    /// <summary>
    /// Per-hit payload. Built at the hit-detection site, consumed by EnemyHealth + listeners.
    /// </summary>
    public readonly struct HitInfo
    {
        public readonly float amount;
        public readonly Vector3 impactPosition;
        public readonly bool isCrit;
        public readonly int sourceId;
        public readonly int targetId;

        public HitInfo(float amount, Vector3 impactPosition, bool isCrit, int sourceId, int targetId)
        {
            this.amount = amount;
            this.impactPosition = impactPosition;
            this.isCrit = isCrit;
            this.sourceId = sourceId;
            this.targetId = targetId;
        }
    }
}
