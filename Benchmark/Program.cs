// See https://aka.ms/new-console-template for more information
using Benchmark;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<BenchmarkROP>();

var benchmark = new BenchmarkROP();
var happyRop = benchmark.RopStyleHappy();
var happyIf = benchmark.IfStyleHappy();
if (happyRop != happyIf)
    throw new InvalidDataException("Happy: Somethings wrong with the world today.");

var sadRop = benchmark.RopStyleSad();
var sadIf = benchmark.IfStyleSad();
if (sadRop != sadIf)
    throw new InvalidDataException("Sad: Somethings wrong with the world today.");
