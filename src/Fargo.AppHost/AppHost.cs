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

_ = builder
.AddProject<Projects.Fargo_Http>("fargo-api")
.WithHttpHealthCheck("/health")
.WithFargoEnvironment(environmentName)
.WithReference(databaseFargo)
.WithReference(serviceMigrations)
.WithReference(serviceSeeds)
.WaitForCompletion(serviceMigrations)
.WaitForCompletion(serviceSeeds);

_ = builder
.AddProject<Projects.Fargo_Grpc>("fargo-grpc")
.WithFargoEnvironment(environmentName)
.WithReference(databaseFargo)
.WithReference(serviceMigrations)
.WithReference(serviceSeeds)
.WaitForCompletion(serviceMigrations)
.WaitForCompletion(serviceSeeds);

builder.Build().Run();
