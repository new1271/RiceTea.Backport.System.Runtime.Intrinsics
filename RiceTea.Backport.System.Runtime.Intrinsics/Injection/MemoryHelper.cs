using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using RiceTea.Backport.Internals;

namespace RiceTea.Backport.Injection;

internal static unsafe partial class MemoryHelper
{
    private static readonly bool _isWindows = PlatformHelper.IsWindows, _isUnix = PlatformHelper.IsUnix;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* AllocNewPage(nuint pageSize)
    {
        if (_isWindows)
            return Native_Win32.VirtualAlloc(null, pageSize,
                Native_Win32.MemoryAllocationTypes.Commit | Native_Win32.MemoryAllocationTypes.Reserve, Native_Win32.PageAccessRights.ExecuteReadWrite);
        if (_isUnix)
            return Native_Unix.mmap(null, pageSize,
                Native_Unix.ProtectMemoryPageFlags.CanRead | Native_Unix.ProtectMemoryPageFlags.CanWrite | Native_Unix.ProtectMemoryPageFlags.CanExecute,
                Native_Unix.MemoryMapFlags.Private | Native_Unix.MemoryMapFlags.Anomymous, -1, 0);
        return (void*)Marshal.AllocHGlobal((nint)pageSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LetMemoryPageCanRX(void* pageStartAddress, nuint pageSize)
    {
        if (_isWindows)
        {
            Native_Win32.PageAccessRights dropped;
            Native_Win32.VirtualProtect(pageStartAddress, pageSize, Native_Win32.PageAccessRights.ExecuteRead, &dropped);
            return;
        }
        if (_isUnix)
        {
            Native_Unix.mprotect(pageStartAddress, pageSize,
                Native_Unix.ProtectMemoryPageFlags.CanRead | Native_Unix.ProtectMemoryPageFlags.CanExecute);
            return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LetMemoryPageCanRWX(void* pageStartAddress, nuint pageSize)
    {
        if (_isWindows)
        {
            Native_Win32.PageAccessRights dropped;
            Native_Win32.VirtualProtect(pageStartAddress, pageSize, Native_Win32.PageAccessRights.ExecuteReadWrite, &dropped);
            return;
        }
        if (_isUnix)
        {
            Native_Unix.mprotect(pageStartAddress, pageSize,
                Native_Unix.ProtectMemoryPageFlags.CanRead | Native_Unix.ProtectMemoryPageFlags.CanExecute | Native_Unix.ProtectMemoryPageFlags.CanWrite);
            return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FlushInstructionCache(void* pageStartAddress, nuint pageSize)
    {
        if (_isWindows)
        {
            Native_Win32.FlushInstructionCache(pageStartAddress, pageSize);
            return;
        }
        if (_isUnix)
        {
            Native_Unix.FlushInstructionCache(pageStartAddress, pageSize);
            return;
        }
    }
}
