namespace RiceTea.Backport.Fallbacks.X86;

using System.Runtime.CompilerServices;

using Intrinsics = System.Runtime.Intrinsics.X86.Bmi1.X64;

partial class Bmi1
{
    /// <summary>
    /// See <see cref="Intrinsics"/>.
    /// </summary>
    public new abstract class X64 : X86Base.X64 
    {
        /// <summary>
        /// See <see cref="Intrinsics.AndNot(ulong, ulong)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong AndNot(ulong left, ulong right) => (~left) & right;

        /// <summary>
        /// See <see cref="Intrinsics.BitFieldExtract(ulong, byte, byte)"/>.
        /// </summary>
        public static ulong BitFieldExtract(ulong value, byte start, byte length)
            => BitFieldExtract(value, (ushort)(start | (length << 8)));

        /// <summary>
        /// See <see cref="Intrinsics.BitFieldExtract(ulong, ushort)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong BitFieldExtract(ulong value, ushort control)
            => Fallbacks.BitFieldExtract(value, control);

        /// <summary>
        /// See <see cref="Intrinsics.ExtractLowestSetBit(ulong)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ExtractLowestSetBit(ulong value) => value & (ulong) -(long)value;

        /// <summary>
        /// See <see cref="Intrinsics.GetMaskUpToLowestSetBit(ulong)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetMaskUpToLowestSetBit(ulong value) => value ^ (value - 1);

        /// <summary>
        /// See <see cref="Intrinsics.ResetLowestSetBit(ulong)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ResetLowestSetBit(ulong value) => value & (value - 1);

        /// <summary>
        /// See <see cref="Intrinsics.TrailingZeroCount(ulong)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong TrailingZeroCount(ulong value)
            => Fallbacks.TrailingZeroCount(value);
    }
}
