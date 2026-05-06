using System;
using System.Runtime.InteropServices;

namespace RiceTea.Backport.Internals;

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

#if NETCOREAPP3_0_OR_GREATER
        IsMono = false;
        IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        IsUnix = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
            RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) || 
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#elif NETSTANDARD2_1_OR_GREATER
        IsMono = Type.GetType("Mono.Runtime") is not null;
        IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        IsUnix = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
            RuntimeInformation.IsOSPlatform(OSPlatform.Create("FREEBSD")) || 
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#else
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
#endif
    }
}