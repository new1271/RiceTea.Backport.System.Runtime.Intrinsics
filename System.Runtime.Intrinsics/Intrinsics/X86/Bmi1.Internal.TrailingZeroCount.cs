#if NETSTANDARD2_0_OR_GREATER
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class Bmi1
{
    private const int TzcntLength_Windows = 4;
#if B32_ARCH || ANYCPU
    private const int TzcntLength_Unix_X86 = 6;
#endif
#if B64_ARCH || ANYCPU
    private const int TzcntLength_Unix_X64 = 4;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectTzcntAsm(ref void* destination, ref uint length)
    {
        if (_isUnix)
        {
#if B64_ARCH
            InjectTzcntAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
            InjectTzcntAsm_Unix_X86(ref destination, ref length);
#else
            if (_isX64)
                InjectTzcntAsm_Unix_X64(ref destination, ref length);
            else
                InjectTzcntAsm_Unix_X86(ref destination, ref length);
#endif
        }
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

#if B32_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectTzcntAsm_Unix_X86(ref void* destination, ref uint length)
    {
        const int Length = TzcntLength_Unix_X86;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
#if NETSTANDARD2_0
        if (!_spanExists)
        {
            UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsArray.GetTzcntDataReference_Unix_X86(), Length);
            return;
        }
#endif
        UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsSpan.GetTzcntDataReference_Unix_X86(), Length);
    }
#endif

#if B64_ARCH || ANYCPU
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectTzcntAsm_Unix_X64(ref void* destination, ref uint length)
    {
        const int Length = TzcntLength_Unix_X64;
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
#if NETSTANDARD2_0
        if (!_spanExists)
        {
            UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsArray.GetTzcntDataReference_Unix_X64(), Length);
            return;
        }
#endif
        UnsafeHelper.CopyBlockUnaligned(destination, in StoreAsSpan.GetTzcntDataReference_Unix_X64(), Length);
    }
#endif

#if NETSTANDARD2_0
    partial class StoreAsArray
    {
        private static readonly byte[] TzcntData_Windows = new byte[TzcntLength_Windows]
        {
            0xF3, 0x0F, 0xBC, 0xC1 // tzcnt eax, ecx
        };
#if B32_ARCH || ANYCPU
        private static readonly byte[] TzcntData_Unix_X86 = new byte[TzcntLength_Unix_X86]
        {
            0xF3, 0x0F, 0xBC, 0x44, 0x24, 0x04 // tzcnt eax, dword ptr [esp+4]
        };
#endif
#if B64_ARCH || ANYCPU
        private static readonly byte[] TzcntData_Unix_X64 = new byte[TzcntLength_Unix_X64]
        {
            0xF3, 0x0F, 0xBC, 0xC7 // tzcnt eax, edi
        };
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetTzcntDataReference_Windows()
            => ref UnsafeHelper.GetReference(TzcntData_Windows);

#if B32_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetTzcntDataReference_Unix_X86()
            => ref UnsafeHelper.GetReference(TzcntData_Unix_X86);
#endif

#if B64_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte GetTzcntDataReference_Unix_X64()
            => ref UnsafeHelper.GetReference(TzcntData_Unix_X64);
#endif
    }
#endif

    partial class StoreAsSpan
    {
        private static ReadOnlySpan<byte> TzcntData_Windows => new byte[TzcntLength_Windows]
        {
            0xF3, 0x0F, 0xBC, 0xC1 // tzcnt eax, ecx
        };
#if B32_ARCH || ANYCPU
        private static ReadOnlySpan<byte> TzcntData_Unix_X86 => new byte[TzcntLength_Unix_X86]
        {
            0xF3, 0x0F, 0xBC, 0x44, 0x24, 0x04 // tzcnt eax, dword ptr [esp+4]
        };
#endif
#if B64_ARCH || ANYCPU
        private static ReadOnlySpan<byte> TzcntData_Unix_X64 => new byte[TzcntLength_Unix_X64]
        {
            0xF3, 0x0F, 0xBC, 0xC7 // tzcnt eax, edi
        };
#endif

        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetTzcntDataReference_Windows()
            => ref UnsafeHelper.GetReference(TzcntData_Windows);

#if B32_ARCH || ANYCPU
        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetTzcntDataReference_Unix_X86()
            => ref UnsafeHelper.GetReference(TzcntData_Unix_X86);
#endif

#if B64_ARCH || ANYCPU
        [MethodImpl(Constants.SpanSourceInliningOptions)]
        public static ref readonly byte GetTzcntDataReference_Unix_X64()
            => ref UnsafeHelper.GetReference(TzcntData_Unix_X64);
#endif
    }
}
#endif
#endif