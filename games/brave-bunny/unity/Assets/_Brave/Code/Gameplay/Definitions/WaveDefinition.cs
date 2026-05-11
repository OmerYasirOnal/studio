#nullable enable
// Tech-spec 02 § WaveDefinition. Loaded from data/waves/<biome>.json by level-designer's tool.
// Gameplay-engineer never modifies the asset — wave timing is non-negotiable (per game CLAUDE.md).

using System;

using UnityEngine;

namespace Brave.Gameplay.Definitions
{
    /// <summary>
    /// Time-keyed schedule of wave events. <see cref="events"/> MUST be sorted by
    /// <see cref="WaveSpawnEntry.triggerMinute"/>; <c>OnValidate</c> asserts it.
    /// </summary>
    [CreateAssetMenu(menuName = "Brave/Wave", fileName = "Wave", order = 6)]
    public sealed class WaveDefinition : ScriptableObject
    {
        [Header("Source biome")]
        public string biomeSlug = string.Empty;

        [Header("Events (ordered by triggerMinute)")]
        public WaveSpawnEntry[] events = Array.Empty<WaveSpawnEntry>();

        private void OnValidate()
        {
            if (events == null) return;
            for (int i = 1; i < events.Length; i++)
            {
                if (events[i].triggerMinute < events[i - 1].triggerMinute)
                {
                    Debug.LogError(
                        $"{biomeSlug}: wave events must be sorted by triggerMinute (idx {i})", this);
                    return;
                }
            }
        }
    }
}
