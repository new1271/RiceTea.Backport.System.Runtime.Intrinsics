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
            if (_isUnix)
                InjectTzcntAsm_Unix(ref destination, ref length);
            else
                InjectTzcntAsm_Windows(ref destination, ref length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectTzcntAsm_Windows(ref void* destination, ref uint length)
        {
            const int Length = TzcntLength_Windows;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
#if NETSTANDARD2_0
            if (!_spanExists)
            {
                UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsArray.GetTzcntDataReference_Windows(), Length);
                return;
            }
#endif
            UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsSpan.GetTzcntDataReference_Windows(), Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectTzcntAsm_Unix(ref void* destination, ref uint length)
        {
            const int Length = TzcntLength_Unix;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
#if NETSTANDARD2_0
            if (!_spanExists)
            {
                UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsArray.GetTzcntDataReference_Unix(), Length);
                return;
            }
#endif
            UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsSpan.GetTzcntDataReference_Unix(), Length);
        }

#if NETSTANDARD2_0
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ref readonly byte GetTzcntDataReference_Windows()
                => ref UnsafeHelper.GetReference(TzcntData_Windows);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ref readonly byte GetTzcntDataReference_Unix()
                => ref UnsafeHelper.GetReference(TzcntData_Unix);
        }
#endif

        partial class StoreAsSpan
        {
            private static ReadOnlySpan<byte> TzcntData_Windows => new byte[TzcntLength_Windows]
            {
                0xF3, 0x48, 0x0F, 0xBC, 0xC1 // tzcnt rax rcx
            };
            private static ReadOnlySpan<byte> TzcntData_Unix => new byte[TzcntLength_Unix]
            {
                0xF3, 0x48, 0x0F, 0xBC, 0xC7 // tzcnt rax, rdi
            };

            [MethodImpl(Constants.SpanSourceInliningOptions)]
            public static ref readonly byte GetTzcntDataReference_Windows()
                => ref UnsafeHelper.GetReference(TzcntData_Windows);

            [MethodImpl(Constants.SpanSourceInliningOptions)]
            public static ref readonly byte GetTzcntDataReference_Unix()
                => ref UnsafeHelper.GetReference(TzcntData_Unix);
        }
    }
}
#endif
#endif