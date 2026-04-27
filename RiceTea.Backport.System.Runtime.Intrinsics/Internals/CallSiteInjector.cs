#if !NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Helpers;
using System.Security;

namespace System.Runtime.Intrinsics.Internals
{
    internal static unsafe class CallSiteInjector
    {
        public const int CallInstructionSize = 5;
        public const int JumpInstructionSize = 5;

        private static readonly PlatformID _platformId = Environment.OSVersion.Platform;

        [ThreadStatic]
        public static void* StartAddress;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void* FindCallSite()
        {
            StackFrame frame = new StackFrame(skipFrames: 2);
            void* callSiteMethodStartAddress = FindRealEntryPoint(frame.GetMethod().MethodHandle); // 呼叫 FindCallSite 函數的那個函數的呼叫端
            int offset = frame.GetNativeOffset();
            if (offset > 0)
                return (byte*)callSiteMethodStartAddress + offset;

            frame = new StackFrame(skipFrames: 1);
            void* injectEndFuncStartAddress = FindRealEntryPoint(frame.GetMethod().MethodHandle);

            switch (_platformId)
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                    {
                        void** backTraces = stackalloc void*[4];
                        Native_Win32.RtlCaptureStackBackTrace(FramesToSkip: 0, FramesToCapture: 1, backTraces, null);
                        Native_Win32.RtlCaptureStackBackTrace(FramesToSkip: 0, FramesToCapture: 1, backTraces + 1, null);

                        ushort captures = Native_Win32.RtlCaptureStackBackTrace(FramesToSkip: backTraces[0] == backTraces[1] ? 2u : 1u, // 跳過這個函數本身和可能存在的 P/Invoke Stub
                             FramesToCapture: 4, backTraces, null);

                        return Compute(backTraces, captures, callSiteMethodStartAddress, injectEndFuncStartAddress);
                    }
                case PlatformID.Unix:
                case PlatformID.MacOSX:
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
                default:
                    throw new PlatformNotSupportedException();
            }

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

        public static void* FindRealEntryPoint(RuntimeMethodHandle handle)
        {
            void* result = (void*)handle.GetFunctionPointer();
            while (TryGetCallingAddress(result, out result)) ;
            return result;
        }

        public static bool TryGetCallingAddress(void* ptr, out void* result)
        {
            switch (*(byte*)ptr)
            {
                case 0xE8:
                    {
                        int offset = *(int*)((byte*)ptr + 1);
                        result = (byte*)ptr + CallInstructionSize + offset;
                        return true;
                    }
                case 0xE9:
                    {
                        int offset = *(int*)((byte*)ptr + 1);
                        result = (byte*)ptr + JumpInstructionSize + offset;
                        return true;
                    }
                case 0xEB:
                    {
                        sbyte shortOffset = *(sbyte*)((byte*)ptr + 1);
                        result = (byte*)ptr + 2 + shortOffset;
                        return true;
                    }
                default:
                    result = ptr;
                    return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InjectCallInstruction(void* ptr, void* target)
        {
            *(byte*)ptr = 0xE8;
            *(int*)((byte*)ptr + 1) = (int)((byte*)target - (byte*)ptr) - CallInstructionSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InjectJumpInstruction(void* ptr, void* target)
        {
            *(byte*)ptr = 0xE9;
            *(int*)((byte*)ptr + 1) = (int)((byte*)target - (byte*)ptr) - JumpInstructionSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillNopInstructions(void* ptr, uint length)
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
            [DllImport("ntdll", CallingConvention = CallingConvention.StdCall)]
            public static extern ushort RtlCaptureStackBackTrace(uint FramesToSkip, uint FramesToCapture, void* BackTrace, uint* BackTraceHash);
        }

        [SuppressUnmanagedCodeSecurity]
        private static class Native_Unix
        {
            [DllImport("c", CallingConvention = CallingConvention.Cdecl)]
            public static extern int backtrace(void** buffer, int size);
        }
    }
}
#endif