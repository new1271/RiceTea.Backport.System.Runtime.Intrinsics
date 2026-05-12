
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class X86Base
{
    private const int BsrLength_Windows = 3;
#if (B32_ARCH || ANYCPU)
    private const int BsrLength_Unix_X86 = 5;
#endif
#if (B64_ARCH || ANYCPU)
    private const int BsrLength_Unix_X64 = 3;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBsrAsm(ref void* destination, ref uint length)
    {
        if (SoftDependencyHelper.SystemMemoryExists)
            StoreAsSpan.InjectBsrAsm(ref destination, ref length);
        else
            StoreAsArray.InjectBsrAsm(ref destination, ref length);
    }

    partial class StoreAsArray
    {
        private static readonly byte[] BsrData_Windows = new byte[BsrLength_Windows]
        {
            0x0F, 0xBD, 0xC1 // bsr eax, ecx
        };
#if (B32_ARCH || ANYCPU)
        private static readonly byte[] BsrData_Unix_X86 = new byte[BsrLength_Unix_X86]
        {
            0x0F, 0xBD, 0x44, 0x24, 0x04 // bsr eax, dword ptr [esp+4]
        };
#endif
#if (B64_ARCH || ANYCPU)
        private static readonly byte[] BsrData_Unix_X64 = new byte[BsrLength_Unix_X64]
        {
            0x0F, 0xBD, 0xC7 // bsr eax, edi
        };
#endif

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InjectBsrAsm(ref void* destination, ref uint length)
        {
            if (IsUnix)
            {
#if B64_ARCH
                InjectBsrAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
                InjectBsrAsm_Unix_X86(ref destination, ref length);
#else
                if (IsX64)
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
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = BsrData_Windows)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }

#if B32_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsrAsm_Unix_X86(ref void* destination, ref uint length)
        {
            const int Length = BsrLength_Unix_X86;
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = BsrData_Unix_X86)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }
#endif

#if B64_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsrAsm_Unix_X64(ref void* destination, ref uint length)
        {
            const int Length = BsrLength_Unix_X64;
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = BsrData_Unix_X64)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }
#endif
    }

    partial class StoreAsSpan
    {
        private static ReadOnlySpan<byte> BsrData_Windows =>
        [
            0x0F, 0xBD, 0xC1 // bsr eax, ecx
        ];
#if (B32_ARCH || ANYCPU)
        private static ReadOnlySpan<byte> BsrData_Unix_X86 =>
        [
            0x0F, 0xBD, 0x44, 0x24, 0x04 // bsr eax, dword ptr [esp+4]
        ];
#endif
#if (B64_ARCH || ANYCPU)
        private static ReadOnlySpan<byte> BsrData_Unix_X64 =>
        [
            0x0F, 0xBD, 0xC7 // bsr eax, edi
        ];
#endif

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InjectBsrAsm(ref void* destination, ref uint length)
        {
            if (IsUnix)
            {
#if B64_ARCH
                InjectBsrAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
                InjectBsrAsm_Unix_X86(ref destination, ref length);
#else
                if (IsX64)
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
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = BsrData_Windows)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }

#if B32_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsrAsm_Unix_X86(ref void* destination, ref uint length)
        {
            const int Length = BsrLength_Unix_X86;
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = BsrData_Unix_X86)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }
#endif

#if B64_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsrAsm_Unix_X64(ref void* destination, ref uint length)
        {
            const int Length = BsrLength_Unix_X64;
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = BsrData_Unix_X64)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }
#endif
    }
}
#endif
#endif