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
        private const int BsfLength_Windows = 4;
        private const int BsfLength_Unix = 4;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsfAsm(ref void* destination, ref uint length)
        {
            if (_isUnix)
                InjectBsfAsm_Unix(ref destination, ref length);
            else
                InjectBsfAsm_Windows(ref destination, ref length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsfAsm_Windows(ref void* destination, ref uint length)
        {
            const int Length = BsfLength_Windows;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
            UnsafeHelper.CopyBlockUnaligned(destination, in Store.BsfData_Windows, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsfAsm_Unix(ref void* destination, ref uint length)
        {
            const int Length = BsfLength_Unix;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
            UnsafeHelper.CopyBlockUnaligned(destination, in Store.BsfData_Unix, Length);
        }

        partial class Store
		{
            public static ref readonly byte BsfData_Windows => ref Unwrap.From(new byte[BsfLength_Windows]
            {
                0x48, 0x0F, 0xBC, 0xC1 // bsf rax, rcx
            });
            public static ref readonly byte BsfData_Unix => ref Unwrap.From(new byte[BsfLength_Unix]
            {
                0x48, 0x0F, 0xBC, 0xC7 // bsf rax, rdi
            });
        }
	}
}
#endif
#endif