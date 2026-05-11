// Brave Bunny — Systems / Analytics
// CLAUDE.md zero-external-paid-API rule: NoOp backend at launch.
// Replace with a real provider (Firebase Analytics is free; Unity Analytics is
// free) post-launch. Do NOT introduce a paid SaaS without an ADR.

#nullable enable

using UnityEngine;

namespace Brave.Systems.Analytics;

public interface IAnalyticsBackend
{
    void Send(AnalyticsEvent evt);
}

/// <summary>
/// NoOp backend that logs every event to the console. Useful in Editor +
/// dev builds so designers can verify event firing without a server.
/// Production builds will swap this for a Firebase Analytics adapter or
/// equivalent free-tier service (post-launch — see CLAUDE.md "permitted
/// because they are revenue side, not generation side").
/// </summary>
public sealed class AnalyticsBackend : IAnalyticsBackend
{
    public void Send(AnalyticsEvent evt)
    {
        // Keep payload concise so Editor log spam is tolerable in dev sessions.
        Debug.Log($"[analytics] {evt.Name} props={evt.Properties.Count}");
    }
}
