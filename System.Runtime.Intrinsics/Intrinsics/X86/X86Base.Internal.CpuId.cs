#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0
#if X86_ARCH || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Injection;
using RiceTea.Backport.Internals;

namespace System.Runtime.Intrinsics.X86;

partial class X86Base
{
    /*
     * extern "C" void  __cdecl
     * cpuid_wrap_vcpp(int foo[4], int e, int f)
     * {
     *     __cpuidex(foo, e, f);
     * }   
     * 
     * extern "C" void  __cdecl
     *  cpuid_wrap_clang(int foo[4], int e, int f)
     * {
     *     unsigned int eax, ebx, ecx, edx;
     *     
     *     __cpuid_count(e, f, eax, ebx, ecx, edx);
     *     
     *     foo[0] = eax;
     *     foo[1] = ebx;
     *     foo[2] = ecx;
     *     foo[3] = edx;
     * }    
     */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static NativeFunctionHolder BuildCpuIdAsm()
    {
#if B64_ARCH
        return BuildCpuIdAsm_X64();
#elif B32_ARCH
        return BuildCpuIdAsm_X86();
#else
        return PlatformHelper.IsX64 ? BuildCpuIdAsm_X64() : BuildCpuIdAsm_X86();
#endif
    }

#if (B32_ARCH || ANYCPU)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static NativeFunctionHolder BuildCpuIdAsm_X86() 
        => SoftDependencyHelper.SystemMemoryExists ? StoreAsSpan.BuildCpuIdAsm_X86() : StoreAsArray.BuildCpuIdAsm_X86();

    partial class StoreAsArray
    {
        public static NativeFunctionHolder BuildCpuIdAsm_X86()
        {
            const int Length = 30;
            byte[] data = new byte[Length] {
                0x8B, 0x44, 0x24, 0x08, 0x8B, 0x4C, 0x24, 0x0C, 
                0x53, 0x56, 0x8B, 0x74, 0x24, 0x0C, 0x0F, 0xA2, 
                0x89, 0x06, 0x89, 0x5E, 0x04, 0x89, 0x4E, 0x08, 
                0x89, 0x56, 0x0C, 0x5E, 0x5B, 0xC3
            };
            return NativeFunctionLoader.LoadIntoMemory(data, Length);
        }
    }

    partial class StoreAsSpan
    {
        public static NativeFunctionHolder BuildCpuIdAsm_X86()
        {
            const int Length = 30;
            ReadOnlySpan<byte> data = [
                0x8B, 0x44, 0x24, 0x08, 0x8B, 0x4C, 0x24, 0x0C,
                0x53, 0x56, 0x8B, 0x74, 0x24, 0x0C, 0x0F, 0xA2,
                0x89, 0x06, 0x89, 0x5E, 0x04, 0x89, 0x4E, 0x08,
                0x89, 0x56, 0x0C, 0x5E, 0x5B, 0xC3
            ];
            return NativeFunctionLoader.LoadIntoMemory(data, Length);
        }
    }
#endif

#if (B64_ARCH || ANYCPU)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static NativeFunctionHolder BuildCpuIdAsm_X64()
        => SoftDependencyHelper.SystemMemoryExists ? StoreAsSpan.BuildCpuIdAsm_X64() : StoreAsArray.BuildCpuIdAsm_X64();

    partial class StoreAsArray
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NativeFunctionHolder BuildCpuIdAsm_X64()
            => IsUnix ? BuildCpuIdAsm_Unix_X64() : BuildCpuIdAsm_Windows_X64();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFunctionHolder BuildCpuIdAsm_Windows_X64()
        {
            const int Length = 36;
            byte[] data = new byte[Length] {
                0x48, 0x89, 0x5C, 0x24, 0x08, 0x49, 0x89, 0xC9, 
                0x89, 0xD0, 0x44, 0x89, 0xC1, 0x0F, 0xA2, 0x41,
                0x89, 0x01, 0x41, 0x89, 0x59, 0x04, 0x48, 0x8B, 
                0x5C, 0x24, 0x08, 0x41, 0x89, 0x49, 0x08, 0x41, 
                0x89, 0x51, 0x0C, 0xC3
            };
            return NativeFunctionLoader.LoadIntoMemory(data, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFunctionHolder BuildCpuIdAsm_Unix_X64()
        {
            const int Length = 24;
            byte[] data = new byte[Length] {
                0x89, 0xD1, 0x89, 0xF0, 0x48, 0x87, 0xDE, 0x0F,
                0xA2, 0x48, 0x87, 0xDE, 0x89, 0x07, 0x89, 0x77,
                0x04, 0x89, 0x4F, 0x08, 0x89, 0x57, 0x0C, 0xC3
            };
            return NativeFunctionLoader.LoadIntoMemory(data, Length);
        }
    }

    partial class StoreAsSpan
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NativeFunctionHolder BuildCpuIdAsm_X64()
            => IsUnix ? BuildCpuIdAsm_Unix_X64() : BuildCpuIdAsm_Windows_X64();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFunctionHolder BuildCpuIdAsm_Windows_X64()
        {
            const int Length = 36;
            ReadOnlySpan<byte> data = [
                0x48, 0x89, 0x5C, 0x24, 0x08, 0x49, 0x89, 0xC9,
                0x89, 0xD0, 0x44, 0x89, 0xC1, 0x0F, 0xA2, 0x41,
                0x89, 0x01, 0x41, 0x89, 0x59, 0x04, 0x48, 0x8B,
                0x5C, 0x24, 0x08, 0x41, 0x89, 0x49, 0x08, 0x41,
                0x89, 0x51, 0x0C, 0xC3
            ];
            return NativeFunctionLoader.LoadIntoMemory(data, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFunctionHolder BuildCpuIdAsm_Unix_X64()
        {
            const int Length = 24;
            ReadOnlySpan<byte> data = [
                0x89, 0xD1, 0x89, 0xF0, 0x48, 0x87, 0xDE, 0x0F,
                0xA2, 0x48, 0x87, 0xDE, 0x89, 0x07, 0x89, 0x77,
                0x04, 0x89, 0x4F, 0x08, 0x89, 0x57, 0x0C, 0xC3
            ];
            return NativeFunctionLoader.LoadIntoMemory(data, Length);
        }
    }
#endif
}
#endif
#endif