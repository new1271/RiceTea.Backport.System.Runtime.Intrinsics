#if NETSTANDARD2_0_OR_GREATER
#if (X86_ARCH && B64_ARCH) || ANYCPU

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Backport.Injection;
using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

using Fallbacks = RiceTea.Backport.Fallbacks.X86.Popcnt.X64;

partial class Popcnt
{
	partial class X64
    {
        private static readonly bool _isSupported = CheckIsSupported();
        private static readonly bool _isUnix = PlatformHelper.IsUnix;
#if NETSTANDARD2_0
        private static readonly bool _spanExists = SoftDependencyHelper.SystemMemoryExists;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool CheckIsSupported()
		{
			if (!X86Base.X64.IsSupported)
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
        public static partial ulong PopCount(ulong value)
		{
			if (!_isSupported)
				ThrowUtils.ThrowPlatformNotSupported();

			InjectStart(value);
			return InjectEnd(Fallbacks.PopCount(value));

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
            static unsafe void ExitLock()
            {
                try
                {
                    CallSiteInjector.LeaveAddressLock(ThreadStatics.StartAddress);
                }
                catch (SynchronizationLockException)
                {
                }
            }
        }

#if NETSTANDARD2_0
        private static partial class StoreAsArray { }
#endif

        private static partial class StoreAsSpan { }
	}
}
#endif
#endif