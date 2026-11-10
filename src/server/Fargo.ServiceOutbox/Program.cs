using Fargo.Application.Abstractions;
using Fargo.Application.Operations.Outbox;
using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;
using Fargo.ServiceOutbox;
using Fargo.ServiceOutbox.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.AddFargoServiceDefaults();

builder.Services.AddFargoDbContext(builder.Configuration);

// The broker health check is disabled on purpose: while the broker is down,
// events wait in the outbox and the service keeps retrying with backoff.
builder.AddRabbitMQClient("fargo-rabbitmq", settings => settings.DisableHealthChecks = true);

builder.Services.AddFargoRabbitMqEventPublisher(builder.Configuration);

builder.Services.AddFargoOutboxRepository();

builder.Services
    .AddOptions<OutboxOptions>()
    .Bind(builder.Configuration.GetSection(OutboxOptions.SectionName))
    .ValidateDataAnnotations();

builder.Services.TryAddSingleton(TimeProvider.System);

// The publish and purge rules live in the application layer. They own their own
// transaction and claim locks, so they are registered directly, bypassing the
// unit-of-work, transaction and audit decorators of AddFargoCommandHandler.
builder.Services
    .AddScoped<ICommandHandler<PublishOutboxBatchCommand, OutboxBatchResult>, PublishOutboxBatchCommandHandler>();

builder.Services
    .AddScoped<ICommandHandler<PurgeProcessedOutboxCommand, int>, PurgeProcessedOutboxCommandHandler>();

builder.Services.AddHostedService<OutboxRelayService>();

var host = builder.Build();

host.Run();
