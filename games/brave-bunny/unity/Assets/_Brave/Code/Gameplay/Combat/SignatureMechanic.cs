#nullable enable
// ADR-0009: polymorphic mechanics via type-name registry. Each character carries a
// signatureMechanicTypeName on its CharacterDefinition; MechanicRegistry resolves it
// to a concrete subclass tagged with [BraveRegister].

using System;

using UnityEngine;

namespace Brave.Gameplay.Combat
{
    /// <summary>
    /// Marks a class as a discoverable mechanic. The token must match the
    /// <c>signatureMechanicTypeName</c> field on the data asset that references it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class BraveRegisterAttribute : Attribute
    {
        public string TypeName { get; }

        public BraveRegisterAttribute(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException("BraveRegister typeName must be non-empty", nameof(typeName));
            TypeName = typeName;
        }
    }

    /// <summary>
    /// Minimal run-time context handed to a signature mechanic. The concrete fields are
    /// filled by RunController; <see cref="Services"/> is the systems-engineer's
    /// GameContext (typed as <c>object</c> here to avoid asmdef coupling).
    /// </summary>
    public interface IRunContext
    {
        object Services { get; }
        Transform HeroTransform { get; }
        float RunSeconds { get; }
        int PlayerLevel { get; }
    }

    /// <summary>
    /// Abstract base for the 8 character signature mechanics. Concrete subclasses live in
    /// this same namespace and each carries a <see cref="BraveRegisterAttribute"/>.
    /// </summary>
    public abstract class SignatureMechanic
    {
        public abstract void Initialize(IRunContext ctx);
        public abstract void Tick(float dt);
        public virtual void Detach() { }
    }

    /// <summary>
    /// Bunny's signature: dodge every 5th auto-attack (per GDD 03-characters.md).
    /// </summary>
    [BraveRegister("bunny.hop_dodge")]
    public sealed class BunnyHopDodge : SignatureMechanic
    {
        private const int DodgeEvery = 5;
        private IRunContext? _ctx;
        private int _attackCount;

        public override void Initialize(IRunContext ctx) => _ctx = ctx;

        public override void Tick(float dt)
        {
            if (_ctx == null) return;
            // Listens to attack events via the CombatResolver direct-method bus.
            // On every Nth attack, raises an i-frame window of 0.4s. Stub.
        }

        /// <summary>Called by the attack pipeline whenever Bunny fires a weapon.</summary>
        public void OnAttackFired()
        {
            _attackCount++;
            if (_attackCount % DodgeEvery == 0)
            {
                // Grant 0.4s i-frames via EnemyHealth.SetInvulnerable, etc. Stub.
            }
        }
    }

    /// <summary>
    /// Tortoise's signature: shell shield blocks the first hit every 8 seconds (placeholder).
    /// </summary>
    [BraveRegister("tortoise.shell_shield")]
    public sealed class TortoiseShellShield : SignatureMechanic
    {
        private const float CooldownSeconds = 8f;
        private float _cooldown;
        private bool _shieldUp = true;

        public override void Initialize(IRunContext ctx) { _shieldUp = true; _cooldown = 0f; }

        public override void Tick(float dt)
        {
            if (_shieldUp) return;
            _cooldown -= dt;
            if (_cooldown <= 0f)
            {
                _shieldUp = true;
                _cooldown = 0f;
            }
        }

        /// <summary>Returns true if a hit should be absorbed; consumes the shield.</summary>
        public bool TryAbsorbHit()
        {
            if (!_shieldUp) return false;
            _shieldUp = false;
            _cooldown = CooldownSeconds;
            return true;
        }
    }
}
