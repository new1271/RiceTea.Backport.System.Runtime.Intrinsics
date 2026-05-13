
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class X86Base
{
    private const int BsrLength_Windows = 3;
#if B32_ARCH || ANYCPU
    private const int BsrLength_Unix_X86 = 5;
#endif
#if B64_ARCH || ANYCPU
    private const int BsrLength_Unix_X64 = 3;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBsrAsm(ref void* destination, ref uint length)
    {
        if (_isUnix)
        {
#if B64_ARCH
            InjectBsrAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
            InjectBsrAsm_Unix_X86(ref destination, ref length);
#else
            if (_isX64)
                InjectBsrAsm_Unix_X64(ref destination, ref length);
            else
                InjectBsrAsm_Unix_X86(ref destination, ref length);
#endif
        }
        else
            InjectBsrAsm_Windows(ref destination, ref length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBsrAsm_Windows(ref void* destination, ref uint length)
    {
        const int Length = BsrLength_Windows;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
#if NETSTANDARD2_0
        if (!_spanExists)
        {
            UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsArray.GetBsrDataReference_Windows(), Length);
            return;
        }
#endif
        UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsSpan.GetBsrDataReference_Windows(), Length);
    }

#if B32_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBsrAsm_Unix_X86(ref void* destination, ref uint length)
    {
        const int Length = BsrLength_Unix_X86;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
#if NETSTANDARD2_0
        if (!_spanExists)
        {
            UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsArray.GetBsrDataReference_Unix_X86(), Length);
            return;
        }
#endif
        UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsSpan.GetBsrDataReference_Unix_X86(), Length);
    }
#endif

#if B64_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBsrAsm_Unix_X64(ref void* destination, ref uint length)
    {
        const int Length = BsrLength_Unix_X64;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
#if NETSTANDARD2_0
        if (!_spanExists)
        {
            UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsArray.GetBsrDataReference_Unix_X64(), Length);
            return;
        }
#endif
        UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsSpan.GetBsrDataReference_Unix_X64(), Length);
    }
#endif

#if NETSTANDARD2_0
    partial class StoreAsArray
    {
        private static readonly byte[] BsrData_Windows = new byte[BsrLength_Windows]
        {
            0x0F, 0xBD, 0xC1 // bsr eax, ecx
        };
#if B32_ARCH || ANYCPU
        private static readonly byte[] BsrData_Unix_X86 = new byte[BsrLength_Unix_X86]
        {
            0x0F, 0xBD, 0x44, 0x24, 0x04 // bsr eax, dword ptr [esp+4]
        };
#endif
#if B64_ARCH || ANYCPU
        private static readonly byte[] BsrData_Unix_X64 = new byte[BsrLength_Unix_X64]
        {
            0x0F, 0xBD, 0xC7 // bsr eax, edi
        };
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetBsrDataReference_Windows()
            => ref UnsafeHelper.GetReference(BsrData_Windows);

#if B32_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetBsrDataReference_Unix_X86()
            => ref UnsafeHelper.GetReference(BsrData_Unix_X86);
#endif

#if B64_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetBsrDataReference_Unix_X64()
            => ref UnsafeHelper.GetReference(BsrData_Unix_X64);
#endif
    }
#endif

    partial class StoreAsSpan
    {
        private static ReadOnlySpan<byte> BsrData_Windows => new byte[BsrLength_Windows]
        {
            0x0F, 0xBD, 0xC1 // bsr eax, ecx
        };
#if B32_ARCH || ANYCPU
        private static ReadOnlySpan<byte> BsrData_Unix_X86 => new byte[BsrLength_Unix_X86]
        {
            0x0F, 0xBD, 0x44, 0x24, 0x04 // bsr eax, dword ptr [esp+4]
        };
#endif
#if B64_ARCH || ANYCPU
        private static ReadOnlySpan<byte> BsrData_Unix_X64 => new byte[BsrLength_Unix_X64]
        {
            0x0F, 0xBD, 0xC7 // bsr eax, edi
        };
#endif

        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetBsrDataReference_Windows()
            => ref UnsafeHelper.GetReference(BsrData_Windows);

#if B32_ARCH || ANYCPU
        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetBsrDataReference_Unix_X86()
            => ref UnsafeHelper.GetReference(BsrData_Unix_X86);
#endif

#if B64_ARCH || ANYCPU
        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetBsrDataReference_Unix_X64()
            => ref UnsafeHelper.GetReference(BsrData_Unix_X64);
#endif
    }
}
#endif
#endif