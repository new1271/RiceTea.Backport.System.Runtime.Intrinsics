#if NETSTANDARD2_0_OR_GREATER
#if X86_ARCH || ANYCPU

using System.Diagnostics;
using System.Runtime.CompilerServices;

using RiceTea.Backport.Injection;
using RiceTea.Backport.Internals;

using Fallbacks = RiceTea.Backport.Fallbacks.X86.Bmi1;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class Bmi1
{
    private static readonly bool _isSupported = CheckIsSupported();
    private static readonly bool _isUnix = PlatformHelper.IsUnix && (PlatformHelper.IsX64 || PlatformHelper.IsMono);
#if ANYCPU
    private static readonly bool _isX64 = PlatformHelper.IsX64;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CheckIsSupported()
    {
        if (!X86Base.IsSupported)
            return false;
        const int Bmi1Mask = 1 << 3;
        return (CpuId(7, 0).Ebx & Bmi1Mask) == Bmi1Mask;
    }

    public static new partial bool IsSupported
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isSupported;
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)] // 避免尾呼叫優化
    public static partial uint TrailingZeroCount(uint value)
    {
        if (!_isSupported)
            ThrowUtils.ThrowPlatformNotSupported();

        CallSiteInjector.OnInjectStart(value);
        return CallSiteInjector.OnInjectEnd(Fallbacks.TrailingZeroCount(value), &InjectTzcntAsm);
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)] // 避免尾呼叫優化
    public static partial uint AndNot(uint left, uint right)
    {
        if (!_isSupported)
            ThrowUtils.ThrowPlatformNotSupported();

        CallSiteInjector.OnInjectStart(left, right);
        return CallSiteInjector.OnInjectEnd(Fallbacks.AndNot(left, right), &InjectAndnAsm);
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial uint BitFieldExtract(uint value, byte start, byte length) => BitFieldExtract(value, (ushort)(start | (length << 8)));

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial uint BitFieldExtract(uint value, ushort control)
    {
        if (!_isSupported)
            ThrowUtils.ThrowPlatformNotSupported();

        CallSiteInjector.OnInjectStart(value, control);
        return CallSiteInjector.OnInjectEnd(Fallbacks.BitFieldExtract(value, control), &InjectBextrAsm);
    }

    private static partial class Store { }
}
#endif
#endif