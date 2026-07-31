using Fargo.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var environmentName = builder.Environment.EnvironmentName;

var postgres = builder
    .AddPostgres("fargo-postgres")
    .WithLifetime(ContainerLifetime.Persistent);

var databaseFargo = postgres.AddDatabase("fargo");

var serviceMigrations = builder
    .AddProject<Projects.Fargo_ServiceMigration>("fargo-migration")
    .WithFargoEnvironment(environmentName)
    .WithReference(databaseFargo)
    .WaitFor(databaseFargo);

var serviceSeeds = builder
    .AddProject<Projects.Fargo_ServiceSeed>("fargo-seed")
    .WithFargoEnvironment(environmentName)
    .WithReference(databaseFargo)
    .WithReference(serviceMigrations)
    .WaitForCompletion(serviceMigrations);

var serviceHttAppi = builder
    .AddProject<Projects.Fargo_HttpApi>("fargo-api")
    .WithHttpHealthCheck("/health")
    .WithFargoEnvironment(environmentName)
    .WithReference(databaseFargo)
    .WithReference(serviceMigrations)
    .WithReference(serviceSeeds)
    .WaitForCompletion(serviceMigrations)
    .WaitForCompletion(serviceSeeds);

if (string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase))
{
    builder
        .AddProject<Projects.Fargo_WebPlayground>("fargo-playground")
        .WithExternalHttpEndpoints()
        .WithReference(serviceHttAppi)
        .WaitFor(serviceHttAppi);
}

builder.Build().Run();
