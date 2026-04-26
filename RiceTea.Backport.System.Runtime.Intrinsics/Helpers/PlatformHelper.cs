using System.Runtime.InteropServices;

namespace System.Runtime.Intrinsics.Helpers;

internal static class PlatformHelper
{
    public static readonly bool IsX86, IsX64;

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
    }
}
