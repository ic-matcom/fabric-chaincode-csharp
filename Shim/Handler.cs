using Protos;
using Grpc.Core;
using Google.Protobuf;

namespace Shim
{

    /// <summary>
    /// Handles messages between peer and chaincode both in the chaincode server and client model.
    /// </summary>
    public class Handler: IHandler
    {
        public State State { get; private set; }
        public IServerStreamWriter<ChaincodeMessage> ResponseStream { get;}

        private IMessageQueue _messageQueue;
        private ServerCallContext _context;

        public IChaincode Chaincode { get; set; }

        public Handler(IServerStreamWriter<ChaincodeMessage> responseStream, IChaincode chaincode, ServerCallContext context)
        {
            State = State.created;
            ResponseStream = responseStream;
            Chaincode = chaincode;
            _context = context;
            _messageQueue = new MessageQueue(this);
        }


        /// <summary>
        /// handleMessage message handles loop for shim side of chaincode/peer stream.
        /// </summary>
        public void HandleMessage(ChaincodeMessage message)
        {
            switch (State)
            {
                case State.ready:
                    HandleReady(message);
                    break;
                case State.established:
                    HandleEstablished(message);
                    break;
                case State.created:
                    HandleCreated(message);
                    break;
                default: break;
            }
        }

        /// <summary>
        /// HandleEstablished handles messages received from the peer when the handler is in the "established" state.
        /// </summary>
        public void HandleEstablished(ChaincodeMessage message)
        {
            if (message.Type != ChaincodeMessage.Types.Type.Ready)
                throw new Exception($"{message.Txid} Chaincode cannot handle message {message.Type} while in state {State}");

            State = State.ready;
        }

        /// <summary>
        /// HandleEstablished handles messages received from the peer when the handler is in the "established" state.
        /// </summary>
        public void HandleCreated(ChaincodeMessage message)
        {
            if (message.Type != ChaincodeMessage.Types.Type.Registered)
                throw new Exception($"{message.Txid} Chaincode cannot handle message {message.Type} while in state {State}");
            
            State = State.established;
        }

        /// <summary>
        /// HandleTransaction calls Invoke on the associated chaincode.
        /// </summary>
        public async Task HandleTransaction(ChaincodeMessage chaincodeMessage)
        {
            ChaincodeInput input;
            ChaincodeMessage errorMessage;

            try
            {
                // Get the function and args from Payload
                input = ChaincodeInput.Parser.ParseFrom(chaincodeMessage.Payload);
            }
            catch
            {
                Console.WriteLine(
                 $"{chaincodeMessage.ChannelId}-{chaincodeMessage.Txid} Incorrect payload format. Sending ERROR message back to peer");
                errorMessage = new ChaincodeMessage
                {
                    Txid = chaincodeMessage.Txid,
                    ChannelId = chaincodeMessage.ChannelId,
                    Type = ChaincodeMessage.Types.Type.Error,
                    Payload = chaincodeMessage.Payload
                };

                await ResponseStream.WriteAsync(errorMessage);
                return;
            }

            ChaincodeStub stub;
            try
            {
                // Create the ChaincodeStub which the chaincode can use to callback
                stub = new ChaincodeStub(this, chaincodeMessage.ChannelId, chaincodeMessage.Txid, input, chaincodeMessage.Proposal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to construct a chaincode stub instance for the INVOKE message: {ex}");
                errorMessage = new ChaincodeMessage
                {
                    Type = ChaincodeMessage.Types.Type.Error,
                    Payload = ByteString.CopyFromUtf8(ex.Message),
                    Txid = chaincodeMessage.Txid,
                    ChannelId = chaincodeMessage.ChannelId
                };
                await ResponseStream.WriteAsync(errorMessage);
                return;
            }

            Response response = await Chaincode.Invoke(stub);

            Console.WriteLine($"[{chaincodeMessage.ChannelId}-{chaincodeMessage.Txid}] Calling chaincode INVOKE succeeded. Sending COMPLETED message back to peer");
            ChaincodeMessage responseMessage = new ChaincodeMessage
            {
                Type = ChaincodeMessage.Types.Type.Completed,
                Payload = response.ToByteString(),
                Txid = chaincodeMessage.Txid,
                ChannelId = chaincodeMessage.ChannelId,
                ChaincodeEvent = stub.ChaincodeEvent
            };

            ResponseStream.WriteAsync(responseMessage).Wait(_context.CancellationToken);
            

        }

        /// <summary>
        /// handleInit calls the Init function of the associated chaincode.
        /// </summary>
        public async Task HandleInit(ChaincodeMessage chaincodeMessage) {
            Console.WriteLine("HANDLE INIT");
            ChaincodeInput input;
            ChaincodeMessage errorMessage;

            try
            {
                // Get the function and args from Payload
                input = ChaincodeInput.Parser.ParseFrom(chaincodeMessage.Payload);
            }
            catch
            {
                   Console.WriteLine(
                    $"{chaincodeMessage.ChannelId}-{chaincodeMessage.Txid} Incorrect payload format. Sending ERROR message back to peer");
                errorMessage = new ChaincodeMessage
                {
                    Txid = chaincodeMessage.Txid,
                    ChannelId = chaincodeMessage.ChannelId,
                    Type = ChaincodeMessage.Types.Type.Error,
                    Payload = chaincodeMessage.Payload
                };

                await ResponseStream.WriteAsync(errorMessage);
                return;
            }

            ChaincodeStub stub;
            try
            {
                // Create the ChaincodeStub which the chaincode can use to callback
                stub = new ChaincodeStub(this, chaincodeMessage.ChannelId, chaincodeMessage.Txid, input, chaincodeMessage.Proposal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to construct a chaincode stub instance for the INIT message: {ex}");
                errorMessage = new ChaincodeMessage
                {
                    Type = ChaincodeMessage.Types.Type.Error,
                    Payload = ByteString.CopyFromUtf8(ex.Message),
                    Txid = chaincodeMessage.Txid,
                    ChannelId = chaincodeMessage.ChannelId
                };
                await ResponseStream.WriteAsync(errorMessage);
                return;
            }

            Response response = await Chaincode.Init(stub);

            Console.WriteLine($"[{chaincodeMessage.ChaincodeEvent}]-{chaincodeMessage.Txid} Calling chaincode INIT, response status {response.Status}");

            if (response.Status >= 500)
            {
                Console.WriteLine($"[{chaincodeMessage.ChannelId}-{chaincodeMessage.Txid}] Calling chaincode INIT returned error response {response.Message}. " +
                                 "Sending ERROR message back to peer");

                errorMessage = new ChaincodeMessage
                {
                    Type = ChaincodeMessage.Types.Type.Error,
                    Payload = ByteString.CopyFromUtf8(response.Message),
                    Txid = chaincodeMessage.Txid,
                    ChannelId = chaincodeMessage.ChannelId
                };
                await ResponseStream.WriteAsync(errorMessage);
                return;
            }
            Console.WriteLine($"[{chaincodeMessage.ChannelId}-{chaincodeMessage.Txid}] Calling chaincode INIT succeeded. Sending COMPLETED message back to peer");
            ChaincodeMessage responseMessage = new ChaincodeMessage
            {
                Type = ChaincodeMessage.Types.Type.Completed,
                Payload = response.ToByteString(),
                Txid = chaincodeMessage.Txid,
                ChannelId = chaincodeMessage.ChannelId,
                ChaincodeEvent = stub.ChaincodeEvent
            };

            await ResponseStream.WriteAsync(responseMessage);
        }

        /// <summary>
        /// handleReady handles messages received from the peer when the handler is in the "ready" state.
        /// </summary>
        /// <param name="chaincodeMessage">chaincode message to handle</param>
        public async void HandleReady(ChaincodeMessage chaincodeMessage)
        {
            switch (chaincodeMessage.Type)
            {
                case ChaincodeMessage.Types.Type.Response:
                    _messageQueue.HandleMessageResponse(chaincodeMessage);
                    break;

                case ChaincodeMessage.Types.Type.Init:
                     await HandleInit(chaincodeMessage);
                    break;

                case ChaincodeMessage.Types.Type.Transaction:
                     await HandleTransaction(chaincodeMessage);
                    break;
            }
        }


        private Task<T> AskPeerAndListen<T>(ChaincodeMessage message, MessageMethod method)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();

            var queueMessage = new QueueMessage<T>(message, method, taskCompletionSource);
            _messageQueue.QueueMessage(queueMessage);

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// HandleDeleteState communicates with the peer to delete a key from the state in the ledger.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="key">key of the asset to delete</param>
        /// <param name="channelId">channel id</param>
        /// <param name="txId">transaction id</param>
        public Task<ByteString> HandleDeleteState(string collection, string key, string channelId, string txId)
        {
            var payload = new DelState
            {
                Key = key,
                Collection = collection
            };
            ChaincodeMessage message = new ChaincodeMessage()
            {
                Type = ChaincodeMessage.Types.Type.DelState,
                Payload = ByteString.CopyFrom(payload.ToByteArray()),
                ChannelId = channelId,
                Txid = txId,
            };

            return AskPeerAndListen<ByteString>(message, MessageMethod.DelState);
        }


        /// <summary>
        /// HandlePutState communicates with the peer to put state information into the ledger.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="key">key of the asset being added to the ledger</param>
        /// <param name="channelId">channel id</param>
        /// <param name="txId">transaction id</param>
        public Task<ByteString> HandlePutState(string collection, string key, ByteString value, string channelId, string txId)
        {
            var payload = new PutState
            {
                Key = key,
                Value = value,
                Collection = collection
            };
            ChaincodeMessage message = new ChaincodeMessage()
            {
                Type = ChaincodeMessage.Types.Type.PutState,
                Payload = ByteString.CopyFrom(payload.ToByteArray()),
                ChannelId = channelId,
                Txid = txId,
            };

            return AskPeerAndListen<ByteString>(message, MessageMethod.PutState);
        }

        /// <summary>
        /// handleGetState communicates with the peer to fetch the requested state information from the ledger.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="key">key of the asset being consulted from the ledger</param>
        /// <param name="channelId">channel id</param>
        /// <param name="txId">transaction id</param>
        public Task<ByteString> HandleGetState(string collection, string key, string channelId, string txId)
        {
            var payload = new GetState
            {
                Key = key,
                Collection = collection
            };
            ChaincodeMessage message = new ChaincodeMessage()
            {
                Type = ChaincodeMessage.Types.Type.GetState,
                Payload = ByteString.CopyFrom(payload.ToByteArray()),
                ChannelId = channelId,
                Txid = txId,
            };
               
            return AskPeerAndListen<ByteString>(message, MessageMethod.GetState);
        }

        public object ParseResponse(ChaincodeMessage response, MessageMethod messageMethod)
        {
            if (response.Type == ChaincodeMessage.Types.Type.Response)
            {
                switch (messageMethod)
                {
                    case MessageMethod.GetState:
                        return response.Payload;

                    default:
                        return response.Payload;
                }
            }

            if (response.Type == ChaincodeMessage.Types.Type.Error)
            {
                throw new Exception(response.Payload.ToStringUtf8());
            }

            var errorMessage = $"[{response.ChannelId}-{response.Txid}] Received incorrect chaincode " +
                               $"in response to the {messageMethod} call: " +
                               $"type={response.Type}, expecting \"RESPONSE\"";
            throw new Exception(errorMessage);
        }
    }
}
