using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Standup
{
    public class MaxStreamingRequestTimeoutInterceptor : Interceptor
    {
        private static readonly RpcException _maxStreamingRequestTimeoutExceededException = new RpcException(new Status(StatusCode.Aborted, "Timeout for receiving requests exceeded."), "Timeout for receiving requests exceeded.");

        private readonly TimeSpan _streamingRequestTimeout;

        public MaxStreamingRequestTimeoutInterceptor(TimeSpan streamingRequestTimeout)
        {
            if (streamingRequestTimeout < TimeSpan.Zero && streamingRequestTimeout != Timeout.InfiniteTimeSpan)
            {
                throw new ArgumentOutOfRangeException(nameof(streamingRequestTimeout), streamingRequestTimeout, $"{nameof(streamingRequestTimeout)} must be a positive value.");
            }

            _streamingRequestTimeout = streamingRequestTimeout != Timeout.InfiniteTimeSpan ? streamingRequestTimeout : TimeSpan.MaxValue;
        }

        public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            return continuation(new TimeoutAsyncStreamReader<TRequest>(requestStream, _streamingRequestTimeout), context);
        }

        private class TimeoutAsyncStreamReader<TRequest> : IAsyncStreamReader<TRequest>
        {
            private readonly IAsyncStreamReader<TRequest> _inner;
            private readonly TimeSpan _timeout;

            public TimeoutAsyncStreamReader(IAsyncStreamReader<TRequest> inner, TimeSpan timeout)
            {
                _inner = inner;
                _timeout = timeout;
            }

            public TRequest Current => _inner.Current;

            public void Dispose()
            {
            }

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                var task = _inner.MoveNext(cancellationToken);

                if (task.IsCompleted || Debugger.IsAttached)
                {
                    return await task;
                }

                var cts = new CancellationTokenSource();
                if (task == await Task.WhenAny(task, Task.Delay(_timeout, cts.Token)))
                {
                    cts.Cancel();
                    return await task;
                }
                else
                {
                    throw _maxStreamingRequestTimeoutExceededException;
                }
            }
        }
    }
}
