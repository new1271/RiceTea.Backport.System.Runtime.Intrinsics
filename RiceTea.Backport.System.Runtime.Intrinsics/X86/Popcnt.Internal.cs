#if !NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Internals;
using System.Threading;

using InlineIL;

namespace System.Runtime.Intrinsics.X86;

partial class Popcnt
{
    private static readonly object? _popcntLock;
    private static readonly bool _isSupported;

    static Popcnt()
    {
        if (CheckIsSupported())
        {
            _popcntLock = new object();
            _isSupported = true;
        }
        else
        {
            _popcntLock = null;
            _isSupported = false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CheckIsSupported()
    {
        if (!X86Base.IsSupported)
            return false;
        const int PopcntMask = 1 << 23;
        return (X86Base.CpuId(0x00000001, 0).Ecx & PopcntMask) == PopcntMask;
    }

    public static partial bool IsSupported
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isSupported;
    }


    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
    public static partial uint PopCount(uint value)
    {
        if (!_isSupported)
            throw new PlatformNotSupportedException();

        PopCount_InjectStart(value);
        return PopCount_InjectEnd(Fallbacks.PopCount(value));
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
    private static unsafe void PopCount_InjectStart(uint value)
    {
        CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
        PopCount_EnterLock();
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe uint PopCount_InjectEnd(uint value)
    {
        try
        {
            CallSiteInjector.InjectAsm(
                startAddress: CallSiteInjector.StartAddress,
                endAddress: CallSiteInjector.FindCallSite(),
                injectorFunc: &InjectPopcntAsm,
                exitLockFunc: &PopCount_ExitLock);
            return value;
        }
        finally
        {
            PopCount_ExitLock();
        }
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void PopCount_EnterLock() => Monitor.Enter(_popcntLock!);

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void PopCount_ExitLock()
    {
        try
        {
            Monitor.Exit(_popcntLock!);
        }
        catch (SynchronizationLockException)
        {
        }
    }

    private static partial class StoreAsArray { }

    private static partial class StoreAsSpan { }
}
#endif