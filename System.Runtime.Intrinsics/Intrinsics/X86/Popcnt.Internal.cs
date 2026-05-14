#if NETSTANDARD2_0_OR_GREATER
#if X86_ARCH || ANYCPU

using System.Diagnostics;
using System.Runtime.CompilerServices;

using RiceTea.Backport.Injection;
using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

using Fallbacks = RiceTea.Backport.Fallbacks.X86.Popcnt;

partial class Popcnt
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
        const int PopcntMask = 1 << 23;
        return (CpuId(0x00000001, 0).Ecx & PopcntMask) == PopcntMask;
    }

    public static new partial bool IsSupported
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isSupported;
    }


    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)] // 避免尾呼叫優化
    public static partial uint PopCount(uint value)
    {
        if (!_isSupported)
            ThrowUtils.ThrowPlatformNotSupported();

        InjectStart(value);
        return InjectEnd(Fallbacks.PopCount(value));

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
                    injectorFunc: &InjectPopcntAsm,
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

    private static partial class Store { }
}
#endif
#endif