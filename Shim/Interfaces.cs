using Protos;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Msp;

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
}
