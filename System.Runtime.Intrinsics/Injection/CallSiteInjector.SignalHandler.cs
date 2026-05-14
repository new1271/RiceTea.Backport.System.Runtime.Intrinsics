using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using RiceTea.Backport.Internals;

using SpanDissolve;

namespace RiceTea.Backport.Injection;

partial class CallSiteInjector
{
    private static unsafe class SignalHandler
    {
        public static readonly void* Address;
        public static readonly void** LastHandlerSlot;

        static SignalHandler()
        {
#if !X86_ARCH
            if (!PlatformHelper.IsX86)
                ThrowUtils.ThrowPlatformNotSupported();
#endif
            void** lastHandlerSlot = (void**)Marshal.AllocHGlobal(sizeof(void*));
            *lastHandlerSlot = MemoryHelper.GetFuncAddress_OnlyUnix("abort");
            (NativeFunctionHolder holder, nuint slotOffset) = Store.BuildSignalHandlerAsm();
            using NativeFunctionAccessScope scope = holder.Enter();
            void* address = scope.Address;
            *(void***)(((byte*)address) + slotOffset) = lastHandlerSlot;

            NativeFunctionLoader.FreezeMemoryPage(address);
            Address = address;
            LastHandlerSlot = lastHandlerSlot;
        }

        private static class Store
        {
#if (X86_ARCH || ANYCPU) && !B32_ARCH && !B64_ARCH
            private static readonly bool _isX86 = PlatformHelper.IsX86;
            private static readonly bool _isX64 = PlatformHelper.IsX64;
#endif

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static (NativeFunctionHolder Holder, nuint SlotOffset) BuildSignalHandlerAsm()
            {
#if X86_ARCH || ANYCPU
#if B64_ARCH
                return BuildSignalHandlerAsm_X64();
#elif B32_ARCH
                return BuildSignalHandlerAsm_X86();
#else
                if (_isX64)
                    return BuildSignalHandlerAsm_X64();
                if (_isX86)
                    return BuildSignalHandlerAsm_X86();
#endif
#endif
                throw new PlatformNotSupportedException();
            }

#if X86_ARCH || ANYCPU
#if B32_ARCH || ANYCPU
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static (NativeFunctionHolder Holder, nuint SlotOffset) BuildSignalHandlerAsm_X86()
            {
                const int Length = 113;
                return (Holder: NativeFunctionLoader.LoadIntoMemoryUnsafe(
                    in SpanDissolver.Dissolve(new byte[Length] // Original from SingalHook project in the solution
                {
                    0x53, 0x56, 0x50, 0xE8, 0x00, 0x00, 0x00, 0x00,
                    0x5B, 0x81, 0xC3, 0x03, 0x00, 0x00, 0x00, 0x8B,
                    0x44, 0x24, 0x14, 0x8B, 0x4C, 0x24, 0x10, 0x83,
                    0xF9, 0x04, 0x75, 0x35, 0x83, 0x78, 0x08, 0x05,
                    0x75, 0x2F, 0x8B, 0x40, 0x0C, 0x8A, 0x08, 0x80,
                    0xF9, 0xF4, 0x75, 0x3F, 0x31, 0xD2, 0x42, 0xB9,
                    0x40, 0x00, 0x00, 0x00, 0x89, 0xD6, 0x85, 0xD2,
                    0x74, 0x05, 0xF3, 0x90, 0x4E, 0x75, 0xFB, 0x83,
                    0xFA, 0x40, 0x8D, 0x14, 0x12, 0x0F, 0x43, 0xD1,
                    0x8A, 0x18, 0x80, 0xFB, 0xF4, 0x74, 0xE5, 0xEB,
                    0x1A, 
                    // Offset 81
                    0x8B, 0x15, 
                    // Offset 83
                    0xEF, 0xBE, 0xAD, 0xDE, 
                    // Offset 87
                    0x85, 0xD2, 0x74, 0x10, 0x83, 0xEC, 0x04, 0xFF,
                    0x74, 0x24, 0x1C, 0x50, 0x51, 0xFF, 0xD2, 0x83,
                    0xC4, 0x14, 0xEB, 0x03, 0x83, 0xC4, 0x04, 0x5E,
                    0x5B, 0xC3
                }), Length), SlotOffset: 83);
            }
#endif

#if B64_ARCH || ANYCPU
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static (NativeFunctionHolder Holder, nuint SlotOffset) BuildSignalHandlerAsm_X64()
            {
                const int Length = 91;
                return (Holder: NativeFunctionLoader.LoadIntoMemoryUnsafe(
                    in SpanDissolver.Dissolve(new byte[Length] // Original from SingalHook project in the solution
                {
                    0x83, 0xFF, 0x04, 0x75, 0x41, 0x83, 0x7E, 0x08,
                    0x05, 0x75, 0x3B, 0x48, 0x8B, 0x46, 0x10, 0x8A,
                    0x08, 0x80, 0xF9, 0xF4, 0x75, 0x44, 0xBA, 0x01,
                    0x00, 0x00, 0x00, 0xB9, 0x40, 0x00, 0x00, 0x00,
                    0x48, 0x89, 0xD6, 0x48, 0x85, 0xD2, 0x74, 0x07,
                    0xF3, 0x90, 0x48, 0xFF, 0xCE, 0x75, 0xF9, 0x48,
                    0x83, 0xFA, 0x40, 0x48, 0x8D, 0x14, 0x12, 0x48,
                    0x0F, 0x43, 0xD1, 0x40, 0x8A, 0x30, 0x40, 0x80,
                    0xFE, 0xF4, 0x74, 0xDC, 0xEB, 0x14, 
                    // Offset 70
                    0x48, 0xB8,
                    // Offset 72
                    0xEF, 0xBE, 0xAD, 0xDE, 0xEF, 0xBE, 0xAD, 0xDE,
                    // Offset 80
                    0x48, 0x8B, 0x00, 0x48, 0x85, 0xC0, 0x74, 0x02,
                    0xFF, 0xE0, 0xC3
                }), Length), SlotOffset: 72);
            }
#endif
#endif
    }
    }
}