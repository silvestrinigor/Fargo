using Fargo.Application.DependencyInjection;
using Fargo.Infrastructure.DependencyInjection;
using Fargo.ServiceDefaults;
using Fargo.ServiceMaintenance.Options;
using Fargo.ServiceMaintenance.Services;

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

var host = builder.Build();

host.Run();