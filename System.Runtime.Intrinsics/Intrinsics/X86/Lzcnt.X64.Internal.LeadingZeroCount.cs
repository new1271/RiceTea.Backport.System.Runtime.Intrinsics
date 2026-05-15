#if NETSTANDARD2_0_OR_GREATER
#if (X86_ARCH && B64_ARCH) || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

using SpanUnwrap;

namespace System.Runtime.Intrinsics.X86;

partial class Lzcnt
{
    unsafe partial class X64
    {
        private const int LzcntLength_Windows = 5;
        private const int LzcntLength_Unix = 5;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectLzcntAsm(ref void* destination, ref uint length)
        {
            if (_isUnix)
                InjectLzcntAsm_Unix(ref destination, ref length);
            else
                InjectLzcntAsm_Windows(ref destination, ref length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectLzcntAsm_Windows(ref void* destination, ref uint length)
        {
            const int Length = LzcntLength_Windows;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
            UnsafeHelper.CopyBlockUnaligned(destination, in Store.LzcntData_Windows, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectLzcntAsm_Unix(ref void* destination, ref uint length)
        {
            const int Length = LzcntLength_Unix;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
            UnsafeHelper.CopyBlockUnaligned(destination, in Store.LzcntData_Unix, Length);
        }

        partial class Store
        {
            public static ref readonly byte LzcntData_Windows => ref Unwrap.From(new byte[LzcntLength_Windows]
            {
                0xF3, 0x48, 0x0F, 0xBD, 0xC1 // lzcnt rax rcx
            });
            public static ref readonly byte LzcntData_Unix => ref Unwrap.From(new byte[LzcntLength_Unix]
            {
                0xF3, 0x48, 0x0F, 0xBD, 0xC7 // lzcnt rax, rdi
            });
        }
    }
}
#endif
#endif