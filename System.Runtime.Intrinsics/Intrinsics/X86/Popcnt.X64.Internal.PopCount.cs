#if NETSTANDARD2_0_OR_GREATER
#if (X86_ARCH && B64_ARCH) || ANYCPU

using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

using SpanDissolve;

namespace System.Runtime.Intrinsics.X86;

partial class Popcnt
{
    unsafe partial class X64
    {
        private const int PopcntLength_Windows = 5;
        private const int PopcntLength_Unix = 5;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectPopcntAsm(ref void* destination, ref uint length)
        {
            if (_isUnix)
                InjectPopcntAsm_Unix(ref destination, ref length);
            else
                InjectPopcntAsm_Windows(ref destination, ref length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectPopcntAsm_Windows(ref void* destination, ref uint length)
        {
            const int Length = PopcntLength_Windows;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
            UnsafeHelper.CopyBlockUnaligned(destination, in Store.PopcntData_Windows, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InjectPopcntAsm_Unix(ref void* destination, ref uint length)
        {
            const int Length = PopcntLength_Unix;
            if (length < Length)
                ThrowUtils.ThrowAccessViolation();
            destination = (byte*)destination + length - Length;
            length = Length;
            UnsafeHelper.CopyBlockUnaligned(destination, in Store.PopcntData_Unix, Length);
        }

        partial class Store
        {
            public static ref readonly byte PopcntData_Windows => ref SpanDissolver.Dissolve(new byte[PopcntLength_Windows]
            {
                0xF3, 0x48, 0x0F, 0xB8, 0xC1 // popcnt rax rcx
            });
            public static ref readonly byte PopcntData_Unix => ref SpanDissolver.Dissolve(new byte[PopcntLength_Unix]
            {
                0xF3, 0x48, 0x0F, 0xB8, 0xC7 // popcnt rax, rdi
            });
        }
    }
}
#endif
#endif