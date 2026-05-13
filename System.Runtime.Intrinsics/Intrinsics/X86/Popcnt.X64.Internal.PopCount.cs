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
            if (_isUnix)
                InjectPopcntAsm_Unix(ref destination, ref length);
            else
                InjectPopcntAsm_Windows(ref destination, ref length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectPopcntAsm_Windows(ref void* destination, ref uint length)
        {
            const int Length = PopcntLength_Windows;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
#if NETSTANDARD2_0
            if (!_spanExists)
            {
                UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsArray.GetPopcntDataReference_Windows(), Length);
                return;
            }
#endif
            UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsSpan.GetPopcntDataReference_Windows(), Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectPopcntAsm_Unix(ref void* destination, ref uint length)
        {
            const int Length = PopcntLength_Unix;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
#if NETSTANDARD2_0
            if (!_spanExists)
            {
                UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsArray.GetPopcntDataReference_Unix(), Length);
                return;
            }
#endif
            UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsSpan.GetPopcntDataReference_Unix(), Length);
        }

#if NETSTANDARD2_0
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ref readonly byte GetPopcntDataReference_Windows()
                => ref UnsafeHelper.GetReference(PopcntData_Windows);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ref readonly byte GetPopcntDataReference_Unix()
                => ref UnsafeHelper.GetReference(PopcntData_Unix);
        }
#endif

        partial class StoreAsSpan
        {
            private static ReadOnlySpan<byte> PopcntData_Windows => new byte[PopcntLength_Windows]
            {
                0xF3, 0x48, 0x0F, 0xB8, 0xC1 // popcnt rax rcx
            };
            private static ReadOnlySpan<byte> PopcntData_Unix => new byte[PopcntLength_Unix]
            {
                0xF3, 0x48, 0x0F, 0xB8, 0xC7 // popcnt rax, rdi
            };

            [MethodImpl(Constants.SpanSourceInliningOptions)]
            public static ref readonly byte GetPopcntDataReference_Windows()
                => ref UnsafeHelper.GetReference(PopcntData_Windows);

            [MethodImpl(Constants.SpanSourceInliningOptions)]
            public static ref readonly byte GetPopcntDataReference_Unix()
                => ref UnsafeHelper.GetReference(PopcntData_Unix);
        }
    }
}
#endif
#endif