﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Halak.JValueComparison
{
    static class Program
    {
        static List<string> Columns;
        static Dictionary<string, List<long>> Times;

        static void Main(string[] args)
        {
            Columns = new List<string>();
            Times = new Dictionary<string, List<long>>();

            var currentProcess = Process.GetCurrentProcess();
            currentProcess.ProcessorAffinity = new IntPtr(2);
            currentProcess.PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            SmallIntArray();
            BigIntArray();
            SmallObject();
            BigObject();

            Console.WriteLine();
            Console.WriteLine("Benchmark Complete");
            Console.WriteLine();

            PrintMarkdownTable();

            Console.ReadKey();
        }

        static void PrintMarkdownTable()
        {
            // | Library | Column1 | Column2 | ... | ColumnN |
            Console.Write("| Library | ");
            Console.Write(string.Join(" | ", Columns));
            Console.WriteLine(" |");

            // |:-------:|:-------:|:-------:| ... |:-------:|
            Console.Write("|:----------------:");
            for (int i = 0; i < Columns.Count; i++)
                Console.Write("|:---------:");
            Console.WriteLine("|");

            for (int i = 0; i < Columns.Count; i++)
            {
                long fastestTime = long.MaxValue;
                string fastestName = string.Empty;
                foreach (var item in Times)
                {
                    if (item.Value[i] < fastestTime)
                    {
                        fastestTime = item.Value[i];
                        fastestName = item.Key;
                    }
                }

                Times[fastestName][i] *= -1; // mark as fastest
            }

            foreach (var item in Times)
            {
                Console.Write("| {0,16}", item.Key);
                foreach (var elapsedTime in item.Value)
                {
                    string elapsedTimeString;
                    if (elapsedTime < 0) // negative is fastest.
                        elapsedTimeString = string.Format("**{0:N0}ms**", Math.Abs(elapsedTime));
                    else
                        elapsedTimeString = string.Format("{0:N0}ms", elapsedTime);

                    Console.Write(" | {0,9}", elapsedTimeString);
                }
                Console.WriteLine(" |");
            }
        }

        static void Header(string title, int count = 100000)
        {
            Console.WriteLine(title);
            Columns.Add(string.Format("{0} × {1:N0}", title, count));
        }

        #region Benchmark
        static void Benchmark(string name, Action action, int count = 100000)
        {
            long elapsedTime = Halak.JValueDev.Program.Benchmark(name, action, count);

            List<long> times;
            if (Times.TryGetValue(name, out times))
                times.Add(elapsedTime);
            else
                Times.Add(name, new List<long>() { elapsedTime });
        }
        #endregion

        static void SmallIntArray()
        {
            string source = "[10, 20, 30, 40]";

            Header(string.Format("Small Int Array ({0:N0}bytes)", source.Length * 2));
            Benchmark("LitJson", () => LitJsonSide.EnumerateArray(source));
            Benchmark("JsonFx", () => JsonFxSide.EnumerateArray(source));
            Benchmark("Json.NET", () => NewtonsoftJsonSide.EnumerateArray(source));
            Benchmark("MiniJSON", () => MiniJSONSide.EnumerateArray(source));
            Benchmark("ServiceStackSide", () => ServiceStackSide.EnumerateArray(source));
            Benchmark("JValue", () => JValueSide.EnumerateArray(source));
            Benchmark("JValue other", () => JValueOtherSide.EnumerateArray(source));
            Console.WriteLine();
        }

        static void BigIntArray()
        {
            int count = 100;

            var random = new Random();
            var o = new int[10000];
            for (int i = 0; i < o.Length; i++)
                o[i] = random.Next();

            string source = '[' + string.Join(",", o) + ']';

            Header(string.Format("Big Int Array ({0:N0}bytes)", source.Length * 2), count);
            Benchmark("LitJson", () => LitJsonSide.EnumerateArray(source), count);
            Benchmark("JsonFx", () => JsonFxSide.EnumerateArray(source), count);
            Benchmark("Json.NET", () => NewtonsoftJsonSide.EnumerateArray(source), count);
            Benchmark("MiniJSON", () => MiniJSONSide.EnumerateArray(source), count);
            Benchmark("ServiceStackSide", () => ServiceStackSide.EnumerateArray(source), count);
            Benchmark("JValue", () => JValueSide.EnumerateArray(source), count);
            Benchmark("JValue other", () => JValueOtherSide.EnumerateArray(source), count);
            Console.WriteLine();
        }

        static void SmallObject()
        {
            string source = @"{
                ""Cupcake"": ""Android 1.5 (API Level 3)"",
                ""Donut"": ""Android 1.6 (API Level 4)"",
                ""Eclair"": ""Android 2.0 (API Level 5)"",
                ""Froyo"": ""Android 2.2 (API Level 8)"",
                ""Gingerbread"": ""Android 2.3(API Level 9)"",
                ""Honeycomb"": ""Android 3.0 (API Level 11)"",
                ""IceCreamSandwich"": ""Android 4.0 (API Level 14)"",
                ""JellyBean"": ""Android 4.1 (API Level 16)"",
                ""KitKat"": ""Android 4.4 (API Level 19)""
            }";
            var keys = new string[] { "JellyBean", "Cupcake", "IceCreamSandwich", "Eclair", "Donut", "Cupcake", "Froyo", "Honeycomb", "KitKat" };

            int count = 10000;
            Header(string.Format("Small Object ({0:N0}bytes)", source.Length * 2), count);
            Benchmark("LitJson", () => LitJsonSide.QueryObject(source, keys), count);
            Benchmark("JsonFx", () => JsonFxSide.QueryObject(source, keys), count);
            Benchmark("Json.NET", () => NewtonsoftJsonSide.QueryObject(source, keys), count);
            Benchmark("MiniJSON", () => MiniJSONSide.QueryObject(source, keys), count);
            Benchmark("ServiceStackSide", () => ServiceStackSide.QueryObject(source, keys), count);
            Benchmark("JValue", () => JValueSide.QueryObject(source, keys), count);
            Benchmark("JValue other", () => JValueOtherSide.QueryObject(source, keys), count);
            Console.WriteLine();
        }

        static void BigObject()
        {
            var random = new Random();
            var o = new HashSet<int>();
            while (o.Count < 100000)
                o.Add(random.Next());

            var allKeys = new List<string>();
            var sb = new StringBuilder(o.Count * 10);
            sb.Append("{");
            bool isFirst = true;
            foreach (var item in o)
            {
                if (isFirst == false)
                    sb.Append(",");
                else
                    isFirst = false;

                var key = string.Format("Hello{0}", item);
                allKeys.Add(key);
                sb.AppendFormat("\"{0}\": {1}", key, random.Next());
            }
            sb.Append("}");

            string[] keys = new string[8]
            {
                allKeys[allKeys.Count / 8 * 0],
                allKeys[allKeys.Count / 8 * 1],
                allKeys[allKeys.Count / 8 * 2],
                allKeys[allKeys.Count / 8 * 3],
                allKeys[allKeys.Count / 8 * 4],
                allKeys[allKeys.Count / 8 * 5],
                allKeys[allKeys.Count / 8 * 6],
                allKeys[allKeys.Count / 8 * 7],
            };

            string source = sb.ToString();
            int count = 10;

            Header(string.Format("Big Object ({0:N0}bytes)", source.Length * 2), count);
            Benchmark("LitJson", () => LitJsonSide.QueryObject(source, keys), count);
            Benchmark("JsonFx", () => JsonFxSide.QueryObject(source, keys), count);
            Benchmark("Json.NET", () => NewtonsoftJsonSide.QueryObject(source, keys), count);
            Benchmark("MiniJSON", () => MiniJSONSide.QueryObject(source, keys), count);
            Benchmark("ServiceStackSide", () => ServiceStackSide.QueryObject(source, keys), count);
            Benchmark("JValue", () => JValueSide.QueryObject(source, keys), count);
            Benchmark("JValue other", () => JValueOtherSide.QueryObject(source, keys), count);
            Console.WriteLine();
        }

        public static void Noop()
        {
        }

        public static void Noop<T>(T value)
        {
        }
    }
}
