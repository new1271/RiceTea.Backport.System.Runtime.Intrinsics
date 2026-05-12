
#if NETSTANDARD2_0_OR_GREATER
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class Bmi1
{
    private const int TzcntLength_Windows = 4;
#if (B32_ARCH || ANYCPU)
    private const int TzcntLength_Unix_X86 = 6;
#endif
#if (B64_ARCH || ANYCPU)
    private const int TzcntLength_Unix_X64 = 4;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectTzcntAsm(ref void* destination, ref uint length)
    {
        if (SoftDependencyHelper.SystemMemoryExists)
            StoreAsSpan.InjectTzcntAsm(ref destination, ref length);
        else
            StoreAsArray.InjectTzcntAsm(ref destination, ref length);
    }

    partial class StoreAsArray
    {
        private static readonly byte[] TzcntData_Windows = new byte[TzcntLength_Windows]
        {
            0xF3, 0x0F, 0xBC, 0xC1 // tzcnt eax, ecx
        };
#if (B32_ARCH || ANYCPU)
        private static readonly byte[] TzcntData_Unix_X86 = new byte[TzcntLength_Unix_X86]
        {
            0xF3, 0x0F, 0xBC, 0x44, 0x24, 0x04 // tzcnt eax, dword ptr [esp+4]
        };
#endif
#if (B64_ARCH || ANYCPU)
        private static readonly byte[] TzcntData_Unix_X64 = new byte[TzcntLength_Unix_X64]
        {
            0xF3, 0x0F, 0xBC, 0xC7 // tzcnt eax, edi
        };
#endif

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InjectTzcntAsm(ref void* destination, ref uint length)
        {
            if (IsUnix)
            {
#if B64_ARCH
                InjectTzcntAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
                InjectTzcntAsm_Unix_X86(ref destination, ref length);
#else
                if (IsX64)
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
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = TzcntData_Windows)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }

#if B32_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectTzcntAsm_Unix_X86(ref void* destination, ref uint length)
        {
            const int Length = TzcntLength_Unix_X86;
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = TzcntData_Unix_X86)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }
#endif

#if B64_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectTzcntAsm_Unix_X64(ref void* destination, ref uint length)
        {
            const int Length = TzcntLength_Unix_X64;
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = TzcntData_Unix_X64)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }
#endif
    }

    partial class StoreAsSpan
    {
        private static ReadOnlySpan<byte> TzcntData_Windows =>
        [
            0xF3, 0x0F, 0xBC, 0xC1 // tzcnt eax, ecx
        ];
#if (B32_ARCH || ANYCPU)
        private static ReadOnlySpan<byte> TzcntData_Unix_X86 =>
        [
            0xF3, 0x0F, 0xBC, 0x44, 0x24, 0x04 // tzcnt eax, dword ptr [esp+4]
        ];
#endif
#if (B64_ARCH || ANYCPU)
        private static ReadOnlySpan<byte> TzcntData_Unix_X64 =>
        [
            0xF3, 0x0F, 0xBC, 0xC7 // tzcnt eax, edi
        ];
#endif

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InjectTzcntAsm(ref void* destination, ref uint length)
        {
            if (IsUnix)
            {
#if B64_ARCH
                InjectTzcntAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
                InjectTzcntAsm_Unix_X86(ref destination, ref length);
#else
                if (IsX64)
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
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = TzcntData_Windows)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }

#if B32_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectTzcntAsm_Unix_X86(ref void* destination, ref uint length)
        {
            const int Length = TzcntLength_Unix_X86;
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = TzcntData_Unix_X86)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }
#endif

#if B64_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectTzcntAsm_Unix_X64(ref void* destination, ref uint length)
        {
            const int Length = TzcntLength_Unix_X64;
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = TzcntData_Unix_X64)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }
#endif
    }
}
#endif
#endif