#if !NETSTANDARD2_1_OR_GREATER
namespace System.Runtime.Intrinsics.X86;

/// <summary>Provides access to X86 LZCNT hardware instructions via intrinsics.</summary>
public static partial class Lzcnt
{
    /// <summary>Gets a value that indicates whether the APIs in this class are supported.</summary>
    /// <value><see langword="true" /> if the APIs are supported; otherwise, <see langword="false" />.</value>
    /// <remarks>A value of <see langword="false" /> indicates that the APIs will throw <see cref="PlatformNotSupportedException" />.</remarks>
    public static partial bool IsSupported { get; }

    /// <summary>
    ///   <para>unsigned int _lzcnt_u32 (unsigned int a)</para>
    ///   <para>  LZCNT r32, r/m32</para>
    /// </summary>
    public static partial uint LeadingZeroCount(uint value);
}
#endif