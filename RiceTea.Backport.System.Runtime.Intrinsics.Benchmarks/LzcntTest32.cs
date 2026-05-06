using BenchmarkDotNet.Attributes;

using Fallbacks = RiceTea.Backport.Fallbacks.X86.Lzcnt;
using Intrinsics = System.Runtime.Intrinsics.X86.Lzcnt;

namespace RiceTea.Benchmark;

[RPlotExporter]
public class LzcntTest32
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
    public uint SoftwareFallback() => Fallbacks.LeadingZeroCount(_number);

    [Benchmark(Baseline = false)]
    public uint HardwareIntrinsic() => Intrinsics.LeadingZeroCount(_number);
}
