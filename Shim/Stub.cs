using Protos;
using Grpc.Core;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Msp;
using Microsoft.Extensions.Logging;

namespace Shim
{
    /// <summary>
    /// Class <c>ChaincodeStub</c> encapsulates the APIs between the chaincode implementation and the Fabric peer.
    /// </summary>
    public class ChaincodeStub : IChaincodeStub
    {
        private Handler _handler { get; set; }
        private Timestamp _txTimestamp { get; set; }
        private string _channelId { get; }
        private string _txId { get; }
        private SerializedIdentity _creator { get; set; }
        public ChaincodeEvent ChaincodeEvent { get; internal set; }

        public IList<string> Args { get; }

        //public ILogger _logger { get; set; }

        public ChaincodeStub(
            Handler handler,
            string channelId,
            string txId,
            ChaincodeInput chaincodeInput
            //ILogger _logger
        )
        {
            _handler = handler;
            _channelId = channelId;
            _txId = txId;
            Args = chaincodeInput.Args.Select(entry => entry.ToStringUtf8()).ToList();
        }

        /// <summary>
        /// Get name and parameters of the function to invoke.
        /// 
        /// </summary>
        /// <returns>Returns an object ChaincodeFunctionParameterInformation containing the chaincode function name to invoke,
        /// and the array of arguments to pass to the target function </returns>
        public ChaincodeFunctionParameterInformation GetFunctionAndParameters()
        {
            if (Args.Count < 1) return null;

            var result = new ChaincodeFunctionParameterInformation
            {
                Function = Args.First()
            };

            result.Parameters.AddRange(Args.Skip(1));

            return result;
        }

        /// <summary>
        /// Returns the channel ID for the proposal for chaincode to process.
        /// </summary>
        public string GetChannelId()
        {
            return _channelId;
        }

        /// <summary>
        /// Indicates the client's timestamp, and will have the same value across all endorsers.
        /// </summary>
        /// <returns>Returns the timestamp when the transaction was created.</returns>
        public Timestamp GetTxTimestamp()
        {
           return _txTimestamp;
        }

        /// <summary>
        /// Returns the identity object of the chaincode invocation's submitter
        /// </summary>
        public SerializedIdentity GetCreator()
        {
            return _creator;
        }

        /// <summary>
        /// Retrieves the current value of the state variable <code>key</code>
        /// </summary>
        /// <param name="key">State variable key to retrieve from the state store</param>
        public Task<ByteString> GetState(string key)
        {
            return _handler.HandleGetState(string.Empty, key, _channelId, _txId);
        }

        /// <summary>
        /// Writes the state variable <code>key</code> of value <code>value</code>
        /// to the state store.If the variable already exists, the value will be
        /// overwritten.
        /// </summary>
        /// <param name="key">State variable key to set the value for</param>
        /// <param name="value">State variable value</param>
        public Task<ByteString> PutState(string key, ByteString value)
        {
            return _handler.HandlePutState(string.Empty, key, value, _channelId, _txId);
        }

        /// <summary>
        /// Deletes the state variable <code>key</code> from the state store.
        /// </summary>
        /// <param name="key">State variable key to delete from the state store</param>
        public Task<ByteString> DeleteState(string key)
        {
            return _handler.HandleDeleteState(string.Empty, key, _channelId, _txId);
        }

        /// <summary>
        /// Creates a composite key by combining the objectType string and the given `attributes` to form a composite
        /// key.The objectType and attributes are expected to have only valid utf8 strings and should not contain
        /// U+0000 (nil byte) and U+10FFFF(biggest and unallocated code point). The resulting composite key can be
        /// used as the key in <code>putState()</code>.
        /// </summary>
        /// <param name="objectType">A string used as the prefix of the resulting key</param>
        /// <param name="attributes">List of attribute values to concatenate into the key</param>
        /// <returns></returns>
        public string CreateCompositeKey(string objectType, IEnumerable<string> attributes)
        {
            ValidateCompositeKeyAttribute(objectType);

            var compositeKey = Constants.CompositeKeyNamespace + objectType + Constants.MinUnicodeRuneValue;

            foreach (var attribute in attributes)
            {
                ValidateCompositeKeyAttribute(attribute);
                compositeKey += attribute + Constants.MinUnicodeRuneValue;
            }

            return compositeKey;
        }

        /// <summary>
        /// Splits the specified key into attributes on which the composite key was formed.
        /// Composite keys found during range queries or partial composite key queries can
        /// therefore be split into their original composite parts, essentially recovering
        /// the values of the attributes.
        /// </summary>
        /// <param name="compositeKey">The composite key to split</param>
        /// <returns></returns>
        public (string str, IList<string> Attributes) SplitCompositeKey(string compositeKey)
        {
            if (string.IsNullOrEmpty(compositeKey) || compositeKey[0] != Constants.CompositeKeyNamespace)
                return ("", new List<string>());

            var splitKey = compositeKey.Substring(1).Split(Constants.MinUnicodeRuneValue).ToList();
            string str = "";
            var attributes = new List<string>();

            if (splitKey.Count > 0 && !string.IsNullOrEmpty(splitKey[0]))
            {
                str = splitKey[0];
                splitKey.RemoveAt(splitKey.Count - 1);

                if (splitKey.Count > 1)
                {
                    splitKey.RemoveAt(0);
                    attributes = splitKey;
                }
            }

            return (str, attributes);
        }

        /// <summary>
        /// Validate attribute for unallowed characters 
        /// </summary>
        /// <param name="str">attribute value</param>
        private void ValidateCompositeKeyAttribute(string str)
        {
            if (string.IsNullOrEmpty(str))
                throw new Exception("not a valid utf8 string");

            foreach (var runeValue in str)
            {
                if (runeValue == Constants.MinUnicodeRuneValue || runeValue == Constants.MaxUnicodeRuneValue)
                {
                    throw new Exception($"{Constants.MinUnicodeRuneValue} and {Constants.MaxUnicodeRuneValue} are not allowed in the input attribute of a composite key");
                }
            }
        }
    }
}

