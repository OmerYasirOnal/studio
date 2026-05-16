#if UNITY_EDITOR
// Wave 13 — namespaced entry-points for the playable MVP Run wiring.
//
// The canonical SceneSetup class is in the global namespace (historical reason —
// it predates the BraveBunny.Editor namespace convention used elsewhere in this
// folder). The Wave-13 brief asks for invocation paths in the form
// `BraveBunny.Editor.SceneSetup.EnsurePlayableMvpRun`. Rather than rename the
// canonical class (which would invalidate every previous -executeMethod path on
// build CI), this file provides a thin namespaced shim that delegates.

using UnityEditor;

namespace BraveBunny.Editor
{
    /// <summary>
    /// Namespaced facade for <see cref="global::SceneSetup"/>'s Wave-13 entry-points.
    /// Mirror — not replacement — of the canonical class.
    /// </summary>
    public static class SceneSetup
    {
        /// <summary>Editor + CLI entry — patches Run.unity to be MVP-playable.</summary>
        public static void EnsurePlayableMvpRun() => global::SceneSetup.EnsurePlayableMvpRun();

        /// <summary>CLI variant that exits the editor after running.</summary>
        public static void EnsurePlayableMvpRunHeadless() => global::SceneSetup.EnsurePlayableMvpRunHeadless();
    }
}
#endif
