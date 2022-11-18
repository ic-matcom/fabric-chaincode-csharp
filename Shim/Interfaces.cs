using Protos;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Msp;
using Grpc.Core;

namespace Shim
{

    // IChaincode  must be implemented by all chaincodes. The fabric runs
    // the transactions by calling these functions as specified.
    public interface IChaincode
    {
        // Init is called during Instantiate transaction after the chaincode container
        // has been established for the first time, allowing the chaincode to
        // initialize its internal data
        Task<Response> Init(IChaincodeStub stub);

        // Invoke is called to update or query the ledger in a proposal transaction.
        // Updated state variables are not committed to the ledger until the
        // transaction is committed.
        Task<Response> Invoke(IChaincodeStub stub);
    }



    // IChaincodeStub is used by deployable chaincode apps to access and
    // modify their ledgers
    public interface IChaincodeStub
    {
        string GetChannelId();
        Timestamp GetTxTimestamp();
        SerializedIdentity GetCreator();
        Task<ByteString> GetState(string key);
        Task<ByteString> PutState(string key, ByteString value);
        Task<ByteString> DeleteState(string key);
        string CreateCompositeKey(string str, IEnumerable<string> attributes);
        (string str, IList<string> Attributes) SplitCompositeKey(string compositeKey);
    }
    public interface IHandler
    {
        public IServerStreamWriter<ChaincodeMessage> ResponseStream { get; }
        object ParseResponse(ChaincodeMessage response, MessageMethod messageMethod);
        Task<ByteString> HandleGetState(string collection, string key, string channelId, string txId);
        Task<ByteString> HandlePutState(string collection, string key, ByteString value, string channelId, string txId);
        Task<ByteString> HandleDeleteState(string collection, string key, string channelId, string txId);


        //Task<Response> HandleInvokeChaincode(
        //    string chaincodeName,
        //    IEnumerable<ByteString> args,
        //    string channelId,
        //    string txId
        //);
    }

    public interface IMessageQueue
    {
        Task QueueMessage(QueueMessage queueMessage);
        void HandleMessageResponse(ChaincodeMessage response);
    }

    public interface IMessageQueueFactory
    {
        IMessageQueue Create(IHandler handler);
    }
}
