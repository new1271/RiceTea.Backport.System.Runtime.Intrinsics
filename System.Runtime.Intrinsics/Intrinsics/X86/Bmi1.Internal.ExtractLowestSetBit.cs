#if NETSTANDARD2_0_OR_GREATER
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

using SpanUnwrap;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class Bmi1
{
    private const int BlsiLength_Windows = 5;
#if B32_ARCH || ANYCPU
    private const int BlsiLength_Unix_X86 = 7;
#endif
#if B64_ARCH || ANYCPU
    private const int BlsiLength_Unix_X64 = 5;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBlsiAsm(ref void* destination, ref uint length)
    {
        if (_isUnix)
        {
#if B64_ARCH
            InjectBlsiAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
            InjectBlsiAsm_Unix_X86(ref destination, ref length);
#else
            if (_isX64)
                InjectBlsiAsm_Unix_X64(ref destination, ref length);
            else
                InjectBlsiAsm_Unix_X86(ref destination, ref length);
#endif
        }
        else
            InjectBlsiAsm_Windows(ref destination, ref length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBlsiAsm_Windows(ref void* destination, ref uint length)
    {
        const int Length = BlsiLength_Windows;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.BlsiData_Windows, Length);
    }

#if B32_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBlsiAsm_Unix_X86(ref void* destination, ref uint length)
    {
        const int Length = BlsiLength_Unix_X86;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.BlsiData_Unix_X86, Length);
    }
#endif

#if B64_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBlsiAsm_Unix_X64(ref void* destination, ref uint length)
    {
        const int Length = BlsiLength_Unix_X64;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.BlsiData_Unix_X64, Length);
    }
#endif

    partial class Store
    {
        public static ref readonly byte BlsiData_Windows => ref Unwrap.From(new byte[BlsiLength_Windows]
        {
            0xC4, 0xE2, 0x78, 0xF3, 0xD9 // blsi eax, ecx
        });
#if B32_ARCH || ANYCPU
        public static ref readonly byte BlsiData_Unix_X86 => ref Unwrap.From(new byte[BlsiLength_Unix_X86]
        {
            0xC4, 0xE2, 0x78, 0xF3, 0x5C, 0x24, 0x04 // blsi eax, dword ptr [esp+4]
        });
#endif
#if B64_ARCH || ANYCPU
        public static ref readonly byte BlsiData_Unix_X64 => ref Unwrap.From(new byte[BlsiLength_Unix_X64]
        {
            0xC4, 0xE2, 0x78, 0xF3, 0xDF // blsi eax, edi
        });
#endif
    }
}
#endif
#endif