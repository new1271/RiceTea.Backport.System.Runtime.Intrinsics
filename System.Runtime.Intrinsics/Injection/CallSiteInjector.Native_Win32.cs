using System;
using System.Runtime.InteropServices;
using System.Security;

namespace RiceTea.Backport.Injection;

unsafe partial class CallSiteInjector
{
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
}