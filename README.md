# grpc-standup
A sample gRPC project for the ASP.NET Community Standup

## How to run this sample

```powershell
# Install the version of dotnet pinned to in the global.json
.\build\get-dotnet.ps1

# Source the activate script to set the right dotnet on your PATH
. .\activate.ps1

# Launch VS
.\startvs.cmd
```

## Enable Server interceptor

Modify `ConfigureServices()` in `/Standup/Startup.cs`

```csharp
services.AddGrpc(options =>
{
    options.Interceptors.Add<MaxStreamingRequestTimeoutInterceptor>(TimeSpan.FromSeconds(10));
});
```
