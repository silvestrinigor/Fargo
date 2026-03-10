var builder = DistributedApplication.CreateBuilder(args);

var sqlserver = builder
    .AddSqlServer("sqlserver")
    .WithImage("mssql/server")
    .WithImageTag("2022-latest")
    .WithLifetime(ContainerLifetime.Session);

var fargodb = sqlserver.AddDatabase("fargo");

var migrations = builder
    .AddProject<Projects.Fargo_MigrationService>("migrations")
    .WithReference(fargodb)
    .WaitFor(fargodb);

builder
    .AddProject<Projects.Fargo_SeedService>("seeds")
    .WithReference(fargodb)
    .WithReference(migrations)
    .WaitForCompletion(migrations);

var apiService = builder
    .AddProject<Projects.Fargo_HttpApi>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(fargodb)
    .WithReference(migrations)
    .WaitForCompletion(migrations);

builder
    .AddProject<Projects.Fargo_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();