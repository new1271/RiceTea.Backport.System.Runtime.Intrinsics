#if !NETSTANDARD2_1_OR_GREATER
#if X86_ARCH || ANYCPU
using System.Runtime.CompilerServices;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class X86Base
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectPauseAsm(ref void* destination, ref uint length)
    {
        const int Length = sizeof(ushort);
        const ushort Data = 0x90_F3; // pause (f3 90)
        if (length < Length)
            throw new AccessViolationException();
        destination = (byte*)destination + length - Length;
        *(ushort*)destination = Data;
        length = Length;
    }
}
#endif
#endif