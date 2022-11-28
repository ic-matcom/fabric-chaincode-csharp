using Shim;

namespace fabric_chaincode_csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            string? address = Environment.GetEnvironmentVariable("CHAINCODE_SERVER_ADDRESS");
            string? chaincodeId = Environment.GetEnvironmentVariable("CHAINCODE_ID");

            ChaincodeServer server = new ChaincodeServer(chaincodeId, address, new Test.AssetTransfer());

            server.Start();
        }
    }
}
