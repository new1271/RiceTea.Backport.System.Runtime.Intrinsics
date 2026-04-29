#if !NETSTANDARD2_1_OR_GREATER
#if !((X86_ARCH && B64_ARCH) || ANYCPU)
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;

namespace System.Runtime.Intrinsics.X86;

partial class X86Base
{
    unsafe partial class X64
    {
        public static partial bool IsSupported
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => false;
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
        public static partial ulong BitScanForward(ulong value) => ThrowUtils.ThrowPlatformNotSupported<ulong>();

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
        public static partial ulong BitScanReverse(ulong value) => ThrowUtils.ThrowPlatformNotSupported<ulong>();

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static partial (long Quotient, long Remainder) DivRem(ulong lower, long upper, long divisor) => ThrowUtils.ThrowPlatformNotSupported<(long Quotient, long Remainder)>();

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static partial (ulong Quotient, ulong Remainder) DivRem(ulong lower, ulong upper, ulong divisor) => ThrowUtils.ThrowPlatformNotSupported<(ulong Quotient, ulong Remainder)>();
    }
}
#endif
#endif