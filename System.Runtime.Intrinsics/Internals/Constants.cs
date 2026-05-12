using System.Runtime.CompilerServices;

namespace RiceTea.Backport.Internals
{
    internal static class Constants
    {
        public const MethodImplOptions SpanSourceInliningOptions =
#if NETSTANDARD2_0
                MethodImplOptions.NoInlining
#else
                MethodImplOptions.AggressiveInlining
#endif
        ;
    }
}
