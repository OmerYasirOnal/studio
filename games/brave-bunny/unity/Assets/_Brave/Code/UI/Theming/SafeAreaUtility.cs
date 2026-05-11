// Brave Bunny — UI / Theming / SafeAreaUtility
// Art bible: docs/07-art-bible/06-ui-visual-direction.md §Safe-area discipline.
//   - iPhone 12/13/14 (notch): 47 top, 34 bottom.
//   - iPhone SE 3 (Touch-ID): 20 top, 0 bottom.
// USS limitation: env(safe-area-inset-*) is NOT supported on Unity 6 USS, so
// we apply Screen.safeArea → root padding at runtime and react to orientation
// changes. Every screen's root element should be tagged "brave-root" and
// passed in once by its controller.
//
// Owner: ui-engineer. Bound by every Controller's Awake().

#nullable enable

using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Theming
{
    /// <summary>
    /// Applies <see cref="Screen.safeArea"/> as USS padding on the supplied
    /// root element. Listens for orientation changes and re-applies.
    /// </summary>
    public sealed class SafeAreaUtility : MonoBehaviour
    {
        private VisualElement? _root;
        private ScreenOrientation _lastOrientation;
        private Vector2 _lastSize;

        public static SafeAreaUtility Attach(GameObject host, VisualElement root)
        {
            var util = host.GetComponent<SafeAreaUtility>() ?? host.AddComponent<SafeAreaUtility>();
            util.Bind(root);
            return util;
        }

        public void Bind(VisualElement root)
        {
            _root = root;
            Apply();
        }

        private void Update()
        {
            // Re-apply only when orientation or screen size changes — cheap enough
            // to run per-frame.
            if (Screen.orientation != _lastOrientation
                || !Mathf.Approximately(Screen.width, _lastSize.x)
                || !Mathf.Approximately(Screen.height, _lastSize.y))
            {
                Apply();
            }
        }

        private void Apply()
        {
            if (_root == null) return;
            _lastOrientation = Screen.orientation;
            _lastSize = new Vector2(Screen.width, Screen.height);

            var safe = Screen.safeArea;
            // Convert from screen-pixel rect → padding values in CSS pixels (USS).
            // UI Toolkit scales by panel reference resolution; we use raw screen
            // ratios here because PanelSettings handles the scale.
            float top = Mathf.Max(0f, Screen.height - safe.yMax);
            float bottom = Mathf.Max(0f, safe.yMin);
            float left = Mathf.Max(0f, safe.xMin);
            float right = Mathf.Max(0f, Screen.width - safe.xMax);

            _root.style.paddingTop = new StyleLength(top);
            _root.style.paddingBottom = new StyleLength(bottom);
            _root.style.paddingLeft = new StyleLength(left);
            _root.style.paddingRight = new StyleLength(right);
        }
    }
}
