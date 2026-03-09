using Fargo.Infrastructure.Persistence;
using Fargo.MigrationService;
using Fargo.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<MigrationService>();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(MigrationService.ActivitySourceName));

builder.AddSqlServerDbContext<FargoWriteDbContext>("Fargo");

var host = builder.Build();

host.Run();