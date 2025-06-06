```

BenchmarkDotNet v0.15.0, Linux Zorin OS 17.3
Intel Core i5-4300M CPU 2.60GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
.NET SDK 8.0.116
  [Host]     : .NET 8.0.16 (8.0.1625.21506), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.16 (8.0.1625.21506), X64 RyuJIT AVX2


```
| Method                   | Mean     | Error    | StdDev   |
|------------------------- |---------:|---------:|---------:|
| ParallelVertexCentroid   | 20.20 ms | 0.403 ms | 1.097 ms |
| ParallelAreaCentroid     | 31.60 ms | 0.621 ms | 1.020 ms |
| ParallelVolumeCentroid   | 28.68 ms | 0.476 ms | 0.445 ms |
| SequentialVertexCentroid | 12.88 ms | 0.103 ms | 0.086 ms |
| SequentialAreaCentroid   | 30.41 ms | 0.534 ms | 0.713 ms |
| SequentialVolumeCentroid | 22.12 ms | 0.265 ms | 0.235 ms |
| ParallelVolume           | 17.82 ms | 0.354 ms | 0.485 ms |
| SequentialVolume         | 12.32 ms | 0.221 ms | 0.173 ms |
| ParallelSurface          | 20.94 ms | 0.417 ms | 0.802 ms |
| SequentialSurface        | 17.02 ms | 0.217 ms | 0.181 ms |
