using System;
using System.Runtime.CompilerServices;

namespace RiceTea.Backport.Injection;

/// <summary>
/// The extension class for <see cref="NativeFunctionLoader"/> that supports <see cref="ReadOnlySpan{T}"/>.
/// </summary>
public static class NativeFunctionLoaderExtensions
{
    extension(NativeFunctionLoader)
    {
        /// <inheritdoc cref="NativeFunctionLoader.LoadIntoMemory(byte*, nuint)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void* LoadIntoMemory(in ReadOnlySpan<byte> source, nuint length)
        {
            fixed (byte* ptr = source)
                return NativeFunctionLoader.LoadIntoMemory(ptr, length);
        }
    }
}