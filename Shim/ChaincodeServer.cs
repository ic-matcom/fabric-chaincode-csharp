using Protos;
using Grpc.Core;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Shim {

    // ChaincodeServer encapsulates basic properties needed for a chaincode server
    public class ChaincodeServer: Chaincode.ChaincodeBase{
        public string CCID {
            get => "basic_1.0:58fd455e43eb6e6f2334c22bdc28022fefa5658f8d675c6b53e0ea4f8d50ed31";
        } // CCID should match chaincode's package name on peer
        public string Address { get => "localhost"; }// Addresss is the listen address of the chaincode server

        public IChaincode Chaincode { get; set; }// CC is the chaincode that handles Init and Invoke

        //TODO: TLSproperties
        // TLSProps is the TLS properties passed to chaincode server

        public ChaincodeServer() { }
        public override async Task Connect(IAsyncStreamReader<ChaincodeMessage> requestStream, IServerStreamWriter<ChaincodeMessage> responseStream, ServerCallContext context)
        {
            Handler handler = new Handler(requestStream, responseStream, Chaincode);
            var chaincodeID = new ChaincodeID() { Name= CCID};
            await responseStream.WriteAsync(new ChaincodeMessage() { Type = ChaincodeMessage.Types.Type.Register, Payload = ByteString.CopyFrom(chaincodeID.ToByteArray()), Timestamp= Timestamp.FromDateTime(DateTime.Now) });
            while (await requestStream.MoveNext())
            {
                ChaincodeMessage request = requestStream.Current;

                handler.HandleMessage(request);
            }
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