using Aspire.Hosting.Dapr;
using Google.Protobuf.WellKnownTypes;
using static System.Net.Mime.MediaTypeNames;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDapr();
var DaprStateStore = builder.AddDaprStateStore("statestore");
var apiService= builder.AddProject<Projects.CBTW_TEST_ApiService>("cbtw-test-api")
    .WithEnvironment("DAPR_STATE_STORE_NAME", "statestore")
    .WithEnvironment("ZipCodeApiKey", "3NQEC3pmEhR5U91a7keGKXOw7wOdE5vTX2BAUkv3jRMTyjh4Kjs43urmmcadRDYt")
    .WithEnvironment("AZURE_CLIENT_ID", builder.Configuration["AZURE_CLIENT_ID"])
    .WithReference(DaprStateStore)
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "cbtw-test-api-dapr",
        DaprHttpPort = 64020,
        DaprGrpcPort = 64021,
        MetricsPort = 64019,
        DaprHttpMaxRequestSize = 64,
        EnableApiLogging = true,
        LogLevel = "debug"
    });

builder.AddProject<Projects.CBTW_TEST_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
