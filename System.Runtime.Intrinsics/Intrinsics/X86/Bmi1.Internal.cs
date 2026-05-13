#if NETSTANDARD2_0_OR_GREATER
#if X86_ARCH || ANYCPU

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Backport.Injection;
using RiceTea.Backport.Internals;

using Fallbacks = RiceTea.Backport.Fallbacks.X86.Bmi1;

namespace System.Runtime.Intrinsics.X86;

partial class Bmi1
{
    private static readonly bool _isSupported = CheckIsSupported();
    private static readonly bool _isUnix = PlatformHelper.IsUnix && (PlatformHelper.IsX64 || PlatformHelper.IsMono);
#if ANYCPU
    private static readonly bool _isX64 = PlatformHelper.IsX64;
#endif
#if NETSTANDARD2_0
    private static readonly bool _spanExists = SoftDependencyHelper.SystemMemoryExists;
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

        InjectStart(value);
        return InjectEnd(Fallbacks.TrailingZeroCount(value));

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static unsafe void InjectStart(uint value)
        {
            void* address = CallSiteInjector.FindCallSite();
            ThreadStatics.StartAddress = address;
            CallSiteInjector.EnterAddressLock(address);
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static unsafe uint InjectEnd(uint value)
        {
            try
            {
                CallSiteInjector.Inject(
                    startAddress: ThreadStatics.StartAddress,
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
        static unsafe void ExitLock() => CallSiteInjector.LeaveAddressLock(ThreadStatics.StartAddress);
    }

#if NETSTANDARD2_0
    private static partial class StoreAsArray { }
#endif

    private static partial class StoreAsSpan { }
}
#endif
#endif