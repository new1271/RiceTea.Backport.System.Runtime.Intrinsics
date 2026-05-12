#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0
#if (X86_ARCH && B64_ARCH) || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

partial class X86Base
{
    unsafe partial class X64
    {
        private const int DivLength_Windows = 9;
        private const int DivLength_Unix = 15;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectDivAsm(ref void* destination, ref uint length)
        {
            if (_isUnix)
                InjectDivAsm_Unix(ref destination, ref length);
            else
                InjectDivAsm_Windows(ref destination, ref length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectDivAsm_Windows(ref void* destination, ref uint length)
        {
            const int Length = DivLength_Windows;
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            length = Length;
#if NETSTANDARD2_0
            if (!_spanExists)
            {
                UnsafeHelper.CopyBlock(destination, in StoreAsArray.GetDivDataReference_Windows(), Length);
                return;
            }
#endif
            UnsafeHelper.CopyBlock(destination, in StoreAsSpan.GetDivDataReference_Windows(), Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectDivAsm_Unix(ref void* destination, ref uint length)
        {
            const int Length = DivLength_Unix;
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            length = Length;
#if NETSTANDARD2_0
            if (!_spanExists)
            {
                UnsafeHelper.CopyBlock(destination, in StoreAsArray.GetDivDataReference_Unix(), Length);
                return;
            }
#endif
            UnsafeHelper.CopyBlock(destination, in StoreAsSpan.GetDivDataReference_Unix(), Length);
        }

#if NETSTANDARD2_0
        partial class StoreAsArray
        {
            private static readonly byte[] DivData_Windows = new byte[DivLength_Windows]
            {
                0x48, 0x89, 0xC8, // mov rax, rcx
                0x49, 0xF7, 0xF0, // div r8
                0x49, 0x89, 0x11 // mov qword ptr [r9], rdx
            };
            private static readonly byte[] DivData_Unix = new byte[DivLength_Unix]
            {
                0x48, 0x89, 0xF8, // mov rax, rdi
                0x48, 0x89, 0xD7, // mov rdi, rdx
                0x48, 0x89, 0xF2, // mov rdx, rsi
                0x48, 0xF7, 0xF7, // div rdi
                0x48, 0x89, 0x11 // mov qword ptr [rcx], rdx
            };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ref readonly byte GetDivDataReference_Windows()
                => ref UnsafeHelper.GetReference(DivData_Windows);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ref readonly byte GetDivDataReference_Unix()
                => ref UnsafeHelper.GetReference(DivData_Unix);
        }
#endif

        partial class StoreAsSpan
        {
            private static ReadOnlySpan<byte> DivData_Windows => new byte[DivLength_Windows]
            {
                0x48, 0x89, 0xC8, // mov rax, rcx
                0x49, 0xF7, 0xF0, // div r8
                0x49, 0x89, 0x11 // mov qword ptr [r9], rdx
            };
            private static ReadOnlySpan<byte> DivData_Unix => new byte[DivLength_Unix]
            {
                0x48, 0x89, 0xF8, // mov rax, rdi
                0x48, 0x89, 0xD7, // mov rdi, rdx
                0x48, 0x89, 0xF2, // mov rdx, rsi
                0x48, 0xF7, 0xF7, // div rdi
                0x48, 0x89, 0x11 // mov qword ptr [rcx], rdx
            };

            [MethodImpl(Constants.SpanSourceInliningOptions)]
            public static ref readonly byte GetDivDataReference_Windows()
                => ref UnsafeHelper.GetReference(DivData_Windows);

            [MethodImpl(Constants.SpanSourceInliningOptions)]
            public static ref readonly byte GetDivDataReference_Unix()
                => ref UnsafeHelper.GetReference(DivData_Unix);
        }
    }
}
#endif
#endif