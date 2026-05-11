using System;
using System.Runtime.CompilerServices;
using System.Threading;

using InlineIL;

namespace RiceTea.Backport.Internals;

internal static unsafe class AtomicHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint Increment(ref nuint reference)
        => UnsafeHelper.PointerSizeConstant switch
        {
            sizeof(int) => UnsafeHelper.As<int, nuint>(Interlocked.Increment(ref UnsafeHelper.As<nuint, int>(ref reference))),
            sizeof(long) => UnsafeHelper.As<long, nuint>(Interlocked.Increment(ref UnsafeHelper.As<nuint, long>(ref reference))),
            UnsafeHelper.PointerSizeConstant_Indeterminate => UnsafeHelper.PointerSize switch
            {
                sizeof(int) => UnsafeHelper.As<int, nuint>(Interlocked.Increment(ref UnsafeHelper.As<nuint, int>(ref reference))),
                sizeof(long) => UnsafeHelper.As<long, nuint>(Interlocked.Increment(ref UnsafeHelper.As<nuint, long>(ref reference))),
                _ => ThrowUtils.ThrowPlatformNotSupported<nuint>()
            },
            _ => ThrowUtils.ThrowPlatformNotSupported<nuint>()
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint Decrement(ref nuint reference)
        => UnsafeHelper.PointerSizeConstant switch
        {
            sizeof(int) => UnsafeHelper.As<int, nuint>(Interlocked.Decrement(ref UnsafeHelper.As<nuint, int>(ref reference))),
            sizeof(long) => UnsafeHelper.As<long, nuint>(Interlocked.Decrement(ref UnsafeHelper.As<nuint, long>(ref reference))),
            UnsafeHelper.PointerSizeConstant_Indeterminate => UnsafeHelper.PointerSize switch
            {
                sizeof(int) => UnsafeHelper.As<int, nuint>(Interlocked.Decrement(ref UnsafeHelper.As<nuint, int>(ref reference))),
                sizeof(long) => UnsafeHelper.As<long, nuint>(Interlocked.Decrement(ref UnsafeHelper.As<nuint, long>(ref reference))),
                _ => ThrowUtils.ThrowPlatformNotSupported<nuint>()
            },
            _ => ThrowUtils.ThrowPlatformNotSupported<nuint>()
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte* Read(ref byte* reference)
    {
        IL.Emit.Ldarg_0();
        IL.Emit.Call(new MethodRef(typeof(Volatile), nameof(Volatile.Read), TypeRef.Type<UIntPtr>().MakeByRefType()));
        IL.Emit.Ret();
        throw IL.Unreachable();
    }
}