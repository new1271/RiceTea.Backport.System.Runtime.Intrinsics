using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RiceTea.Backport.Injection
{
    /// <summary>
    /// A structure that holding the function address from <see cref="NativeFunctionLoader.LoadIntoMemoryUnsafe(byte*, uint)"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public readonly unsafe struct NativeFunctionHolder : IEquatable<NativeFunctionHolder>
    {
        /// <summary>
        /// Represents an empty or uninitialized instance of the <see cref="NativeFunctionHolder"/> structure.
        /// </summary>
        /// <remarks>
        /// Use this field to indicate the absence of a native function or as a default value
        /// when no valid <see cref="NativeFunctionHolder"/> is available.
        /// </remarks>
        public static readonly NativeFunctionHolder Empty = new NativeFunctionHolder(null);

        private readonly void* _address;

        internal NativeFunctionHolder(void* address)
        {
            _address = address;
        }

        /// <summary>
        /// Enter a scope to access the function address associated with this <see cref="NativeFunctionHolder"/>. 
        /// </summary>
        /// <returns></returns>
        public readonly NativeFunctionAccessScope Enter() => new NativeFunctionAccessScope(_address);

        /// <inheritdoc/>
        public readonly bool Equals(NativeFunctionHolder other) => _address == other._address;

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj) => obj is NativeFunctionHolder other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => (int)_address;

        /// <inheritdoc/>
        public static bool operator ==(NativeFunctionHolder left, NativeFunctionHolder right) => left._address == right._address;

        /// <inheritdoc/>
        public static bool operator !=(NativeFunctionHolder left, NativeFunctionHolder right) => left._address != right._address;
    }

    /// <summary>
    /// The access scope for <see cref="NativeFunctionHolder"/>.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public unsafe ref struct NativeFunctionAccessScope : IDisposable
    {
        private readonly bool _shouldRelease;
        private void* _address;

        internal NativeFunctionAccessScope(void* address)
        {
            if (NativeFunctionLoader.CheckIsInCurrentPage(address))
            {
                NativeFunctionLoader.EnterReaderLock();
                _shouldRelease = true;
            }
            _address = address;
        }

        /// <summary>
        /// The function address that associated with this <see cref="NativeFunctionAccessScope"/>.
        /// </summary>
        public readonly void* Address
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _address;
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (_address is null)
                return;
            _address = null;
            if (_shouldRelease)
                NativeFunctionLoader.LeaveReaderLock();
        }
    }
}
