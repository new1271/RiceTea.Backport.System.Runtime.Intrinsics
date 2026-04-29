#if !NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Internals;
using System.Threading;

using InlineIL;

namespace System.Runtime.Intrinsics.X86;

partial class Bmi1
{
    partial class X64
    {
        private static readonly object? _tzcntLock;
        private static readonly bool _isSupported;

        static X64()
        {
            if (CheckIsSupported())
            {
                _tzcntLock = new object();
                _isSupported = true;
            }
            else
            {
                _tzcntLock = null;
                _isSupported = false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CheckIsSupported()
        {
            if (!X86Base.X64.IsSupported)
                return false;
            const int Bmi1Mask = 1 << 3;
            return (X86Base.CpuId(7, 0).Ebx & Bmi1Mask) == Bmi1Mask;
        }

        public static partial bool IsSupported
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isSupported;
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
        public static partial ulong TrailingZeroCount(ulong value)
        {
            if (!_isSupported)
                throw new PlatformNotSupportedException();

            TrailingZeroCount_InjectStart(value);
            return TrailingZeroCount_InjectEnd(Fallbacks.BitScanForward(value));
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
        private static unsafe void TrailingZeroCount_InjectStart(ulong value)
        {
            CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
            TrailingZeroCount_EnterLock();
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe ulong TrailingZeroCount_InjectEnd(ulong value)
        {
            try
            {
                CallSiteInjector.InjectAsm(
                    startAddress: CallSiteInjector.StartAddress,
                    endAddress: CallSiteInjector.FindCallSite(),
                    injectorFunc: &InjectTzcntAsm,
                    exitLockFunc: &TrailingZeroCount_ExitLock);
                return value;
            }
            finally
            {
                TrailingZeroCount_ExitLock();
            }
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TrailingZeroCount_EnterLock() => Monitor.Enter(_tzcntLock!);

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TrailingZeroCount_ExitLock()
        {
            try
            {
                Monitor.Exit(_tzcntLock!);
            }
            catch (SynchronizationLockException)
            {
            }
        }

        private static partial class StoreAsArray { }

        private static partial class StoreAsSpan { }
    }
}
#endif