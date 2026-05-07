using System.Runtime.CompilerServices;

using Intrinsics = System.Runtime.Intrinsics.X86.Popcnt;

namespace RiceTea.Backport.Fallbacks.X86;

/// <summary>
/// See <see cref="Intrinsics"/>.
/// </summary>
public abstract partial class Popcnt : X86Base
{
    /// <summary>
    /// See <see cref="Intrinsics.PopCount(uint)"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint PopCount(uint value)
        => Fallbacks.PopCount(value);
}
