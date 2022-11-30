# fabric-chaincode-csharp
Hyperledger Fabric Contract and Chaincode implementation for CSharp (C#) https://wiki.hyperledger.org/display/fabric

The fabric-chaincode-csharp has only been tested only in wsl/linux using [dotnet 6](https://learn.microsoft.com/en-us/dotnet/core/install/linux)

## 🚀 Quick Testing

Run the following instructions in terminal:
### Export the environment variables

```bash
cd fabric-chaincode-csharp/
```

Export your chaincode package ID, ex:
```bash
export CHAINCODE_ID=basic_1.0:f3e2ca5115bba71aa2fd16e35722b420cb29c42594f0fdd6814daedbc2130b80
```

Set the chaincode server address:
```bash
export CHAINCODE_SERVER_ADDRESS=127.0.0.1:9999
```

### Build library

```bash
dotnet build
```

### Start chaincode service:
```bash
dotnet run
```