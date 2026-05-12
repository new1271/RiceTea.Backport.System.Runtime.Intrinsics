
#if NETSTANDARD2_0_OR_GREATER
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class Popcnt
{
    private const int PopcntLength_Windows = 4;
#if (B32_ARCH || ANYCPU)
    private const int PopcntLength_Unix_X86 = 6;
#endif
#if (B64_ARCH || ANYCPU)
    private const int PopcntLength_Unix_X64 = 4;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectPopcntAsm(ref void* destination, ref uint length)
    {
        if (SoftDependencyHelper.SystemMemoryExists)
            StoreAsSpan.InjectPopcntAsm(ref destination, ref length);
        else
            StoreAsArray.InjectPopcntAsm(ref destination, ref length);
    }

    partial class StoreAsArray
    {
        private static readonly byte[] PopcntData_Windows = new byte[PopcntLength_Windows]
        {
            0xF3, 0x0F, 0xB8, 0xC1 // popcnt eax, ecx
        };
#if (B32_ARCH || ANYCPU)
        private static readonly byte[] PopcntData_Unix_X86 = new byte[PopcntLength_Unix_X86]
        {
            0xF3, 0x0F, 0xB8, 0x44, 0x24, 0x04 // popcnt eax, dword ptr [esp+4]
        };
#endif
#if (B64_ARCH || ANYCPU)
        private static readonly byte[] PopcntData_Unix_X64 = new byte[PopcntLength_Unix_X64]
        {
            0xF3, 0x0F, 0xB8, 0xC7 // popcnt eax, edi
        };
#endif

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InjectPopcntAsm(ref void* destination, ref uint length)
        {
            if (IsUnix)
            {
#if B64_ARCH
                InjectPopcntAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
                InjectPopcntAsm_Unix_X86(ref destination, ref length);
#else
                if (IsX64)
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
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = PopcntData_Windows)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }

#if B32_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectPopcntAsm_Unix_X86(ref void* destination, ref uint length)
        {
            const int Length = PopcntLength_Unix_X86;
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = PopcntData_Unix_X86)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }
#endif

#if B64_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectPopcntAsm_Unix_X64(ref void* destination, ref uint length)
        {
            const int Length = PopcntLength_Unix_X64;
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = PopcntData_Unix_X64)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }
#endif
    }

    partial class StoreAsSpan
    {
        private static ReadOnlySpan<byte> PopcntData_Windows =>
        [
            0xF3, 0x0F, 0xB8, 0xC1 // popcnt eax, ecx
        ];
#if (B32_ARCH || ANYCPU)
        private static ReadOnlySpan<byte> PopcntData_Unix_X86 =>
        [
            0xF3, 0x0F, 0xB8, 0x44, 0x24, 0x04 // popcnt eax, dword ptr [esp+4]
        ];
#endif
#if (B64_ARCH || ANYCPU)
        private static ReadOnlySpan<byte> PopcntData_Unix_X64 =>
        [
            0xF3, 0x0F, 0xB8, 0xC7 // popcnt eax, edi
        ];
#endif

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InjectPopcntAsm(ref void* destination, ref uint length)
        {
            if (IsUnix)
            {
#if B64_ARCH
                InjectPopcntAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
                InjectPopcntAsm_Unix_X86(ref destination, ref length);
#else
                if (IsX64)
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
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = PopcntData_Windows)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }

#if B32_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectPopcntAsm_Unix_X86(ref void* destination, ref uint length)
        {
            const int Length = PopcntLength_Unix_X86;
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = PopcntData_Unix_X86)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }
#endif

#if B64_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectPopcntAsm_Unix_X64(ref void* destination, ref uint length)
        {
            const int Length = PopcntLength_Unix_X64;
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = PopcntData_Unix_X64)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }
#endif
    }
}
#endif
#endif