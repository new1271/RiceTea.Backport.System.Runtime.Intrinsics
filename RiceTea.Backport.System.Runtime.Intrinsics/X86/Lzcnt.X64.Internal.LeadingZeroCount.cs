#if !NETSTANDARD2_1_OR_GREATER
#if (X86_ARCH && B64_ARCH) || ANYCPU
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;

namespace System.Runtime.Intrinsics.X86;

partial class Lzcnt
{
	unsafe partial class X64
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void InjectLzcntAsm(ref void* destination, ref uint length)
		{
#if ANYCPU
            if (!PlatformHelper.IsX64)
                throw new PlatformNotSupportedException();
#endif
			if (SoftDependencyHelper.SystemMemoryExists)
				StoreAsSpan.InjectLzcntAsm(ref destination, ref length);
			else
				StoreAsArray.InjectLzcntAsm(ref destination, ref length);
		}

		partial class StoreAsArray
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			public static void InjectLzcntAsm(ref void* destination, ref uint length)
			{
				if (UseUnixLogic)
					InjectLzcntAsm_Unix(ref destination, ref length);
				else
					InjectLzcntAsm_Windows(ref destination, ref length);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static void InjectLzcntAsm_Windows(ref void* destination, ref uint length)
			{
				const int Length = 5;
				byte[] data = new byte[Length] {
					0xF3, 0x48, 0x0F, 0xBD, 0xC1 // lzcnt rax rcx
                };
				if (length < Length)
					throw new AccessViolationException();
				destination = (byte*)destination + length - Length;
				fixed (byte* source = data)
					UnsafeHelper.CopyBlock(destination, source, Length);
				length = Length;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static void InjectLzcntAsm_Unix(ref void* destination, ref uint length)
			{
				const int Length = 5;
				byte[] data = new byte[Length] {
					0xF3, 0x48, 0x0F, 0xBD, 0xC7 // lzcnt rax, rdi
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
			public static void InjectLzcntAsm(ref void* destination, ref uint length)
			{
				if (UseUnixLogic)
					InjectLzcntAsm_Unix(ref destination, ref length);
				else
					InjectLzcntAsm_Windows(ref destination, ref length);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static void InjectLzcntAsm_Windows(ref void* destination, ref uint length)
			{
				const int Length = 5;
				ReadOnlySpan<byte> data = [
					0xF3, 0x48, 0x0F, 0xBD, 0xC1 // lzcnt eax, ecx
                ];
				if (length < Length)
					throw new AccessViolationException();
				destination = (byte*)destination + length - Length;
				fixed (byte* source = data)
					UnsafeHelper.CopyBlock(destination, source, Length);
				length = Length;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static void InjectLzcntAsm_Unix(ref void* destination, ref uint length)
			{
				const int Length = 5;
				ReadOnlySpan<byte> data = [
					0xF3, 0x48, 0x0F, 0xBD, 0xC7 // lzcnt rax, rdi
                ];
				if (length < Length)
					throw new AccessViolationException();
				destination = (byte*)destination + length - Length;
				fixed (byte* source = data)
					UnsafeHelper.CopyBlock(destination, source, Length);
				length = Length;
			}
		}
	}
}
#endif
#endif