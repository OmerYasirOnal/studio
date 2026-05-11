// Brave Bunny — UI / Components / PrimaryButton
// Feel-pillar timings (docs/02-gdd/11-feel-pillars.md Pillar 5):
//   - Press visual feedback: ≤ 60 ms (scale to 0.95).
//   - Confirmation animation: ≤ 120 ms (scale back to 1.00 + "tick" SFX).
// Disabled "locked" state shakes 3 px horizontally for 180 ms per art bible.
//
// This is a UXML-friendly helper that wraps a standard <c>Button</c> and
// installs the timing behaviour. Authors use a plain <c>&lt;ui:Button&gt;</c>
// in UXML and pass it to <see cref="Wrap"/> in their controller.

#nullable enable

using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Components
{
    /// <summary>
    /// Behavioural wrapper that adds Pillar-5 feel to a standard
    /// <see cref="Button"/>. Does NOT subclass Button so UXML stays clean.
    /// </summary>
    public sealed class PrimaryButton
    {
        private const int PressVisualMs = 60;
        private const int ConfirmAnimMs = 120;
        private const int LockedShakeMs = 180;

        private readonly Button _btn;
        private bool _isLocked;

        public Button Element => _btn;

        public PrimaryButton(Button btn)
        {
            _btn = btn;
            _btn.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _btn.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        public static PrimaryButton Wrap(Button btn) => new(btn);

        public void SetLocked(bool locked)
        {
            _isLocked = locked;
            if (locked)
            {
                _btn.AddToClassList("btn-disabled");
            }
            else
            {
                _btn.RemoveFromClassList("btn-disabled");
            }
        }

        private void OnPointerDown(PointerDownEvent _)
        {
            if (_isLocked)
            {
                ShakeLocked();
                return;
            }
            _btn.style.scale = new Scale(new Vector3(0.95f, 0.95f, 1f));
            _btn.schedule.Execute(() => { /* PressVisualMs anchor */ }).StartingIn(PressVisualMs);
        }

        private void OnPointerUp(PointerUpEvent _)
        {
            if (_isLocked) return;
            _btn.schedule.Execute(() =>
                _btn.style.scale = new Scale(Vector3.one)
            ).StartingIn(ConfirmAnimMs - PressVisualMs);
        }

        private void ShakeLocked()
        {
            const int step = LockedShakeMs / 4;
            var btn = _btn;
            btn.style.translate = new Translate(3f, 0f);
            btn.schedule.Execute(() => btn.style.translate = new Translate(-3f, 0f)).StartingIn(step);
            btn.schedule.Execute(() => btn.style.translate = new Translate(3f, 0f)).StartingIn(step * 2);
            btn.schedule.Execute(() => btn.style.translate = new Translate(0f, 0f)).StartingIn(step * 3);
        }
    }
}
