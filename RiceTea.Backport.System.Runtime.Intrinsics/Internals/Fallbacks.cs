#if !NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;

namespace System.Runtime.Intrinsics;

internal static partial class Fallbacks
{
    private static readonly bool _isSystemMemoryExists = SoftDependencyHelper.SystemMemoryExists;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong BitScanForward(ulong value)
    {
        uint lo = (uint)value;

        if (lo == 0)
            return 32 + BitScanForward((uint)(value >> 32));

        return BitScanForward(lo);
    }

    public static uint BitScanForward(uint value)
    {
        if (_isSystemMemoryExists)
            return (uint)DeBruijn_StoreAsSpan.TrailingZeroCount(value);
        else
            return (uint)DeBruijn_StoreAsArray.TrailingZeroCount(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong BitScanReverse(ulong value)
    {
        uint hi = (uint)(value >> 32);

        if (hi == 0)
            return BitScanReverse((uint)value);

        return 32 + BitScanReverse(hi);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint BitScanReverse(uint value)
    {
        if (_isSystemMemoryExists)
            return (uint)DeBruijn_StoreAsSpan.Log2(value);
        else
            return (uint)DeBruijn_StoreAsArray.Log2(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong LeadingZeroCount(ulong value)
    {
        uint hi = (uint)(value >> 32);

        if (hi == 0)
            return 32 + (31 ^ BitScanReverse((uint)value));

        return 31 ^ BitScanReverse(hi);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint LeadingZeroCount(uint value)
        => 31u ^ BitScanReverse(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint PopCount(uint value)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong PopCount(ulong value)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int DivRem(int upper, uint lower, int divisor, out int remainder)
    {
        if (divisor == 0)
        {
            remainder = 0;
            return ThrowDivideByZeroException<int>();
        }
        if (upper == 0)
        {
            if (((int)lower) < 0)
                goto LongOperation;
            goto ShortOperation;
        }
        if (upper == -1)
        {
            if (((int)lower) > 0)
                goto LongOperation;
            goto ShortOperation;
        }
        goto LongOperation;

    ShortOperation:
        remainder = ((int)lower) % divisor;
        return ((int)lower) / divisor;

    LongOperation:
        return DivRem_Long(upper, lower, divisor, out remainder);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long DivRem(long upper, ulong lower, long divisor, out long remainder)
    {
        if (DivRem_FastCheck(upper, lower, divisor, out long quotient, out remainder))
            return quotient;
        
        switch (upper)
        {
            case 0:
                if (((long)lower) < 0)
                    goto LongOperation;
                goto ShortOperation;
            case -1:
                if (((long)lower) > 0)
                    goto LongOperation;
                goto ShortOperation;
        }

        goto LongOperation;

    ShortOperation:
        remainder = ((long)lower) % divisor;
        return ((long)lower) / divisor;

    LongOperation:
        return DivRem_Long(upper, lower, divisor, out remainder);
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // 避免影響 DivRem 方法體預估長度，導致無法內聯
    private static bool DivRem_FastCheck(long upper, ulong lower, long divisor, out long quotient, out long remainder)
    {
        switch (divisor)
        {
            case 0:
                quotient = 0;
                remainder = 0;
                ThrowDivideByZeroException<int>();
                return true;
            case 1:
                remainder = 0;
                switch (upper)
                {
                    case 0:
                        if ((long)lower >= 0)
                        {
                            quotient = (long)lower;
                            return true;
                        }
                        break;
                    case -1:
                        if ((long)lower < 0)
                        {
                            quotient = (long)lower;
                            return true;
                        }
                        break;
                }
                quotient = 0;
                ThrowArthimeticException<long>();
                return true;
            case -1:
                remainder = 0;
                switch (upper)
                {
                    case 0:
                        if ((long)lower >= 0)
                        {
                            quotient = -(long)lower;
                            return true;
                        }
                        break;
                    case -1:
                        if ((long)lower < 0 && lower != unchecked((ulong)long.MinValue))
                        {
                            quotient = -(long)lower;
                            return true;
                        }
                        break;
                }
                quotient = 0;
                ThrowArthimeticException<long>();
                return true;
        }

        remainder = 0;
        quotient = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int DivRem_Long(int upper, uint lower, int divisor, out int remainder)
    {
        long dividend = ((long)upper) << 32 | lower;
        long quotient = dividend % divisor;
        if (quotient > int.MaxValue || quotient < int.MinValue)
        {
            remainder = 0;
            return ThrowArthimeticException<int>();
        }
        else
        {
            remainder = (int)(dividend % divisor);
            return (int)quotient;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long DivRem_Long(long upper, ulong lower, long divisor, out long remainder)
    {
        unchecked
        {
            bool outputNegative, dividedNegative;
            if (upper < 0)
            {
                lower = 0 - lower;
                upper = (~upper + (lower == 0 ? 1 : 0));

                outputNegative = true;
                dividedNegative = true;
            }
            else
            {
                outputNegative = false;
                dividedNegative = false;
            }

            if (divisor < 0)
            {
                divisor = -divisor;
                outputNegative = !outputNegative;
            }

            // 此處狀態:
            // upper 恆正 (SignBit == 0)
            // lower 恆正，有機會全1，但依舊有效
            // divisor 恆正 (-2^63 此時應視為 2^63)

            ulong unsignedQuotient = DivRem_Long((ulong)upper, lower, (ulong)divisor, out ulong unsignedRemainder);
            if ((unsignedQuotient > long.MaxValue) || unsignedRemainder > long.MaxValue) // 商為 2^63 之情況已被去除，因此此處可以簡化檢查
                goto Overflow;

            remainder = (long)unsignedRemainder;
            if (dividedNegative)
                remainder = -remainder;
            if (outputNegative)
                unsignedQuotient = 0 - unsignedQuotient;

            return (long)unsignedQuotient;
        }

    Overflow:
        remainder = 0;
        return ThrowArthimeticException<long>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ulong DivRem_Long(ulong upper, ulong lower, ulong divisor, out ulong remainder)
    {
        ulong upperQuotient = DivRem(upper, divisor, out ulong upperRemainder); // Shift 64
        if (upperQuotient != 0)
            goto Overflow;

        ulong middleQuotient = DivRem(upperRemainder << 32 | (lower >>> 32), divisor, out ulong middleRemainder); // Shift 32
        if (middleQuotient > uint.MaxValue)
            goto Overflow;

        ulong lowerQuotient = DivRem(middleRemainder << 32 | (lower & uint.MaxValue), divisor, out remainder);

        return lowerQuotient + (middleQuotient << 32);

    Overflow:
        remainder = 0;
        return ThrowArthimeticException<ulong>();

        static ulong DivRem(ulong a, ulong b, out ulong remainder)
        {
            remainder = a % b;
            return a / b;
        }
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T ThrowDivideByZeroException<T>() => throw new DivideByZeroException();

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T ThrowArthimeticException<T>() => throw new ArithmeticException("Overflow or underflow in the arithmetic operation");
}
#endif