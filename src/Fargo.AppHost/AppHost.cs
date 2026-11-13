var builder = DistributedApplication.CreateBuilder(args);

var environmentName = builder.Environment.EnvironmentName;

var postgres = builder
.AddPostgres("fargo-postgres")
.WithLifetime(ContainerLifetime.Persistent);

var databaseFargo = postgres.AddDatabase("fargo-database", "fargo");

// Broker for outbound integration events (published by the event publisher service).
var rabbitmq = builder
.AddRabbitMQ("fargo-rabbitmq")
.WithManagementPlugin()
.WithLifetime(ContainerLifetime.Persistent);

var migrator = builder
.AddProject<Projects.Fargo_Migrator>("fargo-migration")
.WithEnvironment("ASPNETCORE_ENVIRONMENT", environmentName)
.WithEnvironment("DOTNET_ENVIRONMENT", environmentName)
.WithReference(databaseFargo)
.WaitFor(databaseFargo);

var initializer = builder
.AddProject<Projects.Fargo_Initializer>("fargo-initializer")
.WithEnvironment("ASPNETCORE_ENVIRONMENT", environmentName)
.WithEnvironment("DOTNET_ENVIRONMENT", environmentName)
.WithReference(databaseFargo)
.WithReference(migrator)
.WaitForCompletion(migrator);

_ = builder
.AddProject<Projects.Fargo_Api>("fargo-api")
.WithHttpHealthCheck("/health")
.WithEnvironment("ASPNETCORE_ENVIRONMENT", environmentName)
.WithEnvironment("DOTNET_ENVIRONMENT", environmentName)
.WithReference(databaseFargo)
.WithReference(migrator)
.WithReference(initializer)
.WaitForCompletion(migrator)
.WaitForCompletion(initializer);

_ = builder
.AddProject<Projects.Fargo_OutboxRelay>("fargo-event-publisher")
.WithEnvironment("ASPNETCORE_ENVIRONMENT", environmentName)
.WithEnvironment("DOTNET_ENVIRONMENT", environmentName)
.WithReference(databaseFargo)
.WithReference(rabbitmq)
.WithReference(migrator)
.WaitFor(rabbitmq)
.WaitForCompletion(migrator);

// Housekeeping: deletes expired idempotency keys. Needs only the database, so it waits for neither the broker
// nor the other services.
_ = builder
.AddProject<Projects.Fargo_Maintenance>("fargo-maintenance")
.WithEnvironment("ASPNETCORE_ENVIRONMENT", environmentName)
.WithEnvironment("DOTNET_ENVIRONMENT", environmentName)
.WithReference(databaseFargo)
.WithReference(migrator)
.WaitForCompletion(migrator);

// Real-time push (SignalR). It does not wait for the broker: the hub serves
// connections while the broker is down and its consumer retries until it is up.
_ = builder
.AddProject<Projects.Fargo_Realtime>("fargo-realtime")
.WithHttpHealthCheck("/health")
.WithEnvironment("ASPNETCORE_ENVIRONMENT", environmentName)
.WithEnvironment("DOTNET_ENVIRONMENT", environmentName)
.WithReference(databaseFargo)
.WithReference(rabbitmq)
.WithReference(migrator)
.WaitForCompletion(migrator);

builder.Build().Run();
