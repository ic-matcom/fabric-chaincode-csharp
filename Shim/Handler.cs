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
    public class Handler
    {
        public State State { get; set; }
        public IAsyncStreamReader<ChaincodeMessage> RequestStream { get; set; }
        public IServerStreamWriter<ChaincodeMessage> ResponseStream { get; set; }

        public IChaincode Chaincode { get; set; }

        public Handler(IAsyncStreamReader<ChaincodeMessage> requestStream, IServerStreamWriter<ChaincodeMessage> responseStream, IChaincode chaincode)
        {
            State = State.created;
            RequestStream = requestStream;
            ResponseStream = responseStream;
            Chaincode = chaincode;
        }

        public void HandleResponse(ChaincodeMessage chaincodeMessage) { }
        public async Task HandleInit(ChaincodeMessage chaincodeMessage) {
            ChaincodeInput input = new ChaincodeInput();
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

        internal Task<ByteString> HandleDeleteState(string empty, string key, string channelId, string txId)
        {
            throw new NotImplementedException();
        }

        internal Task<ByteString> HandlePutState(string empty, string key, ByteString value, string channelId, string txId)
        {
            throw new NotImplementedException();
        }

        internal Task<ByteString> HandleGetState(string empty, string key, string channelId, string txId)
        {
            throw new NotImplementedException();
        }

        public void HandleTransaction(ChaincodeMessage chaincodeMessage) {
        

        }


        public void HandleReady(ChaincodeMessage chaincodeMessage)
        {
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
    }
}
