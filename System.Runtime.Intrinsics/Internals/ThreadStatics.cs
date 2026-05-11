using System;

namespace RiceTea.Backport.Internals
{
    internal static unsafe class ThreadStatics
    {
        [ThreadStatic]
        public static void* StartAddress;
    }
}
