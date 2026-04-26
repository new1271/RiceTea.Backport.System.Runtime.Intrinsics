#if !NETSTANDARD2_1_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;
using System.Runtime.Intrinsics.Internals;

using InlineIL;

namespace System.Runtime.Intrinsics.X86;

partial class Bmi1
{
    unsafe partial class X64
    {
#if ((X86_ARCH && B64_ARCH) || ANYCPU)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint InjectTzcntAsm(void* destination)
        {
#if !B64_ARCH
            if (!PlatformHelper.IsX64)
                throw new PlatformNotSupportedException();
#endif
            if (SoftDependencyHelper.SystemMemoryExists)
                return StoreAsSpan.InjectTzcntAsm(destination);
            else
                return StoreAsArray.InjectTzcntAsm(destination);
        }

        partial class StoreAsArray
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static uint InjectTzcntAsm(void* destination)
            {
                const int Length = 5;
                byte[] data = new byte[Length] {
                    0xF3, 0x48, 0x0F, 0xBC, 0xC1
                };
                fixed (byte* source = data)
                    UnsafeHelper.CopyBlock(destination, source, Length);
                return Length;
            }
        }

        partial class StoreAsSpan
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static uint InjectTzcntAsm(void* destination)
            {
                const int Length = 5;
                ReadOnlySpan<byte> data = [
                    0xF3, 0x48, 0x0F, 0xBC, 0xC1
                ];
                fixed (byte* source = data)
                    UnsafeHelper.CopyBlock(destination, source, Length);
                return Length;
            }
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint InjectTzcntAsm(void* destination) => throw new PlatformNotSupportedException();
#endif
    }
}
#endif