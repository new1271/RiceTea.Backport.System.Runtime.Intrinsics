#if !NETSTANDARD2_1_OR_GREATER
using System.Runtime.Intrinsics.Helpers;

namespace System.Runtime.Intrinsics;

internal static partial class Fallbacks
{
    private static readonly bool _isSystemMemoryExists = SoftDependencyHelper.SystemMemoryExists;

    public static ulong BitScanForwardSoftwareFallback(ulong value)
    {
        uint lo = (uint)value;

        if (lo == 0)
            return 32 + BitScanForwardSoftwareFallback((uint)(value >> 32));

        return BitScanForwardSoftwareFallback(lo);
    }

    public static uint BitScanForwardSoftwareFallback(uint value)
    {
        if (_isSystemMemoryExists)
            return (uint)DeBruijn_StoreAsSpan.TrailingZeroCount(value);
        else
            return (uint)DeBruijn_StoreAsArray.TrailingZeroCount(value);
    }

    public static ulong LeadingZeroCountSoftwareFallback(ulong value)
    {
        uint hi = (uint)(value >> 32);

        if (hi == 0)
            return 32 + (31 ^ BitScanReverseSoftwareFallback((uint)value));

        return 31 ^ BitScanReverseSoftwareFallback(hi);
    }

    public static uint LeadingZeroCountSoftwareFallback(uint value)
        => 31u ^ BitScanReverseSoftwareFallback(value);

    internal static ulong BitScanReverseSoftwareFallback(ulong value)
    {
        uint hi = (uint)(value >> 32);

        if (hi == 0)
            return BitScanReverseSoftwareFallback((uint)value);

        return 32 + BitScanReverseSoftwareFallback(hi);
    }

    internal static uint BitScanReverseSoftwareFallback(uint value)
    {
        if (_isSystemMemoryExists)
            return (uint)DeBruijn_StoreAsSpan.Log2(value);
        else
            return (uint)DeBruijn_StoreAsArray.Log2(value);
    }

    internal static uint PopCountSoftwareFallback(uint value)
    {
        const uint c1 = 0x_55555555u;
        const uint c2 = 0x_33333333u;
        const uint c3 = 0x_0F0F0F0Fu;
        const uint c4 = 0x_01010101u;

        value -= (value >> 1) & c1;
        value = (value & c2) + ((value >> 2) & c2);
        value = (((value + (value >> 4)) & c3) * c4) >> 24;

        return value;
    }

    internal static ulong PopCountSoftwareFallback(ulong value)
    {
        const ulong c1 = 0x_55555555_55555555ul;
        const ulong c2 = 0x_33333333_33333333ul;
        const ulong c3 = 0x_0F0F0F0F_0F0F0F0Ful;
        const ulong c4 = 0x_01010101_01010101ul;

        value -= (value >> 1) & c1;
        value = (value & c2) + ((value >> 2) & c2);
        value = (((value + (value >> 4)) & c3) * c4) >> 56;

        return (uint)value;
    }
}
#endif