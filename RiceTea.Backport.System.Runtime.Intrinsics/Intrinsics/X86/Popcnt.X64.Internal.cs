#if NETSTANDARD2_0_OR_GREATER
#if (X86_ARCH && B64_ARCH) || ANYCPU
#pragma warning disable IDE0130

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
		private static readonly object? _popcntLock;
		private static readonly bool _isSupported;

		static X64()
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
				CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
				EnterLock();
			}

			[DebuggerHidden]
			[DebuggerStepThrough]
			[MethodImpl(MethodImplOptions.NoInlining)]
			static unsafe ulong InjectEnd(ulong value)
			{
				try
				{
					CallSiteInjector.Inject(
						startAddress: CallSiteInjector.StartAddress,
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
			static void EnterLock() => Monitor.Enter(_popcntLock!);

			[DebuggerHidden]
			[DebuggerStepThrough]
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static void ExitLock()
			{
				try
				{
					Monitor.Exit(_popcntLock!);
				}
				catch (SynchronizationLockException)
				{
				}
			}
		}

		private abstract partial class StoreAsArray : AssemblyCodeStoreBase.X64 { }

		private abstract partial class StoreAsSpan : AssemblyCodeStoreBase.X64 { }
	}
}
#endif
#endif