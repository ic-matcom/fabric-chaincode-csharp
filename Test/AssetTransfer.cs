using Google.Protobuf;
using Protos;
using Shim;
using System.Text.Json;

namespace Test
{
    public class Asset
    {
        public Asset(string id, string name, string color, string size, string owner)
        {
            ID = id;
            NAME = name;
            COLOR = color;
            SIZE = size;
            OWNER = owner;
            // id = id;
            // name = name;
            // color = color;
            // size = size;
            // owner = owner;
        }
        public string ID { get; set; }
        public string NAME { get; set; }
        public string COLOR { get; set; }
        public string SIZE { get; set; }
        public string OWNER { get; set; }
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
                    throw new Exception("Function does not exist");
            }
        }

        public async Task<ByteString> InitLedger(IChaincodeStub stub, Parameters parameters)
        {
            var assets = new List<Asset>()
            {
                // new Asset("Asset0"),
                // new Asset("Asset1"),
                // new Asset("Asset2"),
                // new Asset("Asset3"),
                // new Asset("Asset4"),
                // new Asset("Asset5"),
            };
            for (int i = 0; i < assets.Count; i++)
            {
                string jsonString = JsonSerializer.Serialize(assets[i]);
                await stub.PutState($"key{i}", ByteString.CopyFromUtf8(jsonString));

            }

            return ByteString.Empty;
        }

        public async Task<ByteString> CreateAsset(IChaincodeStub stub, Parameters parameters)
        {

            parameters.AssertCount(5);
            string id = parameters[0];
            string name = parameters[1];
            string color = parameters[2];
            string size = parameters[3];
            string owner = parameters[4];

            var asset = new Asset(id, name, color, size, owner);
            string jsonString = JsonSerializer.Serialize(asset);

            await stub.PutState(id, ByteString.CopyFromUtf8(jsonString));
            
            return ByteString.Empty;
        }

        public async Task<ByteString> ReadAsset(IChaincodeStub stub, Parameters parameters)
        {

            string key = parameters[0];
            var asset =  await stub.GetState(key);
            if (asset == null || asset.Length <= 0) throw new Exception($"Asset {key} does not exist.");

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
            string key = parameters[0];
            string value = parameters[1];

            var serializedJson = await stub.GetState(key);
            var asset = JsonSerializer.Deserialize<Asset>(serializedJson.ToStringUtf8());
            asset.ID = value;
            
            string jsonString = JsonSerializer.Serialize(asset);
            var updatedAsset = await stub.PutState(key, ByteString.CopyFromUtf8(jsonString));
            
            return ByteString.Empty;
        }

    }
}
