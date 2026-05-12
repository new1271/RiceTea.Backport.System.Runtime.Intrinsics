using System;
using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace RiceTea.Backport.Injection;

/// <summary>
/// The extension class for <see cref="NativeFunctionLoader"/> that supports <see cref="ReadOnlySpan{T}"/>.
/// </summary>
public static class NativeFunctionLoaderExtensions
{
    extension(NativeFunctionLoader)
    {
        /// <inheritdoc cref="NativeFunctionLoader.LoadIntoMemory(byte[])"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFunctionHolder LoadIntoMemory(scoped in ReadOnlySpan<byte> source)
        {
            int length = source.Length;
            if (length <= 0)
                return default;
            return NativeFunctionLoader.LoadIntoMemoryUnsafe(in UnsafeHelper.GetReference(source), (uint)length);
        }

        /// <inheritdoc cref="NativeFunctionLoader.LoadIntoMemoryUnsafe(byte[], uint)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFunctionHolder LoadIntoMemoryUnsafe(scoped in ReadOnlySpan<byte> source, uint length) 
            => NativeFunctionLoader.LoadIntoMemoryUnsafe(in UnsafeHelper.GetReference(source), length);
    }
}