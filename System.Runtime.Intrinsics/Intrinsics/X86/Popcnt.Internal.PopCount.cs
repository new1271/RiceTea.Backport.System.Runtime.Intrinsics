#if NETSTANDARD2_0_OR_GREATER
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

using SpanDissolve;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class Popcnt
{
    private const int PopcntLength_Windows = 4;
#if B32_ARCH || ANYCPU
    private const int PopcntLength_Unix_X86 = 6;
#endif
#if B64_ARCH || ANYCPU
    private const int PopcntLength_Unix_X64 = 4;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectPopcntAsm(ref void* destination, ref uint length)
    {
        if (_isUnix)
        {
#if B64_ARCH
            InjectPopcntAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
            InjectPopcntAsm_Unix_X86(ref destination, ref length);
#else
            if (_isX64)
                InjectPopcntAsm_Unix_X64(ref destination, ref length);
            else
                InjectPopcntAsm_Unix_X86(ref destination, ref length);
#endif
        }
        else
            InjectPopcntAsm_Windows(ref destination, ref length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectPopcntAsm_Windows(ref void* destination, ref uint length)
    {
        const int Length = PopcntLength_Windows;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.PopcntData_Windows, Length);
    }

#if B32_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectPopcntAsm_Unix_X86(ref void* destination, ref uint length)
    {
        const int Length = PopcntLength_Unix_X86;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.PopcntData_Unix_X86, Length);
    }
#endif

#if B64_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectPopcntAsm_Unix_X64(ref void* destination, ref uint length)
    {
        const int Length = PopcntLength_Unix_X64;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.PopcntData_Unix_X64, Length);
    }
#endif

    partial class Store
    {
        public static ref readonly byte PopcntData_Windows => ref SpanDissolver.Dissolve(new byte[PopcntLength_Windows]
        {
            0xF3, 0x0F, 0xB8, 0xC1 // popcnt eax, ecx
        });
#if B32_ARCH || ANYCPU
        public static ref readonly byte PopcntData_Unix_X86 => ref SpanDissolver.Dissolve(new byte[PopcntLength_Unix_X86]
        {
            0xF3, 0x0F, 0xB8, 0x44, 0x24, 0x04 // popcnt eax, dword ptr [esp+4]
        });
#endif
#if B64_ARCH || ANYCPU
        public static ref readonly byte PopcntData_Unix_X64 => ref SpanDissolver.Dissolve(new byte[PopcntLength_Unix_X64]
        {
            0xF3, 0x0F, 0xB8, 0xC7 // popcnt eax, edi
        });
#endif
    }
}
#endif
#endif