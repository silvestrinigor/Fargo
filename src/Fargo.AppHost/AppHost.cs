var builder = DistributedApplication.CreateBuilder(args);

var sqlserver = builder
    .AddSqlServer("sqlserver")
    .WithImage("mssql/server")
    .WithImageTag("2022-latest")
    .WithLifetime(ContainerLifetime.Session);

var fargodb = sqlserver.AddDatabase("fargo");
var environmentName = builder.Environment.EnvironmentName;

var migrations = builder
    .AddProject<Projects.Fargo_MigrationService>("migrations")
    .WithEnvironment("DOTNET_ENVIRONMENT", environmentName)
    .WithReference(fargodb)
    .WaitFor(fargodb);

var seeds = builder
    .AddProject<Projects.Fargo_SeedService>("seeds")
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
    .AddProject<Projects.Fargo_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(httpApi)
    .WaitFor(httpApi);

builder
    .AddProject<Projects.Fargo_Web_Identity>("identityfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(httpApi)
    .WithEnvironment("FargoHttpApi__BaseAddress", httpApi.GetEndpoint("http"))
    .WaitFor(httpApi);

builder.Build().Run();
