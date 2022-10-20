using Protos;
using Grpc.Core;
using Google.Protobuf;
using System.Text;

namespace Shim {

    // ChaincodeServer encapsulates basic properties needed for a chaincode server
    public class ChaincodeServer: Chaincode.ChaincodeBase{
        public string CCID {
            get => "basic_1.0:5b59f09c771c6375a95e7ce71c5ed41eb4d98235acc8f78474c178fc5d4a5f7f";
        } // CCID should match chaincode's package name on peer
        public string Address { get => "localhost"; }// Addresss is the listen address of the chaincode server

        //public IChaincode Chaincode { get; set; }// CC is the chaincode that handles Init and Invoke

        //TODO: TLSproperties
        // TLSProps is the TLS properties passed to chaincode server

        public ChaincodeServer() { }
        public override async Task Connect(IAsyncStreamReader<ChaincodeMessage> requestStream, IServerStreamWriter<ChaincodeMessage> responseStream, ServerCallContext context)
        {

            await responseStream.WriteAsync(new ChaincodeMessage() { Type = ChaincodeMessage.Types.Type.Register, Payload = ByteString.CopyFromUtf8(CCID) });
            while (await requestStream.MoveNext())
            {
                var request = requestStream.Current;
                Console.WriteLine("Request Payload ----->", request.Payload);
            }
        }
        public void Start(){}
    }
}