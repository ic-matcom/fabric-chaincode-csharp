using Protos;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Msp;
using Grpc.Core;

namespace Shim
{

    /// <summary>
    /// IChaincode  must be implemented by all chaincodes. Fabric runs
    ///  the transactions by calling these functions as specified.
    /// </summary>

    public interface IChaincode
    {

        /// <summary>
        /// Init is called during Instantiate transaction after the chaincode container
        /// has been established for the first time, allowing the chaincode to
        /// initialize its internal data.
        /// </summary>
        /// <param name="stub"> Instance of ChaincodeStub to provide fuctions that interact with the ledger</param>

        Task<Response> Init(IChaincodeStub stub);

        /// <summary>
        /// Invoke is called to update or query the ledger in a proposal transaction.
        /// Updated state variables are not committed to the ledger until the
        /// transaction is committed.
        /// </summary>
        /// <param name="stub">Instance of ChaincodeStub to provide fuctions that interact with the ledge</param>
        Task<Response> Invoke(IChaincodeStub stub);
    }



    /// <summary>
    /// IChaincodeStub groups functionalities which are used by deployable chaincode apps to access and
    /// modify their ledgers.
    /// Default implementation can be found in <code>ChaincodeStub</code
    /// </summary>
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
        ChaincodeFunctionParameterInformation GetFunctionAndParameters();
    }

    /// <summary>
    /// IHandler groups functionalities which are used to manage communication between chaincode and peer.
    /// Default implementation can be found in <code>Handler</code>.
    /// </summary>
    public interface IHandler
    {
        public IServerStreamWriter<ChaincodeMessage> ResponseStream { get; }
        object ParseResponse(ChaincodeMessage response, MessageMethod messageMethod);
        Task<ByteString> HandleGetState(string collection, string key, string channelId, string txId);
        Task<ByteString> HandlePutState(string collection, string key, ByteString value, string channelId, string txId);
        Task<ByteString> HandleDeleteState(string collection, string key, string channelId, string txId);
    }

    /// <summary>
    /// IMessageQueue groups functionalities that help handle concurrent transaction proposals.
    /// Default implementation can be found in <code>MessageQueue</code>
    /// </summary>
    public interface IMessageQueue
    {
        Task QueueMessage(QueueMessage queueMessage);
        void HandleMessageResponse(ChaincodeMessage response);
    }
}
