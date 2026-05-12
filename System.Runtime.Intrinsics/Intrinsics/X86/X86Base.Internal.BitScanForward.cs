
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class X86Base
{
    private const int BsfLength_Windows = 3;
#if (B32_ARCH || ANYCPU)
    private const int BsfLength_Unix_X86 = 5;
#endif
#if (B64_ARCH || ANYCPU)
    private const int BsfLength_Unix_X64 = 3;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBsfAsm(ref void* destination, ref uint length)
    {
        if (SoftDependencyHelper.SystemMemoryExists)
            StoreAsSpan.InjectBsfAsm(ref destination, ref length);
        else
            StoreAsArray.InjectBsfAsm(ref destination, ref length);
    }

    partial class StoreAsArray
    {
        private static readonly byte[] BsfData_Windows = new byte[BsfLength_Windows] 
        {
            0x0F, 0xBC, 0xC1 // bsf eax, ecx
        };
#if (B32_ARCH || ANYCPU)
        private static readonly byte[] BsfData_Unix_X86 = new byte[BsfLength_Unix_X86] 
        {
            0x0F, 0xBC, 0x44, 0x24, 0x04 // bsf eax, dword ptr [esp+4]
        };
#endif
#if (B64_ARCH || ANYCPU)
        private static readonly byte[] BsfData_Unix_X64 = new byte[BsfLength_Unix_X64] 
        {
            0x0F, 0xBC, 0xC7 // bsf eax, edi
        };
#endif

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InjectBsfAsm(ref void* destination, ref uint length)
        {
            if (IsUnix)
            {
#if B64_ARCH
                InjectBsfAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
                InjectBsfAsm_Unix_X86(ref destination, ref length);
#else
                if (IsX64)
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
            fixed (byte* source = BsfData_Windows)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }

#if B32_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsfAsm_Unix_X86(ref void* destination, ref uint length)
        {
            const int Length = BsfLength_Unix_X86;
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = BsfData_Unix_X86)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
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
            fixed (byte* source = BsfData_Unix_X64)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }
#endif
    }

    partial class StoreAsSpan
    {
        private static ReadOnlySpan<byte> BsfData_Windows =>
        [
            0x0F, 0xBC, 0xC1 // bsf eax, ecx
        ];
#if (B32_ARCH || ANYCPU)
        private static ReadOnlySpan<byte> BsfData_Unix_X86 =>
        [
            0x0F, 0xBC, 0x44, 0x24, 0x04 // bsf eax, dword ptr [esp+4]
        ];
#endif
#if (B64_ARCH || ANYCPU)
        private static ReadOnlySpan<byte> BsfData_Unix_X64 =>
        [
            0x0F, 0xBC, 0xC7 // bsf eax, edi
        ];
#endif

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InjectBsfAsm(ref void* destination, ref uint length)
        {
            if (IsUnix)
            {
#if B64_ARCH
                InjectBsfAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
                InjectBsfAsm_Unix_X86(ref destination, ref length);
#else
                if (IsX64)
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
            fixed (byte* source = BsfData_Windows)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }

#if B32_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsfAsm_Unix_X86(ref void* destination, ref uint length)
        {
            const int Length = BsfLength_Unix_X86;
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = BsfData_Unix_X86)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
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
            fixed (byte* source = BsfData_Unix_X64)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }
#endif
    }
}
#endif
#endif