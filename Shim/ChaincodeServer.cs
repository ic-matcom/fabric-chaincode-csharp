using Protos;
using Grpc.Core;
using Google.Protobuf;

namespace Shim {

    // ChaincodeServer encapsulates basic properties needed for a chaincode server
    public class ChaincodeServer: Chaincode.ChaincodeBase{
        public string CCID { get; } // CCID should match chaincode's package name on peer
        public string Address { get; }// Addresss is the listen address of the chaincode server

        public IChaincode Chaincode { get; set; }// CC is the chaincode that handles Init and Invoke

        public ChaincodeServer(string chaincodeId , string address, IChaincode chaincode) {
            CCID = chaincodeId;
            Address = address;
            Chaincode = chaincode;
        }
        public override async Task Connect(IAsyncStreamReader<ChaincodeMessage> requestStream, IServerStreamWriter<ChaincodeMessage> responseStream, ServerCallContext context)
        {
            Console.WriteLine("CONNECT");

            Handler handler = new Handler(responseStream, Chaincode);
            var chaincodeID = new ChaincodeID() { Name= CCID};


            await responseStream.WriteAsync(new ChaincodeMessage() { Type = ChaincodeMessage.Types.Type.Register, Payload = ByteString.CopyFrom(chaincodeID.ToByteArray()) });

            Console.WriteLine("SENDING REGISTER MESSAGE");
            await Task.Run(async () =>
            {
                while (await requestStream.MoveNext())
                {
                    ChaincodeMessage request = requestStream.Current;

                    handler.HandleMessage(request);
                }
            });
        }
        public void Start(){
            string[] ip_port = Address.Split(':');
            string ip = ip_port[0];
            int port = int.Parse(ip_port[1]);

            Server server = new Server
            {
                Services = { Protos.Chaincode.BindService(this) },
                Ports = { new ServerPort(ip, port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("server listening on port " + port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}