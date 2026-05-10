using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace RiceTea.Backport.Injection;

unsafe partial class CallSiteInjector
{
    [SuppressUnmanagedCodeSecurity]
    private static class Native_Unix
    {
        [DllImport("c", CallingConvention = CallingConvention.Cdecl, EntryPoint = nameof(backtrace))]
        public static extern int backtrace(void** buffer, int size);

        [DllImport("c", CallingConvention = CallingConvention.Cdecl, EntryPoint = nameof(syscall))]
        private static extern nint syscall(nint number, nint signum, void* act, void* oldact, nuint sigsetsize);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint rt_sigaction(nint signum, SignalAction_LinuxX86* act, SignalAction_LinuxX86* oldact)
            => syscall(number: 174, signum, act, oldact, sigsetsize: 8);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint rt_sigaction(nint signum, SignalAction_LinuxX64* act, SignalAction_LinuxX64* oldact) 
            => syscall(number: 13, signum, act, oldact, sigsetsize: 8);

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct SignalAction_LinuxX86
        {
            public void* sa_handler;
            public uint sa_mask_lo;
            public uint sa_flags;
            public void* sa_restorer;
            public uint sa_mask_hi;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct SignalAction_LinuxX64
        {
            public void* sa_handler;
            public ulong sa_flags;
            public void* sa_restorer;
            public ulong sa_mask;
        }
    }
}