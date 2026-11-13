using Fargo.Application.DependencyInjection;
using Fargo.Infrastructure.DependencyInjection;
using Fargo.Maintenance.Options;
using Fargo.Maintenance.Services;
using Fargo.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddFargoServiceDefaults();

builder.Services.AddFargoApplication();

builder.Services.AddFargoInfrastructure(builder.Configuration);

builder.Services
.AddOptions<IdempotencyKeyPurgeOptions>()
.Bind(builder.Configuration.GetSection(IdempotencyKeyPurgeOptions.SectionName))
.ValidateDataAnnotations()
.ValidateOnStart();

builder.Services.AddHostedService<IdempotencyKeyPurgeService>();

builder.Services
.AddOptions<OutboxPurgeOptions>()
.Bind(builder.Configuration.GetSection(OutboxPurgeOptions.SectionName))
.ValidateDataAnnotations()
.ValidateOnStart();

builder.Services.AddHostedService<OutboxPurgeService>();

var host = builder.Build();

host.Run();
