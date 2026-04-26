#if !NETSTANDARD2_1_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class Popcnt
{
#if (X86_ARCH || ANYCPU)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint InjectPopcntAsm(void* destination)
    {
        if (SoftDependencyHelper.SystemMemoryExists)
            return StoreAsSpan.InjectPopcntAsm(destination);
        else
            return StoreAsArray.InjectPopcntAsm(destination);
    }

    partial class StoreAsArray
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static uint InjectPopcntAsm(void* destination)
        {
            const int Length = 4;
            byte[] data = new byte[Length]
            {
                0xF3, 0x0F, 0xB8, 0xC1
            };
            fixed (byte* source = data)
                UnsafeHelper.CopyBlock(destination, source, Length);
            return Length;
        }
    }

    partial class StoreAsSpan
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static uint InjectPopcntAsm(void* destination)
        {
            const int Length = 4;
            ReadOnlySpan<byte> data = [
                0xF3, 0x0F, 0xB8, 0xC1
            ];
            fixed (byte* source = data)
                UnsafeHelper.CopyBlock(destination, source, Length);
            return Length;
        }
    }

#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectPopcntAsm(void* destination) => throw new PlatformNotSupportedException();
#endif
}
#endif