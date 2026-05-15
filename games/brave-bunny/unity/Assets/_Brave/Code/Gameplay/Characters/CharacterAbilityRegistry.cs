#nullable enable
// Wave 10 — Character ability id → CharacterAbility factory registry.
//
// Reuses ADR-0009's [BraveRegister] discovery via the shared MechanicRegistry-style
// reflection scan, but stays in its own dictionary so abilities can be constructed
// independently of SignatureMechanic types. Tokens follow the "ability.<id>" scheme
// to keep them visually distinct from signature tokens.
//
// Boot path: CharacterAbilityRegistry.ScanAssemblies() runs once during Brave.Boot
// alongside MechanicRegistry.ScanAssemblies(). Resolve / Construct lookups are
// allocation-free and case-sensitive.

using System;
using System.Collections.Generic;
using System.Reflection;
using Brave.Gameplay.Combat;

namespace Brave.Gameplay.Characters
{
    /// <summary>
    /// Maps characterId (e.g. "hop") → concrete <see cref="CharacterAbility"/> Type.
    /// Discovers every non-abstract <c>CharacterAbility</c> subclass tagged with
    /// <c>[BraveRegister("ability.&lt;id&gt;")]</c> in the loaded assemblies.
    /// </summary>
    public static class CharacterAbilityRegistry
    {
        // Prefix on [BraveRegister] tokens so character-ability tokens never collide
        // with signature-mechanic tokens (ADR-0009 mandates global uniqueness).
        public const string TokenPrefix = "ability.";

        // Key in this dictionary is the bare ability id (no prefix), to match
        // characters.json:ability_id values directly.
        private static readonly Dictionary<string, Type> _typesById = new(16);
        private static bool _initialised;

        /// <summary>Idempotent scan of supplied assemblies (defaults to all loaded).</summary>
        public static void ScanAssemblies(IEnumerable<Assembly>? assemblies = null)
        {
            if (_initialised) return;

            assemblies ??= AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in assemblies)
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { types = ex.Types ?? Array.Empty<Type>(); }

                for (int i = 0; i < types.Length; i++)
                {
                    var t = types[i];
                    if (t == null || t.IsAbstract || t.IsInterface) continue;
                    if (!typeof(CharacterAbility).IsAssignableFrom(t)) continue;
                    var attr = t.GetCustomAttribute<BraveRegisterAttribute>(inherit: false);
                    if (attr == null) continue;
                    if (!attr.TypeName.StartsWith(TokenPrefix, StringComparison.Ordinal)) continue;

                    var id = attr.TypeName.Substring(TokenPrefix.Length);
                    if (string.IsNullOrEmpty(id))
                        throw new InvalidOperationException(
                            $"CharacterAbility {t.FullName}: [BraveRegister] token '{attr.TypeName}' has empty id");

                    if (_typesById.ContainsKey(id))
                        throw new InvalidOperationException(
                            $"Duplicate CharacterAbility id '{id}' on {t.FullName}");

                    _typesById.Add(id, t);
                }
            }

            _initialised = true;
        }

        /// <summary>Resolve a character ability id (e.g. "hop") to its Type. Throws on miss.</summary>
        public static Type Resolve(string abilityId)
        {
            if (!_initialised) ScanAssemblies();
            if (!_typesById.TryGetValue(abilityId, out var t))
                throw new KeyNotFoundException($"No CharacterAbility with id '{abilityId}'");
            return t;
        }

        public static bool TryResolve(string abilityId, out Type? type)
        {
            if (!_initialised) ScanAssemblies();
            return _typesById.TryGetValue(abilityId, out type);
        }

        /// <summary>Construct a fresh ability instance by id. Used by RunController on hero spawn.</summary>
        public static CharacterAbility Construct(string abilityId)
        {
            var t = Resolve(abilityId);
            var instance = Activator.CreateInstance(t)
                ?? throw new InvalidOperationException($"Activator returned null for ability id '{abilityId}'");
            return (CharacterAbility)instance;
        }

        public static IReadOnlyDictionary<string, Type> All
        {
            get { if (!_initialised) ScanAssemblies(); return _typesById; }
        }

        // For tests: reset registry so a controlled assembly list can be scanned.
        internal static void ResetForTests()
        {
            _typesById.Clear();
            _initialised = false;
        }
    }
}
