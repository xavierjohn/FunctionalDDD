```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2033)
11th Gen Intel Core i7-1185G7 3.00GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.403
  [Host]     : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method               | Mean     | Error    | StdDev   | Median   | Gen0   | Allocated |
|--------------------- |---------:|---------:|---------:|---------:|-------:|----------:|
| RopStyleHappy        | 306.2 ns | 16.51 ns | 47.09 ns | 299.2 ns | 0.0229 |     144 B |
| IfStyleHappy         | 192.6 ns |  3.69 ns |  9.98 ns | 189.4 ns | 0.0229 |     144 B |
| RopStyleSad          | 162.8 ns |  3.25 ns |  7.53 ns | 160.2 ns | 0.0331 |     208 B |
| IfStyleSad           | 136.0 ns |  2.79 ns |  7.20 ns | 134.0 ns | 0.0331 |     208 B |
| RopStyleWithClosure  | 992.0 ns | 26.59 ns | 74.56 ns | 964.3 ns | 0.1545 |     976 B |
| IfStyleWithNoClosure | 771.1 ns | 15.45 ns | 41.77 ns | 756.6 ns | 0.1554 |     976 B |
