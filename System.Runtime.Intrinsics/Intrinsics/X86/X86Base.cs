#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0
#pragma warning disable IDE0130

namespace System.Runtime.Intrinsics.X86;

/// <summary>Provides access to the x86 base hardware instructions via intrinsics.</summary>
public abstract partial class X86Base
{
    internal X86Base() { }

    /// <summary>Gets a value that indicates whether the APIs in this class are supported.</summary>
    /// <value><see langword="true" /> if the APIs are supported; otherwise, <see langword="false" />.</value>
    /// <remarks>A value of <see langword="false" /> indicates that the APIs will throw <see cref="PlatformNotSupportedException" />.</remarks>
    public static partial bool IsSupported { get; }

    /// <summary>
    ///   <para>void __cpuidex (int cpuInfo[4], int function_id, int subfunction_id);</para>
    ///   <para>  CPUID</para>
    /// </summary>
    public static partial (int Eax, int Ebx, int Ecx, int Edx) CpuId(int functionId, int subFunctionId);

    /// <summary>
    ///   <para>unsigned char _BitScanForward (unsigned __int32* index, unsigned __int32 a)</para>
    ///   <para>  BSF reg reg/m32</para>
    ///   <para>The above native signature does not directly correspond to the managed signature.</para>
    /// </summary>
    public static partial uint BitScanForward(uint value);

    /// <summary>
    ///   <para>unsigned char _BitScanReverse (unsigned __int32* index, unsigned __int32 a)</para>
    ///   <para>  BSR reg reg/m32</para>
    ///   <para>The above native signature does not directly correspond to the managed signature.</para>
    /// </summary>
    public static partial uint BitScanReverse(uint value);

    /// <summary>
    ///   <para>unsigned _udiv64(unsigned __int64 dividend, unsigned divisor, unsigned* remainder)</para>
    ///   <para>  DIV reg/m32</para>
    /// </summary>
    public static partial (int Quotient, int Remainder) DivRem(uint lower, int upper, int divisor);

    /// <summary>
    ///   <para>int _div64(__int64 dividend, int divisor, int* remainder)</para>
    ///   <para>  IDIV reg/m32</para>
    /// </summary>
    public static partial (uint Quotient, uint Remainder) DivRem(uint lower, uint upper, uint divisor);

    /// <summary>  IDIV reg/m</summary>
    public static partial (nint Quotient, nint Remainder) DivRem(nuint lower, nint upper, nint divisor);

    /// <summary>  DIV reg/m</summary>
    public static partial (nuint Quotient, nuint Remainder) DivRem(nuint lower, nuint upper, nuint divisor);

    /// <summary>
    /// void _mm_pause (void);
    ///   PAUSE
    /// </summary>
    public static partial void Pause();
}
#endif