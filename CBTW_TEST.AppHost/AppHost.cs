using Aspire.Hosting.Dapr;
using Google.Protobuf.WellKnownTypes;
using static System.Net.Mime.MediaTypeNames;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDapr();
var DaprStateStore = builder.AddDaprStateStore("statestore");
var apiService= builder.AddProject<Projects.CBTW_TEST_ApiService>("cbtw-test-api")
    .WithEnvironment("DAPR_STATE_STORE_NAME", "statestore")
    .WithEnvironment("GEMINI_API_KEY", builder.Configuration["GEMINI_API_KEY"])
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
