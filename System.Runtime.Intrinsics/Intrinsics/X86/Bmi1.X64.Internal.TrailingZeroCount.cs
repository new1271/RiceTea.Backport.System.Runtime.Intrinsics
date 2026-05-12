
#if NETSTANDARD2_0_OR_GREATER
#if (X86_ARCH && B64_ARCH) || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

partial class Bmi1
{
    unsafe partial class X64
    {
        private const int TzcntLength_Windows = 5;
        private const int TzcntLength_Unix = 5;

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
            private static readonly byte[] TzcntData_Windows = new byte[TzcntLength_Windows]
            {
                0xF3, 0x48, 0x0F, 0xBC, 0xC1 // tzcnt rax rcx
            };
            private static readonly byte[] TzcntData_Unix = new byte[TzcntLength_Unix]
            {
                0xF3, 0x48, 0x0F, 0xBC, 0xC7 // tzcnt rax, rdi
            };

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void InjectTzcntAsm(ref void* destination, ref uint length)
            {
                if (IsUnix)
                    InjectTzcntAsm_Unix(ref destination, ref length);
                else
                    InjectTzcntAsm_Windows(ref destination, ref length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static void InjectTzcntAsm_Windows(ref void* destination, ref uint length)
            {
                const int Length = TzcntLength_Windows;
                if (length < Length)
                    throw new AccessViolationException();
                destination = (byte*)destination + length - Length;
                fixed (byte* source = TzcntData_Windows)
                    UnsafeHelper.CopyBlock(destination, source, Length);
                length = Length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static void InjectTzcntAsm_Unix(ref void* destination, ref uint length)
            {
                const int Length = TzcntLength_Unix;
                if (length < Length)
                    throw new AccessViolationException();
                destination = (byte*)destination + length - Length;
                fixed (byte* source = TzcntData_Unix)
                    UnsafeHelper.CopyBlock(destination, source, Length);
                length = Length;
            }
        }

        partial class StoreAsSpan
        {
            private static ReadOnlySpan<byte> TzcntData_Windows =>
            [
                0xF3, 0x48, 0x0F, 0xBC, 0xC1 // tzcnt rax rcx
            ];
            private static ReadOnlySpan<byte> TzcntData_Unix =>
            [
                0xF3, 0x48, 0x0F, 0xBC, 0xC7 // tzcnt rax, rdi
            ];

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void InjectTzcntAsm(ref void* destination, ref uint length)
            {
                if (IsUnix)
                    InjectTzcntAsm_Unix(ref destination, ref length);
                else
                    InjectTzcntAsm_Windows(ref destination, ref length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InjectTzcntAsm_Windows(ref void* destination, ref uint length)
            {
                const int Length = TzcntLength_Windows;
                if (length < Length)
                    throw new AccessViolationException();
                destination = (byte*)destination + length - Length;
                fixed (byte* source = TzcntData_Windows)
                    UnsafeHelper.CopyBlock(destination, source, Length);
                length = Length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InjectTzcntAsm_Unix(ref void* destination, ref uint length)
            {
                const int Length = TzcntLength_Unix;
                if (length < Length)
                    throw new AccessViolationException();
                destination = (byte*)destination + length - Length;
                fixed (byte* source = TzcntData_Unix)
                    UnsafeHelper.CopyBlock(destination, source, Length);
                length = Length;
            }
        }
    }
}
#endif
#endif