// Brave Bunny — Systems / Context
// Tech spec: docs/06-tech-spec/08-state-machine.md (Boot entry actions)
//            docs/06-tech-spec/09-event-bus.md (service registry table)
// Sister file: GameContextBootstrap.cs is the live MonoBehaviour that wires the service graph
// in Boot.unity. This static helper raises the GameContextReady event after registration
// and runs the MechanicRegistry [BraveRegister] reflection scan per ADR-0009.

#nullable enable

using System;
using UnityEngine;
using Brave.Gameplay.Combat;

namespace Brave.Systems.Context;

/// <summary>
/// Boot-time helper that runs after <see cref="GameContextBootstrap"/> has
/// finished registering services. Responsibilities:
/// <list type="bullet">
/// <item>Scan loaded assemblies for <see cref="BraveRegisterAttribute"/> classes (ADR-0009).</item>
/// <item>Register the resulting <see cref="MechanicRegistry"/> with <see cref="GameContext"/>.</item>
/// <item>Raise the static <see cref="GameContextReady"/> event so MonoBehaviours in <c>Boot.unity</c>
///       and Editor utilities can know when service lookups are safe.</item>
/// </list>
/// Kept as a static helper rather than a second MonoBehaviour to keep the bootstrap surface single-file-readable.
/// </summary>
public static class Bootstrapper
{
    /// <summary>
    /// Raised exactly once per app launch, after services are registered and
    /// MechanicRegistry has finished scanning. Listeners that need a valid
    /// <see cref="GameContext"/> at scene-load time should subscribe here.
    /// </summary>
    public static event Action? GameContextReady;

    private static bool _ready;

    /// <summary>
    /// Called by <see cref="GameContextBootstrap.Awake"/> after the service graph
    /// has been registered. Runs the registry scan and broadcasts readiness.
    /// </summary>
    public static void Complete(GameContext ctx)
    {
        if (_ready)
        {
            Debug.LogWarning("[Bootstrapper] Complete() called twice — ignoring second invocation.");
            return;
        }

        // MechanicRegistry is a static singleton (per ADR-0009). Scan triggers the
        // reflection sweep idempotently; no instance is registered with GameContext.
        try
        {
            MechanicRegistry.ScanAssemblies();
        }
        catch (Exception e)
        {
            Debug.LogError($"[Bootstrapper] MechanicRegistry scan failed: {e.Message}");
        }

        _ready = true;
        try
        {
            GameContextReady?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[Bootstrapper] GameContextReady listener threw: {e.Message}");
        }
    }

    /// <summary>Test-only reset hook used by EditMode tests; clears the static flag + listeners.</summary>
    internal static void ResetForTests()
    {
        _ready = false;
        GameContextReady = null;
    }

    public static bool IsReady => _ready;
}
