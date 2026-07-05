using Fargo.Application.Identity;
using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;
using Fargo.ServiceMigration;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<MigrationService>();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(MigrationService.ActivitySourceName));

builder.Services.AddFargoMigrationInfrastructure(builder.Configuration);

builder.Services.AddScoped<ICurrentActor, CurrentActorEmpty>();

var host = builder.Build();

host.Run();
