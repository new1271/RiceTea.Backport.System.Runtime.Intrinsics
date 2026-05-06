using System;
using System.Runtime.CompilerServices;

namespace RiceTea.Backport.Injection;

internal static class NativeFunctionLoaderExtensions
{
    extension(NativeFunctionLoader)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void* LoadAsmCodeIntoMemory(in ReadOnlySpan<byte> span, nuint length)
        {
            fixed (byte* ptr = span)
                return NativeFunctionLoader.LoadIntoMemory(ptr, length);
        }
    }
}