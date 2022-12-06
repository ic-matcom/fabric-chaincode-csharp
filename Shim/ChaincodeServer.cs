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

        private ILogger _logger { get; set; }

        public ChaincodeServer(string chaincodeId , string address, IChaincode chaincode, ILogger logger) {
            CCID = chaincodeId;
            Address = address;
            Chaincode = chaincode;
            _logger = logger;  
        }

        /// <summary>
        /// Connect the stream entry point called by chaincode to register with the Peer.
        /// </summary>
        /// <param name="requestStream">A stream of messages to be read</param>
        /// <param name="responseStream">A writable stream of messages that is used in server-side handlers</param>
        /// <param name="context">Context for a server side call</param>
        public override async Task Connect(IAsyncStreamReader<ChaincodeMessage> requestStream, IServerStreamWriter<ChaincodeMessage> responseStream, ServerCallContext context)
        {
            _logger.LogInformation("Chaincode ID: " + CCID);
            Handler handler = new Handler(responseStream, Chaincode, context, _logger);
            var chaincodeID = new ChaincodeID() { Name= CCID};

            _logger.LogDebug("Sending REGISTER message to peer");
            await responseStream.WriteAsync(new ChaincodeMessage() { Type = ChaincodeMessage.Types.Type.Register, Payload = ByteString.CopyFrom(chaincodeID.ToByteArray()) });

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
            if (CCID == null)
                throw new Exception("Chincode ID most be specified");
            if (Address == null)
                throw new Exception("Addres most be specified");

            string[] ip_port = Address.Split(':');
            string ip = ip_port[0];
            int port = int.Parse(ip_port[1]);

            Server server = new Server
            {
                Services = { Protos.Chaincode.BindService(this) },

            };

            string? key = Environment.GetEnvironmentVariable("CORE_TLS_CLIENT_KEY_PATH");
            string? cert = Environment.GetEnvironmentVariable("CORE_TLS_CLIENT_CERT_PATH");

            if (key == null || cert == null)
                server.Ports.Add(new ServerPort(ip, port, ServerCredentials.Insecure));
            else
            {
                var clientCaCerts = Environment.GetEnvironmentVariable("CORE_PEER_TLS_ROOTCERT_FILE");
                var clientAuth = clientCaCerts != null;
                var serverCredentials = new SslServerCredentials(new List<KeyCertificatePair> { new KeyCertificatePair(cert, key) }, clientCaCerts, clientAuth);
                server.Ports.Add(new ServerPort(ip, port, serverCredentials));
            }

            server.Start();

            _logger.LogInformation("Server listening on port " + port);
            _logger.LogInformation("Press any key to stop the server...");

            server.ShutdownAsync().Wait();
        }
    }
}