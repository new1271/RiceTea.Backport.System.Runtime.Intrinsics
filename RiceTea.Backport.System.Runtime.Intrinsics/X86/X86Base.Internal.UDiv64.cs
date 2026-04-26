using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;
using System.Runtime.Intrinsics.Internals;

namespace System.Runtime.Intrinsics.X86;

unsafe partial class X86Base
{
#if (X86_ARCH || ANYCPU)
    /*
     * extern "C"
     *
     * using uint32 = unsigned __int32;
     * using uint64 = unsigned __int64;
     * 
     * uint32 __cdecl udiv64_wrapper(uint64 dividend, uint32 divisor, uint32* remainder)
     * {
     *     return _udiv64(dividend, divisor, remainder);
     * }
     */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* BuildUDiv64Asm()
    {
#if B64_ARCH
        return BuildUDiv64Asm_X64();
#elif B32_ARCH
        return BuildUDiv64Asm_X86();
#else
        return Helpers.PlatformHelper.IsX64 ? BuildUDiv64Asm_X64() : BuildUDiv64Asm_X86();
#endif
    }

#if (B32_ARCH || ANYCPU)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* BuildUDiv64Asm_X86() 
        => SoftDependencyHelper.SystemMemoryExists ? StoreAsSpan.BuildUDiv64Asm_X86() : StoreAsArray.BuildUDiv64Asm_X86();

    partial class StoreAsArray
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void* BuildUDiv64Asm_X86()
        {
            const int Length = 21;
            byte[] data = new byte[Length] {
                0x8B, 0x44, 0x24, 0x04, 
                0x8B, 0x54, 0x24, 0x08, 
                0xF7, 0x74, 0x24, 0x0C, 
                0x8B, 0x4C, 0x24, 0x10, 
                0x89, 0x11, 0xC2, 0x00, 
                0x00
            };
            return AsmCodeHelper.PackAsmCodeIntoNativeMemory(data, Length);
        }
    }

    partial class StoreAsSpan
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void* BuildUDiv64Asm_X86()
        {
            const int Length = 21;
            ReadOnlySpan<byte> data = [
                0x8B, 0x44, 0x24, 0x04,
                0x8B, 0x54, 0x24, 0x08,
                0xF7, 0x74, 0x24, 0x0C,
                0x8B, 0x4C, 0x24, 0x10,
                0x89, 0x11, 0xC2, 0x00,
                0x00
            ];
            return AsmCodeHelper.PackAsmCodeIntoNativeMemory(data, Length);
        }
    }
#endif

#if (B64_ARCH || ANYCPU)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* BuildUDiv64Asm_X64() 
        => SoftDependencyHelper.SystemMemoryExists ? StoreAsSpan.BuildUDiv64Asm_X64() : StoreAsArray.BuildUDiv64Asm_X64();

    partial class StoreAsArray
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void* BuildUDiv64Asm_X64()
        {
            const int Length = 22;
            byte[] data = new byte[Length] {
                0x41, 0x89, 0xD1, 0x48,
                0x89, 0xC8, 0x48, 0x89,
                0xCA, 0x48, 0xC1, 0xEA,
                0x20, 0x41, 0xF7, 0xF1,
                0x41, 0x89, 0x10, 0xC2,
                0x00, 0x00
            };
            return AsmCodeHelper.PackAsmCodeIntoNativeMemory(data, Length);
        }
    }

    partial class StoreAsSpan
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void* BuildUDiv64Asm_X64()
        {
            const int Length = 22;
            ReadOnlySpan<byte> data = [
                0x41, 0x89, 0xD1, 0x48,
                0x89, 0xC8, 0x48, 0x89,
                0xCA, 0x48, 0xC1, 0xEA,
                0x20, 0x41, 0xF7, 0xF1,
                0x41, 0x89, 0x10, 0xC2,
                0x00, 0x00
            ];
            return AsmCodeHelper.PackAsmCodeIntoNativeMemory(data, Length);
        }
    }

#endif
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* BuildUDiv64Asm() => null;
#endif
}