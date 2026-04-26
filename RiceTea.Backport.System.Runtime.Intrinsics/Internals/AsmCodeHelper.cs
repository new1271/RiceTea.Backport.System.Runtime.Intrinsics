#if !NETSTANDARD2_1_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using InlineIL;

using InlineMethod;

namespace System.Runtime.Intrinsics.Internals;

internal static unsafe class AsmCodeHelper
{
    private static readonly object _syncLock = new object();
    private static readonly nuint _pageSize = unchecked((nuint)Environment.SystemPageSize);

    private static byte* _pageStartAddress, _pageNextAddress, _pageEndAddress;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* PackAsmCodeIntoNativeMemory(byte[] source, nuint length)
    {
        fixed (byte* ptr = source)
            return PackAsmCodeIntoNativeMemory(ptr, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* PackAsmCodeIntoNativeMemory(byte* source, nuint length)
    {
        IL.Emit.Ldarg_1();
        IL.Emit.Call(new MethodRef(typeof(AsmCodeHelper), nameof(GetValidStartAddress)));
        IL.Emit.Dup();
        IL.Emit.Ldarg_0();
        IL.Emit.Ldarg_1();
        IL.Emit.Cpblk();
        IL.Emit.Ret();
        throw IL.Unreachable();
    }

    private static byte* GetValidStartAddress(nuint requestedSize)
    {
        lock (_syncLock)
            return GetValidStartAddressCore(requestedSize);
    }

    [Inline(InlineBehavior.Remove)]
    private static byte* GetValidStartAddressCore(nuint requestedSize)
    {
        nuint addressAlignment = (nuint)sizeof(void*);

        byte* result = _pageNextAddress;
        if (result == null)
            goto NewAllocate;

        byte* pageEndAddress = _pageEndAddress;
        if (result + requestedSize > pageEndAddress)
            goto ChangeAddress;

        _pageNextAddress = result + CeilDiv_Internal(requestedSize, addressAlignment) * addressAlignment;
        goto Result;

    ChangeAddress:
        byte* pageStartAddress = _pageStartAddress;
        LetMemoryPageCanRX(pageStartAddress, unchecked((nuint)(pageEndAddress - pageStartAddress)));

    NewAllocate:
        nuint pageSize = _pageSize;
        if (requestedSize > pageSize)
            pageSize = CeilDiv_Internal(requestedSize, pageSize) * pageSize;
        result = (byte*)AllocNewPage(pageSize);
        _pageStartAddress = result;
        _pageNextAddress = result + CeilDiv_Internal(requestedSize, addressAlignment) * addressAlignment;
        _pageEndAddress = result + pageSize;

    Result:
        return result;
    }

    private static nuint CeilDiv_Internal(nuint a, nuint b) // 由於 Fallbacks 也會使用這個類別，為避免造成循環參考，故在這裡重新實作一份 CeilDiv
    {
        nuint quotient = a / b;
        return quotient + (((a - quotient * b) != 0) ? 1u : 0u);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InjectAsmCode(RuntimeMethodHandle method, byte[] source, nuint length)
    {
        fixed (byte* ptr = source)
            InjectAsmCode(method, ptr, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InjectAsmCode(RuntimeMethodHandle method, byte* source, nuint length)
    {
        RuntimeHelpers.PrepareMethod(method);

        void* dest = (void*)method.GetFunctionPointer();

        while (true)
        {
            switch (*(byte*)dest)
            {
                case 0xE8:
                case 0xE9:
                    int offset = *(int*)((byte*)dest + 1);
                    dest = (byte*)dest + 5 + offset;
                    continue;
                case 0xEB:
                    sbyte shortOffset = *(sbyte*)((byte*)dest + 1);
                    dest = (byte*)dest + 2 + shortOffset;
                    continue;
                default:
                    goto OutOfLoop;
            }
        }
    OutOfLoop:

        // 此處可安全使用 NativeMethods，因為非 Intrinsics static constuctor 會呼叫到的部分
        LetMemoryPageCanRWX(dest, length);
        IL.Push(dest);
        IL.Push(source);
        IL.Push(length);
        IL.Emit.Cpblk();
        FlushInstructionCache(dest, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* AllocNewPage(nuint pageSize)
    {
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32S:
            case PlatformID.Win32Windows:
            case PlatformID.Win32NT:
                return Native_Win32.VirtualAlloc(null, pageSize,
                    Native_Win32.MemoryAllocationTypes.Commit | Native_Win32.MemoryAllocationTypes.Reserve, Native_Win32.PageAccessRights.ExecuteReadWrite);
            case PlatformID.Unix:
            case PlatformID.MacOSX:
                return Native_Unix.mmap(null, pageSize,
                    Native_Unix.ProtectMemoryPageFlags.CanRead | Native_Unix.ProtectMemoryPageFlags.CanWrite | Native_Unix.ProtectMemoryPageFlags.CanExecute,
                    Native_Unix.MemoryMapFlags.Private | Native_Unix.MemoryMapFlags.Anomymous, -1, 0);
        }
        return (void*)Marshal.AllocHGlobal((nint)pageSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LetMemoryPageCanRX(void* pageStartAddress, nuint pageSize)
    {
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32S:
            case PlatformID.Win32Windows:
            case PlatformID.Win32NT:
                {
                    Native_Win32.PageAccessRights dropped;
                    Native_Win32.VirtualProtect(pageStartAddress, pageSize, Native_Win32.PageAccessRights.ExecuteRead, &dropped);
                }
                break;
            case PlatformID.Unix:
            case PlatformID.MacOSX:
                {
                    Native_Unix.mprotect(pageStartAddress, pageSize,
                        Native_Unix.ProtectMemoryPageFlags.CanRead | Native_Unix.ProtectMemoryPageFlags.CanExecute);
                }
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LetMemoryPageCanRWX(void* pageStartAddress, nuint pageSize)
    {
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32S:
            case PlatformID.Win32Windows:
            case PlatformID.Win32NT:
                {
                    Native_Win32.PageAccessRights dropped;
                    Native_Win32.VirtualProtect(pageStartAddress, pageSize, Native_Win32.PageAccessRights.ExecuteReadWrite, &dropped);
                }
                break;
            case PlatformID.Unix:
            case PlatformID.MacOSX:
                {
                    Native_Unix.mprotect(pageStartAddress, pageSize,
                        Native_Unix.ProtectMemoryPageFlags.CanRead | Native_Unix.ProtectMemoryPageFlags.CanExecute | Native_Unix.ProtectMemoryPageFlags.CanWrite);
                }
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FlushInstructionCache(void* pageStartAddress, nuint pageSize)
    {
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32S:
            case PlatformID.Win32Windows:
            case PlatformID.Win32NT:
                {
                    Native_Win32.FlushInstructionCache(pageStartAddress, pageSize);
                }
                break;
            case PlatformID.Unix:
            case PlatformID.MacOSX:
                {
                    Native_Unix.FlushInstructionCache(pageStartAddress, pageSize);
                }
                break;
        }
    }

    private static class Native_Win32
    {
        private static readonly IntPtr _process = GetCurrentProcess();

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        public static extern void* VirtualAlloc(void* address, nuint dwSize, MemoryAllocationTypes allocationTypes, PageAccessRights rights);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        public static extern int VirtualProtect(void* address, nuint dwSize, PageAccessRights rights, PageAccessRights* oldRights);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        private static extern int FlushInstructionCache(IntPtr hProcess, void* lpBaseAddress, nuint dwSize);

        public static void FlushInstructionCache(void* address, nuint dwSize)
            => FlushInstructionCache(_process, address, dwSize);

        [Flags]
        public enum MemoryAllocationTypes : uint
        {
            None = 0,
            Commit = 0x00001000,
            Reserve = 0x00002000,
            ReplacePlaceholder = 0x00004000,
            ReservePlaceholder = 0x00040000,
            Reset = 0x00080000,
            TopDown = 0x00100000,
            WriteWatch = 0x00200000,
            Physical = 0x00400000,
            Rotate = 0x00800000,
            DifferenceImageBaseOk = 0x00800000,
            ResetUndo = 0x01000000,
            LargePages = 0x20000000,
            Alloc4MbPages = 0x80000000,
            Alloc64KPages = (LargePages | Physical),
            UnmapWithTransientBoost = 0x00000001,
            Coalesce_Placeholders = 0x00000001,
            PreservePlaceholder = 0x00000002,
            Decommit = 0x00004000,
            Release = 0x00008000,
            Free = 0x00010000
        }

        [Flags]
        public enum PageAccessRights : uint
        {
            None = 0x00,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            Guard = 0x100,
            NoCache = 0x200,
            WriteCombine = 0x400,
            GraphicsNoAccess = 0x0800,
            GraphicsReadOnly = 0x1000,
            GraphicsReadWrite = 0x2000,
            GraphicsExecute = 0x4000,
            GraphicsExecuteRead = 0x8000,
            GraphicsExecuteReadWrite = 0x10000,
            GraphicsConherent = 0x20000,
            GraphicsNoCache = 0x40000,
            EnclaveThreadControl = 0x80000000,
            RevertToFileMap = 0x80000000,
            TargetsNoUpdate = 0x40000000,
            TargetsInvalid = 0x40000000,
            EnclaveUnvalidated = 0x20000000,
            EnclaveMask = 0x10000000,
            EnclaveDecommit = (EnclaveMask | 0),
            EnclaveSSFirst = (EnclaveMask | 1),
            EnclaveSSRest = (EnclaveMask | 2),
        }
    }

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
#endif