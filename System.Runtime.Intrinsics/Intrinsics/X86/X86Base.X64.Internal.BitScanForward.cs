#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0
#if (X86_ARCH && B64_ARCH) || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

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
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            length = Length;
#if NETSTANDARD2_0
            if (!_spanExists)
            {
                UnsafeHelper.CopyBlock(destination, in StoreAsArray.GetBsfDataReference_Windows(), Length);
                return;
            }
#endif
            UnsafeHelper.CopyBlock(destination, in StoreAsSpan.GetBsfDataReference_Windows(), Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsfAsm_Unix(ref void* destination, ref uint length)
        {
            const int Length = BsfLength_Unix;
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            length = Length;
#if NETSTANDARD2_0
            if (!_spanExists)
            {
                UnsafeHelper.CopyBlock(destination, in StoreAsArray.GetBsfDataReference_Unix(), Length);
                return;
            }
#endif
            UnsafeHelper.CopyBlock(destination, in StoreAsSpan.GetBsfDataReference_Unix(), Length);
        }

#if NETSTANDARD2_0
        partial class StoreAsArray
        {
            private static readonly byte[] BsfData_Windows = new byte[BsfLength_Windows]
			{
                0x48, 0x0F, 0xBC, 0xC1 // bsf rax, rcx
			};
            private static readonly byte[] BsfData_Unix = new byte[BsfLength_Unix]
            {
                0x48, 0x0F, 0xBC, 0xC7 // bsf rax, rdi
            };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ref readonly byte GetBsfDataReference_Windows()
                => ref UnsafeHelper.GetReference(BsfData_Windows);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ref readonly byte GetBsfDataReference_Unix()
                => ref UnsafeHelper.GetReference(BsfData_Unix);
        }
#endif

        partial class StoreAsSpan
		{
            private static ReadOnlySpan<byte> BsfData_Windows => new byte[BsfLength_Windows]
            {
                0x48, 0x0F, 0xBC, 0xC1 // bsf rax, rcx
            };
            private static ReadOnlySpan<byte> BsfData_Unix => new byte[BsfLength_Unix]
            {
                0x48, 0x0F, 0xBC, 0xC7 // bsf rax, rdi
            };

            [MethodImpl(Constants.SpanSourceInliningOptions)]
            public static ref readonly byte GetBsfDataReference_Windows()
                => ref UnsafeHelper.GetReference(BsfData_Windows);

            [MethodImpl(Constants.SpanSourceInliningOptions)]
            public static ref readonly byte GetBsfDataReference_Unix()
                => ref UnsafeHelper.GetReference(BsfData_Unix);
        }
	}
}
#endif
#endif