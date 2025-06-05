// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using Finnimon.Stl;
using Finnimon.Stl.Benchmark;

BenchmarkRunner.Run<StlCalculations>();