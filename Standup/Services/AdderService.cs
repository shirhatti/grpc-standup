using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace Standup
{
    public class AdderService : Adder.AdderBase
    {
        public override async Task<AddReply> Add(IAsyncStreamReader<AddRequest> requestStream, ServerCallContext context)
        {
            int sum = 0;
            while (await requestStream.MoveNext(CancellationToken.None))
            {
                sum += requestStream.Current.Value;
            }
            return new AddReply { Sum = sum };
        }
    }
}
