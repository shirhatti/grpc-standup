using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Standup.Clientnew
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var store = new X509Store(StoreName.My);
            store.Open(OpenFlags.ReadOnly);
            var clientCert = store.Certificates.Find(X509FindType.FindBySubjectName, "P2SChildCert", true)[0];


            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ClientCertificates.Add(clientCert);


            using var httpClient = new HttpClient(httpClientHandler) { BaseAddress = new Uri("https://localhost:5001") };
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
