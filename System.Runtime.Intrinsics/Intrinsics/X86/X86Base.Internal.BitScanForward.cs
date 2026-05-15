#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

using SpanUnwrap;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class X86Base
{
    private const int BsfLength_Windows = 3;
#if B32_ARCH || ANYCPU
    private const int BsfLength_Unix_X86 = 5;
#endif
#if B64_ARCH || ANYCPU
    private const int BsfLength_Unix_X64 = 3;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBsfAsm(ref void* destination, ref uint length)
    {
        if (_isUnix)
        {
#if B64_ARCH
            InjectBsfAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
            InjectBsfAsm_Unix_X86(ref destination, ref length);
#else
            if (_isX64)
                InjectBsfAsm_Unix_X64(ref destination, ref length);
            else
                InjectBsfAsm_Unix_X86(ref destination, ref length);
#endif
        }
        else
            InjectBsfAsm_Windows(ref destination, ref length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBsfAsm_Windows(ref void* destination, ref uint length)
    {
        const int Length = BsfLength_Windows;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.BsfData_Windows, Length);
    }

#if B32_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBsfAsm_Unix_X86(ref void* destination, ref uint length)
    {
        const int Length = BsfLength_Unix_X86;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.BsfData_Unix_X86, Length);
    }
#endif

#if B64_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBsfAsm_Unix_X64(ref void* destination, ref uint length)
    {
        const int Length = BsfLength_Unix_X64;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.BsfData_Unix_X64, Length);
    }
#endif

    partial class Store
    {
        public static ref readonly byte BsfData_Windows => ref Unwrap.From(new byte[BsfLength_Windows]
        {
            0x0F, 0xBC, 0xC1 // bsf eax, ecx
        });
#if B32_ARCH || ANYCPU
        public static ref readonly byte BsfData_Unix_X86 => ref Unwrap.From(new byte[BsfLength_Unix_X86]
        {
            0x0F, 0xBC, 0x44, 0x24, 0x04 // bsf eax, dword ptr [esp+4]
        });
#endif
#if B64_ARCH || ANYCPU
        public static ref readonly byte BsfData_Unix_X64 => ref Unwrap.From(new byte[BsfLength_Unix_X64]
        {
            0x0F, 0xBC, 0xC7 // bsf eax, edi
        });
#endif
    }
}
#endif
#endif