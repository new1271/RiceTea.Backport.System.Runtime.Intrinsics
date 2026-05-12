using System;
using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace RiceTea.Backport.Fallbacks;

partial class Fallbacks
{
#if NETSTANDARD2_0
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetTrailingZeroCountTableReference()
            => ref UnsafeHelper.GetReference(TrailingZeroCountDeBruijn32);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetLog2TableReference()
            => ref UnsafeHelper.GetReference(Log2DeBruijn32);
    }
#endif

    private static class DeBruijn_StoreAsSpan
    {
        public static ReadOnlySpan<byte> TrailingZeroCountDeBruijn32 => new byte[sizeof(uint) * 8]
        {
            00, 01, 28, 02, 29, 14, 24, 03,
            30, 22, 20, 15, 25, 17, 04, 08,
            31, 27, 13, 23, 21, 19, 16, 07,
            26, 12, 18, 06, 11, 05, 10, 09
        };

        public static ReadOnlySpan<byte> Log2DeBruijn32 => new byte[sizeof(uint) * 8]
        {
            00, 09, 01, 10, 13, 21, 02, 29,
            11, 14, 16, 18, 22, 25, 03, 30,
            08, 12, 20, 28, 15, 17, 24, 07,
            19, 27, 23, 06, 26, 05, 04, 31
        };

        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetTrailingZeroCountTableReference()
            => ref UnsafeHelper.GetReference(TrailingZeroCountDeBruijn32);

        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetLog2TableReference()
            => ref UnsafeHelper.GetReference(Log2DeBruijn32);
    }
}