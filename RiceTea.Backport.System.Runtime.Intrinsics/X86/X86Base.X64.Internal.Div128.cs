#if !NETSTANDARD2_1_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Helpers;
using System.Runtime.Intrinsics.Internals;

using InlineIL;

namespace System.Runtime.Intrinsics.X86;

partial class X86Base
{
    partial class X64
    {
#if ((X86_ARCH && B64_ARCH) || ANYCPU)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectDiv128Asm()
        {
#if !B64_ARCH
            if (!Helpers.PlatformHelper.IsX64)
                return;
#endif
            InjectDiv128Asm_X64();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectDiv128Asm_X64()
        {
            if (SoftDependencyHelper.SystemMemoryExists)
                StoreAsSpan.InjectDiv128Asm_X64();
            else
                StoreAsArray.InjectDiv128Asm_X64();
        }

        partial class StoreAsArray
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void InjectDiv128Asm_X64()
            {
                const int Length = 20;
                byte[] data = new byte[Length] {
                    0x48, 0x89, 0xD0,
                    0x4C, 0x89, 0xC2,
                    0x49, 0xF7, 0xF9,
                    0x48, 0x89, 0x01,
                    0x48, 0x89, 0x51, 0x08,
                    0x48, 0x89, 0xC8,
                    0xC3
                };
                IL.Emit.Ldtoken(MethodRef.Method(typeof(X64), nameof(DivRem), typeof(ulong), typeof(long), typeof(long)));
                IL.Pop(out RuntimeMethodHandle method);
                AsmCodeHelper.InjectAsmCode(method, data, Length);
            }
        }

        partial class StoreAsSpan
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void InjectDiv128Asm_X64()
            {
                const int Length = 20;
                ReadOnlySpan<byte> data = [
                    0x48, 0x89, 0xD0,
                    0x4C, 0x89, 0xC2,
                    0x49, 0xF7, 0xF9,
                    0x48, 0x89, 0x01,
                    0x48, 0x89, 0x51, 0x08,
                    0x48, 0x89, 0xC8,
                    0xC3
                ];
                IL.Emit.Ldtoken(MethodRef.Method(typeof(X64), nameof(DivRem), typeof(ulong), typeof(long), typeof(long)));
                IL.Pop(out RuntimeMethodHandle method);
                AsmCodeHelper.InjectAsmCode(method, data, Length);
            }
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectDiv128Asm() {}
#endif
    }
}
#endif