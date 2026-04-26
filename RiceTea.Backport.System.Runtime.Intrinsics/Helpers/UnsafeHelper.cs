using InlineIL;

using InlineMethod;

namespace System.Runtime.Intrinsics.Helpers;

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
    public static TTo As<TFrom, TTo>(TFrom source)
    {
        IL.Emit.Ldarg_0();
        IL.Emit.Ret();
        throw IL.Unreachable();
    }
}
