using System;
using System.Runtime.InteropServices;

namespace RiceTea.Backport.Internals;

internal static class PlatformHelper
{
    public static readonly bool IsX86, IsX64, IsMono, IsUnix, IsWindows, IsLinux, IsMacOSX, IsFreeBSD;

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

        OSPlatform freeBsd;
#if NETCOREAPP3_0_OR_GREATER
        IsMono = false;
        freeBsd = OSPlatform.FreeBSD;
#else
        IsMono = Type.GetType("Mono.Runtime") is not null;
        freeBsd = OSPlatform.Create("FREEBSD");
#endif
        IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            IsUnix = true;
            IsLinux = true;
            IsMacOSX = false;
            IsFreeBSD = false;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            IsUnix = true;
            IsLinux = false;
            IsMacOSX = true;
            IsFreeBSD = false;
        }
        else if (RuntimeInformation.IsOSPlatform(freeBsd))
        {
            IsUnix = true;
            IsLinux = false;
            IsMacOSX = false;
            IsFreeBSD = true;
        }
        else
        {
            IsUnix = false;
            IsLinux = false;
            IsMacOSX = false;
            IsFreeBSD = false;
        }
    }
}