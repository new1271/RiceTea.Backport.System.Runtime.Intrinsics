
using System;
using System.Net;
using System.Runtime.CompilerServices;

using RiceTea.Backport.Internals;

namespace RiceTea.Backport.Injection;

partial class CallSiteInjector
{
    private static unsafe class VEHHandler
    {
        public static readonly void* Address;

        static VEHHandler()
        {
#if !X86_ARCH
            if (!PlatformHelper.IsX86)
                ThrowUtils.ThrowPlatformNotSupported();
#endif

            NativeFunctionHolder holder = SoftDependencyHelper.SystemMemoryExists ?
                StoreAsSpan.BuildVEHHandlerAsm() :
                StoreAsArray.BuildVEHHandlerAsm();
            using NativeFunctionAccessScope scope = holder.Enter();
            void* address = scope.Address;
            NativeFunctionLoader.FreezeMemoryPage(address);
            Address = address;
        }

        private static class StoreAsArray
        {
#if (X86_ARCH || ANYCPU) && !B32_ARCH && !B64_ARCH
            private static readonly bool _isX86 = PlatformHelper.IsX86;
            private static readonly bool _isX64 = PlatformHelper.IsX64;
#endif

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static NativeFunctionHolder BuildVEHHandlerAsm()
            {
#if X86_ARCH || ANYCPU
#if B64_ARCH
                return BuildVEHHandlerAsm_X64();
#elif B32_ARCH
                return BuildVEHHandlerAsm_X86();
#else
                if (_isX64)
                    return BuildVEHHandlerAsm_X64();
                if (_isX86)
                    return BuildVEHHandlerAsm_X86();
#endif
#endif
                throw new PlatformNotSupportedException();
            }

#if X86_ARCH || ANYCPU
#if B32_ARCH || ANYCPU
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static NativeFunctionHolder BuildVEHHandlerAsm_X86()
            {
                const int Length = 86;
                byte[] data = new byte[Length] // Original from VEHHook project in the solution
                {
                    0x8B, 0x44, 0x24, 0x04, 0x8B, 0x00, 0x81, 0x38,
                    0x96, 0x00, 0x00, 0xC0, 0x74, 0x05, 0x33, 0xC0,
                    0xC2, 0x04, 0x00, 0x8B, 0x40, 0x0C, 0x89, 0x44,
                    0x24, 0x04, 0x8B, 0x44, 0x24, 0x04, 0x80, 0x38,
                    0xF4, 0x75, 0x2B, 0xB9, 0x01, 0x00, 0x00, 0x00,
                    0x33, 0xC0, 0x85, 0xC9, 0x74, 0x09, 0x66, 0x90,
                    0xF3, 0x90, 0x40, 0x3B, 0xC1, 0x72, 0xF9, 0x83,
                    0xF9, 0x40, 0x73, 0x04, 0x03, 0xC9, 0xEB, 0x05,
                    0xB9, 0x40, 0x00, 0x00, 0x00, 0x8B, 0x44, 0x24,
                    0x04, 0x80, 0x38, 0xF4, 0x74, 0xDA, 0xB8, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xC2, 0x04, 0x00
                };
                return NativeFunctionLoader.LoadIntoMemoryUnsafe(data, Length);

            }
#endif

#if B64_ARCH || ANYCPU
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static NativeFunctionHolder BuildVEHHandlerAsm_X64()
            {
                const int Length = 90;
                byte[] data = new byte[Length] // Original from VEHHook project in the solution
                {
                    0x48, 0x8B, 0x01, 0x81, 0x38, 0x96, 0x00, 0x00,
                    0xC0, 0x74, 0x03, 0x33, 0xC0, 0xC3, 0x48, 0x8B,
                    0x40, 0x10, 0x48, 0x89, 0x44, 0x24, 0x08, 0x48,
                    0x8B, 0x44, 0x24, 0x08, 0x80, 0x38, 0xF4, 0x75,
                    0x33, 0xB9, 0x01, 0x00, 0x00, 0x00, 0x33, 0xC0,
                    0x48, 0x85, 0xC9, 0x74, 0x0D, 0x0F, 0x1F, 0x00,
                    0xF3, 0x90, 0x48, 0xFF, 0xC0, 0x48, 0x3B, 0xC1,
                    0x72, 0xF6, 0x48, 0x83, 0xF9, 0x40, 0x73, 0x05,
                    0x48, 0x03, 0xC9, 0xEB, 0x05, 0xB9, 0x40, 0x00,
                    0x00, 0x00, 0x48, 0x8B, 0x44, 0x24, 0x08, 0x80,
                    0x38, 0xF4, 0x74, 0xD2, 0xB8, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xC3
                };
                return NativeFunctionLoader.LoadIntoMemoryUnsafe(data, Length);
            }
#endif
#endif
        }

        private static class StoreAsSpan
        {
#if (X86_ARCH || ANYCPU) && !B32_ARCH && !B64_ARCH
            private static readonly bool _isX86 = PlatformHelper.IsX86;
            private static readonly bool _isX64 = PlatformHelper.IsX64;
#endif

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static NativeFunctionHolder BuildVEHHandlerAsm()
            {
#if X86_ARCH || ANYCPU
#if B64_ARCH
                return BuildVEHHandlerAsm_X64();
#elif B32_ARCH
                return BuildVEHHandlerAsm_X86();
#else
                if (_isX64)
                    return BuildVEHHandlerAsm_X64();
                if (_isX86)
                    return BuildVEHHandlerAsm_X86();
#endif
#endif
                throw new PlatformNotSupportedException();
            }

#if X86_ARCH || ANYCPU
#if B32_ARCH || ANYCPU
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static NativeFunctionHolder BuildVEHHandlerAsm_X86()
            {
                const int Length = 86;
                ReadOnlySpan<byte> data = // Original from VEHHook project in the solution
                [
                    0x8B, 0x44, 0x24, 0x04, 0x8B, 0x00, 0x81, 0x38,
                    0x96, 0x00, 0x00, 0xC0, 0x74, 0x05, 0x33, 0xC0,
                    0xC2, 0x04, 0x00, 0x8B, 0x40, 0x0C, 0x89, 0x44,
                    0x24, 0x04, 0x8B, 0x44, 0x24, 0x04, 0x80, 0x38,
                    0xF4, 0x75, 0x2B, 0xB9, 0x01, 0x00, 0x00, 0x00,
                    0x33, 0xC0, 0x85, 0xC9, 0x74, 0x09, 0x66, 0x90,
                    0xF3, 0x90, 0x40, 0x3B, 0xC1, 0x72, 0xF9, 0x83,
                    0xF9, 0x40, 0x73, 0x04, 0x03, 0xC9, 0xEB, 0x05,
                    0xB9, 0x40, 0x00, 0x00, 0x00, 0x8B, 0x44, 0x24,
                    0x04, 0x80, 0x38, 0xF4, 0x74, 0xDA, 0xB8, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xC2, 0x04, 0x00
                ];
                return NativeFunctionLoader.LoadIntoMemoryUnsafe(data, Length);

            }
#endif

#if B64_ARCH || ANYCPU
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static NativeFunctionHolder BuildVEHHandlerAsm_X64()
            {
                const int Length = 90;
                ReadOnlySpan<byte> data = // Original from VEHHook project in the solution
                [
                    0x48, 0x8B, 0x01, 0x81, 0x38, 0x96, 0x00, 0x00,
                    0xC0, 0x74, 0x03, 0x33, 0xC0, 0xC3, 0x48, 0x8B,
                    0x40, 0x10, 0x48, 0x89, 0x44, 0x24, 0x08, 0x48,
                    0x8B, 0x44, 0x24, 0x08, 0x80, 0x38, 0xF4, 0x75,
                    0x33, 0xB9, 0x01, 0x00, 0x00, 0x00, 0x33, 0xC0,
                    0x48, 0x85, 0xC9, 0x74, 0x0D, 0x0F, 0x1F, 0x00,
                    0xF3, 0x90, 0x48, 0xFF, 0xC0, 0x48, 0x3B, 0xC1,
                    0x72, 0xF6, 0x48, 0x83, 0xF9, 0x40, 0x73, 0x05,
                    0x48, 0x03, 0xC9, 0xEB, 0x05, 0xB9, 0x40, 0x00,
                    0x00, 0x00, 0x48, 0x8B, 0x44, 0x24, 0x08, 0x80,
                    0x38, 0xF4, 0x74, 0xD2, 0xB8, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xC3
                ];
                return NativeFunctionLoader.LoadIntoMemoryUnsafe(data, Length);
            }
#endif
#endif
        }
    }
}