namespace RiceTea.Backport.Injection;

unsafe partial class CallSiteInjector
{
    private static class SignalActionRegisterX86
    {
        static SignalActionRegisterX86()
        {
            const int SIGILL = 4;

            const uint SA_SIGINFO = 0x00000004;
            const uint SA_RESTART = 0x10000000;
            const uint SA_RESTORER = 0x04000000;

            void* restorerTrampolineAddress;
            {
                /*
                 * mov eax, 173  ; SYS_rt_sigreturn (0xAD)
                 * int 0x80
                 * nop
                 */
                ulong restorerTrampoline = 0x90_80_CD_00_00_00_AD_B8;
                using NativeFunctionAccessScope scope = NativeFunctionLoader.LoadIntoMemory((byte*)&restorerTrampoline, sizeof(ulong)).Enter();
                restorerTrampolineAddress = scope.Address;
            }
            Native_Unix.SignalAction_LinuxX86 oldact;
            Native_Unix.SignalAction_LinuxX86 act = new Native_Unix.SignalAction_LinuxX86()
            {
                sa_handler = SignalHandler.Address,
                sa_flags = SA_SIGINFO | SA_RESTART | SA_RESTORER,
                sa_mask_lo = 0,
                sa_mask_hi = 0,
                sa_restorer = restorerTrampolineAddress
            };
            NativeFunctionLoader.FreezeMemoryPage(restorerTrampolineAddress);
            Native_Unix.rt_sigaction(SIGILL, &act, &oldact);
            void* oldHandler = oldact.sa_handler;
            if (oldHandler != null)
                *SignalHandler.LastHandlerSlot = (nuint)oldHandler == 1 ? null : oldHandler;
        }
    }
}