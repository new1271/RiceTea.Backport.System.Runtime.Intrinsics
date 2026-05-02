#if !NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;

namespace System.Runtime.Intrinsics;

internal static partial class Fallbacks
{
    private static readonly bool _isSystemMemoryExists = SoftDependencyHelper.SystemMemoryExists;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong BitScanForward(ulong value)
    {
        if (value == 0UL)
            return sizeof(ulong) * 8;
        return TrailingZeroCount(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint BitScanForward(uint value)
    {
        if (value == 0U)
            return sizeof(uint) * 8;
        return TrailingZeroCount(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong BitScanReverse(ulong value)
    {
        if (value == 0UL)
            return ulong.MaxValue;
        return Log2(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint BitScanReverse(uint value)
    {
        if (value == 0U)
            return uint.MaxValue;
        return Log2(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong TrailingZeroCount(ulong value)
    {
        uint lo = (uint)value;

        if (lo == 0)
            return 32 + TrailingZeroCount((uint)(value >> 32));

        return TrailingZeroCount(lo);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint TrailingZeroCount(uint value)
    {
        // uint.MaxValue >> 27 is always in range [0 - 31]
        // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
        nuint index = (nuint)(int)(((value & (uint)-(int)value) * 0x077CB531u) >> 27);
        // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_1100_0100_1010_1100_1101_1101u
        if (_isSystemMemoryExists)
            return DeBruijn_StoreAsSpan.QueryTrailingZeroCountTable(index);
        else
            return DeBruijn_StoreAsArray.QueryTrailingZeroCountTable(index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Log2(ulong value)
    {
        uint hi = (uint)(value >> 32);

        if (hi == 0)
            return Log2((uint)value);

        return 32 + Log2(hi);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Log2(uint value)
    {            
        // Fill trailing zeros with ones, eg 00010010 becomes 00011111
        value |= value >> 01;
        value |= value >> 02;
        value |= value >> 04;
        value |= value >> 08;
        value |= value >> 16;

        // uint.MaxValue >> 27 is always in range [0 - 31]
        // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
        nuint index = (nuint)(int)((value * 0x07C4ACDDu) >> 27);

        // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_1100_0100_1010_1100_1101_1101u
        if (_isSystemMemoryExists)
            return DeBruijn_StoreAsSpan.QueryLog2Table(index);
        else
            return DeBruijn_StoreAsArray.QueryLog2Table(index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong LeadingZeroCount(ulong value)
    {
        uint hi = (uint)(value >> 32);

        if (hi == 0)
            return 32 + (31 ^ Log2((uint)value));

        return 31 ^ Log2(hi);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint LeadingZeroCount(uint value)
        => 31u ^ Log2(value);

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
    public static int DivRem(uint lower, int upper, int divisor, out int remainder)
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
        return DivRem_Long(lower, upper, divisor, out remainder);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint DivRem(uint lower, uint upper, uint divisor, out uint remainder)
    {
        if (divisor == 0)
        {
            remainder = 0;
            return ThrowDivideByZeroException<uint>();
        }
        if (upper == 0)
        {
            remainder = lower % divisor;
            return lower / divisor;
        }

        return DivRem_Long(lower, upper, divisor, out remainder);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long DivRem(ulong lower, long upper, long divisor, out long remainder)
    {
        if (DivRem_FastCheck(lower, upper, divisor, out long quotient, out remainder))
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
        return DivRem_Long(lower, upper, divisor, out remainder);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong DivRem(ulong lower, ulong upper, ulong divisor, out ulong remainder)
    {
        if (DivRem_FastCheck(lower, upper, divisor, out ulong quotient, out remainder))
            return quotient;
        
        if (upper == 0)
        {
            remainder = lower % divisor;
            return lower / divisor;
        }

        return DivRem_Long(lower, upper, divisor, out remainder);
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // 避免影響 DivRem 方法體預估長度，導致無法內聯
    private static bool DivRem_FastCheck(ulong lower, long upper, long divisor, out long quotient, out long remainder)
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

    [MethodImpl(MethodImplOptions.NoInlining)] // 避免影響 DivRem 方法體預估長度，導致無法內聯
    private static bool DivRem_FastCheck(ulong lower, ulong upper, ulong divisor, out ulong quotient, out ulong remainder)
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
                if (upper == 0)
                    quotient = lower;
                else
                    quotient = ThrowArthimeticException<ulong>();
                return true;
        }

        remainder = 0;
        quotient = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int DivRem_Long(uint lower, int upper, int divisor, out int remainder)
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
    private static uint DivRem_Long(uint lower, uint upper, uint divisor, out uint remainder)
    {
        ulong dividend = ((ulong)upper) << 32 | lower;
        ulong quotient = dividend % divisor;
        if (quotient > uint.MaxValue)
        {
            remainder = 0;
            return ThrowArthimeticException<uint>();
        }
        else
        {
            remainder = (uint)(dividend % divisor);
            return (uint)quotient;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long DivRem_Long(ulong lower, long upper, long divisor, out long remainder)
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

            ulong unsignedQuotient = DivRem_Long(lower, (ulong)upper, (ulong)divisor, out ulong unsignedRemainder);
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
    private static ulong DivRem_Long(ulong lower, ulong upper, ulong divisor, out ulong remainder)
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