using Fargo.Application.DependencyInjection;
using Fargo.Infrastructure.DependencyInjection;
using Fargo.ServiceDefaults;
using Fargo.ServiceSeed.Extensions;
using Fargo.ServiceSeed.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.AddFargoServiceDefaults();

builder.Services.AddFargoSeedOptions(builder.Configuration);

builder.Services.AddFargoApplication();

builder.Services.AddFargoInfrastructure(builder.Configuration);

builder.Services.AddHostedService<FargoSeedService>();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(FargoSeedService.ActivitySourceName));

var host = builder.Build();

host.Run();
