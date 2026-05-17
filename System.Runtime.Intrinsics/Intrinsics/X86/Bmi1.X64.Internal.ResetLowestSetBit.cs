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
        private const int BlsrLength_Windows = 5;
        private const int BlsrLength_Unix = 5;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBlsrAsm(ref void* destination, ref uint length)
        {
            if (_isUnix)
                InjectBlsrAsm_Unix(ref destination, ref length);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBlsrAsm_Unix(ref void* destination, ref uint length)
        {
            const int Length = BlsrLength_Unix;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
            UnsafeHelper.CopyBlockUnaligned(destination, in Store.BlsrData_Unix, Length);
        }

        partial class Store
        {
            public static ref readonly byte BlsrData_Windows => ref Unwrap.From(new byte[BlsrLength_Windows]
            {
                0xC4, 0xE2, 0xF8, 0xF3, 0xC9 // blsr rax, rcx
            });
            public static ref readonly byte BlsrData_Unix => ref Unwrap.From(new byte[BlsrLength_Unix]
            {
                0xC4, 0xE2, 0xF8, 0xF3, 0xCF // blsr rax, rdi
            });
        }
    }
}
#endif
#endif