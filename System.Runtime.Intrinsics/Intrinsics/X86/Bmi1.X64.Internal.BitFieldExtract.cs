#if NETSTANDARD2_0_OR_GREATER
#if (X86_ARCH && B64_ARCH) || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

using SpanUnwrap;

namespace System.Runtime.Intrinsics.X86;

partial class Bmi1
{
    unsafe partial class X64
    {
        private const int BextrLength_Windows = 8;
        private const int BextrLength_Unix = 8;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBextrAsm(ref void* destination, ref uint length)
        {
            if (_isUnix)
                InjectBextrAsm_Unix(ref destination, ref length);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBextrAsm_Unix(ref void* destination, ref uint length)
        {
            const int Length = BextrLength_Unix;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
            UnsafeHelper.CopyBlockUnaligned(destination, in Store.BextrData_Unix, Length);
        }

        partial class Store
        {
            public static ref readonly byte BextrData_Windows => ref Unwrap.From(new byte[BextrLength_Windows]
            {
                0x0F, 0xB7, 0xC2, // mov eax, dx
                0xC4, 0xE2, 0xF8, 0xF7, 0xC1// bextr rax, rcx, rax
            });
            public static ref readonly byte BextrData_Unix => ref Unwrap.From(new byte[BextrLength_Unix]
            {
                0x0F, 0xB7, 0xC6, // mov eax, si
                0xC4, 0xE2, 0xF8, 0xF7, 0xC7 // bextr rax, rdi, rax
            });
        }
    }
}
#endif
#endif