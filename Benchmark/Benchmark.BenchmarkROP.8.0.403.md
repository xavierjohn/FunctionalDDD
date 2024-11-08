```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2033)
11th Gen Intel Core i7-1185G7 3.00GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.403
  [Host]     : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method        | Mean     | Error    | StdDev   | Median   | Gen0   | Allocated |
|-------------- |---------:|---------:|---------:|---------:|-------:|----------:|
| RopStyleHappy | 235.8 ns |  2.99 ns |  2.34 ns | 235.1 ns | 0.0229 |     144 B |
| IfStyleHappy  | 195.4 ns |  3.98 ns | 10.82 ns | 191.6 ns | 0.0229 |     144 B |
| RopStyleSad   | 151.8 ns |  1.66 ns |  1.48 ns | 151.4 ns | 0.0331 |     208 B |
| IfStyleSad    | 170.8 ns | 12.07 ns | 35.39 ns | 174.3 ns | 0.0331 |     208 B |
| RopSample1    | 945.3 ns | 18.78 ns | 32.39 ns | 935.1 ns | 0.1545 |     976 B |
| IfSample1     | 782.3 ns | 15.66 ns | 36.30 ns | 773.6 ns | 0.1554 |     976 B |
