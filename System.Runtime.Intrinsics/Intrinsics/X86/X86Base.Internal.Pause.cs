#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class X86Base
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectPauseAsm(ref void* destination, ref uint length)
    {
        const int Length = sizeof(ushort);
        const ushort Data = 0x90_F3; // pause (f3 90)
        if (length < Length)
            ThrowUtils.ThrowAccessViolation();
        destination = (byte*)destination + length - Length;
        length = Length;
        UnsafeHelper.WriteUnaligned(destination, Data);
    }
}
#endif
#endif