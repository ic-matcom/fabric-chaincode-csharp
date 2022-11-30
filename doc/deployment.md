## 🚀 Deployment

### Overview
This document explains how to generate gRPC code for C# from .proto files. 
For more detail consult [gRPC Documentation](https://lisafc.github.io/grpc.github.io/docs/quickstart/csharp.html)
### DOC dev

#### Generate gRPC code
The Grpc.Tools NuGet package contains the protoc and protobuf C# plugin binaries you will need to generate the code.

##### Obtaining the Grpc.Tools NuGet package

Using Visual Studio
This example project already depends on the Grpc.Tools.1.8.x NuGet package, so it should be included in `examples/csharp/helloworld/packages` when the Greeter.sln solution is built from your IDE, or when you restore packages via /path/to/nuget restore on the command line.

If you have a NuGet client that is not at version 2.12
```bash
$ mkdir packages && cd packages
$ /path/to/nuget install Grpc.Tools
```

If you have a NuGet client that is at version 2.12
NuGet 2.12 does not install the files from the Grpc.Tools package necessary on Linux and OS X. Without changing the version of NuGet that you’re using, a possible workaround to obtaining the binaries included in the Grpc.Tools package is by simply downloading the NuGet package and unzipping without a NuGet client, as follows. From your example directory:

```bash 
$ temp_dir=packages/Grpc.Tools.1.8.x/tmp
$ curl_url=https://www.nuget.org/api/v2/package/Grpc.Tools/
$ mkdir -p $temp_dir && cd $temp_dir && curl -sL $curl_url > tmp.zip; unzip tmp.zip && cd .. && cp -r tmp/tools . && rm -rf tmp && cd ../..
```
Commands to generate the gRPC code
Note that you may have to change the platform_architecture directory names (e.g. windows_x86, linux_x64) in the commands below based on your environment.

Note that you may also have to change the permissions of the protoc and protobuf binaries in the Grpc.Tools package under `examples/csharp/helloworld/packages` to executable in order to run the commands below.

From the `examples/csharp/helloworld directory`, or the `examples/csharp/helloworld-from-cli` directory if using the .NET Core SDK:

Windows

```bash 
> packages\Grpc.Tools.1.8.x\tools\windows_x86\protoc.exe -I../../protos --csharp_out Greeter --grpc_out Greeter ../../protos/helloworld.proto --plugin=protoc-gen-grpc=packages/Grpc.Tools.1.8.x/tools/windows_x86/grpc_csharp_plugin.exe
```
Linux (or OS X by using macosx_x64 directory)

```bash
$ packages/Grpc.Tools.1.8.x/tools/linux_x64/protoc -I../../protos --csharp_out Greeter --grpc_out Greeter ../../protos/helloworld.proto --plugin=protoc-gen-grpc=packages/Grpc.Tools.1.8.x/tools/linux_x64/grpc_csharp_plugin
```

Running the appropriate command for your OS regenerates the following files in the directory:
- Greeter/Helloworld.cs contains all the protocol buffer code to populate, serialize, and retrieve our request and response message types
- Greeter/HelloworldGrpc.cs provides generated client and server classes, including:
    -- an abstract class Greeter.GreeterBase to inherit from when defining Greeter service implementations
    -- a class Greeter.GreeterClient that can be used to access remote Greeter instances

https://github.com/grpc/grpc-dotnet/blob/master/examples
https://binodmahto.medium.com/streaming-with-grpc-on-net-34a57be520a1
https://www.mail-archive.com/grpc-io@googlegroups.com/msg05152.html