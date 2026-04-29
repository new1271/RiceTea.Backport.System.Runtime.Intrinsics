#if !NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Helpers;
using System.Runtime.Intrinsics.Internals;
using System.Security;
using System.Threading;

using InlineIL;

using LocalsInit;

namespace System.Runtime.Intrinsics.X86;

[SuppressUnmanagedCodeSecurity]
unsafe partial class X86Base
{
    private static readonly object? _bsfLock, _bsrLock;
    private static readonly void* _cpuIdAsm, _div64Asm, _udiv64Asm;
    private static readonly bool _isSupported;

    static X86Base()
    {
        if (PlatformHelper.IsX86)
        {
            _isSupported = true;
            _cpuIdAsm = BuildCpuIdAsm();
            _div64Asm = BuildDiv64Asm();
            _udiv64Asm = BuildUDiv64Asm();
            _bsfLock = new object();
            _bsrLock = new object();
        }
        else
        {
            _isSupported = false;
            _cpuIdAsm = null;
            _div64Asm = null;
            _udiv64Asm = null;
            _bsfLock = null;
            _bsrLock = null;
        }
    }

    public static partial bool IsSupported => _isSupported;

    [LocalsInit(false)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (int Eax, int Ebx, int Ecx, int Edx) CpuId(int functionId, int subFunctionId)
    {
        if (!_isSupported)
            throw new PlatformNotSupportedException();

        Registers registers;
        ((delegate* unmanaged[Cdecl]<Registers*, int, int, void>)_cpuIdAsm)(&registers, functionId, subFunctionId);
        return UnsafeHelper.As<Registers, (int Eax, int Ebx, int Ecx, int Edx)>(registers);
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
    public static partial uint BitScanForward(uint value)
    {
        if (!_isSupported)
            throw new PlatformNotSupportedException();

        BitScanForward_InjectStart(value);
        return BitScanForward_InjectEnd(Fallbacks.BitScanForward(value));
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
    private static void BitScanForward_InjectStart(uint value)
    {
        CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
        BitScanForward_EnterLock();
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static uint BitScanForward_InjectEnd(uint value)
    {
        try
        {
            CallSiteInjector.InjectAsm(
                startAddress: CallSiteInjector.StartAddress,
                endAddress: CallSiteInjector.FindCallSite(),
                injectorFunc: &InjectBsfAsm,
                exitLockFunc: &BitScanForward_ExitLock);
            return value;
        }
        finally
        {
            BitScanForward_ExitLock();
        }
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void BitScanForward_EnterLock() => Monitor.Enter(_bsfLock!);

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void BitScanForward_ExitLock()
    {
        try
        {
            Monitor.Exit(_bsfLock!);
        }
        catch (SynchronizationLockException)
        {
        }
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
    public static partial uint BitScanReverse(uint value)
    {
        if (!_isSupported)
            throw new PlatformNotSupportedException();

        BitScanReverse_InjectStart(value);
        return BitScanReverse_InjectEnd(Fallbacks.BitScanForward(value));
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
    private static void BitScanReverse_InjectStart(uint value)
    {
        CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
        BitScanReverse_EnterLock();
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static uint BitScanReverse_InjectEnd(uint value)
    {
        try
        {
            CallSiteInjector.InjectAsm(
                startAddress: CallSiteInjector.StartAddress,
                endAddress: CallSiteInjector.FindCallSite(),
                injectorFunc: &InjectBsrAsm,
                exitLockFunc: &BitScanReverse_ExitLock);
            return value;
        }
        finally
        {
            BitScanReverse_ExitLock();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void BitScanReverse_EnterLock() => Monitor.Enter(_bsrLock!);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void BitScanReverse_ExitLock()
    {
        try
        {
            Monitor.Exit(_bsrLock!);
        }
        catch (SynchronizationLockException)
        {
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (int Quotient, int Remainder) DivRem(long dividend, int divisor)
    {
        if (!_isSupported)
            throw new PlatformNotSupportedException();

        int remainder;
        int quotient = ((delegate* unmanaged[Cdecl]<long, int, int*, int>)_div64Asm)(dividend, divisor, &remainder);
        return (quotient, remainder);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (uint Quotient, uint Remainder) DivRem(ulong dividend, uint divisor)
    {
        if (!_isSupported)
            throw new PlatformNotSupportedException();

        uint remainder;
        uint quotient = ((delegate* unmanaged[Cdecl]<ulong, uint, uint*, uint>)_udiv64Asm)(dividend, divisor, &remainder);
        return (quotient, remainder);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (int Quotient, int Remainder) DivRem(uint lower, int upper, int divisor)
        => DivRem(unchecked((long)((ulong)upper << 32 | lower)), divisor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (uint Quotient, uint Remainder) DivRem(uint lower, uint upper, uint divisor)
        => DivRem(unchecked((ulong)upper << 32 | lower), divisor);

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

    private static partial class StoreAsArray { }

    private static partial class StoreAsSpan { }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = sizeof(int) * 4)]
    private readonly struct Registers
    {
        private readonly int _eax, _ebx, _ecx, _edx;

        public override readonly string ToString()
            => $"{{EAX = {_eax}, EBX = {_ebx}, ECX = {_ecx}, EDX = {_edx}}}";
    }
}
#endif