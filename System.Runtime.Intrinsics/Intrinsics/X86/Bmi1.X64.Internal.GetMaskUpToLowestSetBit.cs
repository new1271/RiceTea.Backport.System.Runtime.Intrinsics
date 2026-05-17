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
        private const int BlsmskLength_Windows = 5;
        private const int BlsmskLength_Unix = 5;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBlsmskAsm(ref void* destination, ref uint length)
        {
            if (_isUnix)
                InjectBlsmskAsm_Unix(ref destination, ref length);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBlsmskAsm_Unix(ref void* destination, ref uint length)
        {
            const int Length = BlsmskLength_Unix;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
            UnsafeHelper.CopyBlockUnaligned(destination, in Store.BlsmskData_Unix, Length);
        }

        partial class Store
        {
            public static ref readonly byte BlsmskData_Windows => ref Unwrap.From(new byte[BlsmskLength_Windows]
            {
                0xC4, 0xE2, 0xF8, 0xF3, 0xD1 // blsmsk rax, rcx
            });
            public static ref readonly byte BlsmskData_Unix => ref Unwrap.From(new byte[BlsmskLength_Unix]
            {
                0xC4, 0xE2, 0xF8, 0xF3, 0xD7 // blsmsk rax, rdi
            });
        }
    }
}
#endif
#endif