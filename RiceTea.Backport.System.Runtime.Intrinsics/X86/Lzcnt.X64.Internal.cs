#if !NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static partial ulong LeadingZeroCount(ulong value)
        {
            if (!_isSupported)
                throw new PlatformNotSupportedException();

            InjectLzcntAsm();

            uint hi = (uint)(value >> 32);

            if (hi == 0)
                return 32 + (31 ^ (uint)Fallbacks.Log2SoftwareFallback((uint)value));

            return (31 ^ (uint)Fallbacks.Log2SoftwareFallback(hi));
        }

        private static partial class StoreAsArray { }

        private static partial class StoreAsSpan { }
    }
}
#endif