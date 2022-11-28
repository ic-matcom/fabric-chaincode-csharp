using Protos;
using Grpc.Core;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Msp;

namespace Shim
{
    public class ChaincodeStub : IChaincodeStub
    {
        private const char MinUnicodeRuneValue = '\u0000';
        private const char MaxUnicodeRuneValue = '\u10ff';
        private const char CompositeKeyNamespace = '\x00';
        private Handler Handler { get; set; }
        private Timestamp TxTimestamp { get; set; }
        private string ChannelId { get; }
        private string TxId { get; }
        private SignedProposal SignedProposal { get; }
        private SerializedIdentity Creator { get; set; }
        public ChaincodeEvent ChaincodeEvent { get; internal set; }

        public IList<string> Args { get; }

        public ChaincodeStub(
            Handler handler,
            string channelId,
            string txId,
            ChaincodeInput chaincodeInput,
            SignedProposal signedProposal
        )
        {
            Handler = handler;
            ChannelId = channelId;
            TxId = txId;
            SignedProposal = signedProposal;
            Args = chaincodeInput.Args.Select(entry => entry.ToStringUtf8()).ToList();
        }

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
        public string GetChannelId()
        {
            return ChannelId;
        }

        public Timestamp GetTxTimestamp()
        {
           return TxTimestamp;
        }

        public SerializedIdentity GetCreator()
        {
            return Creator;
        }
        public Task<ByteString> GetState(string key)
        {
            return Handler.HandleGetState(string.Empty, key, ChannelId, TxId);
        }
        public Task<ByteString> PutState(string key, ByteString value)
        {
            return Handler.HandlePutState(string.Empty, key, value, ChannelId, TxId);
        }
        public Task<ByteString> DeleteState(string key)
        {
            return Handler.HandleDeleteState(string.Empty, key, ChannelId, TxId);
        }
        public string CreateCompositeKey(string objectType, IEnumerable<string> attributes)
        {
            ValidateCompositeKeyAttribute(objectType);

            var compositeKey = CompositeKeyNamespace + objectType + MinUnicodeRuneValue;

            foreach (var attribute in attributes)
            {
                ValidateCompositeKeyAttribute(attribute);
                compositeKey += attribute + MinUnicodeRuneValue;
            }

            return compositeKey;
        }
        public (string str, IList<string> Attributes) SplitCompositeKey(string compositeKey)
        {
            if (string.IsNullOrEmpty(compositeKey) || compositeKey[0] != CompositeKeyNamespace)
                return ("", new List<string>());

            var splitKey = compositeKey.Substring(1).Split(MinUnicodeRuneValue).ToList();
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
        private void ValidateCompositeKeyAttribute(string str)
        {

            if (string.IsNullOrEmpty(str))
                throw new Exception("not a valid utf8 string");

            foreach (var runeValue in str)
            {
                if (runeValue == MinUnicodeRuneValue || runeValue == MaxUnicodeRuneValue)
                {
                    throw new Exception($"{MinUnicodeRuneValue} and {MaxUnicodeRuneValue} are not allowed in the input attribute of a composite key");
                }
            }
        }
    }
}

