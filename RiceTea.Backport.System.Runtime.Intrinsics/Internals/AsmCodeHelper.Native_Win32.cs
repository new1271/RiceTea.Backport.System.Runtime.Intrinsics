#if !NETSTANDARD2_1_OR_GREATER
using System.Runtime.InteropServices;
using System.Security;

namespace System.Runtime.Intrinsics.Internals;

unsafe partial class AsmCodeHelper
{
    [SuppressUnmanagedCodeSecurity]
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
}
#endif