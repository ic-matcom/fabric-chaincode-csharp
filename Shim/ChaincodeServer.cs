using Protos;
using Grpc.Core;
using Google.Protobuf;

namespace Shim {


    /// <summary>
    /// The ChaincodeServer class represents a chaincode gRPC server, which waits for connections from peers.
    /// </summary>
    public class ChaincodeServer: Chaincode.ChaincodeBase{

        /// <summary>
        ///  CCID should match chaincode's package name on peer
        /// </summary>
        public string CCID { get; }

        /// <summary>
        /// Addresss is the listen address of the chaincode server
        /// </summary>
        public string Address { get; }

        /// <summary>
        ///  CC is the chaincode that handles Init and Invoke
        /// </summary>
        public IChaincode Chaincode { get; set; }

        public ChaincodeServer(string chaincodeId , string address, IChaincode chaincode) {
            CCID = chaincodeId;
            Address = address;
            Chaincode = chaincode;
        }

        /// <summary>
        /// Connect the stream entry point called by chaincode to register with the Peer.
        /// </summary>
        /// <param name="requestStream">A stream of messages to be read</param>
        /// <param name="responseStream">A writable stream of messages that is used in server-side handlers</param>
        /// <param name="context">Context for a server side call</param>
        public override async Task Connect(IAsyncStreamReader<ChaincodeMessage> requestStream, IServerStreamWriter<ChaincodeMessage> responseStream, ServerCallContext context)
        {
            Console.WriteLine("CONNECT");

            Handler handler = new Handler(responseStream, Chaincode, context);
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

        /// <summary>
        /// Start the server
        /// </summary>
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