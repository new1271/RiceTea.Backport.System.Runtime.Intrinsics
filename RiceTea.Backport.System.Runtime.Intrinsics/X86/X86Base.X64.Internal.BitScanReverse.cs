#if !NETSTANDARD2_1_OR_GREATER
#if (X86_ARCH && B64_ARCH) || ANYCPU
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;

namespace System.Runtime.Intrinsics.X86;

partial class X86Base
{
    unsafe partial class X64
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsrAsm(ref void* destination, ref uint length)
        {
#if ANYCPU
            if (!Helpers.PlatformHelper.IsX64)
                ThrowUtils.ThrowPlatformNotSupported();
#endif
            if (SoftDependencyHelper.SystemMemoryExists)
                StoreAsSpan.InjectBsrAsm(ref destination, ref length);
            else
                StoreAsArray.InjectBsrAsm(ref destination, ref length);
        }

        partial class StoreAsArray
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void InjectBsrAsm(ref void* destination, ref uint length)
            {
                if (UseUnixLogic)
                    InjectBsrAsm_Unix(ref destination, ref length);
                else
                    InjectBsrAsm_Windows(ref destination, ref length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InjectBsrAsm_Windows(ref void* destination, ref uint length)
            {
                const int Length = 4;
                byte[] data = new byte[Length] {
                    0x48, 0x0F, 0xBD, 0xC1 // bsr rax, rcx
                };
                if (length < Length)
                    throw new AccessViolationException();
                destination = (byte*)destination + length - Length;
                fixed (byte* source = data)
                    UnsafeHelper.CopyBlock(destination, source, Length);
                length = Length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InjectBsrAsm_Unix(ref void* destination, ref uint length)
            {
                const int Length = 4;
                byte[] data = new byte[Length] {
                    0x48, 0x0F, 0xBD, 0xC7 // bsr rax, rdi
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
            public static void InjectBsrAsm(ref void* destination, ref uint length)
            {
                if (UseUnixLogic)
                    InjectBsrAsm_Unix(ref destination, ref length);
                else
                    InjectBsrAsm_Windows(ref destination, ref length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InjectBsrAsm_Windows(ref void* destination, ref uint length)
            {
                const int Length = 4;
                ReadOnlySpan<byte> data = [
                    0x48, 0x0F, 0xBD, 0xC1 // bsr rax, rcx
                ];
                if (length < Length)
                    throw new AccessViolationException();
                destination = (byte*)destination + length - Length;
                fixed (byte* source = data)
                    UnsafeHelper.CopyBlock(destination, source, Length);
                length = Length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InjectBsrAsm_Unix(ref void* destination, ref uint length)
            {
                const int Length = 4;
                ReadOnlySpan<byte> data = [
                    0x48, 0x0F, 0xBD, 0xC7 // bsr rax, rdi
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