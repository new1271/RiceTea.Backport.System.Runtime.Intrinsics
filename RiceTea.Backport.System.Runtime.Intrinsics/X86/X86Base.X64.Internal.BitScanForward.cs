#if !NETSTANDARD2_1_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;
using System.Runtime.Intrinsics.Internals;

using InlineIL;

namespace System.Runtime.Intrinsics.X86;

partial class X86Base
{
	unsafe partial class X64
	{
#if ((X86_ARCH && B64_ARCH) || ANYCPU)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void InjectBsfAsm(ref void* destination, ref uint length)
		{
#if !B64_ARCH
            if (!Helpers.PlatformHelper.IsX64)
                ThrowUtils.ThrowPlatformNotSupported();
#endif
			if (SoftDependencyHelper.SystemMemoryExists)
				StoreAsSpan.InjectBsfAsm(ref destination, ref length);
			else
				StoreAsArray.InjectBsfAsm(ref destination, ref length);
		}

		partial class StoreAsArray
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			public static void InjectBsfAsm(ref void* destination, ref uint length)
			{
				if (UseUnixLogic)
					InjectBsfAsm_Unix(ref destination, ref length);
				else
					InjectBsfAsm_Windows(ref destination, ref length);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static void InjectBsfAsm_Windows(ref void* destination, ref uint length)
			{
				const int Length = 4;
				byte[] data = new byte[Length] {
					0x48, 0x0F, 0xBC, 0xC1 // bsf rax, rcx
                };
				if (length < Length)
					throw new AccessViolationException();
				destination = (byte*)destination + length - Length;
				fixed (byte* source = data)
					UnsafeHelper.CopyBlock(destination, source, Length);
				length = Length;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static void InjectBsfAsm_Unix(ref void* destination, ref uint length)
			{
				const int Length = 4;
				byte[] data = new byte[Length] {
					0x48, 0x0F, 0xBC, 0xC7 // bsf rax, rdi
                };
				if (length < Length)
					throw new AccessViolationException();
				destination = (byte*)destination + length - Length;
				fixed (byte* source = data)
					UnsafeHelper.CopyBlock(destination, source, Length);
				length = Length;
			}
		}

		partial class StoreAsSpan
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			public static void InjectBsfAsm(ref void* destination, ref uint length)
			{
				if (UseUnixLogic)
					InjectBsfAsm_Unix(ref destination, ref length);
				else
					InjectBsfAsm_Windows(ref destination, ref length);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static void InjectBsfAsm_Windows(ref void* destination, ref uint length)
			{
				const int Length = 4;
				ReadOnlySpan<byte> data = [
					0x48, 0x0F, 0xBC, 0xC1 // bsf rax, rcx
                ];
				if (length < Length)
					throw new AccessViolationException();
				destination = (byte*)destination + length - Length;
				fixed (byte* source = data)
					UnsafeHelper.CopyBlock(destination, source, Length);
				length = Length;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static void InjectBsfAsm_Unix(ref void* destination, ref uint length)
			{
				const int Length = 4;
				ReadOnlySpan<byte> data = [
					0x48, 0x0F, 0xBC, 0xC7 // bsf rax, rdi
                ];
				if (length < Length)
					throw new AccessViolationException();
				destination = (byte*)destination + length - Length;
				fixed (byte* source = data)
					UnsafeHelper.CopyBlock(destination, source, Length);
				length = Length;
			}
		}
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsfAsm(ref void* destination, ref uint length) => ThrowUtils.ThrowPlatformNotSupported();
#endif
	}
}
#endif