#if NETSTANDARD2_0_OR_GREATER
#pragma warning disable IDE0130

namespace System.Runtime.Intrinsics.X86;

/// <summary>
/// This class provides access to Intel BMI1 hardware instructions via intrinsics
/// </summary>
public abstract partial class Bmi1 : X86Base
{
    internal Bmi1() { }

    /// <summary>Gets a value that indicates whether the APIs in this class are supported.</summary>
    /// <value><see langword="true" /> if the APIs are supported; otherwise, <see langword="false" />.</value>
    /// <remarks>A value of <see langword="false" /> indicates that the APIs will throw <see cref="PlatformNotSupportedException" />.</remarks>
    public static new partial bool IsSupported { get; }

    /// <summary>
    /// int _mm_tzcnt_32 (unsigned int a)
    ///   TZCNT r32, r/m32
    /// </summary>
    public static partial uint TrailingZeroCount(uint value);
}
#endif