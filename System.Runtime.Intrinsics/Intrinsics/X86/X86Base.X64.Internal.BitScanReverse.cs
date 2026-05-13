#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0
#if (X86_ARCH && B64_ARCH) || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

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
#if NETSTANDARD2_0
            if (!_spanExists)
            {
                UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsArray.GetBsrDataReference_Windows(), Length);
                return;
            }
#endif
            UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsSpan.GetBsrDataReference_Windows(), Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsrAsm_Unix(ref void* destination, ref uint length)
        {
            const int Length = BsrLength_Unix;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
#if NETSTANDARD2_0
            if (!_spanExists)
            {
                UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsArray.GetBsrDataReference_Unix(), Length);
                return;
            }
#endif
            UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsSpan.GetBsrDataReference_Unix(), Length);
        }

#if NETSTANDARD2_0
        partial class StoreAsArray
        {
            private static readonly byte[] BsrData_Windows = new byte[BsrLength_Windows]
            {
                0x48, 0x0F, 0xBD, 0xC1 // bsr rax, rcx
			};
            private static readonly byte[] BsrData_Unix = new byte[BsrLength_Unix]
            {
                0x48, 0x0F, 0xBD, 0xC7 // bsr rax, rdi
            };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ref readonly byte GetBsrDataReference_Windows()
                => ref UnsafeHelper.GetReference(BsrData_Windows);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ref readonly byte GetBsrDataReference_Unix()
                => ref UnsafeHelper.GetReference(BsrData_Unix);
        }
#endif

        partial class StoreAsSpan
        {
            private static ReadOnlySpan<byte> BsrData_Windows => new byte[BsrLength_Windows]
            {
                0x48, 0x0F, 0xBD, 0xC1 // bsr rax, rcx
            };
            private static ReadOnlySpan<byte> BsrData_Unix => new byte[BsrLength_Unix]
            {
                0x48, 0x0F, 0xBD, 0xC7 // bsr rax, rdi
            };

            [MethodImpl(Constants.SpanSourceInliningOptions)]
            public static ref readonly byte GetBsrDataReference_Windows()
                => ref UnsafeHelper.GetReference(BsrData_Windows);

            [MethodImpl(Constants.SpanSourceInliningOptions)]
            public static ref readonly byte GetBsrDataReference_Unix()
                => ref UnsafeHelper.GetReference(BsrData_Unix);
        }
    }
}
#endif
#endif