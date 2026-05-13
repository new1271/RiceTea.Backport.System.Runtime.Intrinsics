#if NETSTANDARD2_0_OR_GREATER
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class Popcnt
{
    private const int PopcntLength_Windows = 4;
#if B32_ARCH || ANYCPU
    private const int PopcntLength_Unix_X86 = 6;
#endif
#if B64_ARCH || ANYCPU
    private const int PopcntLength_Unix_X64 = 4;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectPopcntAsm(ref void* destination, ref uint length)
    {
        if (_isUnix)
        {
#if B64_ARCH
            InjectPopcntAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
            InjectPopcntAsm_Unix_X86(ref destination, ref length);
#else
            if (_isX64)
                InjectPopcntAsm_Unix_X64(ref destination, ref length);
            else
                InjectPopcntAsm_Unix_X86(ref destination, ref length);
#endif
        }
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

#if B32_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectPopcntAsm_Unix_X86(ref void* destination, ref uint length)
    {
        const int Length = PopcntLength_Unix_X86;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
#if NETSTANDARD2_0
        if (!_spanExists)
        {
            UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsArray.GetPopcntDataReference_Unix_X86(), Length);
            return;
        }
#endif
        UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsSpan.GetPopcntDataReference_Unix_X86(), Length);
    }
#endif

#if B64_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectPopcntAsm_Unix_X64(ref void* destination, ref uint length)
    {
        const int Length = PopcntLength_Unix_X64;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
#if NETSTANDARD2_0
        if (!_spanExists)
        {
            UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsArray.GetPopcntDataReference_Unix_X64(), Length);
            return;
        }
#endif
        UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsSpan.GetPopcntDataReference_Unix_X64(), Length);
    }
#endif

#if NETSTANDARD2_0
    partial class StoreAsArray
    {
        private static readonly byte[] PopcntData_Windows = new byte[PopcntLength_Windows]
        {
            0xF3, 0x0F, 0xB8, 0xC1 // popcnt eax, ecx
        };
#if B32_ARCH || ANYCPU
        private static readonly byte[] PopcntData_Unix_X86 = new byte[PopcntLength_Unix_X86]
        {
            0xF3, 0x0F, 0xB8, 0x44, 0x24, 0x04 // popcnt eax, dword ptr [esp+4]
        };
#endif
#if B64_ARCH || ANYCPU
        private static readonly byte[] PopcntData_Unix_X64 = new byte[PopcntLength_Unix_X64]
        {
            0xF3, 0x0F, 0xB8, 0xC7 // popcnt eax, edi
        };
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetPopcntDataReference_Windows()
            => ref UnsafeHelper.GetReference(PopcntData_Windows);

#if B32_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetPopcntDataReference_Unix_X86()
            => ref UnsafeHelper.GetReference(PopcntData_Unix_X86);
#endif

#if B64_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetPopcntDataReference_Unix_X64()
            => ref UnsafeHelper.GetReference(PopcntData_Unix_X64);
#endif
    }
#endif

    partial class StoreAsSpan
    {
        private static ReadOnlySpan<byte> PopcntData_Windows => new byte[PopcntLength_Windows]
        {
            0xF3, 0x0F, 0xB8, 0xC1 // popcnt eax, ecx
        };
#if B32_ARCH || ANYCPU
        private static ReadOnlySpan<byte> PopcntData_Unix_X86 => new byte[PopcntLength_Unix_X86]
        {
            0xF3, 0x0F, 0xB8, 0x44, 0x24, 0x04 // popcnt eax, dword ptr [esp+4]
        };
#endif
#if B64_ARCH || ANYCPU
        private static ReadOnlySpan<byte> PopcntData_Unix_X64 => new byte[PopcntLength_Unix_X64]
        {
            0xF3, 0x0F, 0xB8, 0xC7 // popcnt eax, edi
        };
#endif

        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetPopcntDataReference_Windows()
            => ref UnsafeHelper.GetReference(PopcntData_Windows);

#if B32_ARCH || ANYCPU
        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetPopcntDataReference_Unix_X86()
            => ref UnsafeHelper.GetReference(PopcntData_Unix_X86);
#endif

#if B64_ARCH || ANYCPU
        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetPopcntDataReference_Unix_X64()
            => ref UnsafeHelper.GetReference(PopcntData_Unix_X64);
#endif
    }
}
#endif
#endif