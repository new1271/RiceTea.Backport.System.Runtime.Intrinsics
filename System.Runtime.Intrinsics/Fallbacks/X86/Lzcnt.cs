using System.Runtime.CompilerServices;

using Intrinsics = System.Runtime.Intrinsics.X86.Lzcnt;

namespace RiceTea.Backport.Fallbacks.X86;

/// <summary>
/// See <see cref="Intrinsics"/>.
/// </summary>
public abstract partial class Lzcnt : X86Base
{
    /// <summary>
    /// See <see cref="Intrinsics.LeadingZeroCount(uint)"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint LeadingZeroCount(uint value)
        => Fallbacks.LeadingZeroCount(value);
}
