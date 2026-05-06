using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace RiceTea.Backport.Injection;

unsafe partial class MemoryHelper
{
    [SuppressUnmanagedCodeSecurity]
    private static class Native_Unix
    {
        private static readonly void* _cacheflushFunc = GetImportedMethodPointer(null, nameof(cacheflush));

        [DllImport("c", CallingConvention = CallingConvention.Cdecl)]
        public static extern void* mmap(void* ptr, nuint length, ProtectMemoryPageFlags prot, MemoryMapFlags flags, int fd, nint offset);

        [DllImport("c", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mprotect(void* ptr, nuint length, ProtectMemoryPageFlags flags);

        [DllImport("dl", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr dlopen(byte* filename, int flags);

        [DllImport("dl", CallingConvention = CallingConvention.Cdecl)]
        private static extern void* dlsym(IntPtr handle, byte* symbol);

        private static int cacheflush(void* addr, int nbytes, int cache)
        {
            void* func = _cacheflushFunc;
            if (func is null)
                return 0;
            return ((delegate* unmanaged[Cdecl]<void*, int, int, int>)func)(addr, nbytes, cache);
        }

        public static void FlushInstructionCache(void* ptr, nuint size)
        {
            const int ICACHE = 1 << 0;
            const int DCACHE = 1 << 1;
            const int BCACHE = ICACHE | DCACHE;
            for (; size > int.MaxValue; size -= int.MaxValue, ptr = (byte*)ptr + int.MaxValue)
            {
                if (cacheflush(ptr, int.MaxValue, BCACHE) != 0)
                    return;
            }
            if (size > 0)
                cacheflush(ptr, (int)size, BCACHE);
        }

        private static void* GetImportedMethodPointer(string? dllName, string methodName)
        {
            const int RTLD_NOW = 2;
            const int RTLD_LOCAL = 0;

            IntPtr module = dlopen(dllName, RTLD_NOW | RTLD_LOCAL);

            return GetImportedMethodPointerCore(module, methodName);
        }

        public static void* GetImportedMethodPointerCore(IntPtr module, string methodName)
        {
            fixed (byte* ptr = Encoding.ASCII.GetBytes(methodName))
                return dlsym(module, ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IntPtr dlopen(string? filename, int flags)
        {
            if (filename is null)
                return dlopen((byte*)null, flags);

            fixed (byte* ptr = Encoding.UTF8.GetBytes(filename))
                return dlopen(ptr, flags);
        }

        public enum MemoryMapFlags : uint
        {
            Failed = unchecked((uint)-1),

            Shared = 0x01,
            Private = 0x02,
            SharedValidate = 0x03,
            Fixed = 0x10,
            Anomymous = 0x20,
            NoReserve = 0x4000,
            GrowsDown = 0x0100,
            DenyWrite = 0x0800,
            Executable = 0x1000,
            Locked = 0x2000,
            Populate = 0x8000,
            NonBlock = 0x10000,
            Stack = 0x20000,
            HugeTlb = 0x40000,
            Sync = 0x80000,
            FixedNoReplace = 0x100000,
            File = 0
        }

        [Flags]
        public enum ProtectMemoryPageFlags : int
        {
            None = 0x0,
            CanRead = 0x1,
            CanWrite = 0x2,
            CanExecute = 0x4,
        }
    }
}