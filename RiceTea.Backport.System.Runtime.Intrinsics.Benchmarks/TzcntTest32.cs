using BenchmarkDotNet.Attributes;

using Fallbacks = RiceTea.Backport.Fallbacks.X86.Bmi1;
using Intrinsics = System.Runtime.Intrinsics.X86.Bmi1;

namespace RiceTea.Benchmark;

[RPlotExporter]
public class TzcntTest32
{
    private uint _number;

    [GlobalSetup]
    public void Setup()
    {
        byte[] buffer = new byte[sizeof(uint)];
        ThreadLocals.Random.NextBytes(buffer);
        _number = BitConverter.ToUInt32(buffer, 0);
        HardwareIntrinsic(); // Pre-injection
    }

    [Benchmark(Baseline = true)]
    public uint SoftwareFallback() => Fallbacks.TrailingZeroCount(_number);

    [Benchmark(Baseline = false)]
    public uint HardwareIntrinsic() => Intrinsics.TrailingZeroCount(_number);
}
