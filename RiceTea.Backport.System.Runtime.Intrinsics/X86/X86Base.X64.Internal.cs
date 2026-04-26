using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;

namespace System.Runtime.Intrinsics.X86;

partial class X86Base
{
    partial class X64
    {
        private static readonly bool _isSupported;

        static X64()
        {
            if (!PlatformHelper.IsX64)
                return;
            _isSupported = true;
        }

        public static partial bool IsSupported => _isSupported;

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static partial uint BitScanForward(ulong value)
        {
            if (!_isSupported)
                throw new PlatformNotSupportedException();

            InjectBsfAsm();

            uint lo = (uint)value;

            if (lo == 0)
                return 32 + (uint)Fallbacks.TrailingZeroCountSoftwareFallback((uint)(value >> 32));

            return (uint)Fallbacks.TrailingZeroCountSoftwareFallback(lo);
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static partial uint BitScanReverse(ulong value)
        {
            if (!_isSupported)
                throw new PlatformNotSupportedException();

            InjectBsrAsm();

            uint hi = (uint)(value >> 32);

            if (hi == 0)
                return (uint)Fallbacks.Log2SoftwareFallback((uint)value);

            return 32 + (uint)Fallbacks.Log2SoftwareFallback(hi);
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static partial (long Quotient, long Remainder) DivRem(ulong lower, long upper, long divisor)
        {
            if (!_isSupported)
                throw new PlatformNotSupportedException();

            InjectDiv128Asm();

            if (divisor == 0)
                throw new DivideByZeroException();

            if (upper == ((long)lower >> 63))
                return ((long)lower / divisor, (long)lower % divisor);

            bool isNegativeDividend = upper < 0;
            bool isNegativeDivisor = divisor < 0;
            bool isNegativeQuotient = isNegativeDividend ^ isNegativeDivisor;

            ulong uUpper = (ulong)upper;
            ulong uLower = lower;
            if (isNegativeDividend)
            {
                uLower = ~uLower;
                uUpper = ~uUpper;
                if (++uLower == 0) uUpper++;
            }
            ulong uDivisor = (ulong)(isNegativeDivisor ? -divisor : divisor);

            if (uUpper >= uDivisor) throw new OverflowException();

            (ulong uQ, ulong uR) = DivRem(uLower, uUpper, uDivisor);

            long q = (long)uQ;
            long r = (long)uR;

            if (isNegativeQuotient) q = -q;
            if (isNegativeDividend) r = -r;

            return (q, r);
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static partial (ulong Quotient, ulong Remainder) DivRem(ulong lower, ulong upper, ulong divisor)
        {
            if (!_isSupported)
                throw new PlatformNotSupportedException();

            InjectUDiv128Asm();

            if (divisor == 0)
                throw new DivideByZeroException();

            if (upper == 0)
                return (lower / divisor, lower % divisor);

            if (upper >= divisor)
                throw new OverflowException("Quotient is too large (Integer Overflow).");

            ulong quotient = 0;
            ulong remainder = upper;

            for (int i = 63; i >= 0; i--)
            {
                ulong bit = (lower >> i) & 1;
                remainder = (remainder << 1) | bit;

                if (remainder >= divisor)
                {
                    remainder -= divisor;
                    quotient |= (1UL << i);
                }
            }

            return (quotient, remainder);
        }

        private static partial class StoreAsArray { }

        private static partial class StoreAsSpan { }
    }
}