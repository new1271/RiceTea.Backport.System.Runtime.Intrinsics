#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

using SpanUnwrap;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class X86Base
{
#if B32_ARCH || ANYCPU
    private const int DivLength_Windows_X86 = 12;
    private const int DivLength_Unix_X86 = 18;
#endif
#if B64_ARCH || ANYCPU
    private const int DivLength_Windows_X64 = 8;
    private const int DivLength_Unix_X64 = 10;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectDivAsm(ref void* destination, ref uint length)
    {
        if (_isUnix)
        {
#if B64_ARCH
            InjectDivAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
            InjectDivAsm_Unix_X86(ref destination, ref length);
#else
            if (_isX64)
                InjectDivAsm_Unix_X64(ref destination, ref length);
            else
                InjectDivAsm_Unix_X86(ref destination, ref length);
#endif
        }
        else
        {
#if B64_ARCH
            InjectDivAsm_Windows_X64(ref destination, ref length);
#elif B32_ARCH
            InjectDivAsm_Windows_X86(ref destination, ref length);
#else
            if (_isX64)
                InjectDivAsm_Windows_X64(ref destination, ref length);
            else
                InjectDivAsm_Windows_X86(ref destination, ref length);
#endif
        }
    }

#if B32_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectDivAsm_Windows_X86(ref void* destination, ref uint length)
    {
        const int Length = DivLength_Windows_X86;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.DivData_Windows_X86, Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectDivAsm_Unix_X86(ref void* destination, ref uint length)
    {
        const int Length = DivLength_Unix_X86;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.DivData_Unix_X86, Length);
    }
#endif

#if B64_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectDivAsm_Windows_X64(ref void* destination, ref uint length)
    {
        const int Length = DivLength_Windows_X64;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.DivData_Windows_X64, Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectDivAsm_Unix_X64(ref void* destination, ref uint length)
    {
        const int Length = DivLength_Unix_X64;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.CopyBlockUnaligned(destination, in Store.DivData_Unix_X64, Length);
    }
#endif

    partial class Store
    {
#if B32_ARCH || ANYCPU
        public static ref readonly byte DivData_Windows_X86 => ref Unwrap.From(new byte[DivLength_Windows_X86]
        {
            0x89, 0xC8, // mov eax, ecx
            0x8B, 0x4C, 0x24, 0x04, // mov ecx, dword ptr [esp+4]
            0xF7, 0x74, 0x24, 0x08, // div dword ptr [esp+8]
            0x89, 0x11 // mov dword ptr [ecx], edx
        });
        public static ref readonly byte DivData_Unix_X86 => ref Unwrap.From(new byte[DivLength_Unix_X86]
        {
            0x8B, 0x44, 0x24, 0x04, // mov eax, dword ptr [esp+4]
            0x8B, 0x54, 0x24, 0x08, // mov edx, dword ptr [esp+8]
            0x8B, 0x4C, 0x24, 0x10, // mov ecx, dword ptr [esp+16]
            0xF7, 0x74, 0x24, 0x0C, // div dword ptr [esp+12]
            0x89, 0x11 // mov [ecx], edx
        });
#endif
#if B64_ARCH || ANYCPU
        public static ref readonly byte DivData_Windows_X64 => ref Unwrap.From(new byte[DivLength_Windows_X64]
        {
            0x89, 0xC8, // mov eax, ecx
            0x41, 0xF7, 0xF0, // div r8d
            0x41, 0x89, 0x11 // mov dword ptr [r9], edx
        });
        public static ref readonly byte DivData_Unix_X64 => ref Unwrap.From(new byte[DivLength_Unix_X64]
        {
            0x89, 0xF8, // mov eax, edi
            0x89, 0xD7, // mov edi, edx
            0x89, 0xF2, // mov edx, esi
            0xF7, 0xF7, // div edi
            0x89, 0x11 // mov dword ptr [rcx], edx
        });
#endif
    }
}
#endif
#endif