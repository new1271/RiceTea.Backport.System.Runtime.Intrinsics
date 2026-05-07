using BenchmarkDotNet.Attributes;

using Fallbacks = RiceTea.Backport.Fallbacks.X86.Bmi1.X64;
using Intrinsics = System.Runtime.Intrinsics.X86.Bmi1.X64;

namespace RiceTea.Benchmark;

[RPlotExporter]
public class TzcntTest64
{
    private ulong _number;

    [GlobalSetup]
    public void Setup()
    {
        byte[] buffer = new byte[sizeof(ulong)];
        ThreadLocals.Random.NextBytes(buffer);
        _number = BitConverter.ToUInt64(buffer, 0);
        HardwareIntrinsic(); // Pre-injection
    }

    [Benchmark(Baseline = true)]
    public ulong SoftwareFallback() => Fallbacks.TrailingZeroCount(_number);

    [Benchmark(Baseline = false)]
    public ulong HardwareIntrinsic() => Intrinsics.TrailingZeroCount(_number);
}
