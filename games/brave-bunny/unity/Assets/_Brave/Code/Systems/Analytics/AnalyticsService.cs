// Brave Bunny — Systems / Analytics
// CLAUDE.md observability principle: events queue locally, flush in batches.
// 05-performance-budget.md: queue is allocation-light; flush is off-frame.

#nullable enable

using System.Collections.Generic;
using Brave.Systems.Context;

namespace Brave.Systems.Analytics;

public interface IAnalyticsService : IService
{
    void Track(AnalyticsEvent evt);
    void Flush();
    int QueuedCount { get; }
}

/// <summary>
/// In-memory event queue with periodic batched flush. Flush cadence is owned
/// by the caller (typically <c>GameStateManager</c> on state exit, or a Boot
/// coroutine every 30 s). Backend is injected so tests can substitute a fake.
/// </summary>
public sealed class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsBackend _backend;
    private readonly Queue<AnalyticsEvent> _queue = new(capacity: 64);

    public int QueuedCount => _queue.Count;

    public AnalyticsService(IAnalyticsBackend backend) { _backend = backend; }

    public void Track(AnalyticsEvent evt) => _queue.Enqueue(evt);

    public void Flush()
    {
        while (_queue.Count > 0)
        {
            var evt = _queue.Dequeue();
            _backend.Send(evt);
        }
    }
}
