#if NETSTANDARD2_0_OR_GREATER
#if (X86_ARCH && B64_ARCH) || ANYCPU

using System.Diagnostics;
using System.Runtime.CompilerServices;

using RiceTea.Backport.Injection;
using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

using Fallbacks = RiceTea.Backport.Fallbacks.X86.Popcnt.X64;

partial class Popcnt
{
	unsafe partial class X64
    {
        private static readonly bool _isSupported = CheckIsSupported();
        private static readonly bool _isUnix = PlatformHelper.IsUnix;

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

            CallSiteInjector.OnInjectStart(value);
			return CallSiteInjector.OnInjectEnd(Fallbacks.PopCount(value), &InjectPopcntAsm);
        }

        private static partial class Store { }
	}
}
#endif
#endif