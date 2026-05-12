
#if NETSTANDARD2_0_OR_GREATER
#if (X86_ARCH && B64_ARCH) || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

partial class Popcnt
{
    unsafe partial class X64
    {
        private const int PopcntLength_Windows = 5;
        private const int PopcntLength_Unix = 5;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectPopcntAsm(ref void* destination, ref uint length)
        {
#if ANYCPU
            if (!PlatformHelper.IsX64)
                throw new PlatformNotSupportedException();
#endif
            if (SoftDependencyHelper.SystemMemoryExists)
                StoreAsSpan.InjectPopcntAsm(ref destination, ref length);
            else
                StoreAsArray.InjectPopcntAsm(ref destination, ref length);
        }

        partial class StoreAsArray
        {
            private static readonly byte[] PopcntData_Windows = new byte[PopcntLength_Windows]
            {
                0xF3, 0x48, 0x0F, 0xB8, 0xC1 // popcnt rax rcx
            };
            private static readonly byte[] PopcntData_Unix = new byte[PopcntLength_Unix]
            {
                0xF3, 0x48, 0x0F, 0xB8, 0xC7 // popcnt rax, rdi
            };

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void InjectPopcntAsm(ref void* destination, ref uint length)
            {
                if (IsUnix)
                    InjectPopcntAsm_Unix(ref destination, ref length);
                else
                    InjectPopcntAsm_Windows(ref destination, ref length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InjectPopcntAsm_Windows(ref void* destination, ref uint length)
            {
                const int Length = PopcntLength_Windows;
                if (length < Length)
                    throw new AccessViolationException();
                destination = (byte*)destination + length - Length;
                fixed (byte* source = PopcntData_Windows)
                    UnsafeHelper.CopyBlock(destination, source, Length);
                length = Length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InjectPopcntAsm_Unix(ref void* destination, ref uint length)
            {
                const int Length = PopcntLength_Unix;
                if (length < Length)
                    throw new AccessViolationException();
                destination = (byte*)destination + length - Length;
                fixed (byte* source = PopcntData_Unix)
                    UnsafeHelper.CopyBlock(destination, source, Length);
                length = Length;
            }
        }

        partial class StoreAsSpan
        {
            private static ReadOnlySpan<byte> PopcntData_Windows =>
            [
                0xF3, 0x48, 0x0F, 0xB8, 0xC1 // popcnt rax rcx
            ];
            private static ReadOnlySpan<byte> PopcntData_Unix =>
            [
                0xF3, 0x48, 0x0F, 0xB8, 0xC7 // popcnt rax, rdi
            ];

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void InjectPopcntAsm(ref void* destination, ref uint length)
            {
                if (IsUnix)
                    InjectPopcntAsm_Unix(ref destination, ref length);
                else
                    InjectPopcntAsm_Windows(ref destination, ref length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InjectPopcntAsm_Windows(ref void* destination, ref uint length)
            {
                const int Length = PopcntLength_Windows;
                if (length < Length)
                    throw new AccessViolationException();
                destination = (byte*)destination + length - Length;
                fixed (byte* source = PopcntData_Windows)
                    UnsafeHelper.CopyBlock(destination, source, Length);
                length = Length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InjectPopcntAsm_Unix(ref void* destination, ref uint length)
            {
                const int Length = PopcntLength_Unix;
                if (length < Length)
                    throw new AccessViolationException();
                destination = (byte*)destination + length - Length;
                fixed (byte* source = PopcntData_Unix)
                    UnsafeHelper.CopyBlock(destination, source, Length);
                length = Length;
            }
        }
    }
}
#endif
#endif