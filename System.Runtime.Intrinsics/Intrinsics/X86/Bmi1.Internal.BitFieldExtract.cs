#if NETSTANDARD2_0_OR_GREATER
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

using SpanUnwrap;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class Bmi1
{
    private const int BextrLength_Windows = 8;
#if B32_ARCH || ANYCPU
    private const int BextrLength_Unix_X86 = 14;
#endif
#if B64_ARCH || ANYCPU
    private const int BextrLength_Unix_X64 = 8;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBextrAsm(ref void* destination, ref uint length)
    {
        if (_isUnix)
        {
#if B64_ARCH
            InjectBextrAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
            InjectBextrAsm_Unix_X86(ref destination, ref length);
#else
            if (_isX64)
                InjectBextrAsm_Unix_X64(ref destination, ref length);
            else
                InjectBextrAsm_Unix_X86(ref destination, ref length);
#endif
        }
        else
            InjectBextrAsm_Windows(ref destination, ref length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBextrAsm_Windows(ref void* destination, ref uint length)
    {
        const int Length = BextrLength_Windows;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.BextrData_Windows, Length);
    }

#if B32_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBextrAsm_Unix_X86(ref void* destination, ref uint length)
    {
        const int Length = BextrLength_Unix_X86;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.BextrData_Unix_X86, Length);
    }
#endif

#if B64_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBextrAsm_Unix_X64(ref void* destination, ref uint length)
    {
        const int Length = BextrLength_Unix_X64;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.BextrData_Unix_X64, Length);
    }
#endif

    partial class Store
    {
        public static ref readonly byte BextrData_Windows => ref Unwrap.From(new byte[BextrLength_Windows]
        {
            0x0F, 0xB7, 0xC2, // movzx eax, dx
            0xC4, 0xE2, 0x78, 0xF7, 0xC1 // bextr eax, ecx, eax
        });
#if B32_ARCH || ANYCPU
        public static ref readonly byte BextrData_Unix_X86 => ref Unwrap.From(new byte[BextrLength_Unix_X86]
        {
            0x8B, 0x44, 0x24, 0x04, // mov eax, dword ptr [esp+4]
            0x0F, 0xB7, 0x5C, 0x24, 0x08, // movzx ebx, word ptr [esp+8]
            0xC4, 0xE2, 0x60, 0xF7, 0xC0 // bextr eax, eax, ebx
        });
#endif
#if B64_ARCH || ANYCPU
        public static ref readonly byte BextrData_Unix_X64 => ref Unwrap.From(new byte[BextrLength_Unix_X64]
        {
            0x0F, 0xB7, 0xC6, // movzx eax, di
            0xC4, 0xE2, 0x78, 0xF7, 0xC7 // bextr eax, ecx, eax
        });
#endif
    }
}
#endif
#endif