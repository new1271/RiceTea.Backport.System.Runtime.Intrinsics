#if NETSTANDARD2_0_OR_GREATER
#if X86_ARCH || ANYCPU

using System.Diagnostics;
using System.Runtime.CompilerServices;

using RiceTea.Backport.Injection;
using RiceTea.Backport.Internals;

using Fallbacks = RiceTea.Backport.Fallbacks.X86.Lzcnt;

namespace System.Runtime.Intrinsics.X86;

partial class Lzcnt
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
		const int LzcntMask = 1 << 5;
		return (CpuId(unchecked((int)0x80000001), 0).Ecx & LzcntMask) == LzcntMask;
	}

	public static new partial bool IsSupported
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _isSupported;
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)] // 避免尾呼叫優化
    public static partial uint LeadingZeroCount(uint value)
	{
		if (!_isSupported)
			ThrowUtils.ThrowPlatformNotSupported();

		InjectStart(value);
		return InjectEnd(Fallbacks.LeadingZeroCount(value));

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
                    injectorFunc: &InjectLzcntAsm,
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