using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Helpers;
using System.Security;

using LocalsInit;

namespace System.Runtime.Intrinsics.X86;

[SuppressUnmanagedCodeSecurity]
unsafe partial class X86Base
{
    private static readonly bool _isSupported;
    private static readonly void* _cpuIdAsm, _div64Asm, _udiv64Asm;

    static X86Base()
    {
        if (!PlatformHelper.IsX86)
            return;
        _isSupported = true;
        _cpuIdAsm = BuildCpuIdAsm();
        _div64Asm = BuildDiv64Asm();
        _udiv64Asm = BuildUDiv64Asm();
    }

    public static partial bool IsSupported => _isSupported;

    [LocalsInit(false)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (int Eax, int Ebx, int Ecx, int Edx) CpuId(int functionId, int subFunctionId)
    {
        Registers registers;
        ((delegate* unmanaged[Cdecl]<Registers*, int, int, void>)_cpuIdAsm)(&registers, functionId, subFunctionId);
        return UnsafeHelper.As<Registers, (int Eax, int Ebx, int Ecx, int Edx)>(registers);
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static partial uint BitScanForward(uint value)
    {
        if (!_isSupported)
            throw new PlatformNotSupportedException();

        InjectBsfAsm();

        return (uint)Fallbacks.TrailingZeroCountSoftwareFallback(value);
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static partial uint BitScanReverse(uint value)
    {
        if (!_isSupported)
            throw new PlatformNotSupportedException();

        InjectBsrAsm();

        return (uint)Fallbacks.Log2SoftwareFallback(value);
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