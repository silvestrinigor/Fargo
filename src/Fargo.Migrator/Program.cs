using Fargo.Application.DependencyInjection;
using Fargo.Infrastructure.DependencyInjection;
using Fargo.Migrator.Services;
using Fargo.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddFargoServiceDefaults();

builder.Services.AddHostedService<FargoMigrationService>();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(FargoMigrationService.ActivitySourceName));

builder.Services.AddFargoApplication();

builder.Services.AddFargoInfrastructure(builder.Configuration);

var host = builder.Build();

host.Run();
