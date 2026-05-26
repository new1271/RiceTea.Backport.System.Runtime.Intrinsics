#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0
#if X86_ARCH || ANYCPU

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

using LocalsInit;

using RiceTea.Backport.Injection;
using RiceTea.Backport.Internals;

using Fallbacks = RiceTea.Backport.Fallbacks.X86.X86Base;

namespace System.Runtime.Intrinsics.X86;

[SuppressUnmanagedCodeSecurity]
unsafe partial class X86Base
{
    private static readonly bool _isSupported = PlatformHelper.IsX86;
    private static readonly bool _isUnix = PlatformHelper.IsUnix && (PlatformHelper.IsX64 || PlatformHelper.IsMono);
#if ANYCPU
    private static readonly bool _isX64 = PlatformHelper.IsX64;
#endif

    private static NativeFunctionHolder _cpuIdAsm = NativeFunctionHolder.Empty;

    public static partial bool IsSupported
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isSupported;
    }

    [LocalsInit(false)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (int Eax, int Ebx, int Ecx, int Edx) CpuId(int functionId, int subFunctionId)
    {
        if (!_isSupported)
            return ThrowUtils.ThrowPlatformNotSupported<(int Eax, int Ebx, int Ecx, int Edx)>();

        NativeFunctionHolder cpuIdAsm = _cpuIdAsm;
        if (cpuIdAsm == NativeFunctionHolder.Empty)
        {
            cpuIdAsm = BuildCpuIdAsm();
            _cpuIdAsm = cpuIdAsm;
        }

        Registers registers;
        using NativeFunctionAccessScope scope = cpuIdAsm.Enter();
        ((delegate* unmanaged[Cdecl]<Registers*, int, int, void>)scope.Address)(&registers, functionId, subFunctionId);
        return UnsafeHelper.As<Registers, (int Eax, int Ebx, int Ecx, int Edx)>(registers);
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)] // 避免尾呼叫優化
    public static partial uint BitScanForward(uint value)
    {
        if (!_isSupported)
            return ThrowUtils.ThrowPlatformNotSupported<uint>();

        CallSiteInjector.OnInjectStart(value);
        return CallSiteInjector.OnInjectEnd(Fallbacks.BitScanForward(value), &InjectBsfAsm);
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)] // 避免尾呼叫優化
    public static partial uint BitScanReverse(uint value)
    {
        if (!_isSupported)
            return ThrowUtils.ThrowPlatformNotSupported<uint>();

        CallSiteInjector.OnInjectStart(value);
        return CallSiteInjector.OnInjectEnd(Fallbacks.BitScanReverse(value), &InjectBsrAsm);
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)] // 避免尾呼叫優化
    private static int DivRem(Register64* pDividendOrRemainder, int divisor)
    {
        if (!_isSupported)
            return ThrowUtils.ThrowPlatformNotSupported<int>();

        CallSiteInjector.OnInjectStart((nuint)pDividendOrRemainder, divisor);
        return CallSiteInjector.OnInjectEnd(Fallback(pDividendOrRemainder, divisor), &InjectIDivAsm);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int Fallback(Register64* pDividendOrRemainder, int divisor)
        {
            int quotient = Fallbacks.DivRem(pDividendOrRemainder->Lower, pDividendOrRemainder->iUpper, divisor, out int rem);
            pDividendOrRemainder->Lower = (uint)rem;
            return quotient;
        }
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)] // 避免尾呼叫優化
    private static uint DivRem(Register64* pDividendOrRemainder, uint divisor)
    {
        if (!_isSupported)
            return ThrowUtils.ThrowPlatformNotSupported<uint>();

        CallSiteInjector.OnInjectStart((nuint)pDividendOrRemainder, divisor);
        return CallSiteInjector.OnInjectEnd(Fallback(pDividendOrRemainder, divisor), &InjectDivAsm);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint Fallback(Register64* pDividendOrRemainder, uint divisor)
        {
            uint quotient = Fallbacks.DivRem(pDividendOrRemainder->Lower, pDividendOrRemainder->uUpper, divisor, out uint rem);
            pDividendOrRemainder->Lower = rem;
            return quotient;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (int Quotient, int Remainder) DivRem(uint lower, int upper, int divisor)
    {
        Register64 register = new() { Lower = lower, iUpper = upper };
        int quotient = DivRem(&register, divisor);
        return (quotient, (int)register.Lower);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (uint Quotient, uint Remainder) DivRem(uint lower, uint upper, uint divisor)
    {
        Register64 register = new() { Lower = lower, uUpper = upper };
        uint quotient = DivRem(&register, divisor);
        return (quotient, register.Lower);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (nint Quotient, nint Remainder) DivRem(nuint lower, nint upper, nint divisor)
        => UnsafeHelper.PointerSizeConstant switch
        {
            sizeof(int) => DivRem((uint)lower, (int)upper, (int)divisor),
            sizeof(long) => UnsafeHelper.As<(long, long), (nint, nint)>(X64.DivRem(lower, upper, divisor)),
            _ => UnsafeHelper.PointerSize switch
            {
                sizeof(int) => DivRem((uint)lower, (int)upper, (int)divisor),
                sizeof(long) => UnsafeHelper.As<(long, long), (nint, nint)>(X64.DivRem(lower, upper, divisor)),
                _ => throw new PlatformNotSupportedException()
            }
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (nuint Quotient, nuint Remainder) DivRem(nuint lower, nuint upper, nuint divisor)
        => UnsafeHelper.PointerSizeConstant switch
        {
            sizeof(int) => DivRem((uint)lower, (uint)upper, (uint)divisor),
            sizeof(long) => UnsafeHelper.As<(ulong, ulong), (nuint, nuint)>(X64.DivRem(lower, upper, divisor)),
            _ => UnsafeHelper.PointerSize switch
            {
                sizeof(int) => DivRem((uint)lower, (uint)upper, (uint)divisor),
                sizeof(long) => UnsafeHelper.As<(ulong, ulong), (nuint, nuint)>(X64.DivRem(lower, upper, divisor)),
                _ => throw new PlatformNotSupportedException()
            }
        };

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)] // 避免尾呼叫優化
    public static partial void Pause()
    {
        if (!_isSupported)
        {
            ThrowUtils.ThrowPlatformNotSupported();
            return;
        }

        CallSiteInjector.OnInjectStart();
        Fallbacks.Pause();
        CallSiteInjector.OnInjectEnd(&InjectPauseAsm);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = sizeof(int) * 4)]
    private readonly struct Registers
    {
        private readonly int _eax, _ebx, _ecx, _edx;

        public override readonly string ToString()
            => $"{{EAX = {_eax}, EBX = {_ebx}, ECX = {_ecx}, EDX = {_edx}}}";
    }

    [StructLayout(LayoutKind.Explicit, Pack = 4, Size = sizeof(ulong))]
    private struct Register64
    {
        [FieldOffset(0)]
        public uint Lower;
        [FieldOffset(4)]
        public int iUpper;
        [FieldOffset(4)]
        public uint uUpper;
    }

    private static partial class Store { }
}
#endif
#endif