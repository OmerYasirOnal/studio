// Brave Bunny — Systems / Analytics
// No PII per 03-save-system.md privacy posture.
// Events accumulate locally and flush in batches; backend impl is NoOp at launch.

#nullable enable

using System;
using System.Collections.Generic;

namespace Brave.Systems.Analytics;

/// <summary>
/// One analytics event. <see cref="Properties"/> is a free-form dictionary,
/// keys must be snake_case to match the eventual server schema. Timestamp is
/// UTC ticks for cheap serialization.
/// </summary>
public readonly struct AnalyticsEvent
{
    public readonly string Name;
    public readonly IReadOnlyDictionary<string, object> Properties;
    public readonly long TimestampUtcTicks;

    public AnalyticsEvent(string name, IReadOnlyDictionary<string, object> props, DateTime? timestamp = null)
    {
        Name = name;
        Properties = props;
        TimestampUtcTicks = (timestamp ?? DateTime.UtcNow).Ticks;
    }
}
