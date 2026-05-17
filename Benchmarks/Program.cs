using System.Runtime.Intrinsics.X86;

using BenchmarkDotNet.Running;

using RiceTea.Benchmark;

bool bmi1Supported = Bmi1.IsSupported;
bool bmi1Supported64 = Bmi1.X64.IsSupported;
bool lzcntSupported = Lzcnt.IsSupported;
bool lzcntSupported64 = Lzcnt.X64.IsSupported;
bool popcntSupported = Popcnt.IsSupported;
bool popcntSupported64 = Popcnt.X64.IsSupported;

Console.WriteLine("Bmi1.IsSupported = " + bmi1Supported.ToString());
Console.WriteLine("Bmi1.X64.IsSupported = " + bmi1Supported64.ToString());
Console.WriteLine("Lzcnt.IsSupported = " + lzcntSupported.ToString());
Console.WriteLine("Lzcnt.X64.IsSupported = " + lzcntSupported64.ToString());
Console.WriteLine("Popcnt.IsSupported = " + popcntSupported.ToString());
Console.WriteLine("Popcnt.X64.IsSupported = " + popcntSupported64.ToString());

if (bmi1Supported)
    BenchmarkRunner.Run<TzcntTest32>();

if (bmi1Supported64)
    BenchmarkRunner.Run<TzcntTest64>();

if (lzcntSupported)
    BenchmarkRunner.Run<LzcntTest32>();

if (lzcntSupported64)
    BenchmarkRunner.Run<LzcntTest64>();

if (popcntSupported)
    BenchmarkRunner.Run<PopcntTest32>();

if (popcntSupported64)
    BenchmarkRunner.Run<PopcntTest64>();