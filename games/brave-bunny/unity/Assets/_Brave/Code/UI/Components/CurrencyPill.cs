// Brave Bunny — UI / Components / CurrencyPill
// A reusable currency display widget (icon + amount + suffix).
// Used by: Home (carrots/gems chips), Run-end (banked totals), Shop SKUs.
// Art bible: 06-ui-visual-direction.md §Card/Chip style. Numerics in Baloo 2.
//
// Pattern: this is a thin UXML-friendly subclass of <c>VisualElement</c> that
// exposes a single <c>SetAmount(int)</c> with a 120 ms ease-out scale "pop"
// confirmation animation per Pillar 5.

#nullable enable

using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Components
{
    /// <summary>
    /// Re-usable currency chip. Bind via <see cref="Bind"/> after the UXML is
    /// loaded; call <see cref="SetAmount"/> when the wallet changes.
    /// </summary>
    public sealed class CurrencyPill
    {
        private readonly Label _amountLabel;
        private readonly VisualElement _container;
        private int _lastAmount;

        public CurrencyPill(VisualElement container, Label amountLabel)
        {
            _container = container;
            _amountLabel = amountLabel;
        }

        /// <summary>Static helper: query a UXML-instantiated chip by element name.</summary>
        public static CurrencyPill Bind(VisualElement root, string containerName, string amountLabelName)
        {
            var container = root.Q<VisualElement>(containerName)
                ?? throw new System.InvalidOperationException($"CurrencyPill: missing container '{containerName}'");
            var amount = root.Q<Label>(amountLabelName)
                ?? throw new System.InvalidOperationException($"CurrencyPill: missing label '{amountLabelName}'");
            return new CurrencyPill(container, amount);
        }

        public void SetAmount(int amount, bool animate = true)
        {
            _amountLabel.text = FormatAmount(amount);
            if (animate && amount != _lastAmount) PopAnimation();
            _lastAmount = amount;
        }

        public void SetDimmed(bool dimmed)
        {
            _container.style.opacity = dimmed ? 0.5f : 1.0f;
        }

        /// <summary>120 ms scale "pop" — matches Pillar 5 confirmation timing.</summary>
        private void PopAnimation()
        {
            _container.style.scale = new Scale(new Vector3(1.12f, 1.12f, 1f));
            _container.schedule.Execute(() =>
                _container.style.scale = new Scale(Vector3.one)
            ).StartingIn(120);
        }

        /// <summary>Locale-friendly integer format with thousands separators.</summary>
        private static string FormatAmount(int amount)
            => amount.ToString("N0", System.Globalization.CultureInfo.CurrentCulture);
    }
}
