#if !NETSTANDARD2_1_OR_GREATER
#if X86_ARCH || ANYCPU
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class Popcnt
{
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
            const int Length = 4;
            byte[] data = new byte[Length]
            {
                0xF3, 0x0F, 0xB8, 0xC1 // popcnt eax, ecx
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
		private static void InjectPopcntAsm_Unix_X86(ref void* destination, ref uint length)
		{
			const int Length = 6;
			byte[] data = new byte[Length]
			{
				0xF3, 0x0F, 0xB8, 0x44, 0x24, 0x04 // popcnt eax, dword ptr [esp+4]
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
        private static void InjectPopcntAsm_Unix_X64(ref void* destination, ref uint length)
        {
            const int Length = 4;
            byte[] data = new byte[Length]
            {
                 0xF3, 0x0F, 0xB8, 0xC7 // popcnt eax, edi
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InjectPopcntAsm_Windows(ref void* destination, ref uint length)
        {
            const int Length = 4;
            ReadOnlySpan<byte> data = [
                0xF3, 0x0F, 0xB8, 0xC1 // popcnt eax, ecx
            ];
            if (length < Length)
                throw new AccessViolationException();
            destination = (byte*)destination + length - Length;
            fixed (byte* source = data)
                UnsafeHelper.CopyBlock(destination, source, Length);
            length = Length;
        }

#if B32_ARCH || ANYCPU
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void InjectPopcntAsm_Unix_X86(ref void* destination, ref uint length)
		{
			const int Length = 6;
			ReadOnlySpan<byte> data = [
				0xF3, 0x0F, 0xB8, 0x44, 0x24, 0x04 // popcnt eax, dword ptr [esp+4]
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
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InjectPopcntAsm_Unix_X64(ref void* destination, ref uint length)
        {
            const int Length = 4;
            ReadOnlySpan<byte> data = [
                0xF3, 0x0F, 0xB8, 0xC7 // popcnt eax, edi
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