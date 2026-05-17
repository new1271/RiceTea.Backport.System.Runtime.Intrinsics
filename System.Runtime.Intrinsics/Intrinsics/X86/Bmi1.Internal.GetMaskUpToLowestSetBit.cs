#if NETSTANDARD2_0_OR_GREATER
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

using SpanUnwrap;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class Bmi1
{
    private const int BlsmskLength_Windows = 5;
#if B32_ARCH || ANYCPU
    private const int BlsmskLength_Unix_X86 = 7;
#endif
#if B64_ARCH || ANYCPU
    private const int BlsmskLength_Unix_X64 = 5;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBlsmskAsm(ref void* destination, ref uint length)
    {
        if (_isUnix)
        {
#if B64_ARCH
            InjectBlsmskAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
            InjectBlsmskAsm_Unix_X86(ref destination, ref length);
#else
            if (_isX64)
                InjectBlsmskAsm_Unix_X64(ref destination, ref length);
            else
                InjectBlsmskAsm_Unix_X86(ref destination, ref length);
#endif
        }
        else
            InjectBlsmskAsm_Windows(ref destination, ref length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBlsmskAsm_Windows(ref void* destination, ref uint length)
    {
        const int Length = BlsmskLength_Windows;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.BlsmskData_Windows, Length);
    }

#if B32_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBlsmskAsm_Unix_X86(ref void* destination, ref uint length)
    {
        const int Length = BlsmskLength_Unix_X86;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.BlsmskData_Unix_X86, Length);
    }
#endif

#if B64_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBlsmskAsm_Unix_X64(ref void* destination, ref uint length)
    {
        const int Length = BlsmskLength_Unix_X64;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.BlsmskData_Unix_X64, Length);
    }
#endif

    partial class Store
    {
        public static ref readonly byte BlsmskData_Windows => ref Unwrap.From(new byte[BlsmskLength_Windows]
        {
            0xC4, 0xE2, 0x78, 0xF3, 0xD1 // blsmsk eax, ecx
        });
#if B32_ARCH || ANYCPU
        public static ref readonly byte BlsmskData_Unix_X86 => ref Unwrap.From(new byte[BlsmskLength_Unix_X86]
        {
            0xC4, 0xE2, 0x78, 0xF3, 0x54, 0x24, 0x04 // blsmsk eax, dword ptr [esp+4]
        });
#endif
#if B64_ARCH || ANYCPU
        public static ref readonly byte BlsmskData_Unix_X64 => ref Unwrap.From(new byte[BlsmskLength_Unix_X64]
        {
            0xC4, 0xE2, 0x78, 0xF3, 0xD7 // blsmsk eax, edi
        });
#endif
    }
}
#endif
#endif