#pragma warning disable CA2211

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

using RiceTea.Backport.Internals;

namespace RiceTea.Backport.Injection;

/// <summary>
/// A helper class for injecting machine code into call site.
/// </summary>
public static unsafe partial class CallSiteInjector
{
    /// <summary>
    /// The size of x86 CALL instruction
    /// </summary>
    public const int CallInstructionSize = 5;
    /// <summary>
    /// The size of x86 JMP instruction
    /// </summary>
    public const int JumpInstructionSize = 5;
    /// <summary>
    /// The size of x86 JMP short instruction
    /// </summary>
    public const int JumpShortInstructionSize = 2;

    private static readonly bool _isX86 = PlatformHelper.IsX86;
    private static readonly bool _isWindows = PlatformHelper.IsWindows;
    private static readonly bool _isUnix = PlatformHelper.IsUnix;
    private static IntPtr _lastPriorityInstructionHandler;

    /// <summary>
    /// A thread-static field to store the start address for injecting
    /// </summary>
    [ThreadStatic]
    public static void* StartAddress;

    /// <summary>
    /// Inject machine code into specific memory area.
    /// </summary>
    /// <param name="startAddress">The start address for the injecting area.</param>
    /// <param name="endAddress">The end address for the injecting area.</param>
    /// <param name="injectorFunc">The machine code injector function.</param>
    /// <param name="exitLockFunc">The sync-lock exiting function.</param>
    /// <remarks>
    /// The <paramref name="startAddress"/> and <paramref name="endAddress"/> is also included.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Inject(void* startAddress, void* endAddress, delegate* managed<ref void*, ref uint, void> injectorFunc, delegate* managed<void> exitLockFunc)
    {
        if (!_isX86 || (!_isWindows && !_isUnix))
            ThrowUtils.ThrowPlatformNotSupported();

        uint length = (uint)((byte*)endAddress - (byte*)startAddress);
        MemoryHelper.LetMemoryPageCanRWX(startAddress, length);

        InjectCallInstruction(startAddress, exitLockFunc);

        byte* offsetedStartAddress = (byte*)startAddress + CallInstructionSize;
        void* injectAddress = startAddress;
        uint injectLength = length - CallInstructionSize;
        injectorFunc(ref injectAddress, ref injectLength); // 此處傳入可注入之位址和長度，傳出已注入之位址和注入長度

        FillNopInstructions(offsetedStartAddress, (uint)((byte*)injectAddress - offsetedStartAddress));
        byte* injectEndAddress = (byte*)injectAddress + injectLength;
        FillNopInstructions(injectEndAddress, (uint)((byte*)endAddress - injectEndAddress));

        HookPriorityInstructionHandler();

        void* jumpInstructionAddress = (byte*)startAddress - CallInstructionSize;
        InjectHaltInstruction(jumpInstructionAddress); //建立中繼 HALT 指令(通過無效指令攔截來實現自旋，以避免撕裂讀取)
        InjectJumpInstruction(jumpInstructionAddress, injectAddress); // 建立跳轉

        MemoryHelper.FlushInstructionCache(startAddress, length);
    }

    private static void HookPriorityInstructionHandler()
    {
        ref IntPtr handleRef = ref _lastPriorityInstructionHandler;
        if (_isWindows)
        {
            IntPtr newHandle = Native_Win32.AddVectoredExceptionHandler(First: uint.MaxValue, Handler: VEHHandler.Address);
            if (newHandle != IntPtr.Zero)
            {
                IntPtr oldHandle = Interlocked.Exchange(ref handleRef, newHandle);
                if (oldHandle != IntPtr.Zero)
                    Native_Win32.RemoveVectoredExceptionHandler(oldHandle);
            }
            return;
        }
        if (_isUnix)
        {
            // TODO: Implement a signal handler
        }

        ThrowUtils.ThrowPlatformNotSupported();
    }

    /// <summary>
    /// Find the call site for the calling function.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Failed to find the call site.</exception>
    /// <exception cref="PlatformNotSupportedException">The platform is not supported.</exception>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void* FindCallSite()
    {
        StackFrame frame = new StackFrame(skipFrames: 2);
        void* callSiteMethodStartAddress = FindRealEntryPoint(frame); // 呼叫 FindCallSite 函數的那個函數的呼叫端
        int offset = frame.GetNativeOffset();
        if (offset > 0)
            return (byte*)callSiteMethodStartAddress + offset;

        frame = new StackFrame(skipFrames: 1);
        void* injectEndFuncStartAddress = FindRealEntryPoint(frame);

        if (PlatformHelper.IsWindows)
        {
            void** backTraces = stackalloc void*[4];
            Native_Win32.RtlCaptureStackBackTrace(FramesToSkip: 0, FramesToCapture: 1, backTraces, null);
            Native_Win32.RtlCaptureStackBackTrace(FramesToSkip: 0, FramesToCapture: 1, backTraces + 1, null);

            ushort captures = Native_Win32.RtlCaptureStackBackTrace(FramesToSkip: backTraces[0] == backTraces[1] ? 2u : 1u, // 跳過這個函數本身和可能存在的 P/Invoke Stub
                 FramesToCapture: 4, backTraces, null);

            return Compute(backTraces, captures, callSiteMethodStartAddress, injectEndFuncStartAddress);
        }
        if (PlatformHelper.IsUnix)
        {
            void** backTraces = stackalloc void*[6];
            Native_Unix.backtrace(backTraces, 1);
            Native_Unix.backtrace(backTraces + 1, 1);

            offset = backTraces[0] == backTraces[1] ? 2 : 1; // 跳過這個函數本身和可能存在的 P/Invoke Stub
            int limit = offset + 4;
            int captures = Native_Unix.backtrace(backTraces, limit);
            if (captures < 0 || captures > limit)
                throw new InvalidOperationException();

            return Compute(backTraces + offset, (ushort)(captures - offset), callSiteMethodStartAddress, injectEndFuncStartAddress);
        }

        ThrowUtils.ThrowPlatformNotSupported();
        return default;

        static void* Compute(void** backTraces, ushort captures, void* callSiteMethodStartAddress, void* injectEndFuncStartAddress)
        {
            // 此時 backTraces 內的結構 (圓括號表示可能沒有這層):
            // (此函數的 JIT Trampoline), 呼叫此函數的 InjectEnd 函數+偏移, (InjectEnd 函數的 JIT Trampoline), 呼叫 InjectEnd 函數的函數+偏移
            if (captures < 4)
            {
                // 保守模式 (因為此時 backTraces 內部不完全有效)
                switch (captures)
                {
                    case 0:
                    case 1:
                        throw new InvalidOperationException(); // 不可能
                    case 2:
                        return backTraces[1];
                    case 3:
                        {
                            if (((byte*)backTraces[1] - (byte*)callSiteMethodStartAddress) < ((byte*)backTraces[2] - (byte*)callSiteMethodStartAddress))
                                return backTraces[1];
                            else
                                return backTraces[2];
                        }
                    default:
                        throw new InvalidOperationException(); // 不可能
                }
            }
            else
            {
                int backTracesOffset;
                if (((byte*)backTraces[0] - (byte*)injectEndFuncStartAddress) < ((byte*)backTraces[1] - (byte*)injectEndFuncStartAddress))
                    backTracesOffset = 1;
                else
                    backTracesOffset = 2;

                if (((byte*)backTraces[backTracesOffset] - (byte*)callSiteMethodStartAddress) < ((byte*)backTraces[backTracesOffset + 1] - (byte*)callSiteMethodStartAddress))
                    return backTraces[backTracesOffset];
                else
                    return backTraces[backTracesOffset + 1];
            }
        }
    }

    private static void* FindRealEntryPoint(StackFrame frame)
    {
        MethodBase? method = frame.GetMethod();
        if (method is null)
            return null;
        return FindRealEntryPoint(method.MethodHandle);
    }

    private static void* FindRealEntryPoint(RuntimeMethodHandle handle)
    {
        void* result = (void*)handle.GetFunctionPointer();
        while (TryGetCallingAddress(result, out result)) ;
        return result;
    }

    private static bool TryGetCallingAddress(void* ptr, out void* result)
    {
        switch (*(byte*)ptr)
        {
            case 0xE8: // CALL
                {
                    int offset = *(int*)((byte*)ptr + 1);
                    result = (byte*)ptr + CallInstructionSize + offset;
                    return true;
                }
            case 0xE9: // JMP
                {
                    int offset = *(int*)((byte*)ptr + 1);
                    result = (byte*)ptr + JumpInstructionSize + offset;
                    return true;
                }
            case 0xEB: // JMP short
                {
                    sbyte shortOffset = *(sbyte*)((byte*)ptr + 1);
                    result = (byte*)ptr + JumpShortInstructionSize + shortOffset;
                    return true;
                }
            default:
                result = ptr;
                return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectCallInstruction(void* ptr, void* target)
    {
        *(int*)((byte*)ptr + 1) = (int)((byte*)target - (byte*)ptr) - CallInstructionSize;
        *(byte*)ptr = 0xE8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectJumpInstruction(void* ptr, void* target)
    {
        *(int*)((byte*)ptr + 1) = (int)((byte*)target - (byte*)ptr) - JumpInstructionSize;
        *(byte*)ptr = 0xE9;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectHaltInstruction(void* ptr)
    {
        *(byte*)ptr = 0xF4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FillNopInstructions(void* ptr, uint length)
    {
        if (length > 9)
            FillNopInstruction_Long(ptr, length);
        else
            FillNopInstruction_Short(ptr, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FillNopInstruction_Long(void* ptr, uint length)
    {
        UnsafeHelper.InitBlock(ptr, 0, length);
        byte* castedPtr = (byte*)ptr;
        do
        {
            *(uint*)castedPtr = 0x84_1F_0F_66; // nop_9: 66 0F 1F 84 00 00 00 00 00
            castedPtr += 9;
            length -= 9;
        } while (length >= 9);
        switch (length)
        {
            case 8:
                *(uint*)castedPtr = 0x84_1F_0F; // nop_8: 0F 1F 84 00 00 00 00 00
                break;
            case 7:
                *(uint*)castedPtr = 0x80_1F_0F; // nop_7: 0F 1F 80 00 00 00 00
                break;
            case 6:
                *(uint*)castedPtr = 0x44_1F_0F_66; // nop_6: 66 0F 1F 44 00 00
                break;
            case 5:
                *(uint*)castedPtr = 0x44_1F_0F; // nop_5: 0F 1F 44 00 00
                break;
            case 4:
                *(uint*)castedPtr = 0x40_1F_0F; // nop_4: 0F 1F 40 00
                break;
            case 3:
                *(ushort*)castedPtr = 0x1F_0F; // nop_3: 0F 1F 00
                break;
            case 2:
                *(ushort*)castedPtr = 0x90_66; // nop_2: 66 90
                break;
            case 1:
                *castedPtr = 0x90; // nop_1: 90
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FillNopInstruction_Short(void* ptr, uint length)
    {
        byte* castedPtr = (byte*)ptr;
        switch (length)
        {
            case 9:  // nop_9: 66 0F 1F 84 00 00 00 00 00
                *(uint*)castedPtr = 0x84_1F_0F_66;
                *(uint*)(castedPtr + 4) = 0;
                *(castedPtr + 8) = 0;
                break;
            case 8:  // nop_8: 0F 1F 84 00 00 00 00 00
                *(uint*)castedPtr = 0x84_1F_0F;
                *(uint*)(castedPtr + 4) = 0;
                break;
            case 7: // nop_7: 0F 1F 80 00 00 00 00
                *(uint*)castedPtr = 0x80_1F_0F;
                *(ushort*)(castedPtr + 4) = 0;
                *(castedPtr + 6) = 0;
                break;
            case 6: // nop_6: 66 0F 1F 44 00 00
                *(uint*)castedPtr = 0x44_1F_0F_66;
                *(ushort*)(castedPtr + 4) = 0;
                break;
            case 5: // nop_5: 0F 1F 44 00 00
                *(uint*)castedPtr = 0x44_1F_0F;
                *(castedPtr + 4) = 0;
                break;
            case 4:
                *(uint*)castedPtr = 0x40_1F_0F; // nop_4: 0F 1F 40 00
                break;
            case 3:
                *(ushort*)castedPtr = 0x1F_0F; // nop_3: 0F 1F 00
                *(castedPtr + 2) = 0;
                break;
            case 2:
                *(ushort*)castedPtr = 0x90_66; // nop_2: 66 90
                break;
            case 1:
                *castedPtr = 0x90; // nop_1: 90
                break;
        }
    }

    [SuppressUnmanagedCodeSecurity]
    private static class Native_Win32
    {
        [DllImport("ntdll", CallingConvention = CallingConvention.StdCall, EntryPoint = nameof(RtlCaptureStackBackTrace))]
        public static extern ushort RtlCaptureStackBackTrace(uint FramesToSkip, uint FramesToCapture, void* BackTrace, uint* BackTraceHash);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, EntryPoint = nameof(AddVectoredExceptionHandler))]
        public static extern IntPtr AddVectoredExceptionHandler(uint First, void* Handler);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, EntryPoint = nameof(RemoveVectoredExceptionHandler))]
        public static extern uint RemoveVectoredExceptionHandler(IntPtr Handle);
    }

    [SuppressUnmanagedCodeSecurity]
    private static class Native_Unix
    {
        [DllImport("c", CallingConvention = CallingConvention.Cdecl)]
        public static extern int backtrace(void** buffer, int size);
    }
}