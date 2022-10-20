//using Protos;
//using Queryresult;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Google.Protobuf;
//using Google.Protobuf.Collections;
//using Google.Protobuf.WellKnownTypes;
//// using Thinktecture.HyperledgerFabric.Chaincode.NET.Handler.Iterators;

//namespace Shim
//{

//// IChaincode  must be implemented by all chaincodes. The fabric runs
//// the transactions by calling these functions as specified.
// public interface IChaincode
//    {
//        // Init is called during Instantiate transaction after the chaincode container
//	    // has been established for the first time, allowing the chaincode to
//	    // initialize its internal data
//        Task<Response> Init(IChaincodeStub stub);

//        // Invoke is called to update or query the ledger in a proposal transaction.
//	    // Updated state variables are not committed to the ledger until the
//	    // transaction is committed.
//        Task<Response> Invoke(IChaincodeStub stub);
//    }



//// IChaincodeStub is used by deployable chaincode apps to access and
//// modify their ledgers
//    public interface IChaincodeStub
//    {
//        ChaincodeEvent ChaincodeEvent { get; }
//        string Binding { get; }
//        Timestamp TxTimestamp { get; }
//        DecodedSignedProposal DecodedSignedProposal { get; }
//        MapField<string, ByteString> TransientMap { get; }
//        string ChannelId { get; }
//        string TxId { get; }

//		// Args returns the arguments intended for the chaincode Init and Invoke
//		// as an array of byte arrays.
//        IList<string> Args { get; }
//        SerializedIdentity Creator { get; }
//        ChaincodeFunctionParameterInformation GetFunctionAndParameters();
//        Task<ByteString> GetState(string key);
//        Task<ByteString> PutState(string key, ByteString value);
//        Task<ByteString> DeleteState(string key);
//        Task<StateQueryIterator> GetStateByRange(string startKey, string endKey);
//        Task<StateQueryIterator> GetQueryResult(string query);
//        Task<HistoryQueryIterator> GetHistoryForKey(string key);
//        Task InvokeChaincode(string chaincodeName, IEnumerable<ByteString> args, string channel = "");
//        void SetEvent(string name, ByteString payload);
//        string CreateCompositeKey(string objectType, IEnumerable<string> attributes);
//        (string ObjectType, IList<string> Attributes) SplitCompositeKey(string compositeKey);
//        Task<StateQueryIterator> GetStateByPartialCompositeKey(string objectType, IList<string> attributes);
//        Task<ByteString> GetPrivateData(string collection, string key);
//        Task<ByteString> PutPrivateData(string collection, string key, ByteString value);
//        Task<ByteString> DeletePrivateData(string collection, string key);
//        Task<StateQueryIterator> GetPrivateDataByRange(string collection, string startKey, string endKey);

//        Task<StateQueryIterator> GetPrivateDataByPartialCompositeKey(
//            string collection,
//            string objectType,
//            IList<string> attributes
//        );

//        Task<StateQueryIterator> GetPrivateDataQueryResult(string collection, string query);
//    }
//}



//// ICommonIterator allows a chaincode to check whether any more result
//// to be fetched from an iterator and close it when done.
//public interface ICommonIterator {
//	// HasNext returns true if the range query iterator contains additional keys
//	// and values.
//	bool HasNext();

//	// Close closes the iterator. This should be called when done
//	// reading from the iterator to free up resources.
//	error Close();
//}

//// StateQueryIteratorInterface allows a chaincode to iterate over a set of
//// key/value pairs returned by range and execute query.
//public interface IStateQueryIterator : ICommonIterator {// Inherit HasNext() and Close()
	
//	// Next returns the next key and value in the range and execute query iterator.
//	KV Next();
//}

//// HistoryQueryIteratorInterface allows a chaincode to iterate over a set of
//// key/value pairs returned by a history query.
//public interface IHistoryQueryIterator : ICommonIterator {// Inherit HasNext() and Close()
//	// Next returns the next key and value in the history query iterator.
//	KeyModification Next() ;
//}

//// MockQueryIteratorInterface allows a chaincode to iterate over a set of
//// key/value pairs returned by range query.
//// TODO: Once the execute query and history query are implemented in MockStub,
//// we need to update this interface
//public interface IMockQueryIterator: StateQueryIterator  {
//}
