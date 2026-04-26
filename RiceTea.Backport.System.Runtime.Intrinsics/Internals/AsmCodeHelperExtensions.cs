#if !NETSTANDARD2_1_OR_GREATER
using System.Runtime.CompilerServices;

namespace System.Runtime.Intrinsics.Internals;

internal static class AsmCodeHelperExtensions
{
    extension(AsmCodeHelper)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void* PackAsmCodeIntoNativeMemory(in ReadOnlySpan<byte> span, nuint length)
        {
            fixed (byte* ptr = span)
                return AsmCodeHelper.PackAsmCodeIntoNativeMemory(ptr, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void InjectAsmCode(RuntimeMethodHandle method, in ReadOnlySpan<byte> span, nuint length)
        {
            fixed (byte* ptr = span)
                AsmCodeHelper.InjectAsmCode(method, ptr, length);
        }
    }
}
#endif