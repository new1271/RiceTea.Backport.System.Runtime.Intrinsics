#if !NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Internals;
using System.Threading;

using InlineIL;

namespace System.Runtime.Intrinsics.X86;

partial class Bmi1
{
    private static readonly object? _tzcntLock;
    private static readonly bool _isSupported;

    static Bmi1()
    {
        if (CheckIsSupported())
        {
            _isSupported = true;
            _tzcntLock = new object();
        }
        else
        {
            _isSupported = false;
            _tzcntLock = null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CheckIsSupported()
    {
        if (!X86Base.IsSupported)
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
    public static partial uint TrailingZeroCount(uint value)
    {
        if (!_isSupported)
            throw new PlatformNotSupportedException();

        TrailingZeroCount_InjectStart(value);
        return TrailingZeroCount_InjectEnd(Fallbacks.TrailingZeroCountSoftwareFallback(value));
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
    private static unsafe void TrailingZeroCount_InjectStart(uint value)
    {
        CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
        TrailingZeroCount_EnterLock();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe uint TrailingZeroCount_InjectEnd(uint value)
    {
        try
        {
            byte* endAddress = (byte*)CallSiteInjector.FindCallSite(); 
            byte* startAddress = (byte*)CallSiteInjector.StartAddress; // InjectStart() 的下一個位址

            uint length = (uint)(endAddress - startAddress);
            AsmCodeHelper.LetMemoryPageCanRWX(startAddress, length);

            IL.Emit.Ldtoken(new MethodRef(typeof(Bmi1), nameof(TrailingZeroCount_ExitLock)));
            IL.Pop(out RuntimeMethodHandle handle);
            // 無須提前編譯和解析跳轉，此處傳回的會是 JIT Trampoline 位址，JIT 會在那個位址內決定是否需要編譯
            CallSiteInjector.InjectCallInstruction(startAddress, (void*)handle.GetFunctionPointer());

            byte* offsetedStartAddress = startAddress + CallSiteInjector.CallInstructionSize;
            void* injectAddress = offsetedStartAddress;
            uint injectLength = length - CallSiteInjector.CallInstructionSize;
            InjectTzcntAsm(ref injectAddress, ref injectLength); // 此處傳入可注入之位址和長度，傳出已注入之位址和注入長度

            CallSiteInjector.FillNopInstructions(offsetedStartAddress, (uint)((byte*)injectAddress - offsetedStartAddress));
            byte* injectEndAddress = (byte*)injectAddress + injectLength;
            CallSiteInjector.FillNopInstructions(injectEndAddress, (uint)(endAddress - injectEndAddress));

            CallSiteInjector.InjectJumpInstruction(startAddress - CallSiteInjector.JumpInstructionSize, injectAddress); // 建立跳轉

            AsmCodeHelper.FlushInstructionCache(startAddress, length);
            return value;
        }
        finally
        {
            TrailingZeroCount_ExitLock();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TrailingZeroCount_EnterLock() => Monitor.Enter(_tzcntLock!);

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
#endif