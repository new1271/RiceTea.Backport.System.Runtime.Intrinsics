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
#if NETSTANDARD2_0
    private static readonly bool _spanExists = SoftDependencyHelper.SystemMemoryExists;
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
            ThrowUtils.ThrowPlatformNotSupported();

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
            ThrowUtils.ThrowPlatformNotSupported();

        InjectStart(value);
        return InjectEnd(Fallbacks.BitScanForward(value));

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void InjectStart(uint value)
        {
            void* address = CallSiteInjector.FindCallSite();
            ThreadStatics.StartAddress = address;
            CallSiteInjector.EnterAddressLock(address);
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static uint InjectEnd(uint value)
        {
            try
            {
                CallSiteInjector.Inject(
                    startAddress: ThreadStatics.StartAddress,
                    endAddress: CallSiteInjector.FindCallSite(),
                    injectorFunc: &InjectBsfAsm,
                    exitLockFunc: &ExitLock);
                return value;
            }
            finally
            {
                ExitLock();
            }
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ExitLock() => CallSiteInjector.LeaveAddressLock(ThreadStatics.StartAddress);
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)] // 避免尾呼叫優化
    public static partial uint BitScanReverse(uint value)
    {
        if (!_isSupported)
            ThrowUtils.ThrowPlatformNotSupported();

        InjectStart(value);
        return InjectEnd(Fallbacks.BitScanReverse(value));

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void InjectStart(uint value)
        {
            void* address = CallSiteInjector.FindCallSite();
            ThreadStatics.StartAddress = address;
            CallSiteInjector.EnterAddressLock(address);
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static uint InjectEnd(uint value)
        {
            try
            {
                CallSiteInjector.Inject(
                    startAddress: ThreadStatics.StartAddress,
                    endAddress: CallSiteInjector.FindCallSite(),
                    injectorFunc: &InjectBsrAsm,
                    exitLockFunc: &ExitLock);
                return value;
            }
            finally
            {
                ExitLock();
            }
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ExitLock() => CallSiteInjector.LeaveAddressLock(ThreadStatics.StartAddress);
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)] // 避免尾呼叫優化
    private static int DivRem(uint lower, int upper, int divisor, out int rem)
    {
        if (!_isSupported)
            ThrowUtils.ThrowPlatformNotSupported();

        InjectStart(lower, upper, divisor, out rem);
        return InjectEnd(Fallbacks.DivRem(lower, upper, divisor, out rem));

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void InjectStart(uint lower, int upper, int divisor, out int rem)
        {
            rem = 0;
            void* address = CallSiteInjector.FindCallSite();
            ThreadStatics.StartAddress = address;
            CallSiteInjector.EnterAddressLock(address);
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static int InjectEnd(int value)
        {
            try
            {
                CallSiteInjector.Inject(
                    startAddress: ThreadStatics.StartAddress,
                    endAddress: CallSiteInjector.FindCallSite(),
                    injectorFunc: &InjectIDivAsm,
                    exitLockFunc: &ExitLock);
                return value;
            }
            finally
            {
                ExitLock();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ExitLock() => CallSiteInjector.LeaveAddressLock(ThreadStatics.StartAddress);
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)] // 避免尾呼叫優化
    private static uint DivRem(uint lower, uint upper, uint divisor, out uint rem)
    {
        if (!_isSupported)
            ThrowUtils.ThrowPlatformNotSupported();

        InjectStart(lower, upper, divisor, out rem);
        return InjectEnd(Fallbacks.DivRem(lower, upper, divisor, out rem));

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void InjectStart(uint lower, uint upper, uint divisor, out uint rem)
        {
            rem = 0;
            void* address = CallSiteInjector.FindCallSite();
            ThreadStatics.StartAddress = address;
            CallSiteInjector.EnterAddressLock(address);
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static uint InjectEnd(uint value)
        {
            try
            {
                CallSiteInjector.Inject(
                    startAddress: ThreadStatics.StartAddress,
                    endAddress: CallSiteInjector.FindCallSite(),
                    injectorFunc: &InjectDivAsm,
                    exitLockFunc: &ExitLock);
                return value;
            }
            finally
            {
                ExitLock();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ExitLock() => CallSiteInjector.LeaveAddressLock(ThreadStatics.StartAddress);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (int Quotient, int Remainder) DivRem(uint lower, int upper, int divisor)
    {
        int quotient = DivRem(lower, upper, divisor, out int remainder);
        return (quotient, remainder);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (uint Quotient, uint Remainder) DivRem(uint lower, uint upper, uint divisor)
    {
        uint quotient = DivRem(lower, upper, divisor, out uint remainder);
        return (quotient, remainder);
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
            ThrowUtils.ThrowPlatformNotSupported();

        InjectStart();
        Fallbacks.Pause();
        InjectEnd();

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void InjectStart()
        {
            void* address = CallSiteInjector.FindCallSite();
            ThreadStatics.StartAddress = address;
            CallSiteInjector.EnterAddressLock(address);
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void InjectEnd()
        {
            try
            {
                CallSiteInjector.Inject(
                    startAddress: ThreadStatics.StartAddress,
                    endAddress: CallSiteInjector.FindCallSite(),
                    injectorFunc: &InjectPauseAsm,
                    exitLockFunc: &ExitLock);
            }
            finally
            {
                ExitLock();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ExitLock() => CallSiteInjector.LeaveAddressLock(ThreadStatics.StartAddress);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = sizeof(int) * 4)]
    private readonly struct Registers
    {
        private readonly int _eax, _ebx, _ecx, _edx;

        public override readonly string ToString()
            => $"{{EAX = {_eax}, EBX = {_ebx}, ECX = {_ecx}, EDX = {_edx}}}";
    }

#if NETSTANDARD2_0
    private static partial class StoreAsArray { }
#endif

    private static partial class StoreAsSpan { }
}
#endif
#endif