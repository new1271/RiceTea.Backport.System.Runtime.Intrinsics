#if NETSTANDARD2_0_OR_GREATER
#if X86_ARCH || ANYCPU
#pragma warning disable IDE0130

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Backport.Injection;
using RiceTea.Backport.Internals;

using Fallbacks = RiceTea.Backport.Fallbacks.X86.Bmi1;

namespace System.Runtime.Intrinsics.X86;

partial class Bmi1
{
    private static readonly object? _tzcntLock;
    private static readonly bool _isSupported;

    static Bmi1()
    {
        if (CheckIsSupported())
        {
            _isSupported = true;
            _tzcntLock = new object();
        }
        else
        {
            _isSupported = false;
            _tzcntLock = null;
        }
    }

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial uint TrailingZeroCount(uint value)
    {
        if (!_isSupported)
            ThrowUtils.ThrowPlatformNotSupported();

        InjectStart(value);
        return InjectEnd(Fallbacks.TrailingZeroCount(value));

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static unsafe void InjectStart(uint value)
        {
            CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
            EnterLock();
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static unsafe uint InjectEnd(uint value)
        {
            try
            {
                CallSiteInjector.Inject(
                    startAddress: CallSiteInjector.StartAddress,
                    endAddress: CallSiteInjector.FindCallSite(),
                    injectorFunc: &InjectTzcntAsm,
                    exitLockFunc: &ExitLock);
                return value;
            }
            finally
            {
                ExitLock();
            }
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void EnterLock() => Monitor.Enter(_tzcntLock!);

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ExitLock()
        {
            try
            {
                Monitor.Exit(_tzcntLock!);
            }
            catch (SynchronizationLockException)
            {
            }
        }
    }

    private abstract partial class StoreAsArray : AssemblyCodeStoreBase { }

    private abstract partial class StoreAsSpan : AssemblyCodeStoreBase { }
}
#endif
#endif