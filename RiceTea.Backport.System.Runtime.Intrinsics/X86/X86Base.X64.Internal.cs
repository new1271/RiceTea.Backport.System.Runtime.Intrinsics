#if !NETSTANDARD2_1_OR_GREATER
#if (X86_ARCH && B64_ARCH) || ANYCPU
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
        private static readonly object? _bsfLock, _bsrLock, _idivLock, _divLock;
        private static readonly bool _isSupported;

		static X64()
		{
            if (PlatformHelper.IsX64)
            {
                _isSupported = true;
                _bsfLock = new object();
                _bsrLock = new object();
                _idivLock = new object();
                _divLock = new object();
            }
            else
            {
                _isSupported = false;
                _bsfLock = null;
                _bsrLock = null;
                _idivLock = null;
                _divLock = null;
            }
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
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
        private static long DivRem(ulong lower, long upper, long divisor, out long rem)
        {
            if (!_isSupported)
                ThrowUtils.ThrowPlatformNotSupported();

            InjectStart(lower, upper, divisor, out rem);
            return InjectEnd(Fallbacks.DivRem(lower, upper, divisor, out rem));

            [DebuggerHidden]
            [DebuggerStepThrough]
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
            static void InjectStart(ulong lower, long upper, long divisor, out long rem)
            {
                rem = 0;
                CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
                EnterLock();
            }

            [DebuggerHidden]
            [DebuggerStepThrough]
            [MethodImpl(MethodImplOptions.NoInlining)]
            static long InjectEnd(long value)
            {
                try
                {
                    CallSiteInjector.InjectAsm(
                        startAddress: CallSiteInjector.StartAddress,
                        endAddress: CallSiteInjector.FindCallSite(),
                        injectorFunc: &InjectIDivAsm,
                        exitLockFunc: &ExitLock);
                    return value;
                }
                finally
                {
                    ExitLock();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void EnterLock() => Monitor.Enter(_idivLock!);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void ExitLock()
            {
                try
                {
                    Monitor.Exit(_idivLock!);
                }
                catch (SynchronizationLockException)
                {
                }
            }
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
        private static ulong DivRem(ulong lower, ulong upper, ulong divisor, out ulong rem)
        {
            if (!_isSupported)
                ThrowUtils.ThrowPlatformNotSupported();

            InjectStart(lower, upper, divisor, out rem);
            return InjectEnd(Fallbacks.DivRem(lower, upper, divisor, out rem));

            [DebuggerHidden]
            [DebuggerStepThrough]
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
            static void InjectStart(ulong lower, ulong upper, ulong divisor, out ulong rem)
            {
                rem = 0;
                CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
                EnterLock();
            }

            [DebuggerHidden]
            [DebuggerStepThrough]
            [MethodImpl(MethodImplOptions.NoInlining)]
            static ulong InjectEnd(ulong value)
            {
                try
                {
                    CallSiteInjector.InjectAsm(
                        startAddress: CallSiteInjector.StartAddress,
                        endAddress: CallSiteInjector.FindCallSite(),
                        injectorFunc: &InjectDivAsm,
                        exitLockFunc: &ExitLock);
                    return value;
                }
                finally
                {
                    ExitLock();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void EnterLock() => Monitor.Enter(_divLock!);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void ExitLock()
            {
                try
                {
                    Monitor.Exit(_divLock!);
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
			long quotient = DivRem(lower, upper, divisor, out long remainder);
			return (quotient, remainder);
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
		public static partial (ulong Quotient, ulong Remainder) DivRem(ulong lower, ulong upper, ulong divisor)
        {
            ulong quotient = DivRem(lower, upper, divisor, out ulong remainder);
            return (quotient, remainder);
        }

        private abstract partial class StoreAsArray : AssemblyCodeStoreBase.X64 { }

		private abstract partial class StoreAsSpan : AssemblyCodeStoreBase.X64 { }
	}
}
#endif
#endif