#if !NETSTANDARD2_1_OR_GREATER
#if (X86_ARCH && B64_ARCH) || ANYCPU
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;

namespace System.Runtime.Intrinsics.X86;

partial class Bmi1
{
    unsafe partial class X64
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectTzcntAsm(ref void* destination, ref uint length)
        {
#if ANYCPU
            if (!PlatformHelper.IsX64)
                throw new PlatformNotSupportedException();
#endif
            if (SoftDependencyHelper.SystemMemoryExists)
                StoreAsSpan.InjectTzcntAsm(ref destination, ref length);
            else
                StoreAsArray.InjectTzcntAsm(ref destination, ref length);
        }

        partial class StoreAsArray
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void InjectTzcntAsm(ref void* destination, ref uint length)
            {
                if (UseUnixLogic)
                    InjectTzcntAsm_Unix(ref destination, ref length);
                else
                    InjectTzcntAsm_Windows(ref destination, ref length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static void InjectTzcntAsm_Windows(ref void* destination, ref uint length)
            {
                const int Length = 5;
                byte[] data = new byte[Length] {
                    0xF3, 0x48, 0x0F, 0xBC, 0xC1 // tzcnt rax rcx
                };
                if (length < Length)
                    throw new AccessViolationException();
                destination = (byte*)destination + length - Length;
                fixed (byte* source = data)
                    UnsafeHelper.CopyBlock(destination, source, Length);
                length = Length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static void InjectTzcntAsm_Unix(ref void* destination, ref uint length)
            {
                const int Length = 5;
                byte[] data = new byte[Length] {
					0xF3, 0x48, 0x0F, 0xBC, 0xC7 // tzcnt rax, rdi
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
            public static void InjectTzcntAsm(ref void* destination, ref uint length)
            {
				if (UseUnixLogic)
					InjectTzcntAsm_Unix(ref destination, ref length);
				else
					InjectTzcntAsm_Windows(ref destination, ref length);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static void InjectTzcntAsm_Windows(ref void* destination, ref uint length)
            {
                const int Length = 5;
                ReadOnlySpan<byte> data = [
                    0xF3, 0x48, 0x0F, 0xBC, 0xC1 // tzcnt eax, ecx
                ];
                if (length < Length)
                    throw new AccessViolationException();
                destination = (byte*)destination + length - Length;
                fixed (byte* source = data)
                    UnsafeHelper.CopyBlock(destination, source, Length);
                length = Length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InjectTzcntAsm_Unix(ref void* destination, ref uint length)
            {
                const int Length = 5;
                ReadOnlySpan<byte> data = [
					0xF3, 0x48, 0x0F, 0xBC, 0xC7 // tzcnt rax, rdi
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