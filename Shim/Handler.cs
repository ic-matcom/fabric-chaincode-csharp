using Protos;
using Grpc.Core;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Shim
{
    public enum State
    {
        created,
        established,
        ready
    }
    public class Handler: IHandler
    {
        public State State { get; private set; }
        public IAsyncStreamReader<ChaincodeMessage> RequestStream { get; set; }
        public IServerStreamWriter<ChaincodeMessage> ResponseStream { get;}

        private IMessageQueue _messageQueue;

        public IChaincode Chaincode { get; set; }

        public Handler(IAsyncStreamReader<ChaincodeMessage> requestStream, IServerStreamWriter<ChaincodeMessage> responseStream, IChaincode chaincode)
        {
            State = State.created;
            RequestStream = requestStream;
            ResponseStream = responseStream;
            Chaincode = chaincode;
            _messageQueue = new MessageQueue(this);
        }

        public void HandleMessage(ChaincodeMessage message)
        {
            Console.WriteLine("HANDLE MESSAGE");
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
                default: break; //TODO: Send error message
            }
        }

        public void HandleEstablished(ChaincodeMessage message)
        {
            Console.WriteLine("HANDLE ESTABLISHED");
            if (message.Type != ChaincodeMessage.Types.Type.Ready)
                throw new Exception($"{message.Txid} Chaincode cannot handle message {message.Type} while in state {State}");

            State = State.ready;
        }

        public void HandleCreated(ChaincodeMessage message)
        {
            Console.WriteLine("HANDLE CREATED");
            if (message.Type != ChaincodeMessage.Types.Type.Registered)
                throw new Exception($"{message.Txid} Chaincode cannot handle message {message.Type} while in state {State}");
            
            State = State.established;
        }

        public async Task HandleTransaction(ChaincodeMessage chaincodeMessage)
        {
            Console.WriteLine($"HANDLE TRANSACTION: {chaincodeMessage.Txid}, PROPOSAL: {chaincodeMessage.Proposal}");
            ChaincodeInput input;
            ChaincodeMessage errorMessage;

            try
            {
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

            Console.WriteLine($"[{chaincodeMessage.ChannelId}-{chaincodeMessage.Txid}] Calling chaincode INVOKE" +
                                    "succeeded. Sending COMPLETED message back to peer");
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
        public async void HandleResponse(ChaincodeMessage chaincodeMessage) {
            
        }


        
        public async void HandleInit(ChaincodeMessage chaincodeMessage) {
            Console.WriteLine("HANDLE INIT");
            ChaincodeInput input;
            ChaincodeMessage errorMessage;

            try
            {
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

            Console.WriteLine($"[{chaincodeMessage.ChaincodeEvent}]-{chaincodeMessage.Txid} Calling chaincode INIT, " +
                                   $"response status {response.Status}");

            if (response.Status >= 500)
            {
                Console.WriteLine($"[{chaincodeMessage.ChannelId}-{chaincodeMessage.Txid}] Calling chaincode INIT " +
                                 $"returned error response {response.Message}. " +
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
            Console.WriteLine($"[{chaincodeMessage.ChannelId}-{chaincodeMessage.Txid}] Calling chaincode INIT" +
                                    "succeeded. Sending COMPLETED message back to peer");
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
        public void HandleReady(ChaincodeMessage chaincodeMessage)
        {
            Console.WriteLine($"HANDLE READY, CHAINCODE MSG TYPE: {chaincodeMessage.Type}");
            switch (chaincodeMessage.Type)
            {
                case ChaincodeMessage.Types.Type.Response:
                    HandleResponse(chaincodeMessage);
                    break;

                case ChaincodeMessage.Types.Type.Init:
                    HandleInit(chaincodeMessage);
                    break;

                case ChaincodeMessage.Types.Type.Transaction:
                    HandleTransaction(chaincodeMessage);
                    break;
            }
        }
        private Task<T> AskPeerAndListen<T>(ChaincodeMessage message, MessageMethod method)
        {
            Console.WriteLine("ASK PEER AND LISTEN");
            var taskCompletionSource = new TaskCompletionSource<T>();

            var queueMessage = new QueueMessage<T>(message, method, taskCompletionSource);
            _messageQueue.QueueMessage(queueMessage);

            return taskCompletionSource.Task;
        }

        public Task<ByteString> HandleDeleteState(string collection, string key, string channelId, string txId)
        {
            Console.WriteLine("HANDLE DELETE STATE");
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

            return AskPeerAndListen<ByteString>(message, MessageMethod.GetState);
        }

        public Task<ByteString> HandlePutState(string collection, string key, ByteString value, string channelId, string txId)
        {
            Console.WriteLine("HANDLE PUT STATE");
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

            return AskPeerAndListen<ByteString>(message, MessageMethod.GetState);
        }

        public Task<ByteString> HandleGetState(string collection, string key, string channelId, string txId)
        {
            Console.WriteLine("HANDLE GET STATE");
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
            throw new NotImplementedException();
        }
    }
}
