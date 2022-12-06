using Shim;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.Console;
namespace fabric_chaincode_csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("NonHostConsoleApp.Program", LogLevel.Debug)
                    .AddConsole();
            });
            ILogger logger = loggerFactory.CreateLogger<Program>();
   


            string? address = Environment.GetEnvironmentVariable("CHAINCODE_SERVER_ADDRESS");
            string? chaincodeId = Environment.GetEnvironmentVariable("CHAINCODE_ID");

            ChaincodeServer server = new ChaincodeServer(chaincodeId, address, new Test.AssetTransfer(), logger);

            server.Start();
        }
    }
}
