#if NETSTANDARD2_0_OR_GREATER
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

using SpanUnwrap;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class Bmi1
{
    private const int BlsrLength_Windows = 5;
#if B32_ARCH || ANYCPU
    private const int BlsrLength_Unix_X86 = 7;
#endif
#if B64_ARCH || ANYCPU
    private const int BlsrLength_Unix_X64 = 5;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBlsrAsm(ref void* destination, ref uint length)
    {
        if (_isUnix)
        {
#if B64_ARCH
            InjectBlsrAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
            InjectBlsrAsm_Unix_X86(ref destination, ref length);
#else
            if (_isX64)
                InjectBlsrAsm_Unix_X64(ref destination, ref length);
            else
                InjectBlsrAsm_Unix_X86(ref destination, ref length);
#endif
        }
        else
            InjectBlsrAsm_Windows(ref destination, ref length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBlsrAsm_Windows(ref void* destination, ref uint length)
    {
        const int Length = BlsrLength_Windows;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.BlsrData_Windows, Length);
    }

#if B32_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBlsrAsm_Unix_X86(ref void* destination, ref uint length)
    {
        const int Length = BlsrLength_Unix_X86;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.BlsrData_Unix_X86, Length);
    }
#endif

#if B64_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBlsrAsm_Unix_X64(ref void* destination, ref uint length)
    {
        const int Length = BlsrLength_Unix_X64;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.BlsrData_Unix_X64, Length);
    }
#endif

    partial class Store
    {
        public static ref readonly byte BlsrData_Windows => ref Unwrap.From(new byte[BlsrLength_Windows]
        {
            0xC4, 0xE2, 0x78, 0xF3, 0xC9 // blsr eax, ecx
        });
#if B32_ARCH || ANYCPU
        public static ref readonly byte BlsrData_Unix_X86 => ref Unwrap.From(new byte[BlsrLength_Unix_X86]
        {
            0xC4, 0xE2, 0x78, 0xF3, 0x4C, 0x24, 0x04 // blsr eax, dword ptr [esp+4]
        });
#endif
#if B64_ARCH || ANYCPU
        public static ref readonly byte BlsrData_Unix_X64 => ref Unwrap.From(new byte[BlsrLength_Unix_X64]
        {
            0xC4, 0xE2, 0x78, 0xF3, 0xCF // blsr eax, edi
        });
#endif
    }
}
#endif
#endif