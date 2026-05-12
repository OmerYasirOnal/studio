// Brave Bunny — UI / Controllers / SettingsController
// Bound to: _Brave/UI/Documents/Settings.uxml
// Wireframe spec: docs/05-wireframes/14-settings.html
// User stories: US-03 deferred permissions, US-11 audio prefs, US-44 ad opt-in,
//               US-46 no interstitials, US-54 restore purchases (1 tap),
//               US-56 anonymous leaderboard, US-61 block/report.
//
// Slider values are linear 0-1; SettingsService handles linear→dB conversion
// per docs/06-tech-spec/07-audio.md. We commit on modal-close (Back button)
// rather than on every slider tick — see 03-save-system.md trigger list.

#nullable enable

using Brave.UI.Bindings;
using Brave.UI.Theming;
using Brave.Systems.Context;
using Brave.Systems.Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class SettingsController : MonoBehaviour
    {
        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;
        private ISettingsService? _settings;

        private Slider _sliderMusic = null!;
        private Slider _sliderSfx = null!;
        private Toggle _toggleHaptics = null!;
        private Toggle _toggleReducedMotion = null!;
        private Toggle _toggleLargeText = null!;
        private Toggle _toggleHighContrast = null!;
        private Toggle _toggleLefty = null!;
        private Toggle _toggleAutoAim = null!;
        private Toggle _togglePersonalizedAds = null!;
        private Toggle _toggleRewardedOptIn = null!;
        private DropdownField _languageDropdown = null!;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _loc = new LocalizationProvider();
            SafeAreaUtility.Attach(gameObject, _doc.rootVisualElement);
        }

        private void OnEnable()
        {
            var root = _doc.rootVisualElement;

            _sliderMusic = root.Q<Slider>("slider-music")!;
            _sliderSfx = root.Q<Slider>("slider-sfx")!;
            _toggleHaptics = root.Q<Toggle>("toggle-haptics")!;
            _toggleReducedMotion = root.Q<Toggle>("toggle-reduced-motion")!;
            _toggleLargeText = root.Q<Toggle>("toggle-large-text")!;
            _toggleHighContrast = root.Q<Toggle>("toggle-high-contrast")!;
            _toggleLefty = root.Q<Toggle>("toggle-lefty")!;
            _toggleAutoAim = root.Q<Toggle>("toggle-auto-aim")!;
            _togglePersonalizedAds = root.Q<Toggle>("toggle-personalized-ads")!;
            _toggleRewardedOptIn = root.Q<Toggle>("toggle-rewarded-opt-in")!;
            _languageDropdown = root.Q<DropdownField>("dd-language")!;

            HydrateFromService();

            // Audio
            _sliderMusic.RegisterValueChangedCallback(e => _settings?.SetAudioMusic(e.newValue));
            _sliderSfx.RegisterValueChangedCallback(e => _settings?.SetAudioSfx(e.newValue));
            _toggleHaptics.RegisterValueChangedCallback(e => _settings?.SetHaptics(e.newValue));

            // Language
            _languageDropdown.RegisterValueChangedCallback(e =>
            {
                var code = e.newValue switch
                {
                    "Türkçe" => LanguageCode.Tr,
                    "Bahasa" => LanguageCode.Id,
                    "Filipino" => LanguageCode.Ph,
                    _ => LanguageCode.En,
                };
                _settings?.SetLanguage(code);
                _loc.SetLanguage(code);
                _loc.ApplyToTree(_doc.rootVisualElement);
            });

            // Header back — commits + navigates home.
            root.Q<Button>("btn-back")!.clicked += OnBackClicked;

            // Restore purchases — always reachable in 1 tap per US-54.
            root.Q<Button>("btn-restore-purchases")!.clicked += () =>
            {
                if (GameContextBootstrap.Context != null
                    && GameContextBootstrap.Context.TryGet<Brave.Systems.Iap.IIapService>(out var iap))
                {
                    iap.RestorePurchases(_ => { /* result toast wired by NavigationService */ });
                }
            };

            // Tutorial reset
            root.Q<Button>("btn-reset-tutorial")!.clicked += () => UIEvents.RaisePushScreen("TutorialReset");

            // Credits
            root.Q<Button>("btn-credits")!.clicked += () => UIEvents.RaisePushScreen("Credits");

            _loc.ApplyToTree(root);
        }

        private void OnDisable()
        {
            // 03-save-system.md trigger: commit on modal close.
            _settings?.Commit();
        }

        private void HydrateFromService()
        {
            if (GameContextBootstrap.Context == null) return;
            if (!GameContextBootstrap.Context.TryGet<ISettingsService>(out var svc)) return;
            _settings = svc;
            var d = svc.Current;
            _sliderMusic.SetValueWithoutNotify(d.AudioMusic);
            _sliderSfx.SetValueWithoutNotify(d.AudioSfx);
            _toggleHaptics.SetValueWithoutNotify(d.HapticsEnabled);
            _languageDropdown.SetValueWithoutNotify(d.Language switch
            {
                LanguageCode.Tr => "Türkçe",
                LanguageCode.Id => "Bahasa",
                LanguageCode.Ph => "Filipino",
                _ => "English",
            });
        }

        private void OnBackClicked()
        {
            _settings?.Commit();
            UIEvents.RaiseGoHomeRequested();
        }
    }
}
