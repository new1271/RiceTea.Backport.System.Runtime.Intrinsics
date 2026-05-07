using System.Runtime.CompilerServices;

using Intrinsics = System.Runtime.Intrinsics.X86.Bmi1;

namespace RiceTea.Backport.Fallbacks.X86;

/// <summary>
/// See <see cref="Intrinsics"/>.
/// </summary>
public abstract partial class Bmi1 : X86Base
{
    /// <summary>
    /// See <see cref="Intrinsics.TrailingZeroCount(uint)"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint TrailingZeroCount(uint value)
        => Fallbacks.TrailingZeroCount(value);
}
