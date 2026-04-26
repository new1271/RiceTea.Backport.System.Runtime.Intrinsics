#if !NETSTANDARD2_1_OR_GREATER
namespace System.Runtime.Intrinsics.X86;

partial class X86Base
{
    /// <summary>Provides access to the x86 base hardware instructions, that are only available to 64-bit processes, via intrinsics.</summary>
    public static partial class X64
    {
        /// <summary>Gets a value that indicates whether the APIs in this class are supported.</summary>
        /// <value><see langword="true" /> if the APIs are supported; otherwise, <see langword="false" />.</value>
        /// <remarks>A value of <see langword="false" /> indicates that the APIs will throw <see cref="PlatformNotSupportedException" />.</remarks>
        public static partial bool IsSupported { get; }

        /// <summary>
        ///   <para>unsigned char _BitScanForward64 (unsigned __int32* index, unsigned __int64 a)</para>
        ///   <para>  BSF reg reg/m64</para>
        ///   <para>The above native signature does not directly correspond to the managed signature.</para>
        /// </summary>
        public static partial uint BitScanForward(ulong value);

        /// <summary>
        ///   <para>unsigned char _BitScanReverse64 (unsigned __int32* index, unsigned __int64 a)</para>
        ///   <para>  BSR reg reg/m64</para>
        ///   <para>The above native signature does not directly correspond to the managed signature.</para>
        /// </summary>
        public static partial uint BitScanReverse(ulong value);

        /// <summary>
        ///   <para>unsigned __int64 _udiv128(unsigned __int64 highdividend, unsigned __int64 lowdividend, unsigned __int64 divisor, unsigned __int64* remainder)</para>
        ///   <para>  DIV reg/m64</para>
        /// </summary>
        public static partial (long Quotient, long Remainder) DivRem(ulong lower, long upper, long divisor);

        /// <summary>
        ///   <para>__int64 _div128(__int64 highdividend, __int64 lowdividend, __int64 divisor, __int64* remainder)</para>
        ///   <para>  DIV reg/m64</para>
        /// </summary>
        public static partial (ulong Quotient, ulong Remainder) DivRem(ulong lower, ulong upper, ulong divisor);
    }
}
#endif