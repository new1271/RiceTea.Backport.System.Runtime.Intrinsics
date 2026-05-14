#if NETSTANDARD2_0_OR_GREATER
#if (X86_ARCH && B64_ARCH) || ANYCPU

using System.Diagnostics;
using System.Runtime.CompilerServices;

using RiceTea.Backport.Injection;
using RiceTea.Backport.Internals;

using Fallbacks = RiceTea.Backport.Fallbacks.X86.Lzcnt.X64;

namespace System.Runtime.Intrinsics.X86;

partial class Lzcnt
{
	partial class X64
    {
        private static readonly bool _isSupported = CheckIsSupported();
        private static readonly bool _isUnix = PlatformHelper.IsUnix;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool CheckIsSupported()
		{
			if (!X86Base.X64.IsSupported)
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
        public static partial ulong LeadingZeroCount(ulong value)
		{
			if (!_isSupported)
				ThrowUtils.ThrowPlatformNotSupported();

			InjectStart(value);
			return InjectEnd(Fallbacks.LeadingZeroCount(value));

            [DebuggerHidden]
            [DebuggerStepThrough]
            [MethodImpl(MethodImplOptions.NoInlining)]
            static unsafe void InjectStart(ulong value)
            {
                void* address = CallSiteInjector.FindCallSite();
                ThreadStatics.StartAddress = address;
                CallSiteInjector.EnterAddressLock(address);
            }

            [DebuggerHidden]
            [DebuggerStepThrough]
            [MethodImpl(MethodImplOptions.NoInlining)]
            static unsafe ulong InjectEnd(ulong value)
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
}
#endif
#endif