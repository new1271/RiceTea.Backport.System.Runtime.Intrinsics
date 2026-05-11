using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;

using Fallbacks = RiceTea.Backport.Fallbacks.X86.Popcnt.X64;
using Intrinsics = System.Runtime.Intrinsics.X86.Popcnt.X64;

namespace RiceTea.Benchmark;

[RPlotExporter]
public class PopcntTest64
{
    private ulong _number;

    [GlobalSetup]
    public void Setup()
    {
        byte[] buffer = new byte[sizeof(ulong)];
        ThreadLocals.Random.NextBytes(buffer);
        _number = BitConverter.ToUInt64(buffer, 0);
    }

    [Benchmark(Baseline = true)]
    public ulong SoftwareFallback() => Fallbacks.PopCount(_number);

    [Benchmark(Baseline = false)]
    public ulong HardwareIntrinsic() => Intrinsics.PopCount(_number);
}
