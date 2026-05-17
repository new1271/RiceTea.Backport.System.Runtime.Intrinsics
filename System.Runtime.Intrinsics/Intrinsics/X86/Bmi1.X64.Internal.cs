
#if NETSTANDARD2_0_OR_GREATER
#if (X86_ARCH && B64_ARCH) || ANYCPU

using System.Diagnostics;
using System.Runtime.CompilerServices;
using RiceTea.Backport.Injection;
using RiceTea.Backport.Internals;

using Fallbacks = RiceTea.Backport.Fallbacks.X86.Bmi1.X64;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class Bmi1
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
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)] // 避免尾呼叫優化
        public static partial ulong AndNot(ulong left, ulong right)
        {
            if (!_isSupported)
                return ThrowUtils.ThrowPlatformNotSupported<ulong>();

            CallSiteInjector.OnInjectStart(left, right);
            return CallSiteInjector.OnInjectEnd(Fallbacks.AndNot(left, right), &InjectAndnAsm);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static partial ulong BitFieldExtract(ulong value, byte start, byte length) => BitFieldExtract(value, (ushort)(start | (length << 8)));

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)] // 避免尾呼叫優化
        public static partial ulong BitFieldExtract(ulong value, ushort control)
        {
            if (!_isSupported)
                return ThrowUtils.ThrowPlatformNotSupported<ulong>();

            CallSiteInjector.OnInjectStart(value, control);
            return CallSiteInjector.OnInjectEnd(Fallbacks.BitFieldExtract(value, control), &InjectBextrAsm);
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)] // 避免尾呼叫優化
        public static partial ulong ExtractLowestSetBit(ulong value)
        {
            if (!_isSupported)
                return ThrowUtils.ThrowPlatformNotSupported<ulong>();

            CallSiteInjector.OnInjectStart(value);
            return CallSiteInjector.OnInjectEnd(Fallbacks.ExtractLowestSetBit(value), &InjectBlsiAsm);
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)] // 避免尾呼叫優化
        public static partial ulong GetMaskUpToLowestSetBit(ulong value)
        {
            if (!_isSupported)
                return ThrowUtils.ThrowPlatformNotSupported<ulong>();

            CallSiteInjector.OnInjectStart(value);
            return CallSiteInjector.OnInjectEnd(Fallbacks.GetMaskUpToLowestSetBit(value), &InjectBlsmskAsm);
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)] // 避免尾呼叫優化
        public static partial ulong ResetLowestSetBit(ulong value)
        {
            if (!_isSupported)
                return ThrowUtils.ThrowPlatformNotSupported<ulong>();

            CallSiteInjector.OnInjectStart(value);
            return CallSiteInjector.OnInjectEnd(Fallbacks.ResetLowestSetBit(value), &InjectBlsrAsm);
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)] // 避免尾呼叫優化
        public static partial ulong TrailingZeroCount(ulong value)
        {
            if (!_isSupported)
                return ThrowUtils.ThrowPlatformNotSupported<ulong>();

            CallSiteInjector.OnInjectStart(value);
            return CallSiteInjector.OnInjectEnd(Fallbacks.TrailingZeroCount(value), &InjectTzcntAsm);
        }

        private static partial class Store { }
    }
}
#endif
#endif