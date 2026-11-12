using Fargo.Application.DependencyInjection;
using Fargo.Infrastructure.DependencyInjection;
using Fargo.ServiceDefaults;
using Fargo.ServiceEventPublisher.Options;
using Fargo.ServiceEventPublisher.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.AddFargoServiceDefaults();

builder.Services.AddFargoApplication();

builder.Services.AddFargoInfrastructure(builder.Configuration);

// The broker health check is disabled on purpose: while the broker is down,
// events wait in the outbox and the service keeps retrying with backoff.
builder.AddRabbitMQClient("fargo-rabbitmq", settings => settings.DisableHealthChecks = true);

builder.Services
.AddOptions<OutboxOptions>()
.Bind(builder.Configuration.GetSection(OutboxOptions.SectionName))
.ValidateDataAnnotations();

builder.Services.AddHostedService<OutboxRelayService>();

var host = builder.Build();

host.Run();
