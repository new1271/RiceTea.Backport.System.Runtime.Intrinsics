using System;
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
    /// <summary>
    /// See <see cref="Intrinsics.AndNot(uint, uint)"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint AndNot(uint left, uint right) => (~left) & right;

    /// <summary>
    /// See <see cref="Intrinsics.BitFieldExtract(uint, byte, byte)"/>.
    /// </summary>
    public static uint BitFieldExtract(uint value, byte start, byte length) 
        => BitFieldExtract(value, (ushort)(start | (length << 8)));

    /// <summary>
    /// See <see cref="Intrinsics.BitFieldExtract(uint, ushort)"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint BitFieldExtract(uint value, ushort control)
        => Fallbacks.BitFieldExtract(value, control);
}
