using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Runtime.Intrinsics.X86;

partial class Bmi1
{
    private static readonly bool _isSupported;

    static Bmi1()
    {
        if (!CheckIsSupported())
            return;
        _isSupported = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CheckIsSupported()
    {
        if (!X86Base.IsSupported)
            return false;
        const int Bmi1Mask = 1 << 3;
        return (X86Base.CpuId(7, 0).Ebx & Bmi1Mask) == Bmi1Mask;
    }

    public static partial bool IsSupported
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isSupported;
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static partial uint TrailingZeroCount(uint value)
    {
        if (!_isSupported)
            throw new PlatformNotSupportedException();
        InjectTzcntAsm();

        return (uint)Fallbacks.TrailingZeroCountSoftwareFallback(value);
    }

    private static partial class StoreAsArray { }

    private static partial class StoreAsSpan { }
}
