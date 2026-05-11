using UnityEngine;

namespace Brave.Gameplay.Events;

[CreateAssetMenu(menuName = "Brave/Events/BossPhase", fileName = "BossPhaseChannel", order = 2)]
public sealed class BossPhaseChannel : EventChannel<BossPhaseEvent> { }
