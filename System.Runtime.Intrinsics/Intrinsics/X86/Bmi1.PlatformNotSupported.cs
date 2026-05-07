#if NETSTANDARD2_0_OR_GREATER
#if !(X86_ARCH || ANYCPU)
#pragma warning disable IDE0130

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;

namespace System.Runtime.Intrinsics.X86;

partial class Bmi1
{
    public static new partial bool IsSupported
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => false;
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial uint TrailingZeroCount(uint value) => ThrowUtils.ThrowPlatformNotSupported<uint>();
}
#endif
#endif