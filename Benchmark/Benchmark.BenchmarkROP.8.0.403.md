```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2033)
11th Gen Intel Core i7-1185G7 3.00GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.403
  [Host]     : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method        | Mean     | Error    | StdDev   | Gen0   | Allocated |
|-------------- |---------:|---------:|---------:|-------:|----------:|
| RopStyleHappy | 287.3 ns | 10.61 ns | 30.77 ns | 0.0229 |     144 B |
| IfStyleHappy  | 194.3 ns |  3.90 ns |  4.49 ns | 0.0229 |     144 B |
| RopStyleSad   | 155.7 ns |  3.18 ns |  5.90 ns | 0.0331 |     208 B |
| IfStyleSad    | 127.5 ns |  2.55 ns |  2.62 ns | 0.0331 |     208 B |
