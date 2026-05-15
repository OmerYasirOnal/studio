// Brave Bunny — Systems / Progression / TutorialState
// Wave 7C onboarding. Owner: ui-engineer (cross-listed under Progression so
// the persistence concern lives next to other save-backed services).
//
// Responsibility:
//   * Tiny wrapper around ISaveService.Data.TutorialSeen so UI controllers
//     don't reach directly into SaveData fields.
//   * Pure read/write — no UI dependency, no UnityEngine usage; fully
//     EditMode-testable with a stub ISaveService.

#nullable enable

using Brave.Systems.Context;
using Brave.Systems.Save;

namespace Brave.Systems.Progression;

/// <summary>
/// Read/write façade for the first-run tutorial completion flag. Per
/// CLAUDE.md principle 6 (no magic numbers) and ADR-0008 (forward-compat
/// JSON), the field name lives on <see cref="SaveData.TutorialSeen"/>; this
/// service narrows the API surface so controllers can't accidentally write
/// other save sections while toggling the flag.
/// </summary>
public interface ITutorialState : IService
{
    /// <summary>
    /// True if the first-run tutorial overlay should mount on Run-scene start.
    /// Mirrors <c>!SaveData.TutorialSeen</c>.
    /// </summary>
    bool ShouldShow { get; }

    /// <summary>
    /// Marks the tutorial as completed (or skipped). Idempotent — calling
    /// twice is a no-op on the second call. Persists via the injected
    /// <see cref="ISaveService"/> immediately so a process kill before the
    /// next save trigger doesn't lose the flag.
    /// </summary>
    void MarkCompleted();
}

/// <inheritdoc cref="ITutorialState"/>
public sealed class TutorialState : ITutorialState
{
    private readonly ISaveService _save;

    public TutorialState(ISaveService save)
    {
        _save = save ?? throw new System.ArgumentNullException(nameof(save));
    }

    public bool ShouldShow => !_save.Data.TutorialSeen;

    public void MarkCompleted()
    {
        if (_save.Data.TutorialSeen) return;
        _save.Data.TutorialSeen = true;
        _save.Save();
    }
}
