// C# 9 records compile their positional members as init-only setters, which
// the compiler expresses through System.Runtime.CompilerServices.IsExternalInit.
// That type ships with .NET 5 and later; Unity's .NET Standard 2.1 profile
// predates it, so every assembly that *declares* records must supply its own.
// Declaring it here is the documented workaround, not a hack around Unity.
// See docs/DECISIONS.md.

using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit
    {
    }
}
