#if NETSTANDARD2_0_OR_GREATER
#if (X86_ARCH && B64_ARCH) || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

partial class Lzcnt
{
    unsafe partial class X64
    {
        private const int LzcntLength_Windows = 5;
        private const int LzcntLength_Unix = 5;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectLzcntAsm(ref void* destination, ref uint length)
        {
            if (_isUnix)
                InjectLzcntAsm_Unix(ref destination, ref length);
            else
                InjectLzcntAsm_Windows(ref destination, ref length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectLzcntAsm_Windows(ref void* destination, ref uint length)
        {
            const int Length = LzcntLength_Windows;
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            length = Length;
#if NETSTANDARD2_0
            if (!_spanExists)
            {
                UnsafeHelper.CopyBlock(destination, in StoreAsArray.GetLzcntDataReference_Windows(), Length);
                return;
            }
#endif
            UnsafeHelper.CopyBlock(destination, in StoreAsSpan.GetLzcntDataReference_Windows(), Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectLzcntAsm_Unix(ref void* destination, ref uint length)
        {
            const int Length = LzcntLength_Unix;
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            length = Length;
#if NETSTANDARD2_0
            if (!_spanExists)
            {
                UnsafeHelper.CopyBlock(destination, in StoreAsArray.GetLzcntDataReference_Unix(), Length);
                return;
            }
#endif
            UnsafeHelper.CopyBlock(destination, in StoreAsSpan.GetLzcntDataReference_Unix(), Length);
        }

#if NETSTANDARD2_0
        partial class StoreAsArray
        {
            private static readonly byte[] LzcntData_Windows = new byte[LzcntLength_Windows]
            {
                0xF3, 0x48, 0x0F, 0xBD, 0xC1 // lzcnt rax rcx
            };
            private static readonly byte[] LzcntData_Unix = new byte[LzcntLength_Unix]
            {
                0xF3, 0x48, 0x0F, 0xBD, 0xC7 // lzcnt rax, rdi
            };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ref readonly byte GetLzcntDataReference_Windows()
                => ref UnsafeHelper.GetReference(LzcntData_Windows);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ref readonly byte GetLzcntDataReference_Unix()
                => ref UnsafeHelper.GetReference(LzcntData_Unix);
        }
#endif

        partial class StoreAsSpan
        {
            private static ReadOnlySpan<byte> LzcntData_Windows => new byte[LzcntLength_Windows]
            {
                0xF3, 0x48, 0x0F, 0xBD, 0xC1 // lzcnt rax rcx
            };
            private static ReadOnlySpan<byte> LzcntData_Unix => new byte[LzcntLength_Unix]
            {
                0xF3, 0x48, 0x0F, 0xBD, 0xC7 // lzcnt rax, rdi
            };

            [MethodImpl(Constants.SpanSourceInliningOptions)]
            public static ref readonly byte GetLzcntDataReference_Windows()
                => ref UnsafeHelper.GetReference(LzcntData_Windows);

            [MethodImpl(Constants.SpanSourceInliningOptions)]
            public static ref readonly byte GetLzcntDataReference_Unix()
                => ref UnsafeHelper.GetReference(LzcntData_Unix);
        }
    }
}
#endif
#endif