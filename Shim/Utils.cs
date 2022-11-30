namespace Shim
{
    public enum State
    {
        created,
        established,
        ready
    }

    public enum ResponseCodes
    {
        Ok = 200,
        Error = 500
    }

    public enum MessageMethod
    {
        GetState,
        InvokeChaincode,
        PutState,
        DelState
    }
    public class Parameters : List<string>
    {
        public void AssertCount(int count)
        {
            if (Count != count) throw new Exception($"Incorrect number of arguments. Expecting {count}, got {Count}");
        }
    }

    public class ChaincodeFunctionParameterInformation
    {
        public string Function { get; set; }
        public Parameters Parameters { get; set; } = new Parameters();
    }
}
