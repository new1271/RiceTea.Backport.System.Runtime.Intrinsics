using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

using InlineIL;

using RiceTea.Backport.Internals;

namespace RiceTea.Backport.Injection;

/// <summary>
/// A helper class for injecting machine code into call site.
/// </summary>
public static unsafe partial class CallSiteInjector
{
    /// <summary>
    /// The size of x86 CALL instruction
    /// </summary>
    public const int CallInstructionSize = 5;
    /// <summary>
    /// The size of x86 JMP instruction
    /// </summary>
    public const int JumpInstructionSize = 5;
    /// <summary>
    /// The size of x86 JMP short instruction
    /// </summary>
    public const int JumpShortInstructionSize = 2;

    // for JIT dead code elimination (runtime constants)
    private static readonly bool _isX86 = PlatformHelper.IsX86;
    private static readonly bool _isX64 = PlatformHelper.IsX64;
    private static readonly bool _isMono = PlatformHelper.IsMono;
    private static readonly bool _isWindows = PlatformHelper.IsWindows;
    private static readonly bool _isUnix = PlatformHelper.IsUnix;
    private static readonly bool _isLinux = PlatformHelper.IsLinux;
    private static readonly bool _isMacOSX = PlatformHelper.IsMacOSX;
    private static readonly bool _isFreeBSD = PlatformHelper.IsFreeBSD;
    private static readonly bool _isRWXSupported = PlatformHelper.IsRWXSupported;

    // real static fields
    private static readonly ConcurrentDictionary<nuint, ThreadAssociatedLock> _addressLockDict = new();
    private static IntPtr _lastPriorityInstructionHandler;

    [ThreadStatic]
    private static ThreadAssociatedLock? _currentAddressLock;

    private sealed class ThreadAssociatedLock
    {
        private readonly Thread _thread;

        private bool _flag;

        public bool IsCurrentThreadAssociated => Thread.CurrentThread == _thread;

        public bool IsUnlocked
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Volatile.Read(ref _flag);
        }

        public ThreadAssociatedLock()
        {
            _thread = Thread.CurrentThread;
            _flag = false;
        }

        public void Unlock() => Volatile.Write(ref _flag, true);
    }

    /// <summary>
    /// Inject machine code into specific memory area.
    /// </summary>
    /// <param name="startAddress">The start address for the injecting area.</param>
    /// <param name="endAddress">The end address for the injecting area.</param>
    /// <param name="injectorFunc">The machine code injector function.</param>
    /// <param name="exitLockFunc">The sync-lock exiting function.</param>
    /// <exception cref="PlatformNotSupportedException">The platform is not supported.</exception>
    /// <remarks>
    /// The <paramref name="startAddress"/> and <paramref name="endAddress"/> is also included.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Inject(void* startAddress, void* endAddress, delegate* managed<ref void*, ref uint, void> injectorFunc, delegate* managed<void> exitLockFunc)
    {
        if (!_isX86 || (!_isWindows && !_isUnix))
            ThrowUtils.ThrowPlatformNotSupported();

        if (!_isRWXSupported || startAddress is null || startAddress >= endAddress)
            return;

        uint length = (uint)((byte*)endAddress - (byte*)startAddress);
        if (!MemoryHelper.LetMemoryPageCanRWX(startAddress, length)) // We should ignore W^X rule here because hot-patching
            return;

        WriteCallInstruction(startAddress, exitLockFunc);

        void* offsetedStartAddress = (byte*)startAddress + CallInstructionSize;
        void* injectAddress = startAddress;
        uint injectLength = length - CallInstructionSize;

        /*
         * Input: the address and the length that can be injected
         * Output: the address and the length that be injected
         */
        injectorFunc(ref injectAddress, ref injectLength);

        FillNopInstructions(offsetedStartAddress, (uint)((byte*)injectAddress - (byte*)offsetedStartAddress));
        void* injectEndAddress = (byte*)injectAddress + injectLength;
        FillNopInstructions(injectEndAddress, (uint)((byte*)endAddress - (byte*)injectEndAddress));

        void* jumpWritingAddress = (byte*)startAddress - CallInstructionSize;
        // Windows, Linux and the case that jumpWritingAddress is aligned,
        // can be written jump instruction to skip nop sequence
        if (_isLinux || _isWindows || ((nuint)jumpWritingAddress % (nuint)UnsafeHelper.PointerSize) == 0)
            goto WriteJump;
        goto Tail;

    WriteJump:
        WriteJumpInstruction(jumpWritingAddress, injectAddress);
        goto Tail;

    Tail:
        MemoryHelper.FlushInstructionCache(startAddress, length);
    }

    /// <summary>
    /// Find the call site for the calling function.
    /// </summary>
    /// <returns>if found, the result is a pointer to the call site. otherwise be <see langword="null"/>.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void* FindCallSite()
    {
        if (!_isX86 || _isMono || !_isRWXSupported)
            goto Failed;

        if (!StackFrameTool.IsSupported || !StackFrameTool.TryGetNativeFrame(skipFrames: 2, out RuntimeMethodHandle handle, out int offset))
        {
            StackFrame frame =
#if NET5_0_OR_GREATER
            new StackFrame(skipFrames: 2, needFileInfo: false);
#else
            new StackFrame(skipFrames: 2, fNeedFileInfo: false);
#endif
            MethodBase? method = frame.GetMethod();
            if (method is null)
                goto Failed;
            handle = method.MethodHandle;
            offset = frame.GetNativeOffset();
        }
        void* callSiteMethodStartAddress = FindRealEntryPoint(handle); // the caller of caller for FindCallSite
        if (offset > 0)
            return (byte*)callSiteMethodStartAddress + offset;

    Failed:
        return null;
    }

    /// <summary>
    /// Enter the lock for <paramref name="address"/>
    /// </summary>
    /// <remarks>
    /// if <paramref name="address"/> is null, it wiil just return.
    /// </remarks>
    public static void EnterAddressLock(void* address)
    {
        if (address is null)
            return;

        if (_currentAddressLock is not null)
            throw new InvalidOperationException("Nested address lock is not allowed.");

        ThreadAssociatedLock locker = _addressLockDict.GetOrAdd((nuint)address, static _ => new ThreadAssociatedLock());
        if (locker.IsCurrentThreadAssociated)
        {
            _currentAddressLock = locker;
            return;
        }
        if (locker.IsUnlocked)
            return;
        SpinWait wait = new SpinWait();
        do
            wait.SpinOnce();
        while (!locker.IsUnlocked);
    }

    /// <summary>
    /// Leaves the lock for <paramref name="address"/>
    /// </summary>
    /// <remarks>
    /// if <paramref name="address"/> is null, it wiil just return.
    /// </remarks>
    public static void LeaveAddressLock(void* address)
    {
        ThreadAssociatedLock? locker;
        if (address is null || (locker = _currentAddressLock) is null)
            return;
        _currentAddressLock = null;
        locker.Unlock();
        KeyValuePair<nuint, ThreadAssociatedLock> pair = new((nuint)address, locker);
#if NET5_0_OR_GREATER
        _addressLockDict.TryRemove(pair);
#else
        ((ICollection<KeyValuePair<nuint, ThreadAssociatedLock>>)_addressLockDict).Remove(pair);
#endif
    }

    private static void* FindRealEntryPoint(RuntimeMethodHandle handle)
    {
        void* ptr = (void*)handle.GetFunctionPointer();
        while (true)
        {
            switch (UnsafeHelper.ReadUnaligned<byte>(ptr))
            {
                case 0xE8: // CALL
                    {
                        int offset = UnsafeHelper.ReadUnaligned<int>((byte*)ptr + 1);
                        ptr = (byte*)ptr + CallInstructionSize + offset;
                        continue;
                    }
                case 0xE9: // JMP
                    {
                        int offset = UnsafeHelper.ReadUnaligned<int>((byte*)ptr + 1);
                        ptr = (byte*)ptr + JumpInstructionSize + offset;
                        continue;
                    }
                case 0xEB: // JMP short
                    {
                        sbyte shortOffset = UnsafeHelper.ReadUnaligned<sbyte>((byte*)ptr + 1);
                        ptr = (byte*)ptr + JumpShortInstructionSize + shortOffset;
                        continue;
                    }
                default:
                    return ptr;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteCallInstruction(void* ptr, void* target)
    {
        const byte Instruction = 0xE8;
        RealEntryPoint((byte*)ptr, (byte*)target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void RealEntryPoint(byte* ptr, byte* target)
        {
            int offset = (int)(target - ptr) - CallInstructionSize;
            int* pOffset = (int*)(ptr + 1);

            if (((nuint)ptr % (nuint)UnsafeHelper.PointerSize) == 0)
            {
                if (UnsafeHelper.PointerSize < 5)
                    goto NotAligned;
                else
                    goto Aligned_All;
            }
            else
            {
                if (*ptr != Instruction || ((nuint)pOffset) % sizeof(int) != 0)
                    goto NotAligned;
                else
                    goto Aligned_Address;
            }

        NotAligned:
            WriteHaltInstruction(ptr);
            *pOffset = offset;
            *ptr = Instruction;
            return;

        Aligned_All: // whole instruction is aligned, so we use atomic write for better performance
            nuint val = *(nuint*)ptr;
            byte* pVal = (byte*)&val;
            *pVal = Instruction;
            *(int*)(pVal + 1) = offset;
            *(nuint*)ptr = val;
            return;

        Aligned_Address: // only the address is aligned (and the instruction is same)
            *(int*)ptr = offset;
            return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteJumpInstruction(void* ptr, void* target)
    {
        const byte Instruction = 0xE9;
        RealEntryPoint((byte*)ptr, (byte*)target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void RealEntryPoint(byte* ptr, byte* target)
        {
            int offset = (int)(target - ptr) - JumpInstructionSize;
            int* pOffset = (int*)(ptr + 1);

            if (((nuint)ptr % (nuint)UnsafeHelper.PointerSize) == 0)
            {
                if (UnsafeHelper.PointerSize < 5)
                    goto NotAligned;
                else
                    goto Aligned_All;
            }
            else
            {
                if (*ptr != Instruction || ((nuint)pOffset) % sizeof(int) != 0)
                    goto NotAligned;
                else
                    goto Aligned_Address;
            }

        NotAligned:
            WriteHaltInstruction(ptr);
            *pOffset = offset;
            *ptr = Instruction;
            return;

        Aligned_All: // whole instruction is aligned, so we use atomic write for better performance
            nuint val = *(nuint*)ptr;
            byte* pVal = (byte*)&val;
            *pVal = Instruction;
            *(int*)(pVal + 1) = offset;
            *(nuint*)ptr = val;
            return;

        Aligned_Address: // only the address is aligned (and the instruction is same)
            *(int*)ptr = offset;
            return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteHaltInstruction(void* ptr)
    {
        const byte Instruction = 0xF4; // HLT (ring 0 instruction, will be handled by VEH or Signal handler)

        HookPriorityInstructionHandler(); // Hook ring 0 instruction handler
        *(byte*)ptr = Instruction;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FillNopInstructions(void* ptr, uint length)
    {
        if (length > 9)
            LongRoute(ptr, length);
        else
            ShortRoute(ptr, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void LongRoute(void* ptr, uint length)
        {
            UnsafeHelper.InitBlockUnaligned(ptr, 0, length);
            byte* castedPtr = (byte*)ptr;
            do
            {
                *(uint*)castedPtr = 0x84_1F_0F_66; // nop_9: 66 0F 1F 84 00 00 00 00 00
                castedPtr += 9;
                length -= 9;
            } while (length >= 9);
            switch (length) // We just write the data that not zero. (need type the type in UnsafeHelper.WriteUnaligned for non-byte data)
            {
                case 8:
                    *(uint*)castedPtr = 0x00_84_1F_0F; // nop_8: 0F 1F 84 00 00 00 00 00
                    break;
                case 7:
                    *(uint*)castedPtr = 0x00_80_1F_0F; // nop_7: 0F 1F 80 00 00 00 00
                    break;
                case 6:
                    *(uint*)castedPtr = 0x44_1F_0F_66; // nop_6: 66 0F 1F 44 00 00
                    break;
                case 5:
                    *(uint*)castedPtr = 0x00_44_1F_0F; // nop_5: 0F 1F 44 00 00
                    break;
                case 4:
                    *(uint*)castedPtr = 0x00_40_1F_0F; // nop_4: 0F 1F 40 00
                    break;
                case 3:
                    *(ushort*)castedPtr = 0x1F_0F; // nop_3: 0F 1F 00
                    break;
                case 2:
                    *(ushort*)castedPtr = 0x90_66; // nop_2: 66 90
                    break;
                case 1:
                    *castedPtr = 0x90; // nop_1: 90
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ShortRoute(void* ptr, uint length)
        {
            byte* castedPtr = (byte*)ptr;
            switch (length)
            {
                case 9:  // nop_9: 66 0F 1F 84 00 00 00 00 00
                    *(uint*)castedPtr = 0x84_1F_0F_66;
                    *(uint*)(castedPtr + 4) = 0x00_00_00_00;
                    *(castedPtr + 8) = 0x00;
                    break;
                case 8:  // nop_8: 0F 1F 84 00 00 00 00 00
                    *(uint*)castedPtr = 0x00_84_1F_0F;
                    *(uint*)(castedPtr + 4) = 0x00_00_00_00;
                    break;
                case 7: // nop_7: 0F 1F 80 00 00 00 00
                    *(uint*)castedPtr = 0x00_80_1F_0F;
                    *(ushort*)(castedPtr + 4) = 0x00_00;
                    *(castedPtr + 6) = 0x00;
                    break;
                case 6: // nop_6: 66 0F 1F 44 00 00
                    *(uint*)castedPtr = 0x44_1F_0F_66;
                    *(ushort*)(castedPtr + 4) = 0x00_00;
                    break;
                case 5: // nop_5: 0F 1F 44 00 00
                    *(uint*)castedPtr = 0x00_44_1F_0F;
                    *(castedPtr + 4) = 0x00;
                    break;
                case 4: // nop_4: 0F 1F 40 00
                    *(uint*)castedPtr = 0x00_40_1F_0F;
                    break;
                case 3: // nop_3: 0F 1F 00
                    *(ushort*)castedPtr = 0x1F_0F;
                    *(castedPtr + 2) = 0x00;
                    break;
                case 2: // nop_2: 66 90
                    *(ushort*)castedPtr = 0x90_66;
                    break;
                case 1: // nop_1: 90
                    *castedPtr = 0x90;
                    break;
            }
        }
    }

    private static void HookPriorityInstructionHandler()
    {
        ref IntPtr handleRef = ref _lastPriorityInstructionHandler;
        if (_isWindows)
        {
            IntPtr newHandle = Native_Win32.AddVectoredExceptionHandler(First: uint.MaxValue, Handler: VEHHandler.Address);
            if (newHandle != IntPtr.Zero)
            {
                IntPtr oldHandle = Interlocked.Exchange(ref handleRef, newHandle);
                if (oldHandle != IntPtr.Zero)
                    Native_Win32.RemoveVectoredExceptionHandler(oldHandle);
            }
            return;
        }
        if (_isLinux)
        {
            RuntimeTypeHandle typeHandle;
            if (_isX64)
            {
                IL.Emit.Ldtoken(typeof(SignalActionRegisterX64));
                IL.Pop(out typeHandle);
            }
            else if (_isX86)
            {
                IL.Emit.Ldtoken(typeof(SignalActionRegisterX86));
                IL.Pop(out typeHandle);
            }
            else
                goto Throw;
            RuntimeHelpers.RunClassConstructor(typeHandle);
        }

    Throw:
        ThrowUtils.ThrowPlatformNotSupported();
    }
}