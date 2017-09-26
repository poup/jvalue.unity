﻿using System;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;

namespace Halak
{
    class Program
    {
        static void Main(string[] args)
        {
            var switcher = new BenchmarkSwitcher(new[] {
                typeof(ParseInt32Benchmark),
                typeof(ParseSingleBenchmark),
                typeof(ParseDoubleBenchmark),
            });
            switcher.Run(args);
        }
    }
}