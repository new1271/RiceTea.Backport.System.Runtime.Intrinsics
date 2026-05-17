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
        private const int AndnLength_Windows = 5;
        private const int AndnLength_Unix = 5;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectAndnAsm(ref void* destination, ref uint length)
        {
            if (_isUnix)
                InjectAndnAsm_Unix(ref destination, ref length);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectAndnAsm_Unix(ref void* destination, ref uint length)
        {
            const int Length = AndnLength_Unix;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
            UnsafeHelper.CopyBlockUnaligned(destination, in Store.AndnData_Unix, Length);
        }

        partial class Store
        {
            public static ref readonly byte AndnData_Windows => ref Unwrap.From(new byte[AndnLength_Windows]
            {
                0xC4, 0xE2, 0xF0, 0xF2, 0xC2 // andn rax, rcx, rdx
            });
            public static ref readonly byte AndnData_Unix => ref Unwrap.From(new byte[AndnLength_Unix]
            {
                0xC4, 0xE2, 0xC0, 0xF2, 0xC6 // andn rax, rdi, rsi
            });
        }
    }
}
#endif
#endif