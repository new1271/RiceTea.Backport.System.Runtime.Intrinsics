#if !NETSTANDARD2_1_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

using InlineIL;

using InlineMethod;

namespace System.Runtime.Intrinsics.Internals;

internal static unsafe partial class AsmCodeHelper
{
    private static readonly object _syncLock = new object();
    private static readonly nuint _pageSize = unchecked((nuint)Environment.SystemPageSize);
    private static readonly PlatformID _platformId = Environment.OSVersion.Platform;

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

        _pageNextAddress = result + CeilDiv(requestedSize, addressAlignment) * addressAlignment;
        goto Result;

    ChangeAddress:
        byte* pageStartAddress = _pageStartAddress;
        LetMemoryPageCanRX(pageStartAddress, unchecked((nuint)(pageEndAddress - pageStartAddress)));

    NewAllocate:
        nuint pageSize = _pageSize;
        if (requestedSize > pageSize)
            pageSize = CeilDiv(requestedSize, pageSize) * pageSize;
        result = (byte*)AllocNewPage(pageSize);
        _pageStartAddress = result;
        _pageNextAddress = result + CeilDiv(requestedSize, addressAlignment) * addressAlignment;
        _pageEndAddress = result + pageSize;

    Result:
        return result;
    }

    private static nuint CeilDiv(nuint a, nuint b)
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
        switch (_platformId)
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
    public static void LetMemoryPageCanRX(void* pageStartAddress, nuint pageSize)
    {
        switch (_platformId)
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
    public static void LetMemoryPageCanRWX(void* pageStartAddress, nuint pageSize)
    {
        switch (_platformId)
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
    public static void FlushInstructionCache(void* pageStartAddress, nuint pageSize)
    {
        switch (_platformId)
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
}
#endif