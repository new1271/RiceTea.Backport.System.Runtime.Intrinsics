using System.Runtime.CompilerServices;

namespace RiceTea.Backport.Internals;

internal static class SoftDependencyHelper
{
#if NETSTANDARD2_0
    private static readonly bool _systemMemoryExists = CheckSystemMemoryExists();
#endif

    public static bool SystemMemoryExists
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get =>
#if NETSTANDARD2_0
            _systemMemoryExists
#else
            true
#endif
            ;
    }

#if NETSTANDARD2_0
    private static bool CheckSystemMemoryExists()
    {
        try
        {
            return SystemMemoryChecker.CheckSpan() && SystemMemoryChecker.CheckMemory();
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    private static class SystemMemoryChecker
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool CheckSpan() => System.ReadOnlySpan<byte>.Empty.Length == 0;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool CheckMemory() => System.ReadOnlyMemory<byte>.Empty.Length == 0;
    }
#endif
}