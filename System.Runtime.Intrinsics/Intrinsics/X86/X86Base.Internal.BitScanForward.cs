
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class X86Base
{
    private const int BsfLength_Windows = 3;
#if B32_ARCH || ANYCPU
    private const int BsfLength_Unix_X86 = 5;
#endif
#if B64_ARCH || ANYCPU
    private const int BsfLength_Unix_X64 = 3;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBsfAsm(ref void* destination, ref uint length)
    {
        if (_isUnix)
        {
#if B64_ARCH
            InjectBsfAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
            InjectBsfAsm_Unix_X86(ref destination, ref length);
#else
            if (_isX64)
                InjectBsfAsm_Unix_X64(ref destination, ref length);
            else
                InjectBsfAsm_Unix_X86(ref destination, ref length);
#endif
        }
        else
            InjectBsfAsm_Windows(ref destination, ref length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBsfAsm_Windows(ref void* destination, ref uint length)
    {
        const int Length = BsfLength_Windows;
        if (length < Length)
            throw new AccessViolationException();
        destination = (byte*)destination + length - Length;
        length = Length;
#if NETSTANDARD2_0
        if (!_spanExists)
        {
            UnsafeHelper.CopyBlock(destination, in StoreAsArray.GetBsfDataReference_Windows(), Length);
            return;
        }
#endif
        UnsafeHelper.CopyBlock(destination, in StoreAsSpan.GetBsfDataReference_Windows(), Length);
    }

#if B32_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBsfAsm_Unix_X86(ref void* destination, ref uint length)
    {
        const int Length = BsfLength_Unix_X86;
        if (length < Length)
            throw new AccessViolationException();
        destination = (byte*)destination + length - Length;
        length = Length;
#if NETSTANDARD2_0
        if (!_spanExists)
        {
            UnsafeHelper.CopyBlock(destination, in StoreAsArray.GetBsfDataReference_Unix_X86(), Length);
            return;
        }
#endif
        UnsafeHelper.CopyBlock(destination, in StoreAsSpan.GetBsfDataReference_Unix_X86(), Length);
    }
#endif

#if B64_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBsfAsm_Unix_X64(ref void* destination, ref uint length)
    {
        const int Length = BsfLength_Unix_X64;
        if (length < Length)
            throw new AccessViolationException();
        destination = (byte*)destination + length - Length;
        length = Length;
#if NETSTANDARD2_0
        if (!_spanExists)
        {
            UnsafeHelper.CopyBlock(destination, in StoreAsArray.GetBsfDataReference_Unix_X64(), Length);
            return;
        }
#endif
        UnsafeHelper.CopyBlock(destination, in StoreAsSpan.GetBsfDataReference_Unix_X64(), Length);
    }
#endif

#if NETSTANDARD2_0
    partial class StoreAsArray
    {
        private static readonly byte[] BsfData_Windows = new byte[BsfLength_Windows] 
        {
            0x0F, 0xBC, 0xC1 // bsf eax, ecx
        };
#if B32_ARCH || ANYCPU
        private static readonly byte[] BsfData_Unix_X86 = new byte[BsfLength_Unix_X86] 
        {
            0x0F, 0xBC, 0x44, 0x24, 0x04 // bsf eax, dword ptr [esp+4]
        };
#endif
#if B64_ARCH || ANYCPU
        private static readonly byte[] BsfData_Unix_X64 = new byte[BsfLength_Unix_X64] 
        {
            0x0F, 0xBC, 0xC7 // bsf eax, edi
        };
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetBsfDataReference_Windows()
            => ref UnsafeHelper.GetReference(BsfData_Windows);

#if B32_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetBsfDataReference_Unix_X86()
            => ref UnsafeHelper.GetReference(BsfData_Unix_X86);
#endif

#if B64_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetBsfDataReference_Unix_X64()
            => ref UnsafeHelper.GetReference(BsfData_Unix_X64);
#endif
    }
#endif

    partial class StoreAsSpan
    {
        private static ReadOnlySpan<byte> BsfData_Windows => new byte[BsfLength_Windows]
        {
            0x0F, 0xBC, 0xC1 // bsf eax, ecx
        };
#if B32_ARCH || ANYCPU
        private static ReadOnlySpan<byte> BsfData_Unix_X86 => new byte[BsfLength_Unix_X86]
        {
            0x0F, 0xBC, 0x44, 0x24, 0x04 // bsf eax, dword ptr [esp+4]
        };
#endif
#if B64_ARCH || ANYCPU
        private static ReadOnlySpan<byte> BsfData_Unix_X64 => new byte[BsfLength_Unix_X64]
        {
            0x0F, 0xBC, 0xC7 // bsf eax, edi
        };
#endif

        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetBsfDataReference_Windows()
            => ref UnsafeHelper.GetReference(BsfData_Windows);

#if B32_ARCH || ANYCPU
        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetBsfDataReference_Unix_X86()
            => ref UnsafeHelper.GetReference(BsfData_Unix_X86);
#endif

#if B64_ARCH || ANYCPU
        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetBsfDataReference_Unix_X64()
            => ref UnsafeHelper.GetReference(BsfData_Unix_X64);
#endif
    }
}
#endif
#endif