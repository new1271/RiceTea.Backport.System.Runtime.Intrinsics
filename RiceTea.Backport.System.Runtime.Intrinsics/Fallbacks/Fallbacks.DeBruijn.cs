using System;
using System.Runtime.CompilerServices;

using RiceTea.Backport.Fallbacks.X86;
using RiceTea.Backport.Internals;

namespace RiceTea.Backport.Fallbacks;

partial class Fallbacks
{
    private static class DeBruijn_StoreAsArray
    {
        public static readonly byte[] TrailingZeroCountDeBruijn32 = new byte[sizeof(uint) * 8]
        {
            00, 01, 28, 02, 29, 14, 24, 03,
            30, 22, 20, 15, 25, 17, 04, 08,
            31, 27, 13, 23, 21, 19, 16, 07,
            26, 12, 18, 06, 11, 05, 10, 09
        };

        // Source code from https://github.com/dotnet/runtime/blob/1d1bf92fcf43aa6981804dc53c5174445069c9e4/src/libraries/System.Private.CoreLib/src/System/Numerics/BitOperations.cs
        public static readonly byte[] Log2DeBruijn32 = new byte[sizeof(uint) * 8]
        {
            00, 09, 01, 10, 13, 21, 02, 29,
            11, 14, 16, 18, 22, 25, 03, 30,
            08, 12, 20, 28, 15, 17, 24, 07,
            19, 27, 23, 06, 26, 05, 04, 31
        };

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int TrailingZeroCount(uint value)
        {
            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            return UnsafeHelper.AddByteOffset(
                // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_0111_1100_1011_0101_0011_0001u
                ref TrailingZeroCountDeBruijn32[0],
                // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
                (nuint)(int)(((value & (uint)-(int)value) * 0x077CB531u) >> 27)); // Multi-cast mitigates redundant conv.u8
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Log2(uint value)
        {
            // Fill trailing zeros with ones, eg 00010010 becomes 00011111
            value |= value >> 01;
            value |= value >> 02;
            value |= value >> 04;
            value |= value >> 08;
            value |= value >> 16;

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            return UnsafeHelper.AddByteOffset(
                // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_1100_0100_1010_1100_1101_1101u
                ref Log2DeBruijn32[0],
                // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
                (uint)(int)((value * 0x07C4ACDDu) >> 27));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static byte QueryTrailingZeroCountTable(nuint index)
        {
            return UnsafeHelper.AddByteOffset(
                in TrailingZeroCountDeBruijn32[0],
                index);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static byte QueryLog2Table(nuint index)
        {
            return UnsafeHelper.AddByteOffset(
                in Log2DeBruijn32[0],
                index);
        }
    }

    private static class DeBruijn_StoreAsSpan
    {
        public static ReadOnlySpan<byte> TrailingZeroCountDeBruijn32 =>
        [
            00, 01, 28, 02, 29, 14, 24, 03,
            30, 22, 20, 15, 25, 17, 04, 08,
            31, 27, 13, 23, 21, 19, 16, 07,
            26, 12, 18, 06, 11, 05, 10, 09
        ];

        public static ReadOnlySpan<byte> Log2DeBruijn32 =>
        [
            00, 09, 01, 10, 13, 21, 02, 29,
            11, 14, 16, 18, 22, 25, 03, 30,
            08, 12, 20, 28, 15, 17, 24, 07,
            19, 27, 23, 06, 26, 05, 04, 31
        ];

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int TrailingZeroCount(uint value)
        {
            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            return UnsafeHelper.AddByteOffset(
                // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_0111_1100_1011_0101_0011_0001u
                in TrailingZeroCountDeBruijn32.GetPinnableReference(),
                // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
                (nuint)(int)(((value & (uint)-(int)value) * 0x077CB531u) >> 27)); // Multi-cast mitigates redundant conv.u8
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Log2(uint value)
        {
            // Fill trailing zeros with ones, eg 00010010 becomes 00011111
            value |= value >> 01;
            value |= value >> 02;
            value |= value >> 04;
            value |= value >> 08;
            value |= value >> 16;

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            return UnsafeHelper.AddByteOffset(
                // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_1100_0100_1010_1100_1101_1101u
                in Log2DeBruijn32.GetPinnableReference(),
                // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
                (uint)(int)((value * 0x07C4ACDDu) >> 27));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static byte QueryTrailingZeroCountTable(nuint index)
        {
            return UnsafeHelper.AddByteOffset(
                in TrailingZeroCountDeBruijn32.GetPinnableReference(),
                index);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static byte QueryLog2Table(nuint index)
        {
            return UnsafeHelper.AddByteOffset(
                in Log2DeBruijn32.GetPinnableReference(),
                index);
        }
    }
}