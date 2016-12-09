// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Server.Kestrel.Internal;
using Microsoft.AspNetCore.Server.Kestrel.Internal.Http;
using Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure;
using Microsoft.AspNetCore.Testing;
using Moq;

namespace Microsoft.AspNetCore.Server.Kestrel.Performance
{
    public class Writing
    {
        private const int InnerLoopCount = 512;

        private readonly TestFrame<object> _frameWrite;
        private readonly TestFrame<object> _frameWriteAsync;
        private readonly TestFrame<object> _frameWriteAsyncAwaited;
        private readonly TestFrame<object> _frameChunked;
        private readonly byte[] _writeData;

        public Writing()
        {
            _frameChunked = MakeFrame();
            _frameWrite = MakeFrame();
            _frameWriteAsync = MakeFrame();
            _frameWriteAsyncAwaited = MakeFrame();

            _writeData = new byte[1024];
        }

        [Setup]
        public void SetupFrames()
        {
            foreach (var frame in new[] { _frameWrite, _frameWriteAsync, _frameWriteAsyncAwaited })
            {
                frame.Reset();
                frame.RequestHeaders.Add("Content-Length", (InnerLoopCount * _writeData.Length).ToString(CultureInfo.InvariantCulture));
            }
        }

        [Benchmark(OperationsPerInvoke = InnerLoopCount)]
        public void Write()
        {
            _frameWrite.Write(new ArraySegment<byte>(_writeData));
        }

        [Benchmark(OperationsPerInvoke = InnerLoopCount)]
        public void WriteChunked()
        {
            _frameChunked.Write(new ArraySegment<byte>(_writeData));
        }

        [Benchmark(OperationsPerInvoke = InnerLoopCount)]
        public async Task WriteAsync()
        {
            await _frameWriteAsync.WriteAsync(new ArraySegment<byte>(_writeData), default(CancellationToken));
        }

        [Benchmark(OperationsPerInvoke = InnerLoopCount)]
        public async Task WriteAsyncChunked()
        {
            await _frameChunked.WriteAsync(new ArraySegment<byte>(_writeData), default(CancellationToken));
        }

        [Benchmark(OperationsPerInvoke = InnerLoopCount)]
        public async Task WriteAsyncAwaited()
        {
            await _frameWriteAsyncAwaited.WriteAsyncAwaited(new ArraySegment<byte>(_writeData), default(CancellationToken));
        }

        [Benchmark(OperationsPerInvoke = InnerLoopCount)]
        public async Task WriteAsyncAwaitedChunked()
        {
            await _frameChunked.WriteAsyncAwaited(new ArraySegment<byte>(_writeData), default(CancellationToken));
        }

        [Benchmark(OperationsPerInvoke = InnerLoopCount)]
        public async Task ProduceEnd()
        {
            await _frameWrite.ProduceEndAsync();
        }

        [Benchmark(OperationsPerInvoke = InnerLoopCount)]
        public async Task ProduceEndChunked()
        {
            await _frameChunked.ProduceEndAsync();
        }

        private TestFrame<object> MakeFrame()
        {
            var ltp = new LoggingThreadPool(Mock.Of<IKestrelTrace>());
            var pool = new MemoryPool();
            var socketInput = new SocketInput(pool, ltp);

            var serviceContext = new ServiceContext
            {
                DateHeaderValueManager = new DateHeaderValueManager(),
                ServerOptions = new KestrelServerOptions(),
                Log = Mock.Of<IKestrelTrace>()
            };
            var listenerContext = new ListenerContext(serviceContext)
            {
                ServerAddress = ServerAddress.FromUrl("http://localhost:5000")
            };
            var connectionContext = new ConnectionContext(listenerContext)
            {
                Input = socketInput,
                Output = new MockSocketOutput(),
                ConnectionControl = Mock.Of<IConnectionControl>()
            };

            var frame = new TestFrame<object>(application: null, context: connectionContext);
            frame.Reset();
            frame.InitializeHeaders();

            return frame;
        }
    }
}
