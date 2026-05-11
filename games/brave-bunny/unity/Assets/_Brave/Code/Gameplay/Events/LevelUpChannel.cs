using UnityEngine;

namespace Brave.Gameplay.Events;

[CreateAssetMenu(menuName = "Brave/Events/LevelUp", fileName = "LevelUpChannel", order = 1)]
public sealed class LevelUpChannel : EventChannel<LevelUpEvent> { }
