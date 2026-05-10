var builder = DistributedApplication.CreateBuilder(args);

var sqlserver = builder
    .AddSqlServer("sqlserver")
    .WithImage("mssql/server")
    .WithImageTag("2022-latest")
    .WithLifetime(ContainerLifetime.Persistent);

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

var httpApi = builder
    .AddProject<Projects.Fargo_HttpApi>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(fargodb)
    .WithReference(migrations)
    .WaitForCompletion(migrations);

builder
    .AddProject<Projects.Fargo_GrpcApi>("grpcservice")
    .WithHttpHealthCheck("/health")
    .WithReference(fargodb)
    .WithReference(migrations)
    .WaitForCompletion(migrations);

builder
    .AddProject<Projects.Fargo_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(httpApi)
    .WaitFor(httpApi);

builder.Build().Run();
