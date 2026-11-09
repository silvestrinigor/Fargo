using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddFargoServiceDefaults();

builder.Services.AddFargoDbContext(builder.Configuration);

// The broker health check is disabled on purpose: while the broker is down,
// events wait in the outbox and the service keeps retrying with backoff.
builder.AddRabbitMQClient("fargo-rabbitmq", settings => settings.DisableHealthChecks = true);

builder.Services.AddFargoRabbitMqEventPublisher(builder.Configuration);

builder.Services.AddFargoOutboxRelay(builder.Configuration);

var host = builder.Build();

host.Run();
