// Polyfill — enables C# 9+ `init` accessor on .NET Standard 2.1 (Unity 6 LTS).
// Reserved namespace required by spec; this file becomes a no-op once we
// move to a .NET runtime that natively defines this type.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
