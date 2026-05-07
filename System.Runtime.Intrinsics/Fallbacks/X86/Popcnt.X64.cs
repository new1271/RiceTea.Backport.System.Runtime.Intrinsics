namespace RiceTea.Backport.Fallbacks.X86;

using System.Runtime.CompilerServices;

using Intrinsics = System.Runtime.Intrinsics.X86.Popcnt.X64;

partial class Popcnt
{
    /// <summary>
    /// See <see cref="Intrinsics"/>.
    /// </summary>
    public new abstract class X64 : X86Base.X64 
    {
        /// <summary>
        /// See <see cref="Intrinsics.PopCount(ulong)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong PopCount(ulong value)
            => Fallbacks.PopCount(value);
    }
}
