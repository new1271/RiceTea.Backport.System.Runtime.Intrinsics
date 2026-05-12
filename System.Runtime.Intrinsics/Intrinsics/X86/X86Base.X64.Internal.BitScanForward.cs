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
#if ANYCPU
            if (!PlatformHelper.IsX64)
                ThrowUtils.ThrowPlatformNotSupported();
#endif
			if (SoftDependencyHelper.SystemMemoryExists)
				StoreAsSpan.InjectBsfAsm(ref destination, ref length);
			else
				StoreAsArray.InjectBsfAsm(ref destination, ref length);
		}

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

            [MethodImpl(MethodImplOptions.NoInlining)]
			public static void InjectBsfAsm(ref void* destination, ref uint length)
			{
				if (IsUnix)
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
				fixed (byte* source = BsfData_Windows)
					UnsafeHelper.CopyBlock(destination, source, Length);
				length = Length;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static void InjectBsfAsm_Unix(ref void* destination, ref uint length)
			{
				const int Length = BsfLength_Unix;
				if (length < Length)
					throw new AccessViolationException();
				destination = (byte*)destination + length - Length;
				fixed (byte* source = BsfData_Unix)
					UnsafeHelper.CopyBlock(destination, source, Length);
				length = Length;
			}
		}

		partial class StoreAsSpan
		{
            private static ReadOnlySpan<byte> BsfData_Windows =>
            [
                0x48, 0x0F, 0xBC, 0xC1 // bsf rax, rcx
			];
            private static ReadOnlySpan<byte> BsfData_Unix =>
            [
                0x48, 0x0F, 0xBC, 0xC7 // bsf rax, rdi
            ];

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void InjectBsfAsm(ref void* destination, ref uint length)
            {
                if (IsUnix)
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
                fixed (byte* source = BsfData_Windows)
                    UnsafeHelper.CopyBlock(destination, source, Length);
                length = Length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InjectBsfAsm_Unix(ref void* destination, ref uint length)
            {
                const int Length = BsfLength_Unix;
                if (length < Length)
                    throw new AccessViolationException();
                destination = (byte*)destination + length - Length;
                fixed (byte* source = BsfData_Unix)
                    UnsafeHelper.CopyBlock(destination, source, Length);
                length = Length;
            }
        }
	}
}
#endif
#endif