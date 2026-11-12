var builder = DistributedApplication.CreateBuilder(args);

var environmentName = builder.Environment.EnvironmentName;

var postgres = builder
.AddPostgres("fargo-postgres")
.WithLifetime(ContainerLifetime.Persistent);

var databaseFargo = postgres.AddDatabase("fargo-database", "fargo");

// Broker for outbound integration events (published by the outbox service).
var rabbitmq = builder
.AddRabbitMQ("fargo-rabbitmq")
.WithManagementPlugin()
.WithLifetime(ContainerLifetime.Persistent);

var serviceMigrations = builder
.AddProject<Projects.Fargo_ServiceMigration>("fargo-migration")
.WithEnvironment("ASPNETCORE_ENVIRONMENT", environmentName)
.WithEnvironment("DOTNET_ENVIRONMENT", environmentName)
.WithReference(databaseFargo)
.WaitFor(databaseFargo);

var serviceInitializer = builder
.AddProject<Projects.Fargo_ServiceInitializer>("fargo-initializer")
.WithEnvironment("ASPNETCORE_ENVIRONMENT", environmentName)
.WithEnvironment("DOTNET_ENVIRONMENT", environmentName)
.WithReference(databaseFargo)
.WithReference(serviceMigrations)
.WaitForCompletion(serviceMigrations);

_ = builder
.AddProject<Projects.Fargo_ServiceHttp>("fargo-api")
.WithHttpHealthCheck("/health")
.WithEnvironment("ASPNETCORE_ENVIRONMENT", environmentName)
.WithEnvironment("DOTNET_ENVIRONMENT", environmentName)
.WithReference(databaseFargo)
.WithReference(serviceMigrations)
.WithReference(serviceInitializer)
.WaitForCompletion(serviceMigrations)
.WaitForCompletion(serviceInitializer);

_ = builder
.AddProject<Projects.Fargo_ServiceOutbox>("fargo-outbox")
.WithEnvironment("ASPNETCORE_ENVIRONMENT", environmentName)
.WithEnvironment("DOTNET_ENVIRONMENT", environmentName)
.WithReference(databaseFargo)
.WithReference(rabbitmq)
.WithReference(serviceMigrations)
.WaitFor(rabbitmq)
.WaitForCompletion(serviceMigrations);

// Housekeeping: deletes expired idempotency keys. Needs only the database, so it waits for neither the broker
// nor the other services.
_ = builder
.AddProject<Projects.Fargo_ServiceMaintenance>("fargo-maintenance")
.WithEnvironment("ASPNETCORE_ENVIRONMENT", environmentName)
.WithEnvironment("DOTNET_ENVIRONMENT", environmentName)
.WithReference(databaseFargo)
.WithReference(serviceMigrations)
.WaitForCompletion(serviceMigrations);

// Real-time push (SignalR). It does not wait for the broker: the hub serves
// connections while the broker is down and its consumer retries until it is up.
_ = builder
.AddProject<Projects.Fargo_ServiceRealtime>("fargo-realtime")
.WithHttpHealthCheck("/health")
.WithEnvironment("ASPNETCORE_ENVIRONMENT", environmentName)
.WithEnvironment("DOTNET_ENVIRONMENT", environmentName)
.WithReference(databaseFargo)
.WithReference(rabbitmq)
.WithReference(serviceMigrations)
.WaitForCompletion(serviceMigrations);

builder.Build().Run();
