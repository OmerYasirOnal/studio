using UnityEngine;

namespace Brave.Gameplay.Events;

[CreateAssetMenu(menuName = "Brave/Events/EnemyKilled", fileName = "EnemyKilledChannel", order = 3)]
public sealed class EnemyKilledChannel : EventChannel<EnemyKilledEvent> { }
