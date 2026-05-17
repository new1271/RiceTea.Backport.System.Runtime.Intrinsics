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
        private const int BlsiLength_Windows = 5;
        private const int BlsiLength_Unix = 5;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBlsiAsm(ref void* destination, ref uint length)
        {
            if (_isUnix)
                InjectBlsiAsm_Unix(ref destination, ref length);
            else
                InjectBlsiAsm_Windows(ref destination, ref length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBlsiAsm_Windows(ref void* destination, ref uint length)
        {
            const int Length = BlsiLength_Windows;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
            UnsafeHelper.CopyBlockUnaligned(destination, in Store.BlsiData_Windows, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBlsiAsm_Unix(ref void* destination, ref uint length)
        {
            const int Length = BlsiLength_Unix;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
            UnsafeHelper.CopyBlockUnaligned(destination, in Store.BlsiData_Unix, Length);
        }

        partial class Store
        {
            public static ref readonly byte BlsiData_Windows => ref Unwrap.From(new byte[BlsiLength_Windows]
            {
                0xC4, 0xE2, 0xF8, 0xF3, 0xD9 // blsi rax, rcx
            });
            public static ref readonly byte BlsiData_Unix => ref Unwrap.From(new byte[BlsiLength_Unix]
            {
                0xC4, 0xE2, 0xF8, 0xF3, 0xDF // blsi rax, rdi
            });
        }
    }
}
#endif
#endif