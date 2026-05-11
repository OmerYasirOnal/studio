#nullable enable
// Tech-spec 09 § Tier 3: typed ScriptableObject event channels for cross-asmdef loose
// coupling. ADR-0013 (pending) tracks the SO-vs-C#-event decision; this is the SO path.

using System;

using UnityEngine;

namespace Brave.Gameplay.Events
{
    /// <summary>
    /// Generic typed channel — concrete subclasses give each channel a named asset so
    /// designers can wire <c>[SerializeField]</c> references in the inspector.
    /// <c>OnDisable</c> clears stale subscriptions to survive domain reload + scene unload.
    /// </summary>
    public abstract class EventChannel<T> : ScriptableObject
    {
        private event Action<T>? _listeners;

        public void Raise(T arg) => _listeners?.Invoke(arg);
        public void Subscribe(Action<T> listener) => _listeners += listener;
        public void Unsubscribe(Action<T> listener) => _listeners -= listener;

        private void OnDisable() => _listeners = null;
    }
}
