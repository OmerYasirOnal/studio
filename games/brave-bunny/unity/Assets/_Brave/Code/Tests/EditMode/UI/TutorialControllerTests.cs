// QA — TutorialController / TutorialFlowLogic EditMode tests (Wave 7C).
// Subject under test:
//   * Brave.UI.Controllers.TutorialFlowLogic — pure-C# state machine driving
//     the first-run tutorial overlay. Verifies sequential step progression
//     (Move → Attack → PickupXp → Boss → Done), early dismiss via Skip,
//     persistence of the tutorialSeen flag through ITutorialState, and
//     idempotency of step triggers / completion.
//   * Brave.Systems.Progression.TutorialState — verified end-to-end against
//     an InMemoryFileSystem-backed SaveService so the field round-trips
//     through the actual JSON wire format (ADR-0008 forward-compat).
//
// Pattern: matches PauseControllerTests — exercise the logic class against
// fake ITutorialState, plus an integration test on TutorialState itself.

#nullable enable

using System.Collections.Generic;
using Brave.Systems.Progression;
using Brave.Systems.Save;
using Brave.UI.Controllers;
using NUnit.Framework;

namespace Brave.Tests.EditMode.UI
{
    [TestFixture]
    public class TutorialControllerTests
    {
        // ---- constants (no magic numbers — CLAUDE.md principle 6) ----
        private const string SaveRootDir = "/tmp/brave-tutorial-tests";

        // ---- test doubles ----

        /// <summary>
        /// Minimal in-memory ITutorialState. Mirrors the SaveData.tutorialSeen
        /// contract without needing a SaveService — used for step-progression
        /// tests where the persistence path is exercised separately.
        /// </summary>
        private sealed class FakeTutorialState : ITutorialState
        {
            public bool Seen;
            public int MarkCompletedCalls;
            public bool ShouldShow => !Seen;
            public void MarkCompleted()
            {
                MarkCompletedCalls++;
                Seen = true;
            }
        }

        // ---- helpers ----

        private static (TutorialFlowLogic logic, FakeTutorialState state, List<TutorialStep> stepHistory, List<int> dismissals)
            MakeLogic()
        {
            var state = new FakeTutorialState();
            var logic = new TutorialFlowLogic(state);
            var stepHistory = new List<TutorialStep>();
            var dismissals = new List<int>();
            logic.StepChanged += s => stepHistory.Add(s);
            logic.Dismissed += () => dismissals.Add(1);
            return (logic, state, stepHistory, dismissals);
        }

        // ---- initial state ----

        [Test]
        public void NewLogic_StartsAtMoveStep()
        {
            var (logic, _, _, _) = MakeLogic();
            Assert.That(logic.Current, Is.EqualTo(TutorialStep.Move),
                "Tutorial must mount on the Move step (US: first hint = movement).");
            Assert.That(logic.IsDismissed, Is.False);
        }

        [Test]
        public void LocKeyFor_MapsEachStepToExistingEnJsonKey()
        {
            // These keys must exist in _Brave/Localization/en.json; the
            // LocalizationProvider falls back to the key itself when missing,
            // so a typo here would silently render the raw key in-game.
            Assert.That(TutorialFlowLogic.LocKeyFor(TutorialStep.Move), Is.EqualTo("tutorial.move"));
            Assert.That(TutorialFlowLogic.LocKeyFor(TutorialStep.Attack), Is.EqualTo("tutorial.attack"));
            Assert.That(TutorialFlowLogic.LocKeyFor(TutorialStep.PickupXp), Is.EqualTo("tutorial.pickup_xp"));
            Assert.That(TutorialFlowLogic.LocKeyFor(TutorialStep.Boss), Is.EqualTo("tutorial.boss"));
            Assert.That(TutorialFlowLogic.LocKeyFor(TutorialStep.Done), Is.EqualTo("tutorial.pause_hint"));
        }

        // ---- step progression ----

        [Test]
        public void NotifyMoved_AdvancesMoveToAttack()
        {
            var (logic, _, _, _) = MakeLogic();
            logic.NotifyMoved();
            Assert.That(logic.Current, Is.EqualTo(TutorialStep.Attack));
        }

        [Test]
        public void NotifyEnemyKilled_AdvancesAttackToPickupXp()
        {
            var (logic, _, _, _) = MakeLogic();
            logic.NotifyMoved();
            logic.NotifyEnemyKilled();
            Assert.That(logic.Current, Is.EqualTo(TutorialStep.PickupXp));
        }

        [Test]
        public void NotifyLevelUp_AdvancesPickupXpToBoss()
        {
            var (logic, _, _, _) = MakeLogic();
            logic.NotifyMoved();
            logic.NotifyEnemyKilled();
            logic.NotifyLevelUp();
            Assert.That(logic.Current, Is.EqualTo(TutorialStep.Boss));
        }

        [Test]
        public void NotifyBossPhase_AdvancesBossToDone()
        {
            var (logic, _, _, _) = MakeLogic();
            logic.NotifyMoved();
            logic.NotifyEnemyKilled();
            logic.NotifyLevelUp();
            logic.NotifyBossPhase();
            Assert.That(logic.Current, Is.EqualTo(TutorialStep.Done));
        }

        [Test]
        public void FullProgression_FiresStepChangedInOrder()
        {
            var (logic, _, stepHistory, _) = MakeLogic();
            logic.NotifyMoved();
            logic.NotifyEnemyKilled();
            logic.NotifyLevelUp();
            logic.NotifyBossPhase();
            Assert.That(stepHistory, Is.EqualTo(new[]
            {
                TutorialStep.Attack,
                TutorialStep.PickupXp,
                TutorialStep.Boss,
                TutorialStep.Done,
            }), "Step transitions must fire in order with no skips.");
        }

        // ---- idempotency: triggers fired out of order must not skip steps ----

        [Test]
        public void NotifyEnemyKilled_BeforeMove_IsNoOp()
        {
            var (logic, _, _, _) = MakeLogic();
            logic.NotifyEnemyKilled();
            Assert.That(logic.Current, Is.EqualTo(TutorialStep.Move),
                "An enemy-killed signal arriving before the player has moved must NOT skip the Move step.");
        }

        [Test]
        public void NotifyLevelUp_OnMoveStep_IsNoOp()
        {
            var (logic, _, _, _) = MakeLogic();
            logic.NotifyLevelUp();
            Assert.That(logic.Current, Is.EqualTo(TutorialStep.Move));
        }

        [Test]
        public void NotifyMoved_TwiceOnMoveStep_OnlyFiresStepChangedOnce()
        {
            var (logic, _, stepHistory, _) = MakeLogic();
            logic.NotifyMoved();
            logic.NotifyMoved(); // PlayerMover ticks every frame — second call must be a no-op
            Assert.That(logic.Current, Is.EqualTo(TutorialStep.Attack));
            Assert.That(stepHistory, Is.EqualTo(new[] { TutorialStep.Attack }));
        }

        // ---- completion ----

        [Test]
        public void Complete_PersistsAndDismisses()
        {
            var (logic, state, _, dismissals) = MakeLogic();
            logic.NotifyMoved();
            logic.NotifyEnemyKilled();
            logic.NotifyLevelUp();
            logic.NotifyBossPhase();

            logic.Complete();

            Assert.That(state.MarkCompletedCalls, Is.EqualTo(1),
                "Complete() must call ITutorialState.MarkCompleted exactly once.");
            Assert.That(state.Seen, Is.True);
            Assert.That(logic.IsDismissed, Is.True);
            Assert.That(dismissals, Has.Count.EqualTo(1));
        }

        [Test]
        public void Complete_IsIdempotent()
        {
            var (logic, state, _, _) = MakeLogic();
            logic.Complete();
            logic.Complete();
            Assert.That(state.MarkCompletedCalls, Is.EqualTo(1),
                "Calling Complete() twice must not double-fire the save.");
        }

        [Test]
        public void TriggersAfterComplete_AreIgnored()
        {
            var (logic, _, stepHistory, _) = MakeLogic();
            logic.Complete();
            stepHistory.Clear();

            logic.NotifyMoved();
            logic.NotifyEnemyKilled();
            logic.NotifyLevelUp();
            logic.NotifyBossPhase();

            Assert.That(logic.Current, Is.EqualTo(TutorialStep.Completed));
            Assert.That(stepHistory, Is.Empty, "Triggers fired after dismissal must not change step.");
        }

        // ---- skip ----

        [Test]
        public void Skip_FromAnyStep_PersistsAndDismisses()
        {
            var (logic, state, _, dismissals) = MakeLogic();
            // Skip on the very first step — the "Skip Tutorial" button is always
            // available, so the player must be able to opt out immediately.
            logic.Skip();
            Assert.That(state.Seen, Is.True);
            Assert.That(logic.IsDismissed, Is.True);
            Assert.That(dismissals, Has.Count.EqualTo(1));
        }

        [Test]
        public void Skip_FromMidProgression_StillPersists()
        {
            var (logic, state, _, _) = MakeLogic();
            logic.NotifyMoved();
            logic.NotifyEnemyKilled();
            logic.Skip();
            Assert.That(state.Seen, Is.True,
                "Skipping mid-way must still mark the tutorial as seen — we never re-show it.");
            Assert.That(logic.IsDismissed, Is.True);
        }

        // ---- constructor contract ----

        [Test]
        public void Ctor_NullState_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => new TutorialFlowLogic(null!));
        }

        // ---- TutorialState integration (round-trips through SaveService) ----

        [Test]
        public void TutorialState_ShouldShow_DefaultsToTrueOnFreshSave()
        {
            var fs = new InMemoryFileSystem();
            var save = new SaveService(SaveRootDir, fs);
            save.Load(); // fresh defaults

            var state = new TutorialState(save);

            Assert.That(state.ShouldShow, Is.True,
                "Fresh save defaults TutorialSeen=false, so first-run players must see the overlay.");
        }

        [Test]
        public void TutorialState_MarkCompleted_PersistsAcrossReload()
        {
            // ADR-0008 forward-compat: the new field must round-trip through
            // the JSON wire format and be readable by a second SaveService
            // instance backed by the same InMemoryFileSystem.
            var fs = new InMemoryFileSystem();
            var save = new SaveService(SaveRootDir, fs);
            save.Load();

            var state = new TutorialState(save);
            state.MarkCompleted();

            Assert.That(save.Data.TutorialSeen, Is.True);
            Assert.That(state.ShouldShow, Is.False);

            // Simulate process restart: brand-new SaveService over the same fs.
            var save2 = new SaveService(SaveRootDir, fs);
            save2.Load();
            var state2 = new TutorialState(save2);

            Assert.That(save2.Data.TutorialSeen, Is.True,
                "tutorialSeen must persist through Save → write to disk → Load.");
            Assert.That(state2.ShouldShow, Is.False,
                "A returning player must NOT see the tutorial again.");
        }

        [Test]
        public void TutorialState_MarkCompleted_Idempotent_DoesNotDoubleSave()
        {
            // The TutorialState wrapper guards against re-saving when the flag
            // is already true so we don't churn disk I/O on every tutorial mount.
            var fs = new InMemoryFileSystem();
            var save = new SaveService(SaveRootDir, fs);
            save.Load();

            var state = new TutorialState(save);
            state.MarkCompleted();
            var firstSavedAt = save.Data.LastSavedAt;

            state.MarkCompleted();

            Assert.That(save.Data.LastSavedAt, Is.EqualTo(firstSavedAt),
                "Calling MarkCompleted() twice must not re-trigger Save (LastSavedAt unchanged).");
        }

        [Test]
        public void TutorialState_Ctor_NullSave_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => new TutorialState(null!));
        }

        // ---- end-to-end: full flow drives persistence ----

        [Test]
        public void FullFlow_OverRealSaveService_MarksTutorialSeen()
        {
            var fs = new InMemoryFileSystem();
            var save = new SaveService(SaveRootDir, fs);
            save.Load();
            var state = new TutorialState(save);

            Assert.That(state.ShouldShow, Is.True, "Sanity: fresh save shows tutorial.");

            var logic = new TutorialFlowLogic(state);
            logic.NotifyMoved();
            logic.NotifyEnemyKilled();
            logic.NotifyLevelUp();
            logic.NotifyBossPhase();
            logic.Complete();

            Assert.That(state.ShouldShow, Is.False);
            Assert.That(save.Data.TutorialSeen, Is.True,
                "Full tutorial completion must persist the flag through the real SaveData.");
        }
    }
}
