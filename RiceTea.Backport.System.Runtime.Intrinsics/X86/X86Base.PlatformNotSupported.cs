#if !NETSTANDARD2_1_OR_GREATER
#if !(X86_ARCH || ANYCPU)
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;

using LocalsInit;

namespace System.Runtime.Intrinsics.X86;

partial class X86Base
{
    public static partial bool IsSupported
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => false;
    }

    [LocalsInit(false)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (int Eax, int Ebx, int Ecx, int Edx) CpuId(int functionId, int subFunctionId) => ThrowUtils.ThrowPlatformNotSupported<(int Eax, int Ebx, int Ecx, int Edx)>();

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
    public static partial uint BitScanForward(uint value) => ThrowUtils.ThrowPlatformNotSupported<uint>();

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
    public static partial uint BitScanReverse(uint value) => ThrowUtils.ThrowPlatformNotSupported<uint>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (int Quotient, int Remainder) DivRem(uint lower, int upper, int divisor) => ThrowUtils.ThrowPlatformNotSupported<(int Quotient, int Remainder)>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (uint Quotient, uint Remainder) DivRem(uint lower, uint upper, uint divisor) => ThrowUtils.ThrowPlatformNotSupported<(uint Quotient, uint Remainder)>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (nint Quotient, nint Remainder) DivRem(nuint lower, nint upper, nint divisor) => ThrowUtils.ThrowPlatformNotSupported<(nint Quotient, nint Remainder)>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (nuint Quotient, nuint Remainder) DivRem(nuint lower, nuint upper, nuint divisor) => ThrowUtils.ThrowPlatformNotSupported<(nuint Quotient, nuint Remainder)>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial void Pause() => ThrowUtils.ThrowPlatformNotSupported();
}
#endif
#endif