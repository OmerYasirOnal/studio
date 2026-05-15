// QA — QuestPanelController EditMode tests (Wave 9 LiveOps).
// Subject under test: Brave.UI.Controllers.QuestPanelLogic — the pure-C# render
// path. Avoids spinning up a UIDocument by feeding a QuestCardBinding with raw
// UI Toolkit elements created in-process.

#nullable enable

using Brave.Systems.LiveOps;
using Brave.Systems.Progression;
using Brave.UI.Controllers;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Brave.Tests.EditMode.UI
{
    [TestFixture]
    public class QuestPanelControllerTests
    {
        private static QuestCardBinding MakeBinding()
        {
            return new QuestCardBinding
            {
                Title = new Label(),
                Reward = new Label(),
                Progress = new Label(),
                BarFill = new VisualElement(),
                Claim = new Button(),
            };
        }

        private static KillEnemiesQuest MakeKillQuest(int required = 10) =>
            new KillEnemiesQuest(
                "kill_test",
                QuestDifficulty.Easy,
                required,
                new QuestReward(CurrencyType.Carrots, 100),
                "quest.kill_enemies.title");

        [Test]
        public void Render_NullQuest_ClearsLabelsAndDisablesClaim()
        {
            var b = MakeBinding();
            b.Claim!.SetEnabled(true);

            var ok = QuestPanelLogic.Render(b, null, k => k);

            Assert.That(ok, Is.True);
            Assert.That(b.Title!.text, Is.EqualTo(string.Empty));
            Assert.That(b.Reward!.text, Is.EqualTo(string.Empty));
            Assert.That(b.Progress!.text, Is.EqualTo(string.Empty));
            Assert.That(b.Claim!.enabledSelf, Is.False);
        }

        [Test]
        public void Render_QuestInProgress_DisablesClaim()
        {
            var b = MakeBinding();
            var q = MakeKillQuest(10);
            q.OnEvent(new EnemyKilledProgress(false));
            q.OnEvent(new EnemyKilledProgress(false));

            QuestPanelLogic.Render(b, q, k => k.ToUpperInvariant());

            Assert.That(b.Title!.text, Does.Contain("KILL_ENEMIES"),
                "Title must route through the translator.");
            Assert.That(b.Progress!.text, Is.EqualTo("2 / 10"));
            Assert.That(b.Reward!.text, Is.EqualTo("+100"));
            Assert.That(b.Claim!.enabledSelf, Is.False);
        }

        [Test]
        public void Render_QuestComplete_EnablesClaim()
        {
            var b = MakeBinding();
            var q = MakeKillQuest(2);
            q.OnEvent(new EnemyKilledProgress(false));
            q.OnEvent(new EnemyKilledProgress(false));

            QuestPanelLogic.Render(b, q, k => k);

            Assert.That(q.IsClaimable, Is.True);
            Assert.That(b.Claim!.enabledSelf, Is.True);
            Assert.That(b.Progress!.text, Is.EqualTo("2 / 2"));
        }

        [Test]
        public void FormatProgress_UsesInvariantCulture()
        {
            Assert.That(QuestPanelLogic.FormatProgress(1234, 9999), Is.EqualTo("1234 / 9999"));
        }

        [Test]
        public void FormatReward_PrefixesPlus()
        {
            Assert.That(QuestPanelLogic.FormatReward(new QuestReward(CurrencyType.Carrots, 250)),
                Is.EqualTo("+250"));
        }

        [Test]
        public void FillPercent_TracksRatio()
        {
            var q = MakeKillQuest(4);
            q.OnEvent(new EnemyKilledProgress(false));
            Assert.That(QuestPanelLogic.FillPercent(q), Is.EqualTo(25f).Within(0.01f));
        }

        [Test]
        public void Render_PushesPercentWidth()
        {
            var b = MakeBinding();
            var q = MakeKillQuest(4);
            q.OnEvent(new EnemyKilledProgress(false));
            q.OnEvent(new EnemyKilledProgress(false));

            QuestPanelLogic.Render(b, q, k => k);

            // Bar width is a Percent length — assert the value, not the unit suffix
            // (StyleLength.ToString format varies across Unity versions).
            var width = b.BarFill!.style.width.value;
            Assert.That(width.unit, Is.EqualTo(LengthUnit.Percent));
            Assert.That(width.value, Is.EqualTo(50f).Within(0.01f));
        }

        [Test]
        public void Render_NullBindingReturnsFalse()
        {
            Assert.That(QuestPanelLogic.Render(null!, MakeKillQuest(), k => k), Is.False);
        }
    }
}
