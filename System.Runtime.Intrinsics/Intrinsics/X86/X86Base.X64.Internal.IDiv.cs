#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0
#if (X86_ARCH && B64_ARCH) || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

using SpanUnwrap;

namespace System.Runtime.Intrinsics.X86;

partial class X86Base
{
    unsafe partial class X64
    {
        private const int IDivLength_Windows = 9;
        private const int IDivLength_Unix = 15;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectIDivAsm(ref void* destination, ref uint length)
        {
            if (_isUnix)
                InjectIDivAsm_Unix(ref destination, ref length);
            else
                InjectIDivAsm_Windows(ref destination, ref length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectIDivAsm_Windows(ref void* destination, ref uint length)
        {
            const int Length = IDivLength_Windows;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
            UnsafeHelper.CopyBlockUnaligned(destination, in Store.IDivData_Windows, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectIDivAsm_Unix(ref void* destination, ref uint length)
        {
            const int Length = IDivLength_Unix;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
            UnsafeHelper.CopyBlockUnaligned(destination, in Store.IDivData_Unix, Length);
        }

        partial class Store
        {
            public static ref readonly byte IDivData_Windows => ref Unwrap.From(new byte[IDivLength_Windows]
            {
                0x48, 0x89, 0xC8, // mov rax, rcx
                0x49, 0xF7, 0xF8, // idiv r8
                0x49, 0x89, 0x11 // mov qword ptr [r9], rdx
            });
            public static ref readonly byte IDivData_Unix => ref Unwrap.From(new byte[IDivLength_Unix]
            {
                0x48, 0x89, 0xF8, // mov rax, rdi
                0x48, 0x89, 0xD7, // mov rdi, rdx
                0x48, 0x89, 0xF2, // mov rdx, rsi
                0x48, 0xF7, 0xFF, // idiv rdi
                0x48, 0x89, 0x11 // mov qword ptr [rcx], rdx
            });
        }
    }
}
#endif
#endif