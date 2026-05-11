using UnityEngine;

namespace Brave.Gameplay.Events;

[CreateAssetMenu(menuName = "Brave/Events/Pickup", fileName = "PickupChannel", order = 4)]
public sealed class PickupChannel : EventChannel<PickupEvent> { }
