#if !NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;
using System.Runtime.Intrinsics.Internals;
using System.Threading;

using InlineIL;

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
                throw new PlatformNotSupportedException();

            BitScanForward_InjectStart(value);
            return BitScanForward_InjectEnd(Fallbacks.BitScanForwardSoftwareFallback(value));
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
        private static void BitScanForward_InjectStart(ulong value)
        {
            CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
            BitScanForward_EnterLock();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ulong BitScanForward_InjectEnd(ulong value)
        {
            try
            {
                byte* endAddress = (byte*)CallSiteInjector.FindCallSite();
                byte* startAddress = (byte*)CallSiteInjector.StartAddress; // InjectStart() 的下一個位址

                uint length = (uint)(endAddress - startAddress);
                AsmCodeHelper.LetMemoryPageCanRWX(startAddress, length);

                IL.Emit.Ldtoken(new MethodRef(typeof(X86Base), nameof(BitScanForward_ExitLock)));
                IL.Pop(out RuntimeMethodHandle handle);
                // 無須提前編譯和解析跳轉，此處傳回的會是 JIT Trampoline 位址，JIT 會在那個位址內決定是否需要編譯
                CallSiteInjector.InjectCallInstruction(startAddress, (void*)handle.GetFunctionPointer());

                byte* offsetedStartAddress = startAddress + CallSiteInjector.CallInstructionSize;
                void* injectAddress = startAddress;
                uint injectLength = length - CallSiteInjector.CallInstructionSize;
                InjectBsfAsm(ref injectAddress, ref injectLength); // 此處傳入可注入之位址和長度，傳出已注入之位址和注入長度

                CallSiteInjector.FillNopInstructions(offsetedStartAddress, (uint)((byte*)injectAddress - offsetedStartAddress));
                byte* injectEndAddress = (byte*)injectAddress + injectLength;
                CallSiteInjector.FillNopInstructions(injectEndAddress, (uint)(endAddress - injectEndAddress));

                CallSiteInjector.InjectJumpInstruction(startAddress - CallSiteInjector.JumpInstructionSize, injectAddress); // 建立跳轉

                AsmCodeHelper.FlushInstructionCache(startAddress, length);

                return value;
            }
            finally
            {
                BitScanForward_ExitLock();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void BitScanForward_EnterLock() => Monitor.Enter(_bsfLock!);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void BitScanForward_ExitLock()
        {
            try
            {
                Monitor.Exit(_bsfLock!);
            }
            catch (SynchronizationLockException)
            {
            }
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
        public static partial ulong BitScanReverse(ulong value)
        {
            if (!_isSupported)
                throw new PlatformNotSupportedException();

            BitScanReverse_InjectStart(value);
            return BitScanReverse_InjectEnd(Fallbacks.BitScanReverseSoftwareFallback(value));
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
        private static void BitScanReverse_InjectStart(ulong value)
        {
            CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
            BitScanReverse_EnterLock();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ulong BitScanReverse_InjectEnd(ulong value)
        {
            try
            {
                byte* endAddress = (byte*)CallSiteInjector.FindCallSite();
                byte* startAddress = (byte*)CallSiteInjector.StartAddress; // InjectStart() 的下一個位址

                uint length = (uint)(endAddress - startAddress);
                AsmCodeHelper.LetMemoryPageCanRWX(startAddress, length);

                IL.Emit.Ldtoken(new MethodRef(typeof(X86Base), nameof(BitScanReverse_ExitLock)));
                IL.Pop(out RuntimeMethodHandle handle);
                // 無須提前編譯和解析跳轉，此處傳回的會是 JIT Trampoline 位址，JIT 會在那個位址內決定是否需要編譯
                CallSiteInjector.InjectCallInstruction(startAddress, (void*)handle.GetFunctionPointer());

                byte* offsetedStartAddress = startAddress + CallSiteInjector.CallInstructionSize;
                void* injectAddress = startAddress;
                uint injectLength = length - CallSiteInjector.CallInstructionSize;
                InjectBsrAsm(ref injectAddress, ref injectLength); // 此處傳入可注入之位址和長度，傳出已注入之位址和注入長度

                CallSiteInjector.FillNopInstructions(offsetedStartAddress, (uint)((byte*)injectAddress - offsetedStartAddress));
                byte* injectEndAddress = (byte*)injectAddress + injectLength;
                CallSiteInjector.FillNopInstructions(injectEndAddress, (uint)(endAddress - injectEndAddress));

                CallSiteInjector.InjectJumpInstruction(startAddress - CallSiteInjector.JumpInstructionSize, injectAddress); // 建立跳轉

                AsmCodeHelper.FlushInstructionCache(startAddress, length);

                return value;
            }
            finally
            {
                BitScanReverse_ExitLock();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void BitScanReverse_EnterLock() => Monitor.Enter(_bsrLock!);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void BitScanReverse_ExitLock()
        {
            try
            {
                Monitor.Exit(_bsrLock!);
            }
            catch (SynchronizationLockException)
            {
            }
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static partial (long Quotient, long Remainder) DivRem(ulong lower, long upper, long divisor)
        {
            if (!_isSupported)
                throw new PlatformNotSupportedException();

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
                throw new PlatformNotSupportedException();

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

        private static partial class StoreAsArray { }

        private static partial class StoreAsSpan { }
    }
}
#endif