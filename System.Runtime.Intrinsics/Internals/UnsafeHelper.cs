using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using InlineIL;

using InlineMethod;

namespace RiceTea.Backport.Internals;

internal static unsafe class UnsafeHelper
{
    public const int PointerSizeConstant_Indeterminate = 0;

    public const int PointerSizeConstant
#if ANYCPU
            = PointerSizeConstant_Indeterminate;
#elif B32_ARCH
                = sizeof(uint);
#elif B64_ARCH
                = sizeof(ulong);
#else
                = PointerSizeConstant_Indeterminate;
#endif

    public static int PointerSize
    {
        [Inline(InlineBehavior.Keep, export: true)]
        get => PointerSizeConstant switch
        {
            PointerSizeConstant_Indeterminate => sizeof(void*),
            _ => PointerSizeConstant,
        };
    }

    [Inline(InlineBehavior.Remove)]
    public static ref T AddByteOffset<T>(ref readonly T source, nint byteOffset)
    {
        IL.PushInRef(in source);
        IL.Push(byteOffset);
        IL.Emit.Add();
        return ref IL.ReturnRef<T>();
    }

    [Inline(InlineBehavior.Remove)]
    public static ref T AddByteOffset<T>(ref readonly T source, nuint byteOffset)
    {
        IL.PushInRef(in source);
        IL.Push(byteOffset);
        IL.Emit.Add();
        return ref IL.ReturnRef<T>();
    }

    [Inline(InlineBehavior.Remove)]
    public static ref TTo As<TFrom, TTo>(ref TFrom source)
    {
        IL.Emit.Ldarg_0();
        IL.Emit.Ret();
        throw IL.Unreachable();
    }

    [Inline(InlineBehavior.Remove)]
    public static TTo As<TFrom, TTo>(TFrom source)
    {
        IL.Emit.Ldarg_0();
        IL.Emit.Ret();
        throw IL.Unreachable();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    public static T As<T>(object source) where T : class
    {
        IL.Push(source);
        return IL.Return<T>();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    public static nuint ByteOffsetUnsigned<T>(ref readonly T origin, ref readonly T target)
    {
        IL.PushInRef(in target);
        IL.PushInRef(in origin);
        IL.Emit.Sub();
        return IL.Return<nuint>();
    }

    [Inline(InlineBehavior.Remove)]
    public static void CopyBlock(void* destination, void* source, uint byteCount)
    {
        IL.Emit.Ldarg_0();
        IL.Emit.Ldarg_1();
        IL.Emit.Ldarg_2();
        IL.Emit.Cpblk();
    }

    [Inline(InlineBehavior.Remove)]
    public static void CopyBlock(void* destination, ref readonly byte source, uint byteCount)
    {
        IL.Emit.Ldarg_0();
        IL.Emit.Ldarg_1();
        IL.Emit.Ldarg_2();
        IL.Emit.Cpblk();
    }

    [Inline(InlineBehavior.Remove)]
    public static void InitBlock(void* ptr, byte b, uint byteCount)
    {
        IL.Emit.Ldarg_0();
        IL.Emit.Ldarg_1();
        IL.Emit.Ldarg_2();
        IL.Emit.Initblk();
    }

#if NET5_0_OR_GREATER
    [Inline(InlineBehavior.Remove)]
    public static ref T GetReference<T>(T[] array)
        => ref MemoryMarshal.GetArrayDataReference(array);
#else
    private static readonly bool _isMono = PlatformHelper.IsMono;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetReference<T>(T[] array)
    {
        if (!_isMono)
            return ref FastRoute(array);

        return ref LegacyArrayHelper.GetReference(array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ref T FastRoute(T[] array) // 切割方法以誘導 JIT 內聯
            => ref AddByteOffset(ref As<byte, T>(ref As<RawData>(array).Data), PointerSize);
    }

    [StructLayout(LayoutKind.Sequential)]
    private sealed class RawData
    {
        public byte Data;
    }

    private static class LegacyArrayHelper
    {
        private static readonly nuint Offset = GetFirstElementOffsetOfArray();

        private static nuint GetFirstElementOffsetOfArray()
        {
            byte[] array = [default];
            return ByteOffsetUnsigned(ref As<RawData>(array).Data, ref array[0]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetReference<T>(T[] array)
            => ref As<byte, T>(ref AddByteOffset(ref As<RawData>(array).Data, Offset));
    }
#endif
}