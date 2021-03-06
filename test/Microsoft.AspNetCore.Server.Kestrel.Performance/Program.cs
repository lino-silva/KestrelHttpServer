﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Properties;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains;

namespace Microsoft.AspNetCore.Server.Kestrel.Performance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var options = (uint[])Enum.GetValues(typeof(BenchmarkType));
            BenchmarkType type;
            if (args.Length != 1 || !Enum.TryParse(args[0], out type))
            {
                Console.WriteLine($"Please add benchmark to run as parameter:");
                for (var i = 0; i < options.Length; i++)
                {
                    Console.WriteLine($"  {((BenchmarkType)options[i]).ToString()}");
                }

                return;
            }

            RunSelectedBenchmarks(type);
        }

        private static void RunSelectedBenchmarks(BenchmarkType type)
        {
            if (type.HasFlag(BenchmarkType.RequestParsing))
            {
                BenchmarkRunner.Run<RequestParsing>();
            }
        }
    }

    [Flags]
    public enum BenchmarkType : uint
    {
        RequestParsing = 1,
        // add new ones in powers of two - e.g. 2,4,8,16...

        All = uint.MaxValue
    }
}
