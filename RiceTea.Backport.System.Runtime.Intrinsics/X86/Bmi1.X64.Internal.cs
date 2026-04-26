#if !NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Internals;

namespace System.Runtime.Intrinsics.X86;

partial class Bmi1
{
    partial class X64
    {
        private static readonly bool _isSupported;

        static X64()
        {
            if (!CheckIsSupported())
                return;
            _isSupported = true;
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

            CallSiteInjector.InjectStart(value);
            return TrailingZeroCount_InjectEnd(Fallbacks.TrailingZeroCountSoftwareFallback(value));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe ulong TrailingZeroCount_InjectEnd(ulong value)
        {
            byte* callSite = (byte*)CallSiteInjector.FindCallSite();
            byte* startInjectAddress = (byte*)CallSiteInjector.InjectStartAddress;

            uint length = (uint)(callSite - startInjectAddress);
            AsmCodeHelper.LetMemoryPageCanRWX(startInjectAddress, length);

            byte* jumpAddress = startInjectAddress + InjectTzcntAsm(startInjectAddress);
            CallSiteInjector.InjectJumpInstructionAndNopSequence(jumpAddress, (uint)(callSite - jumpAddress));

            AsmCodeHelper.FlushInstructionCache(startInjectAddress, length);

            return value;
        }

        private static partial class StoreAsArray { }

        private static partial class StoreAsSpan { }
    }
}
#endif