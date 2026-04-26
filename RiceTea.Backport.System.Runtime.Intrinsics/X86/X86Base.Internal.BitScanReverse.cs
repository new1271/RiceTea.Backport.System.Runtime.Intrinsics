using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;
using System.Runtime.Intrinsics.Internals;

using InlineIL;

namespace System.Runtime.Intrinsics.X86;

partial class X86Base
{
#if (X86_ARCH || ANYCPU)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBsrAsm()
    {
#if B64_ARCH
        InjectBsrAsm_X64();
#elif B32_ARCH
        InjectBsrAsm_X86();
#else
        if (Helpers.PlatformHelper.IsX64)
            InjectBsrAsm_X64();
        else
            InjectBsrAsm_X86();
#endif
    }

#if (B32_ARCH || ANYCPU)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBsrAsm_X86()
    {
        if (SoftDependencyHelper.SystemMemoryExists)
            StoreAsSpan.InjectBsrAsm_X86();
        else
            StoreAsArray.InjectBsrAsm_X86();
    }

    partial class StoreAsArray
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InjectBsrAsm_X86()
        {
            const int Length = 8;
            byte[] data = new byte[Length] {
                0x0F, 0xBD, 0x44, 0x24, 0x04,
                0xC2, 0x04, 0x00
            };
            IL.Emit.Ldtoken(MethodRef.Method(typeof(X86Base), nameof(BitScanReverse)));
            IL.Pop(out RuntimeMethodHandle method);
            AsmCodeHelper.InjectAsmCode(method, data, Length);
        }
    }

    partial class StoreAsSpan
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InjectBsrAsm_X86()
        {
            const int Length = 8;
            ReadOnlySpan<byte> data = [
                0x0F, 0xBD, 0x44, 0x24, 0x04,
                0xC2, 0x04, 0x00
            ];
            IL.Emit.Ldtoken(MethodRef.Method(typeof(X86Base), nameof(BitScanReverse)));
            IL.Pop(out RuntimeMethodHandle method);
            AsmCodeHelper.InjectAsmCode(method, data, Length);
        }
    }
#endif

#if (B64_ARCH || ANYCPU)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBsrAsm_X64()
    {
        if (SoftDependencyHelper.SystemMemoryExists)
            StoreAsSpan.InjectBsrAsm_X64();
        else
            StoreAsArray.InjectBsrAsm_X64();
    }

    partial class StoreAsArray
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InjectBsrAsm_X64()
        {
            const int Length = 8;
            byte[] data = new byte[Length] {
                0x0F, 0xBD, 0xC1,
                0xC3,
                0xCC, 0xCC, 0xCC, 0xCC 
            };
            IL.Emit.Ldtoken(MethodRef.Method(typeof(X86Base), nameof(BitScanReverse)));
            IL.Pop(out RuntimeMethodHandle method);
            AsmCodeHelper.InjectAsmCode(method, data, Length);
        }
    }

    partial class StoreAsSpan
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InjectBsrAsm_X64()
        {
            const int Length = 8;
            ReadOnlySpan<byte> data = [
                0x0F, 0xBD, 0xC1,
                0xC3,
                0xCC, 0xCC, 0xCC, 0xCC
            ];
            IL.Emit.Ldtoken(MethodRef.Method(typeof(X86Base), nameof(BitScanReverse)));
            IL.Pop(out RuntimeMethodHandle method);
            AsmCodeHelper.InjectAsmCode(method, data, Length);
        }
    }
#endif
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InjectBsrAsm() {};
#endif
}