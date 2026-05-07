using BenchmarkDotNet.Attributes;

using Fallbacks = RiceTea.Backport.Fallbacks.X86.Popcnt;
using Intrinsics = System.Runtime.Intrinsics.X86.Popcnt;

namespace RiceTea.Benchmark;

[RPlotExporter]
public class PopcntTest32
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
    public uint SoftwareFallback() => Fallbacks.PopCount(_number);

    [Benchmark(Baseline = false)]
    public uint HardwareIntrinsic() => Intrinsics.PopCount(_number);
}
