namespace System.Runtime.Intrinsics.X86;

/// <summary>Provides access to X86 POPCNT hardware instructions via intrinsics.</summary>
public static partial class Popcnt
{
    /// <summary>Gets a value that indicates whether the APIs in this class are supported.</summary>
    /// <value><see langword="true" /> if the APIs are supported; otherwise, <see langword="false" />.</value>
    /// <remarks>A value of <see langword="false" /> indicates that the APIs will throw <see cref="PlatformNotSupportedException" />.</remarks>
    public static partial bool IsSupported { get; }

    /// <summary>
    ///   <para>int _mm_popcnt_u32 (unsigned int a)</para>
    ///   <para>  POPCNT r32, r/m32</para>
    /// </summary>
    public static partial uint PopCount(uint value);
}
