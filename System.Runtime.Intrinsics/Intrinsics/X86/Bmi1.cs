#if NETSTANDARD2_0_OR_GREATER

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
    ///   <para>unsigned int _andn_u32 (unsigned int a, unsigned int b)</para>
    ///   <para>  ANDN r32a, r32b, r/m32</para>
    /// </summary>
    public static partial uint AndNot(uint left, uint right);

    /// <summary>
    ///   <para>unsigned int _bextr_u32 (unsigned int a, unsigned int start, unsigned int len)</para>
    ///   <para>  BEXTR r32a, r/m32, r32b</para>
    /// </summary>
    public static partial uint BitFieldExtract(uint value, byte start, byte length);

    /// <summary>
    ///   <para>unsigned int _bextr2_u32 (unsigned int a, unsigned int control)</para>
    ///   <para>  BEXTR r32a, r/m32, r32b</para>
    /// </summary>
    public static partial uint BitFieldExtract(uint value, ushort control);

    /// <summary>
    ///   <para>unsigned int _blsi_u32 (unsigned int a)</para>
    ///   <para>  BLSI r32, r/m32</para>
    /// </summary>
    public static partial uint ExtractLowestSetBit(uint value);

    /// <summary>
    ///   <para>unsigned int _blsmsk_u32 (unsigned int a)</para>
    ///   <para>  BLSMSK r32, r/m32</para>
    /// </summary>
    public static partial uint GetMaskUpToLowestSetBit(uint value);

    /// <summary>
    ///   <para>unsigned int _blsr_u32 (unsigned int a)</para>
    ///   <para>  BLSR r32, r/m32</para>
    /// </summary>
    public static partial uint ResetLowestSetBit(uint value);

    /// <summary>
    /// int _mm_tzcnt_32 (unsigned int a)
    ///   TZCNT r32, r/m32
    /// </summary>
    public static partial uint TrailingZeroCount(uint value);
}
#endif