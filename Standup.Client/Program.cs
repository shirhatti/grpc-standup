using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace Standup.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //var channel = new Channel("localhost:50051", ChannelCredentials.Insecure);
            //var client = new Adder.AdderClient(channel);
            //await StreamingAddAsync(client);
            //await channel.ShutdownAsync();

            using var httpClient = new HttpClient() { BaseAddress = new Uri("https://localhost:5001") };
            var client = GrpcClient.Create<Adder.AdderClient>(httpClient);
            await StreamingAddAsync(client);
        }

        private static async Task StreamingAddAsync(Adder.AdderClient client)
        {
            using var call = client.Add();
            while (true)
            {
                var inputLine = Console.ReadLine();
                if (string.IsNullOrEmpty(inputLine))
                {
                    await call.RequestStream.CompleteAsync();
                    break;
                }
                if (int.TryParse(inputLine, out var parsedInput))
                {
                    await call.RequestStream.WriteAsync(new AddRequest { Value = parsedInput });
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            var response = await call;
            Console.WriteLine($"Sum:\t{response.Sum}");
        }
    }
}
