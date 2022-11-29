//using System.Threading.Tasks;
//using Newtonsoft.Json;

//namespace Shim
//{
//    public static class ChaincodeStubExtensions
//    {
//        public static async Task<T> GetStateJson<T>(this IChaincodeStub stub, string key)
//            where T : class
//        {
//            return JsonConvert.DeserializeObject<T>(await stub.GetState<string>(key));
//        }

//        public static async Task<T> GetState<T>(this IChaincodeStub stub, string key)
//        {
//            var stateResult = await stub.GetState(key);
//            return stateResult.Convert<T>();
//        }

//        public static async Task<T?> TryGetState<T>(this IChaincodeStub stub, string key)
//            where T : struct
//        {
//            try
//            {
//                var stateResult = await stub.GetState(key);
//                return stateResult.Convert<T>();
//            }
//            catch
//            {
//                return null;
//            }
//        }

//        public static Task<bool> PutStateJson<T>(this IChaincodeStub stub, string key, T value)
//            where T : class
//        {
//            return stub.PutState(key, JsonConvert.SerializeObject(value));
//        }

//    }
//}