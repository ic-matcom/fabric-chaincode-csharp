# fabric-chaincode-csharp
Hyperledger Fabric Contract and Chaincode implementation for CSharp (C#) https://wiki.hyperledger.org/display/fabric


## Quick Testing

This library has only been tested only in linux with dotnet 6
```bash
dotnet build
```

### Export the environment variables

Export the chaincode package ID, ex:
```bash
export CHAINCODE_ID=basic_1.0:f3e2ca5115bba71aa2fd16e35722b420cb29c42594f0fdd6814daedbc2130b80
```

Set the chaincode server address:
```bash
export CHAINCODE_SERVER_ADDRESS=127.0.0.1:9999
```

### And start the chaincode service:
```bash
dotnet run
```