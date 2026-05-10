namespace RiceTea.Backport.Injection;

unsafe partial class CallSiteInjector
{
    private static class SignalActionRegisterX64
    {
        static SignalActionRegisterX64()
        {
            const int SIGILL = 4;

            const uint SA_SIGINFO = 0x00000004;
            const uint SA_RESTART = 0x10000000;

            Native_Unix.SignalAction_LinuxX64 oldact;
            Native_Unix.SignalAction_LinuxX64 act = new Native_Unix.SignalAction_LinuxX64()
            {
                sa_handler = SignalHandler.Address,
                sa_flags = SA_SIGINFO | SA_RESTART,
                sa_mask = 0,
                sa_restorer = null
            };
            Native_Unix.rt_sigaction(SIGILL, &act, &oldact);
            void* oldHandler = oldact.sa_handler;
            if (oldHandler != null)
                *SignalHandler.LastHandlerSlot = (nuint)oldHandler == 1 ? null : oldHandler;
        }
    }
}