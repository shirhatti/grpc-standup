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

## Generate Client Certificate

```powershell
# Generate root cert
$cert = New-SelfSignedCertificate -Type Custom -KeySpec Signature `
>> -Subject "CN=gRPCRootCert" -KeyExportPolicy Exportable `
>> -HashAlgorithm sha256 -KeyLength 2048 `
>> -CertStoreLocation "Cert:\CurrentUser\My" -KeyUsageProperty Sign -KeyUsage CertSign

# Make sure to trust this root cert

# Generate child cert with Client Authentication OID
New-SelfSignedCertificate -Type Custom -DnsName P2SChildCert -KeySpec Signature `
>> -Subject "CN=P2SChildCert" -KeyExportPolicy Exportable `
>> -HashAlgorithm sha256 -KeyLength 2048 `
>> -CertStoreLocation "Cert:\CurrentUser\My" `
>> -Signer $cert -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2")
```

