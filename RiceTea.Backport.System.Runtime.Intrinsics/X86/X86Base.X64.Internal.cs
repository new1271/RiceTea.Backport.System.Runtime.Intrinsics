#if !NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;
using System.Runtime.Intrinsics.Internals;
using System.Threading;

namespace System.Runtime.Intrinsics.X86;

partial class X86Base
{
	unsafe partial class X64
	{
		private static readonly bool _isSupported;

		static X64()
		{
			if (!PlatformHelper.IsX64)
				return;
			_isSupported = true;
		}

		public static partial bool IsSupported => _isSupported;

		[DebuggerHidden]
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
		public static partial ulong BitScanForward(ulong value)
		{
			if (!_isSupported)
				ThrowUtils.ThrowPlatformNotSupported();

			InjectStart(value);
			return InjectEnd(Fallbacks.BitScanForward(value));

			[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
			static void InjectStart(ulong value)
			{
				CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
				EnterLock();
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			static ulong InjectEnd(ulong value)
			{
				try
				{
					CallSiteInjector.InjectAsm(
						startAddress: CallSiteInjector.StartAddress,
						endAddress: CallSiteInjector.FindCallSite(),
						injectorFunc: &InjectBsfAsm,
						exitLockFunc: &ExitLock);
					return value;
				}
				finally
				{
					ExitLock();
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static void EnterLock() => Monitor.Enter(_bsfLock!);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static void ExitLock()
			{
				try
				{
					Monitor.Exit(_bsfLock!);
				}
				catch (SynchronizationLockException)
				{
				}
			}
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
		public static partial ulong BitScanReverse(ulong value)
		{
			if (!_isSupported)
				ThrowUtils.ThrowPlatformNotSupported();

			InjectStart(value);
			return InjectEnd(Fallbacks.BitScanReverse(value));

			[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
			static void InjectStart(ulong value)
			{
				CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
				EnterLock();
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			static ulong InjectEnd(ulong value)
			{
				try
				{
					CallSiteInjector.InjectAsm(
						startAddress: CallSiteInjector.StartAddress,
						endAddress: CallSiteInjector.FindCallSite(),
						injectorFunc: &InjectBsrAsm,
						exitLockFunc: &ExitLock);
					return value;
				}
				finally
				{
					ExitLock();
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static void EnterLock() => Monitor.Enter(_bsrLock!);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static void ExitLock()
			{
				try
				{
					Monitor.Exit(_bsrLock!);
				}
				catch (SynchronizationLockException)
				{
				}
			}
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
		public static partial (long Quotient, long Remainder) DivRem(ulong lower, long upper, long divisor)
		{
			if (!_isSupported)
				ThrowUtils.ThrowPlatformNotSupported();

			InjectDiv128Asm();

			if (divisor == 0)
				throw new DivideByZeroException();

			if (upper == ((long)lower >> 63))
				return ((long)lower / divisor, (long)lower % divisor);

			bool isNegativeDividend = upper < 0;
			bool isNegativeDivisor = divisor < 0;
			bool isNegativeQuotient = isNegativeDividend ^ isNegativeDivisor;

			ulong uUpper = (ulong)upper;
			ulong uLower = lower;
			if (isNegativeDividend)
			{
				uLower = ~uLower;
				uUpper = ~uUpper;
				if (++uLower == 0) uUpper++;
			}
			ulong uDivisor = (ulong)(isNegativeDivisor ? -divisor : divisor);

			if (uUpper >= uDivisor) throw new OverflowException();

			(ulong uQ, ulong uR) = DivRem(uLower, uUpper, uDivisor);

			long q = (long)uQ;
			long r = (long)uR;

			if (isNegativeQuotient) q = -q;
			if (isNegativeDividend) r = -r;

			return (q, r);
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
		public static partial (ulong Quotient, ulong Remainder) DivRem(ulong lower, ulong upper, ulong divisor)
		{
			if (!_isSupported)
				ThrowUtils.ThrowPlatformNotSupported();

			InjectUDiv128Asm();

			if (divisor == 0)
				throw new DivideByZeroException();

			if (upper == 0)
				return (lower / divisor, lower % divisor);

			if (upper >= divisor)
				throw new OverflowException("Quotient is too large (Integer Overflow).");

			ulong quotient = 0;
			ulong remainder = upper;

			for (int i = 63; i >= 0; i--)
			{
				ulong bit = (lower >> i) & 1;
				remainder = (remainder << 1) | bit;

				if (remainder >= divisor)
				{
					remainder -= divisor;
					quotient |= (1UL << i);
				}
			}

			return (quotient, remainder);
		}

		private abstract partial class StoreAsArray : AssemblyCodeStoreBase.X64 { }

		private abstract partial class StoreAsSpan : AssemblyCodeStoreBase.X64 { }
	}
}
#endif