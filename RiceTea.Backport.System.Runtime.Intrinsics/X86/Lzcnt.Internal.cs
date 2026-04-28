#if !NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Internals;
using System.Threading;

using InlineIL;

namespace System.Runtime.Intrinsics.X86;

partial class Lzcnt
{
    private static readonly object? _lzcntLock;
    private static readonly bool _isSupported;

    static Lzcnt()
    {
        if (CheckIsSupported())
        {
            _lzcntLock = new object();
            _isSupported = true;
        }
        else
        {
            _lzcntLock = null;
            _isSupported = false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CheckIsSupported()
    {
        if (!X86Base.IsSupported)
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
    public static partial uint LeadingZeroCount(uint value)
    {
        if (!_isSupported)
            throw new PlatformNotSupportedException();

        LeadingZeroCount_InjectStart(value);
        return LeadingZeroCount_InjectEnd(Fallbacks.LeadingZeroCount(value));
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)] // 禁止優化參數傳遞
    private static unsafe void LeadingZeroCount_InjectStart(uint value)
    {
        CallSiteInjector.StartAddress = CallSiteInjector.FindCallSite();
        LeadingZeroCount_EnterLock();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe uint LeadingZeroCount_InjectEnd(uint value)
    {
        try
        {
            byte* endAddress = (byte*)CallSiteInjector.FindCallSite();
            byte* startAddress = (byte*)CallSiteInjector.StartAddress; // InjectStart() 的下一個位址

            uint length = (uint)(endAddress - startAddress);
            AsmCodeHelper.LetMemoryPageCanRWX(startAddress, length);

            IL.Emit.Ldtoken(new MethodRef(typeof(Lzcnt), nameof(LeadingZeroCount_ExitLock)));
            IL.Pop(out RuntimeMethodHandle handle);
            // 無須提前編譯和解析跳轉，此處傳回的會是 JIT Trampoline 位址，JIT 會在那個位址內決定是否需要編譯
            CallSiteInjector.InjectCallInstruction(startAddress, (void*)handle.GetFunctionPointer());

            byte* offsetedStartAddress = startAddress + CallSiteInjector.CallInstructionSize;
            void* injectAddress = startAddress;
            uint injectLength = length - CallSiteInjector.CallInstructionSize;
            InjectLzcntAsm(ref injectAddress, ref injectLength); // 此處傳入可注入之位址和長度，傳出已注入之位址和注入長度

            CallSiteInjector.FillNopInstructions(offsetedStartAddress, (uint)((byte*)injectAddress - offsetedStartAddress));
            byte* injectEndAddress = (byte*)injectAddress + injectLength;
            CallSiteInjector.FillNopInstructions(injectEndAddress, (uint)(endAddress - injectEndAddress));

            CallSiteInjector.InjectJumpInstruction(startAddress - CallSiteInjector.JumpInstructionSize, injectAddress); // 建立跳轉

            AsmCodeHelper.FlushInstructionCache(startAddress, length);

            return value;
        }
        finally
        {
            LeadingZeroCount_ExitLock();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LeadingZeroCount_EnterLock() => Monitor.Enter(_lzcntLock!);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LeadingZeroCount_ExitLock()
    {
        try
        {
            Monitor.Exit(_lzcntLock!);
        }
        catch (SynchronizationLockException)
        {
        }
    }

    private static partial class StoreAsArray { }

    private static partial class StoreAsSpan { }
}
#endif