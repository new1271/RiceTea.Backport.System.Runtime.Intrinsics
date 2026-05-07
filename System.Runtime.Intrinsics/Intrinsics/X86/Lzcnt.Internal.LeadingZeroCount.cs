#if NETSTANDARD2_0_OR_GREATER
#if X86_ARCH || ANYCPU
#pragma warning disable IDE0130

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class Lzcnt
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectLzcntAsm(ref void* destination, ref uint length)
    {
        if (SoftDependencyHelper.SystemMemoryExists)
            StoreAsSpan.InjectLzcntAsm(ref destination, ref length);
        else
            StoreAsArray.InjectLzcntAsm(ref destination, ref length);
    }

    partial class StoreAsArray
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InjectLzcntAsm(ref void* destination, ref uint length)
        {
            if (IsUnix)
            {
#if B64_ARCH
                InjectLzcntAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
                InjectLzcntAsm_Unix_X86(ref destination, ref length);
#else
                if (IsX64)
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
            const int Length = 4;
            byte[] data = new byte[Length]
            {
                0xF3, 0x0F, 0xBD, 0xC1 // lzcnt eax, ecx
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
		private static void InjectLzcntAsm_Unix_X86(ref void* destination, ref uint length)
		{
			const int Length = 6;
			byte[] data = new byte[Length]
			{
				0xF3, 0x0F, 0xBD, 0x44, 0x24, 0x04 // lzcnt eax, dword ptr [esp+4]
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
        private static void InjectLzcntAsm_Unix_X64(ref void* destination, ref uint length)
        {
            const int Length = 4;
            byte[] data = new byte[Length]
            {
                 0xF3, 0x0F, 0xBD, 0xC7 // lzcnt eax, edi
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
        public static void InjectLzcntAsm(ref void* destination, ref uint length)
        {
            if (IsUnix)
            {
#if B64_ARCH
                InjectLzcntAsm_Unix_X64(ref destination, ref length);
#elif B32_ARCH
                InjectLzcntAsm_Unix_X86(ref destination, ref length);
#else
                if (IsX64)
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
            const int Length = 4;
            ReadOnlySpan<byte> data = [
                0xF3, 0x0F, 0xBD, 0xC1 // lzcnt eax, ecx
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
		private static void InjectLzcntAsm_Unix_X86(ref void* destination, ref uint length)
		{
			const int Length = 6;
			ReadOnlySpan<byte> data = [
				0xF3, 0x0F, 0xBD, 0x44, 0x24, 0x04 // lzcnt eax, dword ptr [esp+4]
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
        private static void InjectLzcntAsm_Unix_X64(ref void* destination, ref uint length)
        {
            const int Length = 4;
            ReadOnlySpan<byte> data = [
                0xF3, 0x0F, 0xBD, 0xC7 // lzcnt eax, edi
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