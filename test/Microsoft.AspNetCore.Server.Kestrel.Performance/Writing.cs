// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
        private readonly TestFrame<object> _frame;
        private readonly byte[] _writeData = new byte[1024];

        public Writing()
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

            _frame = new TestFrame<object>(application: null, context: connectionContext);
            _frame.Reset();
            _frame.InitializeHeaders();
        }

        [Benchmark]
        public void Write()
        {
            _frame.Write(new ArraySegment<byte>(_writeData));
        }

        [Benchmark]
        public async Task WriteAsync()
        {
            await _frame.WriteAsync(new ArraySegment<byte>(_writeData), default(CancellationToken));
        }

        [Benchmark]
        public async Task WriteAsyncAwaited()
        {
            await _frame.WriteAsyncAwaited(new ArraySegment<byte>(_writeData), default(CancellationToken));
        }

        [Benchmark]
        public async Task ProduceEnd()
        {
            await _frame.ProduceEndAsync();
        }
    }
}
