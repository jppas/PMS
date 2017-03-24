using System;
using System.Threading.Tasks;
using IdentityModel.Client;
namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var s =  TestDisco();

        }

        static async Task<string> TestDisco()
        {
            var disco = await DiscoveryClient.GetAsync("http://localhost:5000");
            if (disco.IsError) throw new Exception(disco.Error);

            var client = new TokenClient(
                disco.TokenEndpoint,
                "client",
                "secret");

            return "hello";
        }
    }
}