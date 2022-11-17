using Protos;
using Grpc.Core;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Shim {

    // ChaincodeServer encapsulates basic properties needed for a chaincode server
    public class ChaincodeServer: Chaincode.ChaincodeBase{
        public string CCID {
            get => "basic_1.0:88ab5efb3ee0ece0f756871f6bb4dbc62dc294083c4f9f39a82c39f63c287a59";
        } // CCID should match chaincode's package name on peer
        public string Address { get => "localhost"; }// Addresss is the listen address of the chaincode server

        public IChaincode Chaincode { get; set; }// CC is the chaincode that handles Init and Invoke

        //TODO: TLSproperties
        // TLSProps is the TLS properties passed to chaincode server

        public ChaincodeServer() { }
        public override async Task Connect(IAsyncStreamReader<ChaincodeMessage> requestStream, IServerStreamWriter<ChaincodeMessage> responseStream, ServerCallContext context)
        {
            Console.WriteLine("CONNECT");

            Handler handler = new Handler(requestStream, responseStream, Chaincode);
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
            const int Port = 9999;

            Server server = new Server
            {
                Services = { Protos.Chaincode.BindService(new Shim.ChaincodeServer()) },
                Ports = { new ServerPort("127.0.0.1", Port, ServerCredentials.Insecure) }
            };
            server.Start();
        }
    }
}