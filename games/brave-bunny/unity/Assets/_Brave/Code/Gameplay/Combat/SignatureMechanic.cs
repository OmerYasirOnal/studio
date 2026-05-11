#nullable enable
// ADR-0009: polymorphic mechanics via type-name registry. Each character carries a
// signatureMechanicTypeName on its CharacterDefinition; MechanicRegistry resolves it
// to a concrete subclass tagged with [BraveRegister].
//
// The 8 concrete signature mechanics live under
//   Brave.Gameplay.Characters.{Bunny,Tortoise,Fox,Hedgehog,Otter,Panda,Badger,Owl}*
// (see games/brave-bunny/unity/Assets/_Brave/Code/Gameplay/Characters/).

using System;
using UnityEngine;

namespace Brave.Gameplay.Combat
{
    /// <summary>
    /// Marks a class as a discoverable mechanic. The token must match the
    /// <c>signatureMechanicTypeName</c> field on the data asset that references it.
    /// IL2CPP stripping rules in unity/Assets/_Brave/link.xml MUST preserve every
    /// class with this attribute — see core/tools/code-tools/check_link_xml.py.
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
    /// Lightweight per-player context the signature-mechanic implementations consume.
    /// Concrete fields are filled by RunController at run start.
    /// </summary>
    public sealed class PlayerContext
    {
        public Transform? Player { get; init; }
        public IRunContext? Run { get; init; }
    }

    /// <summary>
    /// Abstract base for the 8 character signature mechanics. Concrete subclasses live in
    /// the Brave.Gameplay.Characters namespace and each carries a <see cref="BraveRegisterAttribute"/>.
    /// </summary>
    public abstract class SignatureMechanic
    {
        /// <summary>The token that resolves this mechanic via <see cref="MechanicRegistry"/>.</summary>
        public abstract string TypeName { get; }

        /// <summary>Initialize once when the run begins.</summary>
        public virtual void Initialize(IRunContext ctx) { }

        /// <summary>Called by the run loop every frame (or fixed tick for AI-style mechanics).</summary>
        public virtual void Tick(float dt) { }

        /// <summary>Called when the player picks up this character at loadout.</summary>
        public virtual void OnAttach(PlayerContext ctx) { }

        /// <summary>Called when the player swaps characters or the run ends.</summary>
        public virtual void OnDetach(PlayerContext ctx) { }
    }
}
