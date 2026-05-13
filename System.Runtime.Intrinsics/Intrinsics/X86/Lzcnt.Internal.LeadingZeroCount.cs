#if NETSTANDARD2_0_OR_GREATER
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class Lzcnt
{
    private const int LzcntLength_Windows = 4;
#if B32_ARCH || ANYCPU
    private const int LzcntLength_Unix_X86 = 6;
#endif
#if B64_ARCH || ANYCPU
    private const int LzcntLength_Unix_X64 = 4;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectLzcntAsm(ref void* destination, ref uint length)
    {
        if (_isUnix)
        {
#if B64_ARCH
            InjectLzcntAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
            InjectLzcntAsm_Unix_X86(ref destination, ref length);
#else
            if (_isX64)
                InjectLzcntAsm_Unix_X64(ref destination, ref length);
            else
                InjectLzcntAsm_Unix_X86(ref destination, ref length);
#endif
        }
        else
            InjectLzcntAsm_Windows(ref destination, ref length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectLzcntAsm_Windows(ref void* destination, ref uint length)
    {
        const int Length = LzcntLength_Windows;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
#if NETSTANDARD2_0
        if (!_spanExists)
        {
            UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsArray.GetLzcntDataReference_Windows(), Length);
            return;
        }
#endif
        UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsSpan.GetLzcntDataReference_Windows(), Length);
    }

#if B32_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectLzcntAsm_Unix_X86(ref void* destination, ref uint length)
    {
        const int Length = LzcntLength_Unix_X86;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
#if NETSTANDARD2_0
        if (!_spanExists)
        {
            UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsArray.GetLzcntDataReference_Unix_X86(), Length);
            return;
        }
#endif
        UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsSpan.GetLzcntDataReference_Unix_X86(), Length);
    }
#endif

#if B64_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectLzcntAsm_Unix_X64(ref void* destination, ref uint length)
    {
        const int Length = LzcntLength_Unix_X64;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
#if NETSTANDARD2_0
        if (!_spanExists)
        {
            UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsArray.GetLzcntDataReference_Unix_X64(), Length);
            return;
        }
#endif
        UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsSpan.GetLzcntDataReference_Unix_X64(), Length);
    }
#endif

#if NETSTANDARD2_0
    partial class StoreAsArray
    {
        private static readonly byte[] LzcntData_Windows = new byte[LzcntLength_Windows]
        {
            0xF3, 0x0F, 0xBD, 0xC1 // lzcnt eax, ecx
        };
#if B32_ARCH || ANYCPU
        private static readonly byte[] LzcntData_Unix_X86 = new byte[LzcntLength_Unix_X86]
        {
            0xF3, 0x0F, 0xBD, 0x44, 0x24, 0x04 // lzcnt eax, dword ptr [esp+4]
        };
#endif
#if B64_ARCH || ANYCPU
        private static readonly byte[] LzcntData_Unix_X64 = new byte[LzcntLength_Unix_X64]
        {
            0xF3, 0x0F, 0xBD, 0xC7 // lzcnt eax, edi
        };
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetLzcntDataReference_Windows()
            => ref UnsafeHelper.GetReference(LzcntData_Windows);

#if B32_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetLzcntDataReference_Unix_X86()
            => ref UnsafeHelper.GetReference(LzcntData_Unix_X86);
#endif

#if B64_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetLzcntDataReference_Unix_X64()
            => ref UnsafeHelper.GetReference(LzcntData_Unix_X64);
#endif
    }
#endif

    partial class StoreAsSpan
    {
        private static ReadOnlySpan<byte> LzcntData_Windows => new byte[LzcntLength_Windows]
        {
            0xF3, 0x0F, 0xBD, 0xC1 // lzcnt eax, ecx
        };
#if B32_ARCH || ANYCPU
        private static ReadOnlySpan<byte> LzcntData_Unix_X86 => new byte[LzcntLength_Unix_X86]
        {
            0xF3, 0x0F, 0xBD, 0x44, 0x24, 0x04 // lzcnt eax, dword ptr [esp+4]
        };
#endif
#if B64_ARCH || ANYCPU
        private static ReadOnlySpan<byte> LzcntData_Unix_X64 => new byte[LzcntLength_Unix_X64]
        {
            0xF3, 0x0F, 0xBD, 0xC7 // lzcnt eax, edi
        };
#endif

        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetLzcntDataReference_Windows()
            => ref UnsafeHelper.GetReference(LzcntData_Windows);

#if B32_ARCH || ANYCPU
        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetLzcntDataReference_Unix_X86()
            => ref UnsafeHelper.GetReference(LzcntData_Unix_X86);
#endif

#if B64_ARCH || ANYCPU
        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetLzcntDataReference_Unix_X64()
            => ref UnsafeHelper.GetReference(LzcntData_Unix_X64);
#endif
    }
}
#endif
#endif