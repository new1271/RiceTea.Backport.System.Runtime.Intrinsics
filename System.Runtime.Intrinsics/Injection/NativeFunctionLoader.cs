using System;
using System.Runtime.CompilerServices;
using System.Threading;

using InlineIL;

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
    private static nuint _version, _readerCounter, _writerFlag;

    /// <summary>
    /// Load native function into memory.
    /// </summary>
    /// <param name="source">The source that native function stored.</param>
    /// <param name="length">The length of <paramref name="source"/>.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NativeFunctionHolder LoadIntoMemory(byte[] source, nuint length)
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
    public static NativeFunctionHolder LoadIntoMemory(byte* source, nuint length)
    {
        byte* destination;
        EnterWriterLock();
        try
        {
            destination = GetValidStartAddress(length);
            MemoryHelper.LetMemoryPageCanRW(destination, length);
            UnsafeHelper.CopyBlock(destination, source, (uint)length); 
            MemoryHelper.LetMemoryPageCanRX(destination, length);
            MemoryHelper.FlushInstructionCache(destination, length);
        }
        finally
        {
            LeaveWriterLock();
        }
        return new NativeFunctionHolder(destination);
    }

    /// <summary>
    /// Freeze current memory page to prevent writing new machine codes
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FreezeCurrentMemoryPage()
    {
        lock (_syncLock) // Doesn't need a whole writer lock
        {
            _pageStartAddress = null;
            _pageNextAddress = null;
            _pageEndAddress = null;
            AtomicHelper.Increment(ref _version);
        }
    }

    /// <summary>
    /// Freeze current memory page to prevent writing new machine codes
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FreezeMemoryPage(void* address)
    {
        lock (_syncLock) // Doesn't need a whole writer lock
        {
            if (!CheckIsInCurrentPage(address))
                return;
            _pageStartAddress = null;
            _pageNextAddress = null;
            _pageEndAddress = null;
            AtomicHelper.Increment(ref _version);
        }
    }

    private static byte* GetValidStartAddress(nuint requestedSize)
    {
        const uint AddressAlignment = 16u;

        byte* result = _pageNextAddress;
        if (result == null)
            goto NewAllocate;

        byte* pageEndAddress = _pageEndAddress;
        if (result + requestedSize > pageEndAddress)
            goto NewAllocate;

        _pageNextAddress = result + CeilDiv(requestedSize, AddressAlignment) * AddressAlignment;
        goto Result;

    NewAllocate:
        nuint pageSize = _pageSize;
        if (requestedSize > pageSize)
            pageSize = CeilDiv(requestedSize, pageSize) * pageSize;
        result = (byte*)MemoryHelper.AllocMemoryPage(pageSize);
        _pageStartAddress = result;
        _pageNextAddress = result + CeilDiv(requestedSize, AddressAlignment) * AddressAlignment;
        _pageEndAddress = result + pageSize;
        AtomicHelper.Increment(ref _version);

    Result:
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nuint CeilDiv(nuint a, nuint b)
    {
        nuint quotient = a / b;
        return quotient + (((a - quotient * b) != 0) ? 1u : 0u);
    }

    internal static bool CheckIsInCurrentPage(void* address)
    {
        nuint version = Volatile.Read(ref _version);
        byte* pageStartAddress, pageEndAddress;
        do
        {
            pageStartAddress = AtomicHelper.Read(ref _pageStartAddress);
            pageEndAddress = AtomicHelper.Read(ref _pageEndAddress);
            nuint newVersion = Volatile.Read(ref _version);
            if (version == newVersion)
                break;
            version = newVersion;
        } while (true);
        return address >= pageStartAddress && address < pageEndAddress;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void EnterReaderLock()
    {
        AtomicHelper.Increment(ref _readerCounter);
        SpinWait.SpinUntil(static () => (nuint)Volatile.Read(ref _writerFlag) == 0u);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EnterWriterLock()
    {
        Monitor.Enter(_syncLock);
        Volatile.Write(ref _writerFlag, unchecked((nuint)(-1)));
        SpinWait.SpinUntil(static () => Volatile.Read(ref _readerCounter) == default);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void LeaveReaderLock() => AtomicHelper.Decrement(ref _readerCounter);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LeaveWriterLock()
    {
        Volatile.Write(ref _writerFlag, default);
        Monitor.Exit(_syncLock);
    }
}