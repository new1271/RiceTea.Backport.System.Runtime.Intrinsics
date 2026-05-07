using System;
using System.Runtime.CompilerServices;

using InlineIL;

using InlineMethod;

using RiceTea.Backport.Internals;

namespace RiceTea.Backport.Injection;

/// <summary>
/// A helper class for loading native function into memory.
/// </summary>
public static unsafe partial class NativeFunctionLoader
{
    private static readonly object _syncLock = new object();
    private static readonly nuint _pageSize = unchecked((nuint)Environment.SystemPageSize);
    private static readonly PlatformID _platformId = Environment.OSVersion.Platform;

    private static byte* _pageStartAddress, _pageNextAddress, _pageEndAddress;

    /// <summary>
    /// Load native function into memory.
    /// </summary>
    /// <param name="source">The source that native function stored.</param>
    /// <param name="length">The length of <paramref name="source"/>.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* LoadIntoMemory(byte[] source, nuint length)
    {
        fixed (byte* ptr = source)
            return LoadIntoMemory(ptr, length);
    }

    /// <summary>
    /// Load native function into memory.
    /// </summary>
    /// <param name="source">The source that native function stored.</param>
    /// <param name="length">The length of <paramref name="source"/>.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* LoadIntoMemory(byte* source, nuint length)
    {
        byte* destination = GetValidStartAddress(length);
        UnsafeHelper.CopyBlock(destination, source, (uint)length);
        return destination;
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
        MemoryHelper.LetMemoryPageCanRX(pageStartAddress, unchecked((nuint)(pageEndAddress - pageStartAddress)));

    NewAllocate:
        nuint pageSize = _pageSize;
        if (requestedSize > pageSize)
            pageSize = CeilDiv(requestedSize, pageSize) * pageSize;
        result = (byte*)MemoryHelper.AllocNewPage(pageSize);
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
}