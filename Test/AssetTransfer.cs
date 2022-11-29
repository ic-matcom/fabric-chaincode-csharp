using Google.Protobuf;
using Protos;
using Shim;
using Newtonsoft.Json;
namespace Test
{
    public class Asset
    {
        public Asset(string id, string color, int size, string owner, int appraisedValue)
        {
            Id = id;
            Color = color;
            Size = size;
            Owner = owner;
            AppraisedValue = appraisedValue;
        }


        [JsonProperty("ID")]
        public string Id { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("owner")]
        public string Owner { get; set; }

        [JsonProperty("appraisedValue")]
        public int AppraisedValue { get; set; }
    }
    public class AssetTransfer: IChaincode
    {
        public async Task<Response> Init(IChaincodeStub stub)
        {
            ChaincodeFunctionParameterInformation f = stub.GetFunctionAndParameters();

            try
            {
                return Shim.Shim.Success(await InitLedger(stub, f.Parameters));
            }
            catch (Exception ex)
            {
                return Shim.Shim.Error(ex);
            }
        }

        public async Task<Response> Invoke(IChaincodeStub stub)
        {
            ChaincodeFunctionParameterInformation f = stub.GetFunctionAndParameters();
                
            switch (f.Function)
            {
                case "InitLedger":
                    try
                    {
                        return Shim.Shim.Success(await InitLedger(stub, f.Parameters));
                    }
                    catch (Exception ex)
                    {
                        return Shim.Shim.Error(ex);
                    }
                case "ReadAsset":
                    try
                    {
                        return Shim.Shim.Success(await ReadAsset(stub, f.Parameters));
                    }
                    catch (Exception ex)
                    {
                        return Shim.Shim.Error(ex);
                    }
                case "DeleteAsset":
                    try
                    {
                        return Shim.Shim.Success(await DeleteAsset(stub, f.Parameters));
                    }
                    catch (Exception ex)
                    {
                        return Shim.Shim.Error(ex);
                    }
                case "CreateAsset":
                    try
                    {
                        return Shim.Shim.Success(await CreateAsset(stub, f.Parameters));
                    }
                    catch (Exception ex)
                    {
                        return Shim.Shim.Error(ex);
                    }
                case "UpdateAsset":
                    try
                    {
                        return Shim.Shim.Success(await UpdateAsset(stub, f.Parameters));
                    }
                    catch (Exception ex)
                    {
                        return Shim.Shim.Error(ex);
                    }
                default:
                    return Shim.Shim.Error($"Function {f.Function} does not exist");
            }
        }

        public async Task<ByteString> InitLedger(IChaincodeStub stub, Parameters parameters)
        {
            var assets = new List<Asset>()
            {
                 new Asset("asset1", "blue",5,"Tomoko",  300),
                 new Asset("asset2", "red", 5,"Brad",  400),
                 new Asset("asset3", "green", 10,  "Jin Soo",  500),
                 new Asset("asset4", "yellow", 10, "Max",  600),
                 new Asset("asset5", "black", 15,  "Adriana",  700),
                 new Asset("asset6", "white", 15,  "Michel",  800),
            };
            for (int i = 0; i < assets.Count; i++)
            {
                string jsonString = JsonConvert.SerializeObject(assets[i]);
                await stub.PutState(assets[i].Id, ByteString.CopyFromUtf8(jsonString));

            }

            return ByteString.Empty;
        }

        public async Task<ByteString> CreateAsset(IChaincodeStub stub, Parameters parameters)
        {

            parameters.AssertCount(5);
            string id = parameters[0];
            string color = parameters[1];
            int size = int.Parse(parameters[2]);
            string owner = parameters[3];
            int appraisedValue = int.Parse(parameters[4]);

            var asset = new Asset(id, color, size, owner, appraisedValue);
            string jsonString = JsonConvert.SerializeObject(asset);

            await stub.PutState(id, ByteString.CopyFromUtf8(jsonString));
            
            return ByteString.Empty;
        }

        public async Task<ByteString> ReadAsset(IChaincodeStub stub, Parameters parameters)
        {

            string key = parameters[0];
            var asset =  await stub.GetState(key);
            if (asset == null || asset.Length <= 0) return ByteString.CopyFromUtf8($"Asset {key} does not exist.");

            return asset;
        }
        public async Task<ByteString> DeleteAsset(IChaincodeStub stub, Parameters parameters)
        {

            string key = parameters[0];
            await stub.DeleteState(key); 
            return ByteString.Empty;
        }
        public async Task<ByteString> UpdateAsset(IChaincodeStub stub, Parameters parameters)
        {
            string id = parameters[0];
            string color = parameters[1];
            int size = int.Parse(parameters[2]);
            string owner = parameters[3];
            int appraisedValue = int.Parse(parameters[4]);

            var serializedJson = await stub.GetState(id);

            if (serializedJson == null || serializedJson.Length <= 0)
                return ByteString.CopyFromUtf8($"Asset {id} does not exist.");

            var asset = JsonConvert.DeserializeObject<Asset>(serializedJson.ToStringUtf8());
            
            asset.Id = id;
            asset.Color = color;
            asset.Size = size;  
            asset.Owner = owner;    
            asset.AppraisedValue = appraisedValue;
            
            string jsonString = JsonConvert.SerializeObject(asset);
            await stub.PutState(id, ByteString.CopyFromUtf8(jsonString));
            
            return ByteString.Empty;
        }

    }
}
