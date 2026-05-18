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
    .WithReference(fargodb)
    .WaitFor(fargodb);

builder
    .AddProject<Projects.Fargo_SeedService>("seeds")
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
    .WaitForCompletion(migrations);

var grpcApi = builder
    .AddProject<Projects.Fargo_GrpcApi>("grpcservice")
    .WithHttpHealthCheck("/health")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", environmentName)
    .WithEnvironment("DOTNET_ENVIRONMENT", environmentName)
    .WithReference(fargodb)
    .WithReference(migrations)
    .WaitForCompletion(migrations);

builder
    .AddProject<Projects.Fargo_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(httpApi)
    .WithReference(grpcApi)
    .WaitFor(httpApi);

builder.Build().Run();
