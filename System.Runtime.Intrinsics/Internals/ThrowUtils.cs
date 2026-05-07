using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RiceTea.Backport.Internals;

internal static class ThrowUtils
{
    [DoesNotReturn]
    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowPlatformNotSupported() => throw new PlatformNotSupportedException();

    [DoesNotReturn]
    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static T ThrowPlatformNotSupported<T>() => throw new PlatformNotSupportedException();
}
