namespace RiceTea.Backport.Fallbacks.X86;

using System.Runtime.CompilerServices;

using Intrinsics = System.Runtime.Intrinsics.X86.Lzcnt.X64;

partial class Lzcnt
{
    /// <summary>
    /// See <see cref="Intrinsics"/>.
    /// </summary>
    public new abstract class X64 : X86Base.X64 
    {
        /// <summary>
        /// See <see cref="Intrinsics.LeadingZeroCount(ulong)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong LeadingZeroCount(ulong value)
            => Fallbacks.LeadingZeroCount(value);
    }
}
