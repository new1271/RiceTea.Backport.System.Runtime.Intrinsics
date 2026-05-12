using System;
using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace RiceTea.Backport.Fallbacks;

partial class Fallbacks
{
#if NETSTANDARD2_0
    private static class DeBruijn_StoreAsArray
    {        
        private static readonly byte[] TrailingZeroCount_32 = new byte[sizeof(uint) * 8]
        {
            00, 01, 28, 02, 29, 14, 24, 03,
            30, 22, 20, 15, 25, 17, 04, 08,
            31, 27, 13, 23, 21, 19, 16, 07,
            26, 12, 18, 06, 11, 05, 10, 09
        };

        private static readonly byte[] Log2_32 = new byte[sizeof(uint) * 8]
        {
            00, 09, 01, 10, 13, 21, 02, 29,
            11, 14, 16, 18, 22, 25, 03, 30,
            08, 12, 20, 28, 15, 17, 24, 07,
            19, 27, 23, 06, 26, 05, 04, 31
        };

#if B64_ARCH || ANYCPU
        private static readonly byte[] TrailingZeroCount_64 = new byte[sizeof(ulong) * 8]
        {
            00, 01, 02, 53, 03, 07, 54, 27, 04, 38, 41, 08, 34, 55, 48, 28,
            62, 05, 39, 46, 44, 42, 22, 09, 24, 35, 59, 56, 49, 18, 29, 11,
            63, 52, 06, 26, 37, 40, 33, 47, 61, 45, 43, 21, 23, 58, 17, 10,
            51, 25, 36, 32, 60, 20, 57, 16, 50, 31, 19, 15, 30, 14, 13, 12
        };

        private static readonly byte[] Log2_64 = new byte[sizeof(ulong) * 8]
        {
            00, 58, 01, 59, 47, 53, 02, 60, 39, 48, 27, 54, 33, 42, 03, 61,
            51, 37, 40, 49, 18, 28, 20, 55, 30, 34, 11, 43, 14, 22, 04, 62,
            57, 46, 52, 38, 26, 32, 41, 50, 36, 17, 19, 29, 10, 13, 21, 56,
            45, 25, 31, 35, 16, 09, 12, 44, 24, 15, 08, 23, 07, 06, 05, 63
        };
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetTrailingZeroCountTableReference_32()
            => ref UnsafeHelper.GetReference(TrailingZeroCount_32);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetLog2TableReference_32()
            => ref UnsafeHelper.GetReference(Log2_32);

#if B64_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetTrailingZeroCountTableReference_64()
            => ref UnsafeHelper.GetReference(TrailingZeroCount_64);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetLog2TableReference_64()
            => ref UnsafeHelper.GetReference(Log2_64);
#endif
    }
#endif

    private static class DeBruijn_StoreAsSpan
    {
        private static ReadOnlySpan<byte> TrailingZeroCount_32 => new byte[sizeof(uint) * 8]
        {
            00, 01, 28, 02, 29, 14, 24, 03,
            30, 22, 20, 15, 25, 17, 04, 08,
            31, 27, 13, 23, 21, 19, 16, 07,
            26, 12, 18, 06, 11, 05, 10, 09
        };

        private static ReadOnlySpan<byte> Log2_32 => new byte[sizeof(uint) * 8]
        {
            00, 09, 01, 10, 13, 21, 02, 29,
            11, 14, 16, 18, 22, 25, 03, 30,
            08, 12, 20, 28, 15, 17, 24, 07,
            19, 27, 23, 06, 26, 05, 04, 31
        };

#if B64_ARCH || ANYCPU
        private static ReadOnlySpan<byte> TrailingZeroCount_64 => new byte[sizeof(ulong) * 8]
        {
            00, 01, 02, 53, 03, 07, 54, 27, 04, 38, 41, 08, 34, 55, 48, 28,
            62, 05, 39, 46, 44, 42, 22, 09, 24, 35, 59, 56, 49, 18, 29, 11,
            63, 52, 06, 26, 37, 40, 33, 47, 61, 45, 43, 21, 23, 58, 17, 10,
            51, 25, 36, 32, 60, 20, 57, 16, 50, 31, 19, 15, 30, 14, 13, 12
        };

        private static ReadOnlySpan<byte> Log2_64 => new byte[sizeof(ulong) * 8]
        {
            00, 58, 01, 59, 47, 53, 02, 60, 39, 48, 27, 54, 33, 42, 03, 61,
            51, 37, 40, 49, 18, 28, 20, 55, 30, 34, 11, 43, 14, 22, 04, 62,
            57, 46, 52, 38, 26, 32, 41, 50, 36, 17, 19, 29, 10, 13, 21, 56,
            45, 25, 31, 35, 16, 09, 12, 44, 24, 15, 08, 23, 07, 06, 05, 63
        };
#endif

        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetTrailingZeroCountTableReference_32()
            => ref UnsafeHelper.GetReference(TrailingZeroCount_32);

        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetLog2TableReference_32()
            => ref UnsafeHelper.GetReference(Log2_32);

#if B64_ARCH || ANYCPU
        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetTrailingZeroCountTableReference_64()
            => ref UnsafeHelper.GetReference(TrailingZeroCount_64);

        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetLog2TableReference_64()
            => ref UnsafeHelper.GetReference(Log2_64);
#endif
    }
}