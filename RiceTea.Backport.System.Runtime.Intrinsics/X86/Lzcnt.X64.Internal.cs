#if !NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Internals;

namespace System.Runtime.Intrinsics.X86;

partial class Lzcnt
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
            const int LzcntMask = 1 << 5;
            return (X86Base.CpuId(unchecked((int)0x80000001), 0).Ecx & LzcntMask) == LzcntMask;
        }

        public static partial bool IsSupported
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isSupported;
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
        public static partial ulong LeadingZeroCount(ulong value)
        {
            if (!_isSupported)
                throw new PlatformNotSupportedException();

            CallSiteInjector.InjectStart(value);
            return LeadingZeroCount_InjectEnd(Fallbacks.LeadingZeroCountSoftwareFallback(value));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe ulong LeadingZeroCount_InjectEnd(ulong value)
        {
            byte* callSite = (byte*)CallSiteInjector.FindCallSite();
            byte* startInjectAddress = (byte*)CallSiteInjector.InjectStartAddress;

            uint length = (uint)(callSite - startInjectAddress);
            AsmCodeHelper.LetMemoryPageCanRWX(startInjectAddress, length);

            byte* jumpAddress = startInjectAddress + InjectLzcntAsm(startInjectAddress);
            CallSiteInjector.InjectJumpInstructionAndNopSequence(jumpAddress, (uint)(callSite - jumpAddress));

            AsmCodeHelper.FlushInstructionCache(startInjectAddress, length);

            return value;
        }

        private static partial class StoreAsArray { }

        private static partial class StoreAsSpan { }
    }
}
#endif