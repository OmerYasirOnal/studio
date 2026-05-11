// Brave Bunny — UI / Bindings / UIEvents
// Static UI-intent bus. Controllers raise high-level user intents (start run,
// open settings, etc.) without depending on the concrete RunService or
// NavigationService. The Boot scene wires consumers in
// GameContextBootstrap.Awake or a sibling UIRoot MonoBehaviour.
//
// Tech-spec 09-event-bus.md: this is a Tier-2 UI-only bus, scope-limited to
// scene-navigation + modal-open intents. Gameplay events go through the
// ScriptableObject channels under Gameplay/Events/.

#nullable enable

using System;

namespace Brave.UI.Bindings
{
    /// <summary>UI-layer intent bus. Subscribers are wired at boot.</summary>
    public static class UIEvents
    {
        public static event Action? StartRunRequested;
        public static event Action<string>? PushScreen;
        public static event Action? OpenMailbox;
        public static event Action<bool>? AdDoubleRewardsRequested; // true=accept, false=decline
        public static event Action? ShareRunRequested;
        public static event Action? GoHomeRequested;
        public static event Action? RetryRunRequested;
        public static event Action? PauseRunRequested;
        public static event Action<int>? UpgradePicked;   // 0|1|2 → card index
        public static event Action? BanishRequested;
        public static event Action? RerollRequested;

        public static void RaiseStartRunRequested() => StartRunRequested?.Invoke();
        public static void RaisePushScreen(string screen) => PushScreen?.Invoke(screen);
        public static void RaiseOpenMailbox() => OpenMailbox?.Invoke();
        public static void RaiseAdDoubleRewardsRequested(bool accept) => AdDoubleRewardsRequested?.Invoke(accept);
        public static void RaiseShareRunRequested() => ShareRunRequested?.Invoke();
        public static void RaiseGoHomeRequested() => GoHomeRequested?.Invoke();
        public static void RaiseRetryRunRequested() => RetryRunRequested?.Invoke();
        public static void RaisePauseRunRequested() => PauseRunRequested?.Invoke();
        public static void RaiseUpgradePicked(int index) => UpgradePicked?.Invoke(index);
        public static void RaiseBanishRequested() => BanishRequested?.Invoke();
        public static void RaiseRerollRequested() => RerollRequested?.Invoke();

        /// <summary>EditMode-test reset hook. Production code never calls this.</summary>
        public static void ResetAllSubscribers()
        {
            StartRunRequested = null;
            PushScreen = null;
            OpenMailbox = null;
            AdDoubleRewardsRequested = null;
            ShareRunRequested = null;
            GoHomeRequested = null;
            RetryRunRequested = null;
            PauseRunRequested = null;
            UpgradePicked = null;
            BanishRequested = null;
            RerollRequested = null;
        }
    }
}
