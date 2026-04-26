#if !NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Internals;

namespace System.Runtime.Intrinsics.X86;

partial class Popcnt
{
    private static readonly bool _isSupported;

    static Popcnt()
    {
        if (!CheckIsSupported())
            return;
        _isSupported = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CheckIsSupported()
    {
        if (!X86Base.IsSupported)
            return false;
        const int PopcntMask = 1 << 23;
        return (X86Base.CpuId(unchecked((int)0x80000001), 0).Ecx & PopcntMask) == PopcntMask;
    }

    public static partial bool IsSupported
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isSupported;
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoOptimization)]
    public static partial uint PopCount(uint value)
    {
        if (!_isSupported)
            throw new PlatformNotSupportedException();

        CallSiteInjector.InjectStart(value);
        return PopCount_InjectEnd(Fallbacks.PopCountSoftwareFallback(value));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe uint PopCount_InjectEnd(uint value)
    {
        byte* callSite = (byte*)CallSiteInjector.FindCallSite();
        byte* startInjectAddress = (byte*)CallSiteInjector.InjectStartAddress;

        uint length = (uint)(callSite - startInjectAddress);
        AsmCodeHelper.LetMemoryPageCanRWX(startInjectAddress, length);

        byte* jumpAddress = startInjectAddress + InjectPopcntAsm(startInjectAddress);
        CallSiteInjector.InjectJumpInstructionAndNopSequence(jumpAddress, (uint)(callSite - jumpAddress));

        AsmCodeHelper.FlushInstructionCache(startInjectAddress, length);

        return value;
    }

    private static partial class StoreAsArray { }

    private static partial class StoreAsSpan { }
}
#endif