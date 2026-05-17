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
        private const int TzcntLength_Windows = 5;
        private const int TzcntLength_Unix = 5;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectTzcntAsm(ref void* destination, ref uint length)
        {
            if (_isUnix)
                InjectTzcntAsm_Unix(ref destination, ref length);
            else
                InjectTzcntAsm_Windows(ref destination, ref length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectTzcntAsm_Windows(ref void* destination, ref uint length)
        {
            const int Length = TzcntLength_Windows;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
            UnsafeHelper.CopyBlockUnaligned(destination, in Store.TzcntData_Windows, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectTzcntAsm_Unix(ref void* destination, ref uint length)
        {
            const int Length = TzcntLength_Unix;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
            UnsafeHelper.CopyBlockUnaligned(destination, in Store.TzcntData_Unix, Length);
        }

        partial class Store
        {
            public static ref readonly byte TzcntData_Windows => ref Unwrap.From(new byte[TzcntLength_Windows]
            {
                0xF3, 0x48, 0x0F, 0xBC, 0xC1 // tzcnt rax, rcx
            });
            public static ref readonly byte TzcntData_Unix => ref Unwrap.From(new byte[TzcntLength_Unix]
            {
                0xF3, 0x48, 0x0F, 0xBC, 0xC7 // tzcnt rax, rdi
            });
        }
    }
}
#endif
#endif