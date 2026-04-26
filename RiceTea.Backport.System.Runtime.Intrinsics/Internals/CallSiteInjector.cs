#if !NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Helpers;
using System.Security;
using System.Text.RegularExpressions;

using InlineIL;

namespace System.Runtime.Intrinsics.Internals
{
    internal static unsafe class CallSiteInjector
    {
        private static readonly PlatformID _platformId = Environment.OSVersion.Platform;

        [ThreadStatic]
        private static void* _startAddress;

        public static void* InjectStartAddress => _startAddress;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InjectStart<T>(T arg)
        {
            _startAddress = (byte*)FindCallSite() - 5; // 暫時為硬編碼 (現在只支援 x86 Instructions)
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InjectStart<T1, T2>(T1 arg1, T2 arg2)
        {
            _startAddress = (byte*)FindCallSite() - 5; // 暫時為硬編碼 (現在只支援 x86 Instructions)
        }

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
                        int captures = Native_Unix.backtrace(backTraces, offset + 4);
                        if (captures < 0 || captures > ushort.MaxValue)
                            throw new InvalidOperationException();

                        return Compute(backTraces + offset, (ushort)captures, callSiteMethodStartAddress, injectEndFuncStartAddress);
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
                case 0xE9:
                    int offset = *(int*)((byte*)ptr + 1);
                    result = (byte*)ptr + 5 + offset;
                    return true;
                case 0xEB:
                    sbyte shortOffset = *(sbyte*)((byte*)ptr + 1);
                    result = (byte*)ptr + 2 + shortOffset;
                    return true;
                default:
                    result = ptr;
                    return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InjectJumpInstructionAndNopSequence(void* ptr, uint jumpOffset)
        {
            *(byte*)ptr = 0xE9;
            *(uint*)((byte*)ptr + 1) = jumpOffset - 5;
            UnsafeHelper.InitBlock(((byte*)ptr) + 5, 0x90, jumpOffset - 5);
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