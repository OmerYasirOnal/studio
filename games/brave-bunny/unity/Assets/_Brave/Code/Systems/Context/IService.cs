// Brave Bunny — Systems / Context
// Tech spec: docs/06-tech-spec/09-event-bus.md (service locator pattern)
// Empty marker interface for anything registered with GameContext.

#nullable enable

namespace Brave.Systems.Context;

/// <summary>
/// Marker interface for all app-lifetime services registered against
/// <see cref="GameContext"/>. Has no members — exists purely so callers can
/// statically constrain on "is a service" and so reflection passes can find
/// the full set of registered services for diagnostics.
/// </summary>
public interface IService
{
}
