var builder = DistributedApplication.CreateBuilder(args);

var environmentName = builder.Environment.EnvironmentName;

var sqlserver = builder
    .AddSqlServer("sqlserver")
    .WithImage("mssql/server")
    .WithImageTag("2022-latest")
    .WithLifetime(ContainerLifetime.Persistent);

var fargodb = sqlserver.AddDatabase("fargo");

var migrations = builder
    .AddProject<Projects.Fargo_ServiceMigration>("migrations")
    .WithEnvironment("DOTNET_ENVIRONMENT", environmentName)
    .WithReference(fargodb)
    .WaitFor(fargodb);

var seeds = builder
    .AddProject<Projects.Fargo_ServiceSeed>("seeds")
    .WithEnvironment("DOTNET_ENVIRONMENT", environmentName)
    .WithReference(fargodb)
    .WithReference(migrations)
    .WaitForCompletion(migrations);

var httpApi = builder
    .AddProject<Projects.Fargo_HttpApi>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", environmentName)
    .WithEnvironment("DOTNET_ENVIRONMENT", environmentName)
    .WithReference(fargodb)
    .WithReference(migrations)
    .WithReference(seeds)
    .WaitForCompletion(migrations)
    .WaitForCompletion(seeds);

builder
    .AddProject<Projects.Fargo_WebIdentity>("identityfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(httpApi)
    .WithEnvironment("FargoHttpApi__BaseAddress", httpApi.GetEndpoint("http"))
    .WaitFor(httpApi);

if (string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase))
{
    builder
        .AddProject<Projects.Fargo_WebPlayground>("playgroundfrontend")
        .WithExternalHttpEndpoints()
        .WithReference(httpApi)
        .WithEnvironment("FargoHttpApi__BaseAddress", httpApi.GetEndpoint("http"))
        .WaitFor(httpApi);
}

builder.Build().Run();
