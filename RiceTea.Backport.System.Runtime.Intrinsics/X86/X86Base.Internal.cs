#if !NETSTANDARD2_1_OR_GREATER
#if X86_ARCH || ANYCPU
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Helpers;
using System.Runtime.Intrinsics.Internals;
using System.Security;
using System.Threading;

using LocalsInit;

namespace System.Runtime.Intrinsics.X86;

[SuppressUnmanagedCodeSecurity]
unsafe partial class X86Base
{
	private static readonly object? _bsfLock, _bsrLock, _idivLock, _divLock, _pauseLock;
	private static readonly void* _cpuIdAsm;
	private static readonly bool _isSupported;

	static X86Base()
	{
		if (PlatformHelper.IsX86)
		{
			_isSupported = true;
            _cpuIdAsm = BuildCpuIdAsm();
            _bsfLock = new object();
			_bsrLock = new object();
            _idivLock = new object();
            _divLock = new object();
            _pauseLock = new object();
		}
		else
		{
			_isSupported = false;
			_cpuIdAsm = null;
			_bsfLock = null;
			_bsrLock = null;
            _idivLock = null;
            _divLock = null;
            _pauseLock = null;
		}
	}

	public static partial bool IsSupported
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isSupported;
    }

    [LocalsInit(false)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static partial (int Eax, int Ebx, int Ecx, int Edx) CpuId(int functionId, int subFunctionId)
	{
		if (!_isSupported)
			ThrowUtils.ThrowPlatformNotSupported();

		Registers registers;
		((delegate* unmanaged[Cdecl]<Registers*, int, int, void>)_cpuIdAsm)(&registers, functionId, subFunctionId);
		return UnsafeHelper.As<Registers, (int Eax, int Ebx, int Ecx, int Edx)>(registers);
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
	public static partial uint BitScanForward(uint value)
	{
		if (!_isSupported)
			ThrowUtils.ThrowPlatformNotSupported();

		InjectStart(value);
		return InjectEnd(Fallbacks.BitScanForward(value));

		[DebuggerHidden]
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
		static void InjectStart(uint value)
		{
			CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
			EnterLock();
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.NoInlining)]
		static uint InjectEnd(uint value)
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

		[DebuggerHidden]
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void EnterLock() => Monitor.Enter(_bsfLock!);

		[DebuggerHidden]
		[DebuggerStepThrough]
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
	public static partial uint BitScanReverse(uint value)
	{
		if (!_isSupported)
			ThrowUtils.ThrowPlatformNotSupported();

		InjectStart(value);
		return InjectEnd(Fallbacks.BitScanReverse(value));

		[DebuggerHidden]
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
		static void InjectStart(uint value)
		{
			CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
			EnterLock();
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.NoInlining)]
		static uint InjectEnd(uint value)
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
	private static int DivRem(uint lower, int upper, int divisor, out int rem)
    {
		if (!_isSupported)
			ThrowUtils.ThrowPlatformNotSupported();

		InjectStart(lower, upper, divisor, out rem);
		return InjectEnd(Fallbacks.DivRem(lower, upper, divisor, out rem));

		[DebuggerHidden]
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
		static void InjectStart(uint lower, int upper, int divisor, out int rem)
        {
			rem = 0;
			CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
			EnterLock();
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.NoInlining)]
		static int InjectEnd(int value)
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
	private static uint DivRem(uint lower, uint upper, uint divisor, out uint rem)
    {
		if (!_isSupported)
			ThrowUtils.ThrowPlatformNotSupported();

		InjectStart(lower, upper, divisor, out rem);
		return InjectEnd(Fallbacks.DivRem(lower, upper, divisor, out rem));

		[DebuggerHidden]
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
		static void InjectStart(uint lower, uint upper, uint divisor, out uint rem)
        {
			rem = 0;
			CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
			EnterLock();
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.NoInlining)]
		static uint InjectEnd(uint value)
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static partial (int Quotient, int Remainder) DivRem(uint lower, int upper, int divisor)
	{
		int quotient = DivRem(lower, upper, divisor, out int remainder);
		return (quotient, remainder);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static partial (uint Quotient, uint Remainder) DivRem(uint lower, uint upper, uint divisor)
    {
        uint quotient = DivRem(lower, upper, divisor, out uint remainder);
        return (quotient, remainder);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static partial (nint Quotient, nint Remainder) DivRem(nuint lower, nint upper, nint divisor)
		=> UnsafeHelper.PointerSizeConstant switch
		{
			sizeof(int) => DivRem((uint)lower, (int)upper, (int)divisor),
			sizeof(long) => UnsafeHelper.As<(long, long), (nint, nint)>(X64.DivRem(lower, upper, divisor)),
			_ => UnsafeHelper.PointerSize switch
			{
				sizeof(int) => DivRem((uint)lower, (int)upper, (int)divisor),
				sizeof(long) => UnsafeHelper.As<(long, long), (nint, nint)>(X64.DivRem(lower, upper, divisor)),
				_ => throw new PlatformNotSupportedException()
			}
		};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static partial (nuint Quotient, nuint Remainder) DivRem(nuint lower, nuint upper, nuint divisor)
		=> UnsafeHelper.PointerSizeConstant switch
		{
			sizeof(int) => DivRem((uint)lower, (uint)upper, (uint)divisor),
			sizeof(long) => UnsafeHelper.As<(ulong, ulong), (nuint, nuint)>(X64.DivRem(lower, upper, divisor)),
			_ => UnsafeHelper.PointerSize switch
			{
				sizeof(int) => DivRem((uint)lower, (uint)upper, (uint)divisor),
				sizeof(long) => UnsafeHelper.As<(ulong, ulong), (nuint, nuint)>(X64.DivRem(lower, upper, divisor)),
				_ => throw new PlatformNotSupportedException()
			}
		};

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
    public static partial void Pause()
    {
        if (!_isSupported)
            ThrowUtils.ThrowPlatformNotSupported();

        InjectStart();
		Thread.SpinWait(iterations: 1);
        InjectEnd();

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
        static void InjectStart()
        {
            CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
            EnterLock();
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void InjectEnd()
        {
            try
            {
                CallSiteInjector.InjectAsm(
                    startAddress: CallSiteInjector.StartAddress,
                    endAddress: CallSiteInjector.FindCallSite(),
                    injectorFunc: &InjectPauseAsm,
                    exitLockFunc: &ExitLock);
            }
            finally
            {
                ExitLock();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void EnterLock() => Monitor.Enter(_pauseLock!);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ExitLock()
        {
            try
            {
                Monitor.Exit(_pauseLock!);
            }
            catch (SynchronizationLockException)
            {
            }
        }
    }

    private abstract partial class StoreAsArray : AssemblyCodeStoreBase { }

	private abstract partial class StoreAsSpan : AssemblyCodeStoreBase { }

	[StructLayout(LayoutKind.Sequential, Pack = 4, Size = sizeof(int) * 4)]
	private readonly struct Registers
	{
		private readonly int _eax, _ebx, _ecx, _edx;

		public override readonly string ToString()
			=> $"{{EAX = {_eax}, EBX = {_ebx}, ECX = {_ecx}, EDX = {_edx}}}";
	}
}
#endif
#endif