using Aspire.Hosting.Dapr;
using Google.Protobuf.WellKnownTypes;
using static System.Net.Mime.MediaTypeNames;

var builder = DistributedApplication.CreateBuilder(args);

var apiService= builder.AddProject<Projects.CBTW_TEST_ApiService>("apiservice")
    .WithEnvironment("DAPR_STATE_STORE_NAME", "statestore")
    .WithEnvironment("GEMINI_API_KEY", builder.Configuration["GEMINI_API_KEY"]);

builder.AddProject<Projects.CBTW_TEST_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
