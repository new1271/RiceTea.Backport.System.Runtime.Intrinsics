#if !NETSTANDARD2_1_OR_GREATER
namespace System.Runtime.Intrinsics.X86;

partial class Bmi1
{
    /// <summary>Provides access to the x86 BMI1 hardware instructions, that are only available to 64-bit processes, via intrinsics.</summary>
    public static partial class X64
    {
        /// <summary>Gets a value that indicates whether the APIs in this class are supported.</summary>
        /// <value><see langword="true" /> if the APIs are supported; otherwise, <see langword="false" />.</value>
        /// <remarks>A value of <see langword="false" /> indicates that the APIs will throw <see cref="PlatformNotSupportedException" />.</remarks>
        public static partial bool IsSupported { get; }

        /// <summary>
        ///   <para>__int64 _mm_tzcnt_64 (unsigned __int64 a)</para>
        ///   <para>  TZCNT r64, r/m64</para>
        ///   <para>This intrinsic is only available on 64-bit processes</para>
        /// </summary>
        public static partial ulong TrailingZeroCount(ulong value);
    }
}
#endif