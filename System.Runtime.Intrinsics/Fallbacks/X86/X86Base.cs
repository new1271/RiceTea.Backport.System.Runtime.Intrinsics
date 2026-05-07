using System.Runtime.CompilerServices;
using System.Threading;


#if !NET5_0_OR_GREATER
using RiceTea.Backport.Internals;
#endif

using Intrinsics = System.Runtime.Intrinsics.X86.X86Base;

namespace RiceTea.Backport.Fallbacks.X86;

/// <summary>
/// See <see cref="Intrinsics"/>.
/// </summary>
public abstract partial class X86Base
{
    internal X86Base() { }

#if !NET5_0_OR_GREATER
    /// <summary>
    /// See <see cref="Intrinsics.BitScanForward(uint)"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint BitScanForward(uint value)
    {
        if (value == 0U)
            return sizeof(uint) * 8;
        return Fallbacks.TrailingZeroCount(value);
    }

    /// <summary>
    /// See <see cref="Intrinsics.BitScanReverse(uint)"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint BitScanReverse(uint value)
    {
        if (value == 0U)
            return uint.MaxValue;
        return Fallbacks.Log2(value);
    }

    /// <summary>
    /// See <see cref="Intrinsics.DivRem(uint, int, int)"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int Quotient, int Remainder) DivRem(uint lower, int upper, int divisor)
        => (Fallbacks.DivRem(lower, upper, divisor, out int remainder), remainder);

    /// <summary>
    /// See <see cref="Intrinsics.DivRem(uint, uint, uint)"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (uint Quotient, uint Remainder) DivRem(uint lower, uint upper, uint divisor)
        => (Fallbacks.DivRem(lower, upper, divisor, out uint remainder), remainder);

    /// <summary>
    /// See <see cref="Intrinsics.DivRem(nuint, nint, nint)"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (nint Quotient, nint Remainder) DivRem(nuint lower, nint upper, nint divisor)
        => UnsafeHelper.PointerSizeConstant switch
        {
            sizeof(int) => DivRem((uint)lower, (int)upper, (int)divisor),
            sizeof(long) => UnsafeHelper.As<(long, long), (nint, nint)>(X64.DivRem(lower, upper, divisor)),
            _ => UnsafeHelper.PointerSize switch
            {
                sizeof(int) => DivRem((uint)lower, (int)upper, (int)divisor),
                sizeof(long) => UnsafeHelper.As<(long, long), (nint, nint)>(X64.DivRem(lower, upper, divisor)),
                _ => ThrowUtils.ThrowPlatformNotSupported<(nint Quotient, nint Remainder)>()
            }
        };

    /// <summary>
    /// See <see cref="Intrinsics.DivRem(nuint, nuint, nuint)"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (nuint Quotient, nuint Remainder) DivRem(nuint lower, nuint upper, nuint divisor)
        => UnsafeHelper.PointerSizeConstant switch
        {
            sizeof(int) => DivRem((uint)lower, (uint)upper, (uint)divisor),
            sizeof(long) => UnsafeHelper.As<(ulong, ulong), (nuint, nuint)>(X64.DivRem(lower, upper, divisor)),
            _ => UnsafeHelper.PointerSize switch
            {
                sizeof(int) => DivRem((uint)lower, (uint)upper, (uint)divisor),
                sizeof(long) => UnsafeHelper.As<(ulong, ulong), (nuint, nuint)>(X64.DivRem(lower, upper, divisor)),
                _ => ThrowUtils.ThrowPlatformNotSupported<(nuint Quotient, nuint Remainder)>()
            }
        };

    /// <summary>
    /// See <see cref="Intrinsics.Pause()"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Pause() => Thread.SpinWait(iterations: 1);
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int DivRem(uint lower, int upper, int divisor, out int remainder)
        => Fallbacks.DivRem(lower, upper, divisor, out remainder);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint DivRem(uint lower, uint upper, uint divisor, out uint remainder)
        => Fallbacks.DivRem(lower, upper, divisor, out remainder);
}
