#if !NETSTANDARD2_1_OR_GREATER
#if !(X86_ARCH || ANYCPU)
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;

namespace System.Runtime.Intrinsics.X86;

partial class Popcnt
{
	public static new partial bool IsSupported
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => false;
	}

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
    public static partial uint PopCount(uint value) => ThrowUtils.ThrowPlatformNotSupported<uint>();
}
#endif
#endif