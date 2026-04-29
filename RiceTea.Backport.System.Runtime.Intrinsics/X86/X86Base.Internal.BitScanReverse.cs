#if !NETSTANDARD2_1_OR_GREATER
#if X86_ARCH || ANYCPU
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class X86Base
{
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
            const int Length = 3;
            byte[] data = new byte[Length] {
                0x0F, 0xBD, 0xC1 // bsr eax, ecx
            };
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = data)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }

#if B32_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsrAsm_Unix_X86(ref void* destination, ref uint length)
        {
            const int Length = 5;
            byte[] data = new byte[Length] {
                0x0F, 0xBD, 0x44, 0x24, 0x04 // bsr eax, dword ptr [esp+4]
            };
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = data)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }
#endif

#if B64_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsrAsm_Unix_X64(ref void* destination, ref uint length)
        {
            const int Length = 3;
            byte[] data = new byte[Length] {
                0x0F, 0xBD, 0xC7 // bsr eax, edi
            };
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = data)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }
#endif
    }

    partial class StoreAsSpan
    {
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
            const int Length = 3;
            ReadOnlySpan<byte> data = [
                0x0F, 0xBD, 0xC1 // bsr eax, ecx
            ];
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = data)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }

#if B32_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsrAsm_Unix_X86(ref void* destination, ref uint length)
        {
            const int Length = 5;
            ReadOnlySpan<byte> data = [
                0x0F, 0xBD, 0x44, 0x24, 0x04 // bsr eax, dword ptr [esp+4]
            ];
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = data)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }
#endif

#if B64_ARCH || ANYCPU
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsrAsm_Unix_X64(ref void* destination, ref uint length)
        {
            const int Length = 3;
            ReadOnlySpan<byte> data = [
                0x0F, 0xBD, 0xC7 // bsr eax, edi
            ];
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = data)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }
#endif
    }
}
#endif
#endif