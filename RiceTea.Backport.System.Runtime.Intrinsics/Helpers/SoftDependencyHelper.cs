#if !NETSTANDARD2_1_OR_GREATER
using System.Runtime.CompilerServices;

namespace System.Runtime.Intrinsics.Helpers;

internal static class SoftDependencyHelper
{
    private static readonly bool _systemMemoryExists = CheckSystemMemoryExists();

    public static bool SystemMemoryExists
    {
        get => _systemMemoryExists;
    }

    private static bool CheckSystemMemoryExists()
    {
        try
        {
            return SystemMemoryChecker.CheckSpan() && SystemMemoryChecker.CheckMemory();
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static class SystemMemoryChecker
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool CheckSpan() => ReadOnlySpan<byte>.Empty.Length == 0;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool CheckMemory() => ReadOnlyMemory<byte>.Empty.Length == 0;
    }
}
#endif