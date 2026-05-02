#if !NETSTANDARD2_1_OR_GREATER
namespace System.Runtime.Intrinsics.Helpers
{
    internal abstract class AssemblyCodeStoreBase
    {
        public static readonly bool IsX64;
        public static readonly bool IsUnix;

        static AssemblyCodeStoreBase()
        {
            if (PlatformHelper.IsX64)
            {
                IsX64 = true;
                IsUnix = PlatformHelper.IsUnix;
            }
            else
            {
                IsX64 = false;
                IsUnix = PlatformHelper.IsUnix && PlatformHelper.IsMono;
            }
        }

        internal abstract class X64
        {
            public static readonly bool IsUnix = PlatformHelper.IsUnix;
        }
    }
}
#endif