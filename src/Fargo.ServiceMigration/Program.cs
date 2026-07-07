using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;
using Fargo.ServiceMigration;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<MigrationService>();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(MigrationService.ActivitySourceName));

builder.Services.AddFargoInfrastructure(
    builder.Configuration, configure => configure.UserEmptyCurrentUser());

var host = builder.Build();

host.Run();
