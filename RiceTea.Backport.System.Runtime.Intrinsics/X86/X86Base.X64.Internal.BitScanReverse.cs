#if !NETSTANDARD2_1_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;
using System.Runtime.Intrinsics.Internals;

using InlineIL;

namespace System.Runtime.Intrinsics.X86;

partial class X86Base
{
    unsafe partial class X64
    {
#if ((X86_ARCH && B64_ARCH) || ANYCPU)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsrAsm(ref void* destination, ref uint length)
        {
#if !B64_ARCH
            if (!Helpers.PlatformHelper.IsX64)
                throw new PlatformNotSupportedException();
#endif
            if (SoftDependencyHelper.SystemMemoryExists)
                StoreAsSpan.InjectBsrAsm(ref destination, ref length);
            else 
                StoreAsArray.InjectBsrAsm(ref destination, ref length);
        }

        partial class StoreAsArray
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void InjectBsrAsm(ref void* destination, ref uint length)
            {
                const int Length = 4;
                byte[] data = new byte[Length] {
                    0x48, 0x0F, 0xBD, 0xC1
                };
                IL.Emit.Ldtoken(MethodRef.Method(typeof(X64), nameof(BitScanReverse)));
                IL.Pop(out RuntimeMethodHandle method);
                AsmCodeHelper.InjectAsmCode(method, data, Length);
            }
        }

        partial class StoreAsSpan
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void InjectBsrAsm(ref void* destination, ref uint length)
            {
                const int Length = 4;
                ReadOnlySpan<byte> data = [
                    0x48, 0x0F, 0xBD, 0xC1
                ];
                IL.Emit.Ldtoken(MethodRef.Method(typeof(X64), nameof(BitScanReverse)));
                IL.Pop(out RuntimeMethodHandle method);
                AsmCodeHelper.InjectAsmCode(method, data, Length);
            }
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectBsrAsm(ref void* destination, ref uint length) => throw new PlatformNotSupportedException();
#endif
    }
}
#endif