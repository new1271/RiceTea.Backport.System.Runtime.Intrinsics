#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0
#if (X86_ARCH && B64_ARCH) || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

using SpanDissolve;

namespace System.Runtime.Intrinsics.X86;

partial class X86Base
{
    unsafe partial class X64
    {
        private const int BsrLength_Windows = 4;
        private const int BsrLength_Unix = 4;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsrAsm(ref void* destination, ref uint length)
        {
            if (_isUnix)
                InjectBsrAsm_Unix(ref destination, ref length);
            else
                InjectBsrAsm_Windows(ref destination, ref length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsrAsm_Windows(ref void* destination, ref uint length)
        {
            const int Length = BsrLength_Windows;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
            UnsafeHelper.CopyBlockUnaligned(destination, in Store.BsrData_Windows, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsrAsm_Unix(ref void* destination, ref uint length)
        {
            const int Length = BsrLength_Unix;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
            UnsafeHelper.CopyBlockUnaligned(destination, in Store.BsrData_Unix, Length);
        }

        partial class Store
        {
            public static ref readonly byte BsrData_Windows => ref SpanDissolver.Dissolve(new byte[BsrLength_Windows]
            {
                0x48, 0x0F, 0xBD, 0xC1 // bsr rax, rcx
            });
            public static ref readonly byte BsrData_Unix => ref SpanDissolver.Dissolve(new byte[BsrLength_Unix]
            {
                0x48, 0x0F, 0xBD, 0xC7 // bsr rax, rdi
            });
        }
    }
}
#endif
#endif