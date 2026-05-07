#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0
#if (X86_ARCH && B64_ARCH) || ANYCPU
#pragma warning disable IDE0130

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

partial class X86Base
{
    unsafe partial class X64
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectIDivAsm(ref void* destination, ref uint length)
        {
#if ANYCPU
            if (!PlatformHelper.IsX64)
                ThrowUtils.ThrowPlatformNotSupported();
#endif
            if (SoftDependencyHelper.SystemMemoryExists)
                StoreAsSpan.InjectIDivAsm(ref destination, ref length);
            else
                StoreAsArray.InjectIDivAsm(ref destination, ref length);
        }

        partial class StoreAsArray
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void InjectIDivAsm(ref void* destination, ref uint length)
            {
                if (IsUnix)
                    InjectIDivAsm_Unix(ref destination, ref length);
                else
                    InjectIDivAsm_Windows(ref destination, ref length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InjectIDivAsm_Windows(ref void* destination, ref uint length)
            {
                const int Length = 9;
                byte[] data = new byte[Length] {
                    0x48, 0x89, 0xC8, // mov rax, rcx
                    0x49, 0xF7, 0xF8, // idiv r8
                    0x49, 0x89, 0x11 // mov qword ptr [r9], rdx
                };
                if (length < Length)
                    throw new AccessViolationException();
                destination = (byte*)destination + length - Length;
                fixed (byte* source = data)
                    UnsafeHelper.CopyBlock(destination, source, Length);
                length = Length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InjectIDivAsm_Unix(ref void* destination, ref uint length)
            {
                const int Length = 15;
                byte[] data = new byte[Length] {
                    0x48, 0x89, 0xF8, // mov rax, rdi
                    0x48, 0x89, 0xD7, // mov rdi, rdx
                    0x48, 0x89, 0xF2, // mov rdx, rsi
                    0x48, 0xF7, 0xFF, // idiv rdi
                    0x48, 0x89, 0x11 // mov qword ptr [rcx], rdx
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
            public static void InjectIDivAsm(ref void* destination, ref uint length)
            {
                if (IsUnix)
                    InjectIDivAsm_Unix(ref destination, ref length);
                else
                    InjectIDivAsm_Windows(ref destination, ref length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InjectIDivAsm_Windows(ref void* destination, ref uint length)
            {
                const int Length = 9;
                ReadOnlySpan<byte> data = [
                    0x48, 0x89, 0xC8, // mov rax, rcx
                    0x49, 0xF7, 0xF8, // idiv r8
                    0x49, 0x89, 0x11 // mov qword ptr [r9], rdx
                ];
                if (length < Length)
                    throw new AccessViolationException();
                destination = (byte*)destination + length - Length;
                fixed (byte* source = data)
                    UnsafeHelper.CopyBlock(destination, source, Length);
                length = Length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InjectIDivAsm_Unix(ref void* destination, ref uint length)
            {
                const int Length = 15;
                ReadOnlySpan<byte> data = [
                    0x48, 0x89, 0xF8, // mov rax, rdi
                    0x48, 0x89, 0xD7, // mov rdi, rdx
                    0x48, 0x89, 0xF2, // mov rdx, rsi
                    0x48, 0xF7, 0xFF, // idiv rdi
                    0x48, 0x89, 0x11 // mov qword ptr [rcx], rdx
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