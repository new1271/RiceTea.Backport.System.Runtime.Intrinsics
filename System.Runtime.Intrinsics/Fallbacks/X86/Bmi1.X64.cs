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
        /// See <see cref="Intrinsics.TrailingZeroCount(ulong)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong TrailingZeroCount(ulong value)
            => Fallbacks.TrailingZeroCount(value);
    }
}
