#if !NETSTANDARD2_1_OR_GREATER

namespace System.Runtime.Intrinsics.Helpers;

internal static class PlatformHelper
{
    public static readonly bool IsX86, IsX64, IsMono, IsUnix, IsWindows;

    static PlatformHelper()
    {
#if X86_ARCH
        IsX86 = true;
#if B64_ARCH
        IsX64 = true;
#elif B32_ARCH
        IsX64 = false;
#else
        IsX64 = UIntPtr.Size == sizeof(ulong);
#endif
#else
        var arch = RuntimeInformation.ProcessArchitecture;
        IsX86 = arch switch
        {
            Architecture.X86 or Architecture.X64 => true,
            _ => false,
        };
        IsX64 = arch == Architecture.X64;
#endif

        IsMono = Type.GetType("Mono.Runtime") is not null;
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32S:
            case PlatformID.Win32Windows:
            case PlatformID.Win32NT:
                IsWindows = true;
                IsUnix = false;
                break;
            case PlatformID.Unix:
            case PlatformID.MacOSX:
                IsWindows = false;
                IsUnix = true;
                break;
            default:
                IsWindows =false;
                IsUnix = false;
                break;
        }
    }
}
#endif