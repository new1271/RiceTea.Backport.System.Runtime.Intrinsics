using BenchmarkDotNet.Attributes;

using Fallbacks = RiceTea.Backport.Fallbacks.X86.Lzcnt.X64;
using Intrinsics = System.Runtime.Intrinsics.X86.Lzcnt.X64;

namespace RiceTea.Benchmark;

[RPlotExporter]
public class LzcntTest64
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
    public ulong SoftwareFallback() => Fallbacks.LeadingZeroCount(_number);

    [Benchmark(Baseline = false)]
    public ulong HardwareIntrinsic() => Intrinsics.LeadingZeroCount(_number);
}
