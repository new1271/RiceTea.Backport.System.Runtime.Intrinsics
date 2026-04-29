#if !NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;
using System.Runtime.Intrinsics.Internals;
using System.Threading;

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
		return (CpuId(0x00000001, 0).Ecx & PopcntMask) == PopcntMask;
	}

	public static new partial bool IsSupported
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
			ThrowUtils.ThrowPlatformNotSupported();

		InjectStart(value);
		return InjectEnd(Fallbacks.PopCount(value));

		[DebuggerHidden]
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
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
				CallSiteInjector.InjectAsm(
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

	private abstract partial class StoreAsArray : AssemblyCodeStoreBase { }

	private abstract partial class StoreAsSpan : AssemblyCodeStoreBase { }
}
#endif