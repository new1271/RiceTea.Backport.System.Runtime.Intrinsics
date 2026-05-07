using System.Runtime.CompilerServices;

using Intrinsics = System.Runtime.Intrinsics.X86.X86Base.X64;

namespace RiceTea.Backport.Fallbacks.X86;

partial class X86Base
{
    /// <summary>
    /// See <see cref="Intrinsics"/>.
    /// </summary>
    public abstract class X64
    {
#if !NET5_0_OR_GREATER
        /// <summary>
        /// See <see cref="Intrinsics.BitScanForward(ulong)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong BitScanForward(ulong value)
        {
            if (value == 0U)
                return sizeof(ulong) * 8;
            return Fallbacks.TrailingZeroCount(value);
        }

        /// <summary>
        /// See <see cref="Intrinsics.BitScanReverse(ulong)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong BitScanReverse(ulong value)
        {
            if (value == 0U)
                return ulong.MaxValue;
            return Fallbacks.Log2(value);
        }

        /// <summary>
        /// See <see cref="Intrinsics.DivRem(ulong, long, long)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (long Quotient, long Remainder) DivRem(ulong lower, long upper, long divisor)
            => (Fallbacks.DivRem(lower, upper, divisor, out long remainder), remainder);

        /// <summary>
        /// See <see cref="Intrinsics.DivRem(ulong, ulong, ulong)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (ulong Quotient, ulong Remainder) DivRem(ulong lower, ulong upper, ulong divisor)
            => (Fallbacks.DivRem(lower, upper, divisor, out ulong remainder), remainder);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static long DivRem(ulong lower, long upper, long divisor, out long remainder)
            => Fallbacks.DivRem(lower, upper, divisor, out remainder);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ulong DivRem(ulong lower, ulong upper, ulong divisor, out ulong remainder)
            => Fallbacks.DivRem(lower, upper, divisor, out remainder);
    }
}
