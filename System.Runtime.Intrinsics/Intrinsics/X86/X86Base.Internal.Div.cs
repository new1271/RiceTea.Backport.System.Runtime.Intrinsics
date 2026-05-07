#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0
#if X86_ARCH || ANYCPU
#pragma warning disable IDE0130

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class X86Base
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectDivAsm(ref void* destination, ref uint length)
    {
        if (SoftDependencyHelper.SystemMemoryExists)
            StoreAsSpan.InjectDivAsm(ref destination, ref length);
        else
            StoreAsArray.InjectDivAsm(ref destination, ref length);
    }

    partial class StoreAsArray
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InjectDivAsm(ref void* destination, ref uint length)
        {
            if (IsUnix)
            {
#if B64_ARCH
                InjectDivAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
                InjectDivAsm_Unix_X86(ref destination, ref length);
#else
                if (IsX64)
                    InjectDivAsm_Unix_X64(ref destination, ref length);
                else
                    InjectDivAsm_Unix_X86(ref destination, ref length);
#endif
            }
            else
            {
#if B64_ARCH
                InjectDivAsm_Windows_X64(ref destination, ref length);
#elif B32_ARCH
                InjectDivAsm_Windows_X86(ref destination, ref length);
#else
                if (IsX64)
                    InjectDivAsm_Windows_X64(ref destination, ref length);
                else
                    InjectDivAsm_Windows_X86(ref destination, ref length);
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectDivAsm_Windows_X86(ref void* destination, ref uint length)
        {
            const int Length = 12;
            byte[] data = new byte[Length] {
                0x89, 0xC8, // mov eax, ecx
                0x8B, 0x4C, 0x24, 0x04, // mov ecx, dword ptr [esp+4]
                0xF7, 0x74, 0x24, 0x08, // div dword ptr [esp+8]
                0x89, 0x11 // mov dword ptr [ecx], edx
            };
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = data)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectDivAsm_Windows_X64(ref void* destination, ref uint length)
        {
            const int Length = 8;
            byte[] data = new byte[Length] {
                0x89, 0xC8, // mov eax, ecx
                0x41, 0xF7, 0xF0, // div r8d
                0x41, 0x89, 0x11 // mov dword ptr [r9], edx
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
        private static void InjectDivAsm_Unix_X86(ref void* destination, ref uint length)
        {
            const int Length = 18;
            byte[] data = new byte[Length] {
                0x8B, 0x44, 0x24, 0x04, // mov eax, dword ptr [esp+4]
                0x8B, 0x54, 0x24, 0x08, // mov edx, dword ptr [esp+8]
                0x8B, 0x4C, 0x24, 0x10, // mov ecx, dword ptr [esp+16]
                0xF7, 0x74, 0x24, 0x0C, // div dword ptr [esp+12]
                0x89, 0x11 // mov [ecx], edx
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
        private static void InjectDivAsm_Unix_X64(ref void* destination, ref uint length)
        {
            const int Length = 10;
            byte[] data = new byte[Length] {
                0x89, 0xF8, // mov eax, edi
                0x89, 0xD7, // mov edi, edx
                0x89, 0xF2, // mov edx, esi
                0xF7, 0xF7, // div edi
                0x89, 0x11 // mov dword ptr [rcx], edx
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
        public static void InjectDivAsm(ref void* destination, ref uint length)
        {
            if (IsUnix)
            {
#if B64_ARCH
                InjectDivAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
                InjectDivAsm_Unix_X86(ref destination, ref length);
#else
                if (IsX64)
                    InjectDivAsm_Unix_X64(ref destination, ref length);
                else
                    InjectDivAsm_Unix_X86(ref destination, ref length);
#endif
            }
            else
            {
#if B64_ARCH
                InjectDivAsm_Windows_X64(ref destination, ref length);
#elif B32_ARCH
                InjectDivAsm_Windows_X86(ref destination, ref length);
#else
                if (IsX64)
                    InjectDivAsm_Windows_X64(ref destination, ref length);
                else
                    InjectDivAsm_Windows_X86(ref destination, ref length);
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectDivAsm_Windows_X86(ref void* destination, ref uint length)
        {
            const int Length = 12;
            ReadOnlySpan<byte> data = [
                0x89, 0xC8, // mov eax, ecx
                0x8B, 0x4C, 0x24, 0x04, // mov ecx, dword ptr [esp+4]
                0xF7, 0x74, 0x24, 0x08, // div dword ptr [esp+8]
                0x89, 0x11 // mov dword ptr [ecx], edx
            ];
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = data)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectDivAsm_Windows_X64(ref void* destination, ref uint length)
        {
            const int Length = 8;
            ReadOnlySpan<byte> data = [
                0x89, 0xC8, // mov eax, ecx
                0x41, 0xF7, 0xF0, // div r8d
                0x41, 0x89, 0x11 // mov dword ptr [r9], edx
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
        private static void InjectDivAsm_Unix_X86(ref void* destination, ref uint length)
        {
            const int Length = 18;
            ReadOnlySpan<byte> data = [
                0x8B, 0x44, 0x24, 0x04, // mov eax, dword ptr [esp+4]
                0x8B, 0x54, 0x24, 0x08, // mov edx, dword ptr [esp+8]
                0x8B, 0x4C, 0x24, 0x10, // mov ecx, dword ptr [esp+16]
                0xF7, 0x74, 0x24, 0x0C, // div dword ptr [esp+12]
                0x89, 0x11 // mov [ecx], edx
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
        private static void InjectDivAsm_Unix_X64(ref void* destination, ref uint length)
        {
            const int Length = 10;
            ReadOnlySpan<byte> data = [
                0x89, 0xF8, // mov eax, edi
                0x89, 0xD7, // mov edi, edx
                0x89, 0xF2, // mov edx, esi
                0xF7, 0xF7, // div edi
                0x89, 0x11 // mov dword ptr [rcx], edx
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