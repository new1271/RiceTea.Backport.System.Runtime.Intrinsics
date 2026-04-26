#if !NETSTANDARD2_1_OR_GREATER
using System.Runtime.Intrinsics.Helpers;

namespace System.Runtime.Intrinsics;

internal static partial class Fallbacks
{
    private static readonly bool _isSystemMemoryExists = SoftDependencyHelper.SystemMemoryExists;

    public static int TrailingZeroCountSoftwareFallback(uint value)
    {
        if (_isSystemMemoryExists)
            return DeBruijn_StoreAsSpan.TrailingZeroCount(value);
        else
            return DeBruijn_StoreAsArray.TrailingZeroCount(value);
    }

    internal static int Log2SoftwareFallback(uint value)
    {
        if (_isSystemMemoryExists)
            return DeBruijn_StoreAsSpan.Log2(value);
        else
            return DeBruijn_StoreAsArray.Log2(value);
    }

    internal static int PopCountSoftwareFallback(uint value)
    {
        const uint c1 = 0x_55555555u;
        const uint c2 = 0x_33333333u;
        const uint c3 = 0x_0F0F0F0Fu;
        const uint c4 = 0x_01010101u;

        value -= (value >> 1) & c1;
        value = (value & c2) + ((value >> 2) & c2);
        value = (((value + (value >> 4)) & c3) * c4) >> 24;

        return (int)value;
    }

    internal static int PopCountSoftwareFallback(ulong value)
    {
        const ulong c1 = 0x_55555555_55555555ul;
        const ulong c2 = 0x_33333333_33333333ul;
        const ulong c3 = 0x_0F0F0F0F_0F0F0F0Ful;
        const ulong c4 = 0x_01010101_01010101ul;

        value -= (value >> 1) & c1;
        value = (value & c2) + ((value >> 2) & c2);
        value = (((value + (value >> 4)) & c3) * c4) >> 56;

        return (int)value;
    }
}
#endif