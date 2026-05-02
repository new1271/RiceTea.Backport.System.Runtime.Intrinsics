#if !NETSTANDARD2_1_OR_GREATER
#if (X86_ARCH && B64_ARCH) || ANYCPU
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;
using System.Runtime.Intrinsics.Internals;
using System.Threading;

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
            return (CpuId(7, 0).Ebx & Bmi1Mask) == Bmi1Mask;
        }

        public static new partial bool IsSupported
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
                ThrowUtils.ThrowPlatformNotSupported();

            InjectStart(value);
            return InjectEnd(Fallbacks.TrailingZeroCount(value));

            [DebuggerHidden]
            [DebuggerStepThrough]
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
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
                    CallSiteInjector.InjectAsm(
                        startAddress: CallSiteInjector.StartAddress,
                        endAddress: CallSiteInjector.FindCallSite(),
                        injectorFunc: &InjectTzcntAsm,
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
            static void EnterLock() => Monitor.Enter(_tzcntLock!);

            [DebuggerHidden]
            [DebuggerStepThrough]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void ExitLock()
            {
                try
                {
                    Monitor.Exit(_tzcntLock!);
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