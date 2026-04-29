using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Runtime.Intrinsics.Helpers
{
    internal static class ThrowUtils
    {
        [DoesNotReturn]
        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowPlatformNotSupported() => throw new PlatformNotSupportedException();
    }
}
