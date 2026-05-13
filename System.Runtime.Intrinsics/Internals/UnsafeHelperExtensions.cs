using System;
using System.Runtime.InteropServices;

using InlineMethod;

namespace RiceTea.Backport.Internals
{
    internal static class UnsafeHelperExtensions
    {
        extension(UnsafeHelper)
        {
            [Inline(InlineBehavior.Remove)]
            public static ref readonly T GetReference<T>(scoped in ReadOnlySpan<T> span)
                => ref MemoryMarshal.GetReference(span);
        }
    }
}
