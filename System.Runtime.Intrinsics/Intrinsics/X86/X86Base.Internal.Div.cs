#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

using SpanUnwrap;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class X86Base
{
#if B32_ARCH || ANYCPU
    private const int DivLength_Windows_X86 = 13;
    private const int DivLength_Unix_X86 = 19;
#endif
#if B64_ARCH || ANYCPU
    private const int DivLength_Windows_X64 = 14;
    private const int DivLength_Unix_X64 = 9;
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
            0x53, // push ebx
            0x89, 0xD3, // mov ebx, edx
            0x8B, 0x01, // mov eax, dword ptr [ecx]
            0x8B, 0x51, 0x04, // mov edx, dword ptr [ecx+4]
            0xF7, 0xF3, // div ebx
            0x89, 0x11, // mov dword ptr [ecx], edx
            0x5B // pop ebx
        });
        public static ref readonly byte DivData_Unix_X86 => ref Unwrap.From(new byte[DivLength_Unix_X86]
        {
            0x53, // push ebx
            0x8B, 0x5C, 0x24, 0x0C, // mov ebx, [esp+12]
            0x8B, 0x4C, 0x24, 0x08, // mov ecx, [esp+8]
            0x8B, 0x01, // mov eax, [ecx]
            0x8B, 0x51, 0x04, // mov eax, [ecx+4]
            0xF7, 0xF3, // div ebx
            0x89, 0x11, // mov dword ptr [ecx], edx
            0x5B // pop ebx
        });
#endif
#if B64_ARCH || ANYCPU
        public static ref readonly byte DivData_Windows_X64 => ref Unwrap.From(new byte[DivLength_Windows_X64]
        {
            0x41, 0x89, 0xD0, // mov r8d, edx
            0x8B, 0x01, // mov eax, dword ptr [rcx]
            0x67, 0x8B, 0x51, 0x04, // mov edx, dword ptr [ecx+4]
            0x41, 0xF7, 0xF0, // div r8d
            0x89, 0x11 // mov dword ptr [rcx], edx
        });
        public static ref readonly byte DivData_Unix_X64 => ref Unwrap.From(new byte[DivLength_Unix_X64]
        {
            0x8B, 0x06, // mov eax, dword ptr [rsi]
            0x8B, 0x56, 0x04, // mov edx, dword ptr [rsi+4]
            0xF7, 0xF7, // div edi
            0x89, 0x16 // mov dword ptr [rsi], edx
        });
#endif
    }
}
#endif
#endif