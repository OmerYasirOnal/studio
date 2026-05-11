#nullable enable
// ADR-0009: type-name registry. Boot-time reflection scan; runtime resolves to concrete Type.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Brave.Gameplay.Combat
{
    /// <summary>
    /// Boot-time discovery of all classes carrying <see cref="BraveRegisterAttribute"/>.
    /// Called once by <c>Brave.Boot.Bootstrap</c> (~10 ms one-time cost per tech-spec 08).
    /// EditMode tests assert every <c>signatureMechanicTypeName</c> resolves before runtime.
    /// </summary>
    public static class MechanicRegistry
    {
        private static readonly Dictionary<string, Type> _typesByName = new(64);
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
                    var attr = t.GetCustomAttribute<BraveRegisterAttribute>(inherit: false);
                    if (attr == null) continue;
                    if (_typesByName.ContainsKey(attr.TypeName))
                        throw new InvalidOperationException(
                            $"Duplicate BraveRegister type-name '{attr.TypeName}' on {t.FullName}");
                    _typesByName.Add(attr.TypeName, t);
                }
            }

            _initialised = true;
        }

        /// <summary>Resolve a type-name to a Type. Throws on miss; tests gate this at edit-time.</summary>
        public static Type Resolve(string typeName)
        {
            if (!_initialised) ScanAssemblies();
            if (!_typesByName.TryGetValue(typeName, out var t))
                throw new KeyNotFoundException($"No BraveRegister type-name '{typeName}'");
            return t;
        }

        public static bool TryResolve(string typeName, out Type? type)
        {
            if (!_initialised) ScanAssemblies();
            return _typesByName.TryGetValue(typeName, out type);
        }

        /// <summary>Construct a mechanic instance by type-name. Used by RunController on spawn.</summary>
        public static SignatureMechanic Construct(string typeName)
        {
            var t = Resolve(typeName);
            var instance = Activator.CreateInstance(t)
                ?? throw new InvalidOperationException($"Activator returned null for '{typeName}'");
            return (SignatureMechanic)instance;
        }

        public static IReadOnlyDictionary<string, Type> All
        {
            get { if (!_initialised) ScanAssemblies(); return _typesByName; }
        }

        // For tests: reset registry so a controlled assembly list can be scanned.
        internal static void ResetForTests()
        {
            _typesByName.Clear();
            _initialised = false;
        }
    }
}
