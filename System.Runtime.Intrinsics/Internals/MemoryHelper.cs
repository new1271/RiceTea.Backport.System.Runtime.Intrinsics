using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RiceTea.Backport.Internals;

internal static unsafe partial class MemoryHelper
{
    private static readonly bool _isWindows = PlatformHelper.IsWindows, _isUnix = PlatformHelper.IsUnix;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* AllocMemoryPage(nuint pageSize)
    {
        if (_isWindows)
            return Native_Win32.VirtualAlloc(null, pageSize,
                Native_Win32.MemoryAllocationTypes.Commit | Native_Win32.MemoryAllocationTypes.Reserve, Native_Win32.PageAccessRights.ReadWrite);
        if (_isUnix)
            return Native_Unix.mmap(null, pageSize,
                Native_Unix.ProtectMemoryPageFlags.CanRead | Native_Unix.ProtectMemoryPageFlags.CanWrite,
                Native_Unix.MemoryMapFlags.Private | Native_Unix.MemoryMapFlags.Anomymous, -1, 0);
        return (void*)Marshal.AllocHGlobal((nint)pageSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FreeMemoryPage(void* ptr , nuint pageSize)
    {
        if (_isWindows)
            Native_Win32.VirtualFree(ptr, dwSize: 0, 0x00008000);
        else if (_isUnix)
            Native_Unix.munmap(ptr, pageSize);
        else
            Marshal.FreeHGlobal((IntPtr)ptr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* GetFuncAddress_OnlyUnix(string name)
    {
        if (!_isUnix)
            return (void*)ThrowUtils.ThrowPlatformNotSupported<nuint>();
        return Native_Unix.GetImportedMethodPointer(null, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LetMemoryPageCanRW(void* pageStartAddress, nuint pageSize)
    {
        if (_isWindows)
        {
            Native_Win32.PageAccessRights dropped;
            return Native_Win32.VirtualProtect(pageStartAddress, pageSize, Native_Win32.PageAccessRights.ReadWrite, &dropped) != 0;
        }
        if (_isUnix)
        {
            return Native_Unix.mprotect(pageStartAddress, pageSize,
                Native_Unix.ProtectMemoryPageFlags.CanRead | Native_Unix.ProtectMemoryPageFlags.CanWrite) != -1;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LetMemoryPageCanRX(void* pageStartAddress, nuint pageSize)
    {
        if (_isWindows)
        {
            Native_Win32.PageAccessRights dropped;
            return Native_Win32.VirtualProtect(pageStartAddress, pageSize, Native_Win32.PageAccessRights.ExecuteRead, &dropped) != 0;
        }
        if (_isUnix)
        {
            return Native_Unix.mprotect(pageStartAddress, pageSize,
                Native_Unix.ProtectMemoryPageFlags.CanRead | Native_Unix.ProtectMemoryPageFlags.CanExecute) != -1;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LetMemoryPageCanRWX(void* pageStartAddress, nuint pageSize)
    {
        if (_isWindows)
        {
            Native_Win32.PageAccessRights dropped;
            return Native_Win32.VirtualProtect(pageStartAddress, pageSize, Native_Win32.PageAccessRights.ExecuteReadWrite, &dropped) != 0;
        }
        if (_isUnix)
        {
            return Native_Unix.mprotect(pageStartAddress, pageSize,
                Native_Unix.ProtectMemoryPageFlags.CanRead | Native_Unix.ProtectMemoryPageFlags.CanExecute | Native_Unix.ProtectMemoryPageFlags.CanWrite) != -1;
        }
        return false;
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
