using Grpc.Core;
using Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fabric_chaincode_csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            const int Port = 9999;

            Server server = new Server
            {
                Services = { Chaincode.BindService( new Shim.ChaincodeServer()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();
            

            Console.WriteLine("server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
