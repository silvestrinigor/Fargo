var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Fargo_HttpApi>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Fargo_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.AddProject<Projects.Fargo_Web>("fargo-web");

builder.Build().Run();
