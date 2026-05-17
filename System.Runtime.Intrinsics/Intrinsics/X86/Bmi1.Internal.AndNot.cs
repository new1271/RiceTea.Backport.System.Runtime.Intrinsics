#if NETSTANDARD2_0_OR_GREATER
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

using SpanUnwrap;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class Bmi1
{
    private const int AndnLength_Windows = 5;
#if B32_ARCH || ANYCPU
    private const int AndnLength_Unix_X86 = 11;
#endif
#if B64_ARCH || ANYCPU
    private const int AndnLength_Unix_X64 = 5;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectAndnAsm(ref void* destination, ref uint length)
    {
        if (_isUnix)
        {
#if B64_ARCH
            InjectAndnAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
            InjectAndnAsm_Unix_X86(ref destination, ref length);
#else
            if (_isX64)
                InjectAndnAsm_Unix_X64(ref destination, ref length);
            else
                InjectAndnAsm_Unix_X86(ref destination, ref length);
#endif
        }
        else
            InjectAndnAsm_Windows(ref destination, ref length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectAndnAsm_Windows(ref void* destination, ref uint length)
    {
        const int Length = AndnLength_Windows;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.AndnData_Windows, Length);
    }

#if B32_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectAndnAsm_Unix_X86(ref void* destination, ref uint length)
    {
        const int Length = AndnLength_Unix_X86;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.AndnData_Unix_X86, Length);
    }
#endif

#if B64_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectAndnAsm_Unix_X64(ref void* destination, ref uint length)
    {
        const int Length = AndnLength_Unix_X64;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.AndnData_Unix_X64, Length);
    }
#endif

    partial class Store
    {
        public static ref readonly byte AndnData_Windows => ref Unwrap.From(new byte[AndnLength_Windows]
        {
            0xC4, 0xE2, 0x70, 0xF2, 0xC2 // andn eax, ecx, edx
        });
#if B32_ARCH || ANYCPU
        public static ref readonly byte AndnData_Unix_X86 => ref Unwrap.From(new byte[AndnLength_Unix_X86]
        {
            0x8B, 0x44, 0x24, 0x04, // mov eax, dword ptr [esp+4]
            0xC4, 0xE2, 0x78, 0xF2, 0x44, 0x24, 0x08 // andn eax, eax, dword ptr [esp+8]
        });
#endif
#if B64_ARCH || ANYCPU
        public static ref readonly byte AndnData_Unix_X64 => ref Unwrap.From(new byte[AndnLength_Unix_X64]
        {
            0xC4, 0xE2, 0x40, 0xF2, 0xC6 // andn eax, edi, esi
        });
#endif
    }
}
#endif
#endif