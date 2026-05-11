using UnityEngine;

namespace Brave.Gameplay.Events;

[CreateAssetMenu(menuName = "Brave/Events/Death", fileName = "DeathChannel", order = 0)]
public sealed class DeathChannel : EventChannel<DeathEvent> { }
