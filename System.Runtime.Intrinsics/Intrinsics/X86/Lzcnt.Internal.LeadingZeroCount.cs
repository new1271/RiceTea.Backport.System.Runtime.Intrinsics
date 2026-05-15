#if NETSTANDARD2_0_OR_GREATER
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

using SpanUnwrap;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class Lzcnt
{
    private const int LzcntLength_Windows = 4;
#if B32_ARCH || ANYCPU
    private const int LzcntLength_Unix_X86 = 6;
#endif
#if B64_ARCH || ANYCPU
    private const int LzcntLength_Unix_X64 = 4;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectLzcntAsm(ref void* destination, ref uint length)
    {
        if (_isUnix)
        {
#if B64_ARCH
            InjectLzcntAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
            InjectLzcntAsm_Unix_X86(ref destination, ref length);
#else
            if (_isX64)
                InjectLzcntAsm_Unix_X64(ref destination, ref length);
            else
                InjectLzcntAsm_Unix_X86(ref destination, ref length);
#endif
        }
        else
            InjectLzcntAsm_Windows(ref destination, ref length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectLzcntAsm_Windows(ref void* destination, ref uint length)
    {
        const int Length = LzcntLength_Windows;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.LzcntData_Windows, Length);
    }

#if B32_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectLzcntAsm_Unix_X86(ref void* destination, ref uint length)
    {
        const int Length = LzcntLength_Unix_X86;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.LzcntData_Unix_X86, Length);
    }
#endif

#if B64_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectLzcntAsm_Unix_X64(ref void* destination, ref uint length)
    {
        const int Length = LzcntLength_Unix_X64;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.LzcntData_Unix_X64, Length);
    }
#endif

    partial class Store
    {
        public static ref readonly byte LzcntData_Windows => ref Unwrap.From(new byte[LzcntLength_Windows]
        {
            0xF3, 0x0F, 0xBD, 0xC1 // lzcnt eax, ecx
        });
#if B32_ARCH || ANYCPU
        public static ref readonly byte LzcntData_Unix_X86 => ref Unwrap.From(new byte[LzcntLength_Unix_X86]
        {
            0xF3, 0x0F, 0xBD, 0x44, 0x24, 0x04 // lzcnt eax, dword ptr [esp+4]
        });
#endif
#if B64_ARCH || ANYCPU
        public static ref readonly byte LzcntData_Unix_X64 => ref Unwrap.From(new byte[LzcntLength_Unix_X64]
        {
            0xF3, 0x0F, 0xBD, 0xC7 // lzcnt eax, edi
        });
#endif
    }
}
#endif
#endif