// Brave Bunny — UI / Controllers / AchievementToastController (Wave 10).
// Bound to: _Brave/UI/Documents/AchievementToast.uxml
//
// Subscribes to AchievementUnlockedChannel. Shows the toast for ToastSeconds
// (3s default) then hides via the .is-hidden class. If a second unlock fires
// while a toast is already on-screen the controller queues display.

#nullable enable

using System.Collections.Generic;
using Brave.Gameplay.Events;
using Brave.Systems.Localization;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class AchievementToastController : MonoBehaviour
    {
        public const float ToastSeconds = 3f;
        public const string HiddenClass = "is-hidden";
        public const string VisibleClass = "is-visible";
        public const string RootName = "achievement-toast-root";
        public const string NameLabelName = "lbl-achievement-toast-name";

        [Tooltip("Channel SO that drives the toast. Required.")]
        [SerializeField] private AchievementUnlockedChannel? _channel;

        private UIDocument _doc = null!;
        private VisualElement? _root;
        private Label? _name;
        private readonly Queue<AchievementUnlockedEvent> _queue = new();
        private bool _showing;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            var root = _doc.rootVisualElement;
            _root = root.Q<VisualElement>(RootName);
            _name = root.Q<Label>(NameLabelName);
            Hide();
            if (_channel != null) _channel.Subscribe(OnAchievementUnlocked);
        }

        private void OnDisable()
        {
            if (_channel != null) _channel.Unsubscribe(OnAchievementUnlocked);
            _queue.Clear();
            _showing = false;
        }

        private void OnAchievementUnlocked(AchievementUnlockedEvent evt)
        {
            _queue.Enqueue(evt);
            if (!_showing) ShowNext();
        }

        private void ShowNext()
        {
            if (_queue.Count == 0)
            {
                _showing = false;
                Hide();
                return;
            }
            _showing = true;
            var evt = _queue.Dequeue();
            if (_name != null) _name.text = Loc.T(evt.displayLocKey);
            Show();
            // schedule.Execute is robust against domain-reload because it sits on the
            // VisualElement (gets nuked with the panel). Matches the WaveToast pattern.
            if (_root != null)
            {
                _root.schedule.Execute(ShowNext).StartingIn((long)(ToastSeconds * 1000));
            }
        }

        private void Show()
        {
            if (_root == null) return;
            if (_root.ClassListContains(HiddenClass)) _root.RemoveFromClassList(HiddenClass);
            if (!_root.ClassListContains(VisibleClass)) _root.AddToClassList(VisibleClass);
        }

        private void Hide()
        {
            if (_root == null) return;
            if (!_root.ClassListContains(HiddenClass)) _root.AddToClassList(HiddenClass);
            if (_root.ClassListContains(VisibleClass)) _root.RemoveFromClassList(VisibleClass);
        }

        // ----- Test seam -----
        public void DriveEvent(AchievementUnlockedEvent evt) => OnAchievementUnlocked(evt);
        public bool IsShowing => _showing;
        public string CurrentDisplay => _name?.text ?? string.Empty;
    }
}
