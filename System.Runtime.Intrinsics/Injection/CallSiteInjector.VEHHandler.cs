#pragma warning disable CA2211

using System;
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

            Address = SoftDependencyHelper.SystemMemoryExists ? 
                StoreAsSpan.BuildVEHHandlerAsm() : 
                StoreAsArray.BuildVEHHandlerAsm();
        }

        private static class StoreAsArray
        {
#if (X86_ARCH || ANYCPU) && !B32_ARCH && !B64_ARCH
            private static readonly bool _isX86 = PlatformHelper.IsX86;
            private static readonly bool _isX64 = PlatformHelper.IsX64;
#endif

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void* BuildVEHHandlerAsm()
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
            private static void* BuildVEHHandlerAsm_X86()
            {
                const int Length = 50;
                byte[] data = new byte[Length] // Original from VEHHook project in the solution
                {
                    0x8B, 0x84, 0x24, 0xFC, 0xFF, 0xFF, 0xFF, 
                    0x8B, 0x00, 
                    0x81, 0x38, 0x96, 0x00, 0x00, 0xC0,
                    0x75, 0x1C, 
                    0x8B, 0x40, 0x0C, 
                    0x8A, 0x08, 
                    0x80, 0xF9, 0xF4, 
                    0x74, 0x0C,
                    0x31, 0xC0, 
                    0x80, 0xF9, 0xE9, 
                    0x0F, 0x95, 0xC0, 
                    0x48, 
                    0xC2, 0x04, 0x00,
                    0x83, 0xC8, 0xFF, 
                    0xC2, 0x04, 0x00,
                    0x31, 0xC0, 
                    0xC2, 0x04, 0x00
                };
                return NativeFunctionLoader.LoadIntoMemory(data, Length);

            }
#endif

#if B64_ARCH || ANYCPU
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void* BuildVEHHandlerAsm_X64()
            {
                const int Length = 49;
                byte[] data = new byte[Length] // Original from VEHHook project in the solution
                {
                    0x48, 0x8B, 0x01, 
                    0x81, 0x38, 0x96, 0x00, 0x00, 0xC0, 
                    0x74, 0x05, 
                    0x31, 0xC0, 
                    0xC2, 0x00, 0x00, 
                    0x48, 0x8B, 0x40, 0x10, 
                    0x0F, 0xB6, 0x08,
                    0x80, 0xF9, 0xF4, 
                    0x74, 0x0D, 
                    0x31, 0xC0, 
                    0x80, 0xF9, 0xE9, 
                    0x0F, 0x95, 0xC0, 
                    0xFF, 0xC8, 
                    0xC2, 0x00, 0x00, 
                    0xB8, 0xFF, 0xFF, 0xFF, 0xFF, 
                    0xC2, 0x00, 0x00
                };
                return NativeFunctionLoader.LoadIntoMemory(data, Length);
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
            public static void* BuildVEHHandlerAsm()
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
            private static void* BuildVEHHandlerAsm_X86()
            {
                const int Length = 50;
                ReadOnlySpan<byte> data = // Original from VEHHook project in the solution
                [
                    0x8B, 0x84, 0x24, 0xFC, 0xFF, 0xFF, 0xFF,
                    0x8B, 0x00,
                    0x81, 0x38, 0x96, 0x00, 0x00, 0xC0,
                    0x75, 0x1C,
                    0x8B, 0x40, 0x0C,
                    0x8A, 0x08,
                    0x80, 0xF9, 0xF4,
                    0x74, 0x0C,
                    0x31, 0xC0,
                    0x80, 0xF9, 0xE9,
                    0x0F, 0x95, 0xC0,
                    0x48,
                    0xC2, 0x04, 0x00,
                    0x83, 0xC8, 0xFF,
                    0xC2, 0x04, 0x00,
                    0x31, 0xC0,
                    0xC2, 0x04, 0x00
                ];
                return NativeFunctionLoader.LoadIntoMemory(data, Length);

            }
#endif

#if B64_ARCH || ANYCPU
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void* BuildVEHHandlerAsm_X64()
            {
                const int Length = 49;
                ReadOnlySpan<byte> data = // Original from VEHHook project in the solution
                [
                    0x48, 0x8B, 0x01,
                    0x81, 0x38, 0x96, 0x00, 0x00, 0xC0,
                    0x74, 0x05,
                    0x31, 0xC0,
                    0xC2, 0x00, 0x00,
                    0x48, 0x8B, 0x40, 0x10,
                    0x0F, 0xB6, 0x08,
                    0x80, 0xF9, 0xF4,
                    0x74, 0x0D,
                    0x31, 0xC0,
                    0x80, 0xF9, 0xE9,
                    0x0F, 0x95, 0xC0,
                    0xFF, 0xC8,
                    0xC2, 0x00, 0x00,
                    0xB8, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xC2, 0x00, 0x00
                ];
                return NativeFunctionLoader.LoadIntoMemory(data, Length);
            }
#endif
#endif
        }
    }
}