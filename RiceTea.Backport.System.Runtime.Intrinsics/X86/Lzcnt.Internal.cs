#if !NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Internals;
using System.Threading;

using InlineIL;

namespace System.Runtime.Intrinsics.X86;

partial class Lzcnt
{
    private static readonly object? _lzcntLock;
    private static readonly bool _isSupported;

    static Lzcnt()
    {
        if (CheckIsSupported())
        {
            _lzcntLock = new object();
            _isSupported = true;
        }
        else
        {
            _lzcntLock = null;
            _isSupported = false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CheckIsSupported()
    {
        if (!X86Base.IsSupported)
            return false;
        const int LzcntMask = 1 << 5;
        return (X86Base.CpuId(unchecked((int)0x80000001), 0).Ecx & LzcntMask) == LzcntMask;
    }

    public static partial bool IsSupported
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isSupported;
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
    public static partial uint LeadingZeroCount(uint value)
    {
        if (!_isSupported)
            throw new PlatformNotSupportedException();

        LeadingZeroCount_InjectStart(value);
        return LeadingZeroCount_InjectEnd(Fallbacks.LeadingZeroCount(value));
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
    private static unsafe void LeadingZeroCount_InjectStart(uint value)
    {
        CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
        LeadingZeroCount_EnterLock();
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe uint LeadingZeroCount_InjectEnd(uint value)
    {
        try
        {
            CallSiteInjector.InjectAsm(
                startAddress: CallSiteInjector.StartAddress,
                endAddress: CallSiteInjector.FindCallSite(),
                injectorFunc: &InjectLzcntAsm,
                exitLockFunc: &LeadingZeroCount_ExitLock);
            return value;
        }
        finally
        {
            LeadingZeroCount_ExitLock();
        }
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LeadingZeroCount_EnterLock() => Monitor.Enter(_lzcntLock!);

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LeadingZeroCount_ExitLock()
    {
        try
        {
            Monitor.Exit(_lzcntLock!);
        }
        catch (SynchronizationLockException)
        {
        }
    }

    private static partial class StoreAsArray { }

    private static partial class StoreAsSpan { }
}
#endif