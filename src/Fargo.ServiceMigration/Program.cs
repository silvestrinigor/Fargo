using Fargo.Application.Identity;
using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;
using Fargo.ServiceMigration;
using Fargo.ServiceMigration.Security;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<MigrationService>();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(MigrationService.ActivitySourceName));

builder.Services.AddFargoMigrationInfrastructure(builder.Configuration);

builder.Services.AddScoped<ICurrentActor, CurrentUser>();

var host = builder.Build();

host.Run();
