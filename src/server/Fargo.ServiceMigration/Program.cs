using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;
using Fargo.ServiceMigration;

var builder = Host.CreateApplicationBuilder(args);

builder.AddFargoServiceDefaults();

builder.Services.AddHostedService<FargoMigrationService>();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(FargoMigrationService.ActivitySourceName));

builder.Services.AddFargoDbContext(builder.Configuration);

var host = builder.Build();

host.Run();
