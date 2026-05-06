# RiceTea.Backport.System.Runtime.Intrinsics

Provides APIs for accessing processor specific instructions. (backports to .NET Standard 2.0 by Rice Tea)<br/>
<br/>
Only implements those APIs that I may use<br/>
<br/>
## Provides those types and methods:
- System.Runtime.Intrinsics.X86.Bmi1
  - TrailingZeroCount
- System.Runtime.Intrinsics.X86.Bmi1.X64
  - TrailingZeroCount
- System.Runtime.Intrinsics.X86.Lzcnt
  - LeadingZeroCount
- System.Runtime.Intrinsics.X86.Lzcnt.X64
  - LeadingZeroCount
- System.Runtime.Intrinsics.X86.Popcnt
  - PopCount
- System.Runtime.Intrinsics.X86.Popcnt.X64
  - PopCount
- System.Runtime.Intrinsics.X86.X86Base
  - CpuId
  - BitScanForward
  - BitScanReverse
  - DivRem
  - Pause
- System.Runtime.Intrinsics.X86.X86Base.X64
  - CpuId
  - BitScanForward
  - BitScanReverse
  - DivRem

## Performances
### Environment: 
```
CPU: Intel Core(TM) i7-10700F @ 2.90GHz
.NET version: .NET Framework 4.8.1 x64
BDN version: 0.15.7
```
### Bmi1.TrailingZeroCount:
![TZCNT Compitition](resources/RiceTea.Benchmark.TzcntTest32-barplot.png)
### Bmi1.X64.TrailingZeroCount:
![TZCNT Compitition](resources/RiceTea.Benchmark.TzcntTest64-barplot.png)
### Lzcnt.LeadingZeroCount:
![LZCNT Compitition](resources/RiceTea.Benchmark.LzcntTest32-barplot.png)
### Lzcnt.X64.LeadingZeroCount:
![LZCNT Compitition](resources/RiceTea.Benchmark.LzcntTest64-barplot.png)
### Popcnt.PopCount:
![POPCNT Compitition](resources/RiceTea.Benchmark.PopcntTest32-barplot.png)
### Popcnt.X64.PopCount:
![POPCNT Compitition](resources/RiceTea.Benchmark.PopcntTest64-barplot.png)