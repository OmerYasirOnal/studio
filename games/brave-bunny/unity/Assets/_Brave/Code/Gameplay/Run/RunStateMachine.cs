#nullable enable
// Tech-spec 08 § Implementation pattern. Game-state graph hard-coded; transitions
// explicit + reviewable. This class lives in Gameplay because Run is its scope; the
// full graph (Boot/MainMenu/Store/Settings/MetaUpgrade) is owned by Brave.Boot.

using System;
using System.Collections.Generic;

using UnityEngine;

namespace Brave.Gameplay.Run
{
    /// <summary>
    /// State graph slice that lives inside Run. The boot-level GameStateManager owns Boot,
    /// MainMenu, etc.; this state machine governs Loadout → RunIntro → Run → RunPaused →
    /// RunEnd within the run scene's lifetime.
    /// </summary>
    public sealed class RunStateMachine
    {
        public enum State
        {
            Boot       = 0,
            MainMenu   = 1,
            Loadout    = 2,
            RunIntro   = 3,
            Run        = 4,
            RunPaused  = 5,
            RunEnd     = 6,
        }

        private static readonly Dictionary<State, State[]> _allowed = new()
        {
            { State.Boot,      new[] { State.MainMenu } },
            { State.MainMenu,  new[] { State.Loadout } },
            { State.Loadout,   new[] { State.RunIntro, State.MainMenu } },
            { State.RunIntro,  new[] { State.Run, State.MainMenu } },
            { State.Run,       new[] { State.RunPaused, State.RunEnd } },
            { State.RunPaused, new[] { State.Run, State.RunEnd } },
            { State.RunEnd,    new[] { State.MainMenu } },
        };

        public State Current { get; private set; } = State.Boot;

        public event Action<State, State>? Transitioned;

        public void TransitionTo(State next)
        {
            if (!IsAllowed(Current, next))
                throw new InvalidOperationException($"Illegal transition {Current} -> {next}");
            var prev = Current;
            Current = next;
            Transitioned?.Invoke(prev, next);
        }

        public static bool IsAllowed(State from, State to)
        {
            if (!_allowed.TryGetValue(from, out var list)) return false;
            for (int i = 0; i < list.Length; i++)
                if (list[i] == to) return true;
            return false;
        }
    }
}
